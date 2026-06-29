using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly ApiService _api;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentController(ApiService api, IHttpContextAccessor httpContextAccessor)
    {
        _api = api;
        _httpContextAccessor = httpContextAccessor;
    }

    // GET /Payment/Success?orderId=xxx
    // PayOS redirect về đây sau khi thanh toán thành công
    public async Task<IActionResult> Success(int orderId)
    {
        ViewBag.OrderId = orderId;

        // Lấy trạng thái thanh toán từ API
        var resp = await _api.GetAsync<JsonElement>($"/api/payments/status/{orderId}");
        if (resp?.Data is JsonElement data && data.ValueKind == JsonValueKind.Object)
        {
            ViewBag.PaymentStatus = data.TryGetProperty("paymentStatus", out var ps)
                ? ps.GetString() : "Unknown";
            ViewBag.TransactionCode = data.TryGetProperty("transactionCode", out var tc)
                ? tc.GetString() : null;
        }
        else
        {
            ViewBag.PaymentStatus = "Processing";
        }

        return View();
    }

    // GET /Payment/Cancel?orderId=xxx
    // PayOS redirect về đây nếu khách huỷ thanh toán
    public IActionResult Cancel(int orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }
}
