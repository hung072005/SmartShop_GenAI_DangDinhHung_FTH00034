using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartShop.API.Data;
using SmartShop.API.Models;
using SmartShop.API.Services;

namespace SmartShop.API.Controllers;

public record AnalyzeReviewRequest(int ProductId, string CustomerName, string Content, int Rating);

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly GeminiService _gemini;

    public ReviewsController(SmartShopDbContext context, GeminiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeReview([FromBody] AnalyzeReviewRequest req)
    {
        var product = await _context.Products.FindAsync(req.ProductId);
        if (product == null) return NotFound(new { message = $"Product {req.ProductId} not found." });

        try
        {
            var analysis = await _gemini.AnalyzeReviewAsync(req.Content, req.Rating);

            var review = new Review
            {
                ProductId = req.ProductId,
                CustomerName = req.CustomerName,
                Content = req.Content,
                Rating = req.Rating,
                Sentiment = analysis.Sentiment,
                Score = analysis.Score,
                Summary = analysis.Summary,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(review);
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "Invalid JSON returned from Gemini AI." });
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
