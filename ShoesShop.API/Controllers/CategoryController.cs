using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;
    public CategoryController(ICategoryService service) => _service = service;

    /// <summary>UC-26: Danh sách category dạng tree</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<CategoryResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<CategoryResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<CategoryResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-26: Tạo category (hỗ trợ parent-child)</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.CategoryId },
                ApiResponse<CategoryResponse>.Ok(result, "Tạo danh mục thành công."));
        }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<CategoryResponse>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResponse<CategoryResponse>.Ok(result, "Cập nhật danh mục thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<CategoryResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<CategoryResponse>.Fail(ex.Message)); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa danh mục thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
    }
}
