using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoesShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    // Nhận Service thông qua Dependency Injection giống ProductsController
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // UC-06: Tiếp nhận lệnh POST tạo đơn hàng từ AJAX Frontend
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderInputDto input)
    {
        try
        {
            var orderId = await _orderService.CreateOrderAsync(input);

            // Trả về đúng cấu trúc ApiResponse như các hàm khác của bạn
            return Ok(new ApiResponse<int> { Success = true, Message = "Tạo đơn hàng thành công!", Data = orderId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
        }
    }

    // 🌟 BỔ SUNG: API lấy danh sách lịch sử đơn hàng của User phục vụ trang History.cshtml
    // Endpoint: GET https://localhost:7214/api/orders/user/1
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrderHistory(int userId)
    {
        try
        {
            // Gọi Service lấy danh sách OrderDto dùng chung từ Project Shared
            var data = await _orderService.GetOrderHistoryByUserIdAsync(userId);

            return Ok(new ApiResponse<List<OrderDto>>
            {
                Success = true,
                Message = "Tải lịch sử đơn hàng thành công!",
                Data = data
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<List<OrderDto>>
            {
                Success = false,
                Message = "Lỗi khi lấy lịch sử đơn hàng: " + ex.Message,
                Data = null
            });
        }
    }

    // UC-21: Tiếp nhận lệnh PUT hủy đơn hàng và hoàn kho từ AJAX Frontend
    // Endpoint: PUT https://localhost:7214/api/orders/1/cancel?userId=1
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromQuery] int userId = 1)
    {
        try
        {
            // Gọi Service xử lý nghiệp vụ hủy đơn & hoàn kho
            var result = await _orderService.CancelOrderAsync(id, userId);

            if (result)
            {
                // Đã đồng bộ sang cấu trúc ApiResponse giống hàm CreateOrder giúp AJAX Frontend dễ đọc
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "🎉 Hủy đơn hàng và hoàn trả số lượng giày vào kho thành công!"
                });
            }

            return BadRequest(new ApiResponse<object> { Success = false, Message = "Hủy đơn hàng không thành công." });
        }
        catch (Exception ex)
        {
            // Trả về lý do vì sao không cho hủy (ví dụ: Đơn đã giao...)
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}