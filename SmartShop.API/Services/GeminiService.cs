using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartShop.API.Services;

public class ReviewAnalysisResult
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = "Neutral";

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private static readonly ConcurrentDictionary<string, List<object>> _chatSessions = new();

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is missing in User Secrets.");
        _model = config["Gemini:Model"] ?? "gemini-3.8-flash";
    }

    private string Endpoint => $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

    public async Task<string> GenerateProductDescriptionAsync(string name, string category, string features)
    {
        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = "You are a professional copywriter for an electronics store. Write a compelling product description of at most 80 words based on the given product name, category, and features. Return only the description text." } }
            },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = $"Product Name: {name}\nCategory: {category}\nFeatures: {features}" } } }
            }
        };

        return await SendGeminiRequestAsync(payload);
    }

    public async Task<ReviewAnalysisResult> AnalyzeReviewAsync(string content, int rating)
    {
        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = "Analyze the customer product review and rating. Classify sentiment strictly as Positive, Negative, or Neutral, provide a confidence score between 0.0 and 1.0, and write a concise one-sentence summary." } }
            },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = $"Rating: {rating}/5\nReview: {content}" } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseSchema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        sentiment = new { type = "STRING", @enum = new[] { "Positive", "Negative", "Neutral" } },
                        score = new { type = "NUMBER" },
                        summary = new { type = "STRING" }
                    },
                    required = new[] { "sentiment", "score", "summary" }
                }
            }
        };

        var jsonText = await SendGeminiRequestAsync(payload);
        var result = JsonSerializer.Deserialize<ReviewAnalysisResult>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result ?? throw new JsonException("Failed to deserialize Gemini JSON response.");
    }

    public async Task<string> ChatWithAssistantAsync(string sessionId, string userMessage, string productCatalogContext)
    {
        var history = _chatSessions.GetOrAdd(sessionId, _ => new List<object>());

        lock (history)
        {
            history.Add(new { role = "user", parts = new[] { new { text = userMessage } } });
        }

        var systemPrompt = $"""
            You are Smart Shop's AI shopping assistant.
            You MUST ONLY answer questions about the products listed in the catalog below.
            If the user asks anything unrelated to these products or general knowledge outside this catalog, politely refuse to answer.
            Product Catalog:
            {productCatalogContext}
            """;

        object[] currentContents;
        lock (history)
        {
            currentContents = history.ToArray();
        }

        var payload = new
        {
            systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = currentContents
        };

        var replyText = await SendGeminiRequestAsync(payload);

        lock (history)
        {
            history.Add(new { role = "model", parts = new[] { new { text = replyText } } });
        }

        return replyText;
    }

    private async Task<string> SendGeminiRequestAsync(object payload)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(Endpoint, content);

        if ((int)response.StatusCode == 429)
            throw new HttpRequestException("Gemini API quota exceeded (429 Too Many Requests).", null, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Gemini API Error ({response.StatusCode}): {responseBody}", null, response.StatusCode);

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return text ?? string.Empty;
    }
}
