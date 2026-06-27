using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;

namespace ShoesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff")]
public class DashboardController : Controller
{
    private readonly ApiService _api;
    public DashboardController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewData["Breadcrumb"] = "Dashboard";
        var resp = await _api.GetAsync<object>("/api/admin/dashboard");
        return View(resp?.Data);
    }
}
