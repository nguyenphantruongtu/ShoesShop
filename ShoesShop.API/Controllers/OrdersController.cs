using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs;
using System;
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
}