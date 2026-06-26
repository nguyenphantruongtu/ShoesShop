using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Order;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F9. Order Tracking (Customer)
/// UC-19: GET /api/orders          — Lịch sử đơn hàng (filter theo status)
/// UC-20: GET /api/orders/{id}     — Chi tiết đơn + timeline trạng thái
/// UC-22: (trong UC-20, kèm Shipment.TrackingNumber + CarrierName)
/// </summary>
[ApiController]
[Route("api/orders")]
[Authorize(Roles = Roles.Customer)]
public class CustomerOrderController : ControllerBase
{
    private readonly IOrderService _service;
    public CustomerOrderController(IOrderService service) => _service = service;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── UC-19 ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lịch sử đơn hàng của customer đang đăng nhập.
    /// status: Pending | Confirmed | Preparing | Shipping | Delivered | Cancelled
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetMyOrdersAsync(UserId, status, page, pageSize);
        return Ok(ApiResponse<OrderListResponse>.Ok(result));
    }

    // ── UC-20 + UC-22 ────────────────────────────────────────────────────────

    /// <summary>
    /// Chi tiết đơn hàng của customer.
    /// Response bao gồm: Items, StatusHistory (timeline), Shipment (tracking number + carrier - UC-22)
    /// </summary>
    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetMyOrderDetail(int orderId)
    {
        try
        {
            var result = await _service.GetMyOrderDetailAsync(orderId, UserId);
            return Ok(ApiResponse<OrderDetailResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDetailResponse>.Fail(ex.Message));
        }
    }

    // ── UC-21: Hủy đơn (customer) ────────────────────────────────────────────

    [HttpPatch("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelMyOrder(int orderId, [FromBody] CancelOrderRequest request)
    {
        try
        {
            var result = await _service.CancelByCustomerAsync(orderId, UserId, request.CancelReason);
            return Ok(ApiResponse<OrderDetailResponse>.Ok(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDetailResponse>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<OrderDetailResponse>.Fail(ex.Message));
        }
    }
}
