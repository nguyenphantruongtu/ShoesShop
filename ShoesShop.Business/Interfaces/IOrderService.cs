using ShoesShop.Shared.DTOs.Order;

namespace ShoesShop.Business.Interfaces;

public interface IOrderService
{
    /// <summary>UC-17: Tạo đơn hàng từ checkout, trả về orderId</summary>
    Task<int> CreateOrderAsync(CreateOrderRequest request, int userId);

    /// <summary>UC-31: Danh sách đơn hàng cho Staff (filter status, search mã đơn/SĐT)</summary>
    Task<OrderListResponse> GetOrdersAsync(string? search, string? status, int page, int pageSize);

    /// <summary>UC-32: Chi tiết đơn hàng</summary>
    Task<OrderDetailResponse> GetOrderDetailAsync(int orderId);

    /// <summary>UC-32: Xác nhận đơn (Pending → Confirmed)</summary>
    Task<OrderDetailResponse> ConfirmOrderAsync(int orderId, int staffId);

    /// <summary>UC-33 + UC-34: Cập nhật trạng thái (Confirmed→Preparing→Shipping→Delivered).
    /// Khi NewStatus = "Shipping" bắt buộc phải có CarrierName + TrackingNumber</summary>
    Task<OrderDetailResponse> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request, int staffId);

    /// <summary>UC-35: Hủy đơn (với lý do) + rollback stock về variant</summary>
    Task<OrderDetailResponse> CancelOrderAsync(int orderId, CancelOrderRequest request, int staffId);

    /// <summary>UC-19: Lịch sử đơn hàng của customer</summary>
    Task<OrderListResponse> GetMyOrdersAsync(int userId, string? status, int page, int pageSize);

    /// <summary>UC-20 + UC-22: Chi tiết đơn hàng của customer (kèm timeline + tracking)</summary>
    Task<OrderDetailResponse> GetMyOrderDetailAsync(int orderId, int userId);
}