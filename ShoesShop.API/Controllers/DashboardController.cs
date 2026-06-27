using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Dashboard;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F14. Dashboard &amp; Reports (Admin)
/// UC-39: GET /api/admin/dashboard — Doanh thu hôm nay/tuần/tháng, số đơn theo trạng thái,
///         biểu đồ 14 ngày gần nhất, top 10 sản phẩm bán chạy
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>
    /// UC-39: Tổng quan dashboard.
    /// Revenue: doanh thu hôm nay / tuần này / tháng này (tính từ đơn PaymentStatus = Paid).
    /// Orders: số đơn theo từng trạng thái.
    /// RevenueChart: biểu đồ doanh thu 14 ngày gần nhất.
    /// TopProducts: top 10 sản phẩm bán chạy nhất.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result));
    }
}
