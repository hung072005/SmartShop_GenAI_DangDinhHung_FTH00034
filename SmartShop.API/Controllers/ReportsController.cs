using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartShop.API.Data;

namespace SmartShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly SmartShopDbContext _context;

    public ReportsController(SmartShopDbContext context)
    {
        _context = context;
    }

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviewReport()
    {
        var rawReport = await _context.Reviews
            .Include(r => r.Product)
            .GroupBy(r => new { r.ProductId, r.Product!.ProductName })
            .Select(g => new
            {
                ProductName = g.Key.ProductName,
                TotalReviews = g.Count(),
                Positive = g.Count(r => r.Sentiment == "Positive"),
                Negative = g.Count(r => r.Sentiment == "Negative"),
                Neutral = g.Count(r => r.Sentiment == "Neutral"),
                AverageRating = Math.Round(g.Average(r => (double)r.Rating), 1)
            })
            .ToListAsync();

        var result = rawReport.Select((item, index) => new
        {
            No = index + 1,
            item.ProductName,
            item.TotalReviews,
            item.Positive,
            item.Negative,
            item.Neutral,
            item.AverageRating
        });

        return Ok(result);
    }
}
