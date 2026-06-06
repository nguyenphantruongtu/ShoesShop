using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Web.Controllers;

public class ReportsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> TopProducts(int top = 10)
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var url = $"https://localhost:7214/api/reports/top-products?top={top}";
            var resp = await client.GetAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResp = JsonSerializer.Deserialize<ApiResponse<IEnumerable<TopProductDto>>>(json, options);
                return View(apiResp?.Data ?? new List<TopProductDto>());
            }
        }
        catch { }
        return View(new List<TopProductDto>());
    }

    [HttpGet("ByBrand")]
    public async Task<IActionResult> ByBrand(DateTime? from = null, DateTime? to = null, int top = 20)
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var url = $"https://localhost:7214/api/reports/by-brand?top={top}";
            if (from.HasValue) url += $"&from={System.Web.HttpUtility.UrlEncode(from.Value.ToString("o"))}";
            if (to.HasValue) url += $"&to={System.Web.HttpUtility.UrlEncode(to.Value.ToString("o"))}";

            var resp = await client.GetAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResp = JsonSerializer.Deserialize<ApiResponse<IEnumerable<RevenueGroupDto>>>(json, options);
                return View(apiResp?.Data ?? new List<RevenueGroupDto>());
            }
        }
        catch { }
        return View(new List<RevenueGroupDto>());
    }

    [HttpGet("ByCategory")]
    public async Task<IActionResult> ByCategory(DateTime? from = null, DateTime? to = null, int top = 20)
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var url = $"https://localhost:7214/api/reports/by-category?top={top}";
            if (from.HasValue) url += $"&from={System.Web.HttpUtility.UrlEncode(from.Value.ToString("o"))}";
            if (to.HasValue) url += $"&to={System.Web.HttpUtility.UrlEncode(to.Value.ToString("o"))}";

            var resp = await client.GetAsync(url);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResp = JsonSerializer.Deserialize<ApiResponse<IEnumerable<RevenueGroupDto>>>(json, options);
                return View(apiResp?.Data ?? new List<RevenueGroupDto>());
            }
        }
        catch { }
        return View(new List<RevenueGroupDto>());
    }
}
