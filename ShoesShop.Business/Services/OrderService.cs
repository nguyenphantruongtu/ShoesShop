using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ShoesShop.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    // UC-06: Tiếp nhận tạo đơn hàng từ AJAX Frontend
    public async Task<int> CreateOrderAsync(OrderInputDto input)
    {
        if (input == null || input.OrderItems == null || input.OrderItems.Count == 0)
        {
            throw new ArgumentException("Danh sách sản phẩm trong giỏ hàng trống!");
        }

        await _orderRepository.BeginTransactionAsync();
        try
        {
            string uniqueOrderCode = "HD" + DateTime.Now.ToString("yyMMdd") + new Random().Next(100, 999);

            var order = new Order
            {
                OrderCode = uniqueOrderCode,
                UserId = 1, // Mặc định tài khoản "duy pham"
                RecipientName = string.IsNullOrWhiteSpace(input.RecipientName) ? "Khách vãng lai" : input.RecipientName,
                RecipientPhone = string.IsNullOrWhiteSpace(input.RecipientPhone) ? "0338025819" : input.RecipientPhone,
                ShippingAddress = string.IsNullOrWhiteSpace(input.ShippingAddress) ? "Chưa cung cấp" : input.ShippingAddress,
                Province = string.IsNullOrWhiteSpace(input.Province) ? "Chưa rõ" : input.Province,
                District = string.IsNullOrWhiteSpace(input.District) ? "Chưa rõ" : input.District,
                Ward = string.IsNullOrWhiteSpace(input.Ward) ? "Chưa rõ" : input.Ward,
                SubTotal = input.SubTotal > 0 ? input.SubTotal : input.TotalAmount,
                ShippingFee = 0,
                DiscountAmount = 0,
                TotalAmount = input.TotalAmount > 0 ? input.TotalAmount : 500000,
                Note = input.Note,
                OrderStatus = "Pending",
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.Now 
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            foreach (var item in input.OrderItems)
            {
                int validVariantId = item.VariantId <= 0 ? 1 : item.VariantId;

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    VariantId = validVariantId,
                    ProductName = string.IsNullOrWhiteSpace(item.ProductName) ? "Sản phẩm giày" : item.ProductName,
                    SizeValue = string.IsNullOrWhiteSpace(item.SizeValue) ? "Mặc định" : item.SizeValue,
                    ColorName = string.IsNullOrWhiteSpace(item.ColorName) ? "Mặc định" : item.ColorName,
                    UnitPrice = item.UnitPrice > 0 ? item.UnitPrice : 500000,
                    Quantity = item.Quantity > 0 ? item.Quantity : 1,
                    LineTotal = (item.UnitPrice > 0 ? item.UnitPrice : 500000) * (item.Quantity > 0 ? item.Quantity : 1)
                };

                await _orderRepository.AddAsync(orderItem);
            }

            await _orderRepository.SaveChangesAsync();
            await _orderRepository.CommitTransactionAsync();

            return order.OrderId;
        }
        catch (Exception ex)
        {
            await _orderRepository.RollbackTransactionAsync();
            string internalErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw new Exception("Lỗi hệ thống từ SQL Server: " + internalErrorMessage);
        }
    }

    // UC-21: Xử lý hủy đơn hàng phía Backend API
    public async Task<bool> CancelOrderAsync(int orderId, int userId)
    {
        await _orderRepository.BeginTransactionAsync();
        try
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                throw new Exception("Đơn hàng không tồn tại!");
            }

            // Chỉ cho phép hủy khi trạng thái là Chờ xử lý hoặc Đã xác nhận
            if (order.OrderStatus != "Pending" && order.OrderStatus != "Confirmed")
            {
                throw new Exception($"Không thể hủy đơn hàng do đơn đang ở trạng thái: {order.OrderStatus}");
            }

            // Đã sửa cú pháp chuỗi ghi chú
            string timeString = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            order.OrderStatus = "Canceled";
            order.Note = (string.IsNullOrWhiteSpace(order.Note) ? "" : order.Note + " | ") + $"Khách hủy đơn vào ngày: {timeString}";

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            await _orderRepository.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _orderRepository.RollbackTransactionAsync();
            throw new Exception("Lỗi xử lý hủy đơn: " + ex.Message);
        }
    }

    // Tải dữ liệu lịch sử đơn hàng của User ra trang giao diện
    public async Task<List<OrderDto>> GetOrderHistoryByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);

        if (orders == null) return new List<OrderDto>();

        return orders.Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            OrderDate = o.CreatedAt,
            TotalAmount = o.TotalAmount,
            OrderStatus = o.OrderStatus ?? "Pending"
        }).ToList();
    }
}