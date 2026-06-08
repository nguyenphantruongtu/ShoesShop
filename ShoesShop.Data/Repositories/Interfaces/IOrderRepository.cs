using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    /// <summary>Danh sách đơn hàng có filter (search = OrderCode hoặc SĐT khách)</summary>
    Task<(List<Order> Orders, int TotalCount)> GetPaginatedAsync(
        string? search, string? status, int page, int pageSize);

    /// <summary>Chi tiết đơn: kèm Items (Variant), Shipment, StatusHistory, User</summary>
    Task<Order?> GetByIdWithDetailsAsync(int orderId);

    Task UpdateAsync(Order order);
    Task AddStatusHistoryAsync(OrderStatusHistory history);

    Task<Shipment?> GetShipmentByOrderIdAsync(int orderId);
    Task AddShipmentAsync(Shipment shipment);
    Task UpdateShipmentAsync(Shipment shipment);

    /// <summary>Lấy variant để rollback stock khi hủy đơn</summary>
    Task<ProductVariant?> GetVariantByIdAsync(int variantId);
    Task UpdateVariantAsync(ProductVariant variant);

    // UC-19: Lịch sử đơn hàng của customer
    Task<(List<Order> Orders, int TotalCount)> GetByUserIdAsync(
        int userId, string? status, int page, int pageSize);

    // UC-20: Chi tiết đơn của customer (kiểm tra ownership)
    Task<Order?> GetByIdAndUserIdAsync(int orderId, int userId);
}
