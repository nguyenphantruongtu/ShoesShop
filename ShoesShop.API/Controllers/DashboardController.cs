using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var dto = await _dashboardService.GetOverviewAsync();
        return Ok(new ApiResponse<DashboardDto> { Success = true, Data = dto });
    }
}
