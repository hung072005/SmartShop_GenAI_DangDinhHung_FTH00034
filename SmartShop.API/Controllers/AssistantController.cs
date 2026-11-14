using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.API.Data;
using SmartShop.API.Services;

namespace SmartShop.API.Controllers;

public record ChatRequest(string SessionId, string Message);

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly GeminiService _gemini;

    public AssistantController(SmartShopDbContext context, GeminiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SessionId) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { error = "SessionId and Message are required." });

        var products = await _context.Products.ToListAsync();
        var catalogContext = string.Join("\n", products.Select(p =>
            $"- ID: {p.ProductId} | Name: {p.ProductName} | Category: {p.Category} | Price: {p.Price:N0} VND | Features: {p.Features}"));

        try
        {
            var reply = await _gemini.ChatWithAssistantAsync(req.SessionId, req.Message, catalogContext);
            return Ok(new { sessionId = req.SessionId, reply });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return StatusCode(429, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
