using Microsoft.AspNetCore.Mvc;
using ShoesShop.Shared.Constants;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("types")]
    public IActionResult GetTypes()
    {
        return Ok(new ApiResponse<IReadOnlyList<ExportTypeDto>>
        {
            Success = true,
            Data = _exportService.GetExportTypes()
        });
    }

    [HttpGet("csv")]
    public async Task<IActionResult> DownloadCsv(
        [FromQuery] string type,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int top = 20)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Vui lòng chọn loại dữ liệu export." });
        }

        try
        {
            var (content, fileName) = await _exportService.ExportCsvAsync(type, from, to, top);
            return File(content, CsvExportDefaults.MediaType, fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}
