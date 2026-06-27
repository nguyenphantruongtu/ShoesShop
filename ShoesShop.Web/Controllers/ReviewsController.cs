using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Controllers;

[Authorize(Roles = "Customer")]
public class ReviewsController : Controller
{
    private readonly ApiService _api;
    public ReviewsController(ApiService api) => _api = api;

    // ══ POST /Reviews/Create — Gửi đánh giá từ trang chi tiết đơn hàng ══
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int orderItemId, int orderId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Vui lòng chọn số sao từ 1 đến 5.";
            return RedirectToAction("Detail", "Orders", new { id = orderId });
        }

        var body = new { orderItemId, rating, comment };
        var (result, _) = await _api.PostAsync<JsonElement>("/api/reviews", body);

        if (result?.Success == true)
            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
        else
            TempData["Error"] = result?.Message ?? "Gửi đánh giá thất bại.";

        return RedirectToAction("Detail", "Orders", new { id = orderId });
    }
}
