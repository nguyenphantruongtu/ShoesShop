using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/admin/products/{productId:int}/variants")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class ProductVariantController : ControllerBase
{
    private readonly IProductVariantService _service;
    public ProductVariantController(IProductVariantService service) => _service = service;

    /// <summary>UC-30: Danh sách variant của sản phẩm</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int productId)
        => Ok(ApiResponse<List<VariantResponse>>.Ok(await _service.GetByProductAsync(productId)));

    /// <summary>UC-30: Tạo variant mới (size × color × stock)</summary>
    [HttpPost]
    public async Task<IActionResult> Create(int productId, [FromBody] CreateVariantRequest request)
    {
        try { return Ok(ApiResponse<VariantResponse>.Ok(await _service.CreateAsync(productId, request), "Tạo variant thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VariantResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<VariantResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-30: Cập nhật variant (giá, SKU, active)</summary>
    [HttpPut("{variantId:int}")]
    public async Task<IActionResult> Update(int productId, int variantId, [FromBody] UpdateVariantRequest request)
    {
        try { return Ok(ApiResponse<VariantResponse>.Ok(await _service.UpdateAsync(productId, variantId, request), "Cập nhật thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VariantResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<VariantResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-30: Cập nhật stock per SKU</summary>
    [HttpPatch("{variantId:int}/stock")]
    public async Task<IActionResult> UpdateStock(int productId, int variantId, [FromBody] UpdateStockRequest request)
    {
        try { return Ok(ApiResponse<VariantResponse>.Ok(await _service.UpdateStockAsync(productId, variantId, request), "Cập nhật tồn kho thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VariantResponse>.Fail(ex.Message)); }
    }

    [HttpDelete("{variantId:int}")]
    public async Task<IActionResult> Delete(int productId, int variantId)
    {
        try { await _service.DeleteAsync(productId, variantId); return Ok(ApiResponse<string>.Ok(string.Empty, "Xóa variant thành công.")); }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<string>.Fail(ex.Message)); }
    }
}
