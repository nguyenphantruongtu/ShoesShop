using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class VouchersController : Controller
{
    private readonly ApiService _api;
    public VouchersController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Breadcrumb"] = "Voucher";
        var resp = await _api.GetAsync<JsonElement>("/api/admin/vouchers");
        return View(resp?.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string code, string? description, string discountType,
        decimal discountValue, decimal minOrderAmount, decimal? maxDiscountAmount,
        int? usageLimit, int? usageLimitPerUser, DateTime startDate, DateTime endDate, bool isActive = true)
    {
        var body = new { code, description, discountType, discountValue, minOrderAmount, maxDiscountAmount, usageLimit, usageLimitPerUser, startDate, endDate, isActive };
        var (r, _) = await _api.PostAsync<JsonElement>("/api/admin/vouchers", body);
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Tạo voucher thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string? description, decimal discountValue,
        decimal minOrderAmount, decimal? maxDiscountAmount, int? usageLimit,
        int? usageLimitPerUser, DateTime startDate, DateTime endDate, bool isActive)
    {
        var body = new { description, discountValue, minOrderAmount, maxDiscountAmount, usageLimit, usageLimitPerUser, startDate, endDate, isActive };
        var (r, _) = await _api.PutAsync<JsonElement>($"/api/admin/vouchers/{id}", body);
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Cập nhật voucher thành công!" : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (r, _) = await _api.DeleteAsync<JsonElement>($"/api/admin/vouchers/{id}");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã xóa voucher." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }
}
