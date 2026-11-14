using Microsoft.AspNetCore.Mvc;
using SmartShop.API.Data;
using SmartShop.API.Services;

namespace SmartShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly SmartShopDbContext _context;
    private readonly GeminiService _gemini;

    public ProductsController(SmartShopDbContext context, GeminiService gemini)
    {
        _context = context;
        _gemini = gemini;
    }

    [HttpPost("{id}/generatedescription")]
    public async Task<IActionResult> GenerateDescription(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound(new { message = $"Product {id} not found." });

        try
        {
            var description = await _gemini.GenerateProductDescriptionAsync(product.ProductName, product.Category, product.Features);
            product.Description = description.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { product.ProductId, product.ProductName, product.Description });
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
