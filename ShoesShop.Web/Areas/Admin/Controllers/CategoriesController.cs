using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class CategoriesController : Controller
{
    private readonly ApiService _api;
    public CategoriesController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Breadcrumb"] = "Danh mục";
        var resp = await _api.GetAsync<JsonElement>("/api/admin/categories");
        return View(resp?.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string categoryName, string? slug, int? parentCategoryId, string? description, string? imageUrl, bool isActive = true)
    {
        var (r, _) = await _api.PostAsync<JsonElement>("/api/admin/categories", new { categoryName, slug, parentCategoryId, description, imageUrl, isActive });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Thêm danh mục thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string categoryName, string? slug, int? parentCategoryId, string? description, string? imageUrl, bool isActive)
    {
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/categories/{id}", new { categoryName, slug, parentCategoryId, description, imageUrl, isActive });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật danh mục thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/categories/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa danh mục." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }
}
