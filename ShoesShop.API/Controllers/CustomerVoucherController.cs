using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Voucher;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

/// <summary>
/// UC-16: Customer áp mã voucher ở giỏ hàng/checkout để xem trước số tiền được giảm.
/// Endpoint này chỉ tính toán (không ghi DB) — voucher chỉ thực sự được dùng
/// (tăng UsedCount) khi đơn hàng được tạo thành công qua POST /api/orders.
/// </summary>
[ApiController]
[Route("api/vouchers")]
[Authorize(Roles = Roles.Customer)]
public class CustomerVoucherController : ControllerBase
{
    private readonly IVoucherService _service;
    public CustomerVoucherController(IVoucherService service) => _service = service;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyVoucherRequest request)
    {
        try
        {
            var result = await _service.ValidateForOrderAsync(request.Code, request.SubTotal, UserId);
            return Ok(ApiResponse<VoucherPreviewResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<VoucherPreviewResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<VoucherPreviewResponse>.Fail(ex.Message));
        }
    }
}
