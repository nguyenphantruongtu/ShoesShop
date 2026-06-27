using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class BrandsController : Controller
{
    private readonly ApiService _api;
    public BrandsController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Breadcrumb"] = "Thương hiệu";
        var resp = await _api.GetAsync<JsonElement>("/api/admin/brands");
        return View(resp?.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string brandName, string? logoUrl, string? description, bool isActive = true)
    {
        var (r, _) = await _api.PostAsync<JsonElement>("/api/admin/brands", new { brandName, logoUrl, description, isActive });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Thêm thương hiệu thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string brandName, string? logoUrl, string? description, bool isActive)
    {
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/brands/{id}", new { brandName, logoUrl, description, isActive });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật thương hiệu thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/brands/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa thương hiệu." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }
}
