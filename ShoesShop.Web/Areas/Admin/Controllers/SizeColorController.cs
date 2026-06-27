using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class SizeColorController : Controller
{
    private readonly ApiService _api;
    public SizeColorController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Breadcrumb"] = "Size & Màu sắc";
        var sizes  = await _api.GetAsync<JsonElement>("/api/admin/sizes");
        var colors = await _api.GetAsync<JsonElement>("/api/admin/colors");
        ViewBag.Sizes  = sizes?.Data;
        ViewBag.Colors = colors?.Data;
        return View();
    }

    // ── Sizes ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSize(string sizeValue, int displayOrder = 0)
    {
        var (r, _) = await _api.PostAsync<JsonElement>("/api/admin/sizes", new { sizeValue, displayOrder });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Thêm size thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSize(int id, string sizeValue, int displayOrder)
    {
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/sizes/{id}", new { sizeValue, displayOrder });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật size thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSize(int id)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/sizes/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa size." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    // ── Colors ──
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateColor(string colorName, string? hexCode)
    {
        var (r, _) = await _api.PostAsync<JsonElement>("/api/admin/colors", new { colorName, hexCode });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Thêm màu thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditColor(int id, string colorName, string? hexCode)
    {
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/colors/{id}", new { colorName, hexCode });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật màu thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteColor(int id)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/colors/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa màu." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }
}
