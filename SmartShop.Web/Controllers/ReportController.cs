using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Web.Models;

namespace SmartShop.Web.Controllers;

public class ReportController : Controller
{
    private readonly HttpClient _httpClient;

    public ReportController(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5200");
    }

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("/api/reports/reviews");
        var list = new List<ReviewReportViewModel>();

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            list = JsonSerializer.Deserialize<List<ReviewReportViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();
        }

        return View(list);
    }
}
