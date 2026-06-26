using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Controllers;

[Authorize(Roles = "Customer")]
public class OrdersController : Controller
{
    private readonly ApiService _api;
    public OrdersController(ApiService api) => _api = api;

    // ══ GET /Orders — Danh sách đơn hàng của tôi ══
    public async Task<IActionResult> Index(string? status, int page = 1)
    {
        var qs   = $"?page={page}&pageSize=10" + (string.IsNullOrEmpty(status) ? "" : $"&status={status}");
        var resp = await _api.GetAsync<JsonElement>($"/api/orders{qs}");
        ViewBag.Status = status;
        ViewBag.Page   = page;
        return View(resp?.Data);
    }

    // ══ GET /Orders/Detail/{id} ══
    public async Task<IActionResult> Detail(int id)
    {
        var resp = await _api.GetAsync<JsonElement>($"/api/orders/{id}");
        if (resp?.Data is null
            || resp.Data.ValueKind == JsonValueKind.Undefined
            || resp.Data.ValueKind == JsonValueKind.Null
            || resp.Success == false)
        {
            TempData["Error"] = resp?.Message ?? "Không tìm thấy đơn hàng.";
            return RedirectToAction(nameof(Index));
        }
        return View(resp.Data);
    }

    // ══ POST /Orders/Cancel/{id} ══
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string cancelReason)
    {
        var body = new { cancelReason };
        var (result, _) = await _api.PatchAsync<JsonElement>($"/api/orders/{id}/cancel", body);

        if (result?.Success == true)
            TempData["Success"] = "Đã hủy đơn hàng.";
        else
            TempData["Error"] = result?.Message ?? "Hủy đơn thất bại.";

        return RedirectToAction(nameof(Detail), new { id });
    }
}
