using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class OrdersController : Controller
{
    private readonly ApiService _api;
    public OrdersController(ApiService api) => _api = api;

    public async Task<IActionResult> Index(string? search, string? status, int page = 1)
    {
        ViewData["Breadcrumb"] = "Đơn hàng";
        var qs = $"?page={page}&pageSize=15" +
                 (string.IsNullOrEmpty(search) ? "" : $"&search={Uri.EscapeDataString(search)}") +
                 (string.IsNullOrEmpty(status) ? "" : $"&status={Uri.EscapeDataString(status)}");
        var resp = await _api.GetAsync<JsonElement>($"/api/staff/orders{qs}");
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Page   = page;
        return View(resp?.Data);
    }

    public async Task<IActionResult> Detail(int id)
    {
        ViewData["Breadcrumb"] = $"Đơn hàng #{id}";
        var resp = await _api.GetAsync<JsonElement>($"/api/staff/orders/{id}");
        if (resp?.Data == null) return NotFound();
        return View(resp.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/staff/orders/{id}/confirm");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xác nhận đơn hàng!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus, string? note)
    {
        var body = new { newStatus, note };
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/staff/orders/{id}/status", body);
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật trạng thái thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string cancelReason)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/staff/orders/{id}/cancel", new { cancelReason });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã hủy đơn hàng!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Detail), new { id });
    }
}
