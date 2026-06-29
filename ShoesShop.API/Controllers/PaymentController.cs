using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Payment;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F8. Payment (PayOS)
/// UC-18: POST /api/payments/create-link — Tạo PayOS payment link
/// UC-43: POST /api/payments/webhook    — Webhook callback từ PayOS
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentController(IPaymentService service) => _service = service;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── UC-18: Tạo payment link ────────────────────────────────────────────────
    [HttpPost("create-link")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequest request)
    {
        try
        {
            var result = await _service.CreatePaymentLinkAsync(request.OrderId, UserId);
            return Ok(ApiResponse<CreatePaymentLinkResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CreatePaymentLinkResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CreatePaymentLinkResponse>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CreatePaymentLinkResponse>.Fail($"Lỗi tạo link thanh toán: {ex.Message}"));
        }
    }

    // ── UC-43: Webhook handler ─────────────────────────────────────────────────
    /// <summary>
    /// PayOS gọi endpoint này sau khi xử lý thanh toán.
    /// KHÔNG cần JWT — PayOS server gọi trực tiếp.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] PaymentWebhookRequest body)
    {
        var success = await _service.HandleWebhookAsync(body);
        if (!success)
            return BadRequest(new { code = "01", desc = "Invalid signature or processing error" });

        return Ok(new { code = "00", desc = "success" });
    }

    // ── Trạng thái thanh toán (Customer) ─────────────────────────────────────
    [HttpGet("status/{orderId:int}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetPaymentStatus(int orderId)
    {
        try
        {
            var result = await _service.GetPaymentStatusAsync(orderId, UserId);
            return Ok(ApiResponse<PaymentStatusResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentStatusResponse>.Fail(ex.Message));
        }
    }

    // ── Admin/Staff xem trạng thái thanh toán ────────────────────────────────
    [HttpGet("admin/status/{orderId:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    public async Task<IActionResult> GetPaymentStatusAdmin(int orderId)
    {
        try
        {
            var result = await _service.GetPaymentStatusAdminAsync(orderId);
            return Ok(ApiResponse<PaymentStatusResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PaymentStatusResponse>.Fail(ex.Message));
        }
    }
}
