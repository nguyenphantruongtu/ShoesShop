using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DashboardController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync("https://localhost:7214/api/dashboard/overview");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResp = JsonSerializer.Deserialize<ApiResponse<DashboardDto>>(json, options);
                return View(apiResp?.Data ?? new DashboardDto());
            }
        }
        catch { }

        return View(new DashboardDto());
    }
}
