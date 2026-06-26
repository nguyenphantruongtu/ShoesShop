using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShoesShop.Shared.Constants;
using ShoesShop.Shared.DTOs;
using ShoesShop.Web.Models;

namespace ShoesShop.Web.Controllers;

public class ExportController : Controller
{
    private const string ApiBaseUrl = "https://localhost:7214";
    private readonly IHttpClientFactory _httpClientFactory;

    public ExportController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new ExportViewModel
        {
            From = DateTime.Today.AddMonths(-1),
            To = DateTime.Today,
            ExportTypes = await LoadExportTypesAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Download(ExportViewModel model)
    {
        model.ExportTypes = await LoadExportTypesAsync();

        if (string.IsNullOrWhiteSpace(model.SelectedType))
        {
            model.ErrorMessage = "Vui lòng chọn loại dữ liệu cần export.";
            return View("Index", model);
        }

        var client = _httpClientFactory.CreateClient();
        try
        {
            var url = $"{ApiBaseUrl}/api/export/csv?type={Uri.EscapeDataString(model.SelectedType)}&top={model.Top}";
            if (model.From.HasValue)
            {
                url += $"&from={Uri.EscapeDataString(model.From.Value.ToString("o"))}";
            }

            if (model.To.HasValue)
            {
                url += $"&to={Uri.EscapeDataString(model.To.Value.ToString("o"))}";
            }

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                model.ErrorMessage = "Không thể export dữ liệu. Vui lòng kiểm tra API và thử lại.";
                return View("Index", model);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, CsvExportDefaults.MediaType, fileName);
        }
        catch
        {
            model.ErrorMessage = "Không kết nối được tới API. Hãy chạy ShoesShop.API trước.";
            return View("Index", model);
        }
    }

    private async Task<IReadOnlyList<ExportTypeDto>> LoadExportTypesAsync()
    {
        var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync($"{ApiBaseUrl}/api/export/types");
            if (!response.IsSuccessStatusCode)
            {
                return DefaultExportTypes();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResp = JsonSerializer.Deserialize<ApiResponse<IReadOnlyList<ExportTypeDto>>>(json, options);
            return apiResp?.Data ?? DefaultExportTypes();
        }
        catch
        {
            return DefaultExportTypes();
        }
    }

    private static IReadOnlyList<ExportTypeDto> DefaultExportTypes() =>
    [
        new ExportTypeDto { Key = "products", Label = "Danh sách sản phẩm", SupportsDateRange = false, SupportsTop = false },
        new ExportTypeDto { Key = "orders", Label = "Danh sách đơn hàng", SupportsDateRange = true, SupportsTop = false },
        new ExportTypeDto { Key = "top-products", Label = "Top sản phẩm bán chạy", SupportsDateRange = true, SupportsTop = true },
        new ExportTypeDto { Key = "by-brand", Label = "Doanh thu theo thương hiệu", SupportsDateRange = true, SupportsTop = true },
        new ExportTypeDto { Key = "by-category", Label = "Doanh thu theo danh mục", SupportsDateRange = true, SupportsTop = true }
    ];
}
