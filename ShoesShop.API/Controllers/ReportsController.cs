using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int top = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _reportService.GetTopSellingProductsAsync(top, from, to);
        return Ok(new ApiResponse<IEnumerable<TopProductDto>> { Success = true, Data = result });
    }

    [HttpGet("by-brand")]
    public async Task<IActionResult> GetByBrand([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int top = 20)
    {
        var result = await _reportService.GetRevenueByBrandAsync(from: from, to: to, top: top);
        return Ok(new ApiResponse<IEnumerable<RevenueGroupDto>> { Success = true, Data = result });
    }

    [HttpGet("by-category")]
    public async Task<IActionResult> GetByCategory([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int top = 20)
    {
        var result = await _reportService.GetRevenueByCategoryAsync(from: from, to: to, top: top);
        return Ok(new ApiResponse<IEnumerable<RevenueGroupDto>> { Success = true, Data = result });
    }
}
