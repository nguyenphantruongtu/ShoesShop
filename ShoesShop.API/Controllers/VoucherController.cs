using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Voucher;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F6. Voucher
/// UC-36: Admin quản lý voucher — CRUD + điều kiện áp dụng
/// GET    /api/admin/vouchers
/// GET    /api/admin/vouchers/{id}
/// POST   /api/admin/vouchers
/// PUT    /api/admin/vouchers/{id}
/// DELETE /api/admin/vouchers/{id}
/// </summary>
[ApiController]
[Route("api/admin/vouchers")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _service;
    public VoucherController(IVoucherService service) => _service = service;

    /// <summary>UC-36: Danh sách tất cả voucher</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<VoucherResponse>>.Ok(result));
    }

    /// <summary>UC-36: Lấy voucher theo ID</summary>
    [HttpGet("{voucherId:int}")]
    public async Task<IActionResult> GetById(int voucherId)
    {
        try
        {
            var result = await _service.GetByIdAsync(voucherId);
            return Ok(ApiResponse<VoucherResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VoucherResponse>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// UC-36: Tạo voucher mới.
    /// DiscountType: "Percentage" (giảm %) hoặc "Fixed" (giảm tiền cố định)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVoucherRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { voucherId = result.VoucherId },
                ApiResponse<VoucherResponse>.Ok(result, "Tạo voucher thành công."));
        }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<VoucherResponse>.Fail(ex.Message)); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<VoucherResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-36: Cập nhật voucher (không đổi được Code và DiscountType)</summary>
    [HttpPut("{voucherId:int}")]
    public async Task<IActionResult> Update(int voucherId, [FromBody] UpdateVoucherRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(voucherId, request);
            return Ok(ApiResponse<VoucherResponse>.Ok(result, "Cập nhật voucher thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<VoucherResponse>.Fail(ex.Message)); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<VoucherResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-36: Xóa voucher (chỉ khi chưa được dùng lần nào)</summary>
    [HttpDelete("{voucherId:int}")]
    public async Task<IActionResult> Delete(int voucherId)
    {
        try
        {
            await _service.DeleteAsync(voucherId);
            return Ok(ApiResponse<object>.Ok(null, "Xóa voucher thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<object>.Fail(ex.Message)); }
    }
}
