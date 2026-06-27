using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/admin/brands")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class BrandController : ControllerBase
{
    private readonly IBrandService _service;
    public BrandController(IBrandService service) => _service = service;

    /// <summary>UC-27: Danh sách thương hiệu</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<BrandResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { return Ok(ApiResponse<BrandResponse>.Ok(await _service.GetByIdAsync(id))); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<BrandResponse>.Fail(ex.Message)); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.BrandId },
                ApiResponse<BrandResponse>.Ok(result, "Tạo thương hiệu thành công."));
        }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<BrandResponse>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandRequest request)
    {
        try { return Ok(ApiResponse<BrandResponse>.Ok(await _service.UpdateAsync(id, request), "Cập nhật thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<BrandResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<BrandResponse>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _service.DeleteAsync(id); return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }
}
