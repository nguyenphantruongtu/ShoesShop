using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApiService _api;
    public UsersController(ApiService api) => _api = api;

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Breadcrumb"] = "Người dùng";
        var qs   = $"?page={page}&pageSize=15" + (string.IsNullOrEmpty(search) ? "" : $"&search={Uri.EscapeDataString(search)}");
        var resp = await _api.GetAsync<JsonElement>($"/api/admin/users{qs}");
        ViewBag.Search = search;
        ViewBag.Page   = page;
        return View(resp?.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/users/{id}/lock");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã khóa tài khoản." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/users/{id}/unlock");
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã mở khóa tài khoản." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id, string roleName)
    {
        var (r, _) = await _api.PatchAsync<JsonElement>($"/api/admin/users/{id}/role", new { roleName });
        TempData[r?.Success == true ? "Success" : "Error"] = r?.Success == true ? "Đã thay đổi vai trò." : (r?.Message ?? "Thất bại");
        return RedirectToAction(nameof(Index));
    }
}
