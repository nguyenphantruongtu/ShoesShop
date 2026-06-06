using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    public ProductController(IProductService service) => _service = service;

    /// <summary>UC-29: Danh sách sản phẩm (search, filter, paging)</summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetListAsync(search, categoryId, brandId, isActive, page, pageSize);
        return Ok(ApiResponse<ProductListResponse>.Ok(result));
    }

    /// <summary>UC-29: Chi tiết sản phẩm (bao gồm ảnh + variants)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { return Ok(ApiResponse<ProductDetailResponse>.Ok(await _service.GetByIdAsync(id))); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProductDetailResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-29: Tạo sản phẩm (kèm multi-image)</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ProductId },
                ApiResponse<ProductDetailResponse>.Ok(result, "Tạo sản phẩm thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProductDetailResponse>.Fail(ex.Message)); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        try { return Ok(ApiResponse<ProductDetailResponse>.Ok(await _service.UpdateAsync(id, request), "Cập nhật thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProductDetailResponse>.Fail(ex.Message)); }
    }

    /// <summary>Soft delete (IsActive = false)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _service.DeleteAsync(id); return Ok(ApiResponse<string>.Ok(string.Empty, "Đã ẩn sản phẩm.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
    }

    // -------- IMAGE MANAGEMENT --------

    /// <summary>UC-29: Thêm ảnh cho sản phẩm</summary>
    [HttpPost("{id:int}/images")]
    public async Task<IActionResult> AddImage(int id, [FromBody] AddProductImageRequest request)
    {
        try { return Ok(ApiResponse<ProductImageResponse>.Ok(await _service.AddImageAsync(id, request), "Thêm ảnh thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProductImageResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-29: Xóa ảnh khỏi sản phẩm</summary>
    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        try { await _service.DeleteImageAsync(id, imageId); return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa ảnh thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
    }

    /// <summary>UC-29: Set primary image</summary>
    [HttpPatch("{id:int}/images/{imageId:int}/set-primary")]
    public async Task<IActionResult> SetPrimaryImage(int id, int imageId)
    {
        try { return Ok(ApiResponse<ProductImageResponse>.Ok(await _service.SetPrimaryImageAsync(id, imageId), "Đã set ảnh chính.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<ProductImageResponse>.Fail(ex.Message)); }
    }
}
