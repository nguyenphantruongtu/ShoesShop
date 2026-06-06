using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;

namespace ShoesShop.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.AdminOrStaff)]
public class SizeColorController : ControllerBase
{
    private readonly ISizeColorService _service;
    public SizeColorController(ISizeColorService service) => _service = service;

    // ========== UC-28: SIZE ==========

    [HttpGet("api/admin/sizes")]
    public async Task<IActionResult> GetSizes()
        => Ok(ApiResponse<List<SizeResponse>>.Ok(await _service.GetAllSizesAsync()));

    [HttpPost("api/admin/sizes")]
    public async Task<IActionResult> CreateSize([FromBody] CreateSizeRequest request)
    {
        try { return Ok(ApiResponse<SizeResponse>.Ok(await _service.CreateSizeAsync(request), "Thêm size thành công.")); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<SizeResponse>.Fail(ex.Message)); }
    }

    [HttpPut("api/admin/sizes/{id:int}")]
    public async Task<IActionResult> UpdateSize(int id, [FromBody] UpdateSizeRequest request)
    {
        try { return Ok(ApiResponse<SizeResponse>.Ok(await _service.UpdateSizeAsync(id, request), "Cập nhật thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<SizeResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<SizeResponse>.Fail(ex.Message)); }
    }

    [HttpDelete("api/admin/sizes/{id:int}")]
    public async Task<IActionResult> DeleteSize(int id)
    {
        try { await _service.DeleteSizeAsync(id); return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa size thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
    }

    // ========== UC-28: COLOR ==========

    [HttpGet("api/admin/colors")]
    public async Task<IActionResult> GetColors()
        => Ok(ApiResponse<List<ColorResponse>>.Ok(await _service.GetAllColorsAsync()));

    [HttpPost("api/admin/colors")]
    public async Task<IActionResult> CreateColor([FromBody] CreateColorRequest request)
    {
        try { return Ok(ApiResponse<ColorResponse>.Ok(await _service.CreateColorAsync(request), "Thêm màu thành công.")); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<ColorResponse>.Fail(ex.Message)); }
    }

    [HttpPut("api/admin/colors/{id:int}")]
    public async Task<IActionResult> UpdateColor(int id, [FromBody] UpdateColorRequest request)
    {
        try { return Ok(ApiResponse<ColorResponse>.Ok(await _service.UpdateColorAsync(id, request), "Cập nhật thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ColorResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<ColorResponse>.Fail(ex.Message)); }
    }

    [HttpDelete("api/admin/colors/{id:int}")]
    public async Task<IActionResult> DeleteColor(int id)
    {
        try { await _service.DeleteColorAsync(id); return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa màu thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
    }
}
