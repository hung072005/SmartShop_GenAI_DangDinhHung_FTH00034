# Smart Shop - Generative AI for .NET Developers with Google AI (SET01)

## 1. Gemini API Key Protection
- The Gemini API Key is **never hard-coded** in `appsettings.json` or C# source files.
- During development, the key is stored securely outside the repository using **.NET User Secrets**:
  ```bash
  dotnet user-secrets set "Gemini:ApiKey" "<YOUR_API_KEY>"
  In production environments, the key is injected via environment variables (Gemini__ApiKey).

2. Prompts Used in APIs
Requirement 1: Generate Product Description (POST /api/products/{id}/generatedescription)
System Instruction:

You are a professional copywriter for an electronics store. Write a compelling product description of at most 80 words based on the given product name, category, and features. Return only the description text.

User Prompt:

Product Name: {ProductName}
Category: {Category}
Features: {Features}

Requirement 2: Analyze Customer Review (POST /api/reviews/analyze)
System Instruction:

Analyze the customer product review and rating. Classify sentiment strictly as Positive, Negative, or Neutral, provide a confidence score between 0.0 and 1.0, and write a concise one-sentence summary.

User Prompt:

Rating: {Rating}/5
Review: {Content}

Structured Output: Configured via responseMimeType: "application/json" and responseSchema enforcing sentiment, score, and summary.

Requirement 3: AI Shopping Assistant (POST /api/assistant/chat)
System Instruction:

You are Smart Shop's AI shopping assistant. You MUST ONLY answer questions about the products listed in the catalog below. If the user asks anything unrelated to these products or general knowledge outside this catalog, politely refuse to answer.
Product Catalog: [Dynamic list loaded from MySQL Product table]

User Prompt:

{UserMessage} (Maintained in conversation history array mapped by sessionId).
