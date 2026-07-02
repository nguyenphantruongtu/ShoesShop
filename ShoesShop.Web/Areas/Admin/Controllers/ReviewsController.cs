using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class ReviewsController : Controller
{
    private readonly ApiService _api;
    public ReviewsController(ApiService api) => _api = api;

    public async Task<IActionResult> Index(string? status, int page = 1)
    {
        ViewData["Breadcrumb"] = "Đánh giá sản phẩm";
        // pageSize lớn để gom đủ dữ liệu theo sản phẩm cho giao diện gallery (nhóm theo sản phẩm, lọc theo click)
        var qs = $"?page={page}&pageSize=60" +
                 (string.IsNullOrEmpty(status) ? "" : $"&status={Uri.EscapeDataString(status)}");
        var resp = await _api.GetAsync<JsonElement>($"/api/admin/reviews{qs}");
        ViewBag.Status = status;
        ViewBag.Page   = page;
        return View(resp?.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? status, int page = 1)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/reviews/{id}/approve");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã duyệt đánh giá." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index), new { status, page });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? status, int page = 1)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/reviews/{id}/reject");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã ẩn đánh giá." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index), new { status, page });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? status, int page = 1)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/reviews/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa đánh giá." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index), new { status, page });
    }
}
