using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F10. Order Management (Staff)
/// UC-25: Endpoint này chỉ [Authorize(Roles = AdminOrStaff)] — sidebar/dashboard được bảo vệ tự động
/// UC-31: GET  /api/staff/orders              — Danh sách đơn hàng (filter status, search)
/// UC-32: GET  /api/staff/orders/{id}         — Chi tiết đơn + xác nhận
///        PATCH /api/staff/orders/{id}/confirm — Pending → Confirmed
/// UC-33: PATCH /api/staff/orders/{id}/status  — Cập nhật trạng thái (Delivered ⇒ COD tự Paid)
/// UC-35: PATCH /api/staff/orders/{id}/cancel  — Hủy đơn + rollback stock
/// </summary>
[ApiController]
[Route("api/staff/orders")]
[Authorize(Roles = Roles.AdminOrStaff)]
public class StaffOrderController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly IPaymentService _payment;
    public StaffOrderController(IOrderService service, IPaymentService payment)
    {
        _service = service;
        _payment = payment;
    }

    private int StaffId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── UC-31 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Danh sách đơn hàng với filter và phân trang.
    /// status: Pending | Confirmed | Preparing | Shipping | Delivered | Cancelled
    /// search: mã đơn hoặc SĐT khách hàng
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        var result = await _service.GetOrdersAsync(search, status, page, pageSize);
        return Ok(ApiResponse<OrderListResponse>.Ok(result));
    }

    // ── UC-32 ────────────────────────────────────────────────────────────────

    /// <summary>Chi tiết đơn hàng (kèm items, shipment, status history)</summary>
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetDetail(int orderId)
    {
        try
        {
            await _payment.SyncPaymentStatusAsync(orderId);
            var result = await _service.GetOrderDetailAsync(orderId);
            return Ok(ApiResponse<OrderDetailResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDetailResponse>.Fail(ex.Message));
        }
    }

    /// <summary>UC-32: Xác nhận đơn — Pending → Confirmed</summary>
    [HttpPatch("{orderId:int}/confirm")]
    public async Task<IActionResult> Confirm(int orderId)
    {
        try
        {
            var result = await _service.ConfirmOrderAsync(orderId, StaffId);
            return Ok(ApiResponse<OrderDetailResponse>.Ok(result, "Xác nhận đơn hàng thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
    }

    // ── UC-33 + UC-34 ────────────────────────────────────────────────────────

    /// <summary>
    /// Cập nhật trạng thái đơn hàng.
    /// Luồng hợp lệ: Confirmed→Preparing → Shipping → Delivered
    /// Khi NewStatus = "Delivered": đơn COD tự động được đánh dấu đã thanh toán (PaymentStatus = Paid).
    /// </summary>
    [HttpPatch("{orderId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var result = await _service.UpdateStatusAsync(orderId, request, StaffId);
            return Ok(ApiResponse<OrderDetailResponse>.Ok(result, $"Cập nhật trạng thái sang '{request.NewStatus}' thành công."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
    }

    // ── UC-35 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Hủy đơn hàng với lý do + rollback stock về variant.
    /// Chỉ được hủy khi status: Pending | Confirmed | Preparing
    /// </summary>
    [HttpPatch("{orderId:int}/cancel")]
    public async Task<IActionResult> Cancel(int orderId, [FromBody] CancelOrderRequest request)
    {
        try
        {
            var result = await _service.CancelOrderAsync(orderId, request, StaffId);
            return Ok(ApiResponse<OrderDetailResponse>.Ok(result, "Đã hủy đơn hàng và hoàn trả tồn kho."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<OrderDetailResponse>.Fail(ex.Message)); }
    }
}
