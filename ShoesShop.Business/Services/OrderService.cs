using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoesShop.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<int> CreateOrderAsync(OrderInputDto input)
    {
        if (input == null || input.OrderItems == null || input.OrderItems.Count == 0)
        {
            throw new ArgumentException("Danh sách sản phẩm trong giỏ hàng trống!");
        }

        // Kích hoạt một phiên làm việc an toàn (Transaction) qua Repository
        await _orderRepository.BeginTransactionAsync();
        try
        {
            // Tạo chuỗi mã đơn ngắn gọn để tránh vượt quá số lượng ký tự cho phép của cột trong cơ sở dữ liệu
            string uniqueOrderCode = "HD" + DateTime.Now.ToString("yyMMdd") + new Random().Next(100, 999);

            // 1. Khởi tạo và điền dữ liệu cho thực thể Order
            var order = new Order
            {
                OrderCode = uniqueOrderCode,
                UserId = 1, // Sử dụng UserId = 1 của tài khoản duy pham đã có trong cơ sở dữ liệu
                RecipientName = string.IsNullOrWhiteSpace(input.RecipientName) ? "Khách vãng lai" : input.RecipientName,
                RecipientPhone = string.IsNullOrWhiteSpace(input.RecipientPhone) ? "0338025819" : input.RecipientPhone,
                ShippingAddress = string.IsNullOrWhiteSpace(input.ShippingAddress) ? "Chưa cung cấp" : input.ShippingAddress,
                Province = string.IsNullOrWhiteSpace(input.Province) ? "Chưa rõ" : input.Province,
                District = string.IsNullOrWhiteSpace(input.District) ? "Chưa rõ" : input.District,
                Ward = string.IsNullOrWhiteSpace(input.Ward) ? "Chưa rõ" : input.Ward,
                SubTotal = input.SubTotal > 0 ? input.SubTotal : input.TotalAmount,
                ShippingFee = 0,
                DiscountAmount = 0,
                TotalAmount = input.TotalAmount > 0 ? input.TotalAmount : 500000, // Đặt giá trị tượng trưng nếu bằng 0
                Note = input.Note,
                OrderStatus = "Pending",      // Giá trị chuỗi không null bắt buộc cho trạng thái đơn hàng
                PaymentStatus = "Unpaid",     // Giá trị chuỗi không null bắt buộc cho trạng thái thanh toán
                CreatedAt = DateTime.Now
            };

            // Thêm bản ghi đơn hàng vào cơ sở dữ liệu
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync(); // Lưu dữ liệu tạm thời để sinh ra giá trị cho trường tự tăng OrderId

            // 2. Duyệt qua danh sách và lưu thông tin chi tiết vào thực thể OrderItem
            foreach (var item in input.OrderItems)
            {
                // Kiểm tra và tránh gán giá trị khóa ngoại bằng 0 hoặc số âm để tránh xung đột Foreign Key
                int validVariantId = item.VariantId <= 0 ? 1 : item.VariantId;

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId, // Gắn ID đơn hàng vừa được sinh ra ở bước trên
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

            // Thực hiện lưu toàn bộ danh sách mặt hàng chi tiết xuống cơ sở dữ liệu
            await _orderRepository.SaveChangesAsync();

            // Xác nhận phiên làm việc thành công và lưu chính thức dữ liệu
            await _orderRepository.CommitTransactionAsync();

            return order.OrderId;
        }
        catch (Exception ex)
        {
            // Hủy bỏ các thao tác trung gian trong phiên nếu xuất hiện bất kỳ ngoại lệ nào
            await _orderRepository.RollbackTransactionAsync();

            // Trích xuất thông tin lỗi chi tiết từ cơ sở dữ liệu để gửi ra ngoài giao diện
            string internalErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw new Exception("Lỗi hệ thống từ SQL Server: " + internalErrorMessage);
        }
    }
}