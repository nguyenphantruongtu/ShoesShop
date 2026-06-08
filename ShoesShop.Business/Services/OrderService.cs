using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;

    // ── UC-31: List ─────────────────────────────────────────────────────────
    public async Task<OrderListResponse> GetOrdersAsync(
        string? search, string? status, int page, int pageSize)
    {
        var (orders, total) = await _repo.GetPaginatedAsync(search, status, page, pageSize);

        return new OrderListResponse
        {
            Orders = orders.Select(o => new OrderListItem
            {
                OrderId       = o.OrderId,
                OrderCode     = o.OrderCode,
                CustomerName  = o.RecipientName,
                CustomerPhone = o.RecipientPhone,
                TotalAmount   = o.TotalAmount,
                OrderStatus   = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                ItemCount     = o.OrderItems.Count,
                CreatedAt     = o.CreatedAt,
                HandledByStaff = o.HandledByStaff?.FullName
            }).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    // ── UC-32: Detail ───────────────────────────────────────────────────────
    public async Task<OrderDetailResponse> GetOrderDetailAsync(int orderId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");
        return MapToDetail(order);
    }

    // ── UC-32: Confirm (Pending → Confirmed) ────────────────────────────────
    public async Task<OrderDetailResponse> ConfirmOrderAsync(int orderId, int staffId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (order.OrderStatus != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Chỉ có thể xác nhận đơn ở trạng thái Pending. Hiện tại: {order.OrderStatus}.");

        order.OrderStatus    = OrderStatus.Confirmed;
        order.HandledByStaffId = staffId;
        order.UpdatedAt      = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        await _repo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId         = orderId,
            Status          = OrderStatus.Confirmed,
            Note            = "Staff xác nhận đơn hàng.",
            ChangedByUserId = staffId,
            ChangedAt       = DateTime.UtcNow
        });

        return MapToDetail(order);
    }

    // ── UC-33 + UC-34: Update status ────────────────────────────────────────
    public async Task<OrderDetailResponse> UpdateStatusAsync(
        int orderId, UpdateOrderStatusRequest request, int staffId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        var newStatus = request.NewStatus;

        // Validate transition
        if (!OrderStatus.IsValidTransition(order.OrderStatus, newStatus))
            throw new InvalidOperationException(
                $"Không thể chuyển từ '{order.OrderStatus}' sang '{newStatus}'.");

        // UC-34: Khi chuyển sang Shipping bắt buộc có CarrierName + TrackingNumber
        if (newStatus == OrderStatus.Shipping)
        {
            if (string.IsNullOrWhiteSpace(request.CarrierName))
                throw new ArgumentException("CarrierName là bắt buộc khi chuyển sang Shipping.");
            if (string.IsNullOrWhiteSpace(request.TrackingNumber))
                throw new ArgumentException("TrackingNumber là bắt buộc khi chuyển sang Shipping.");

            await HandleShipmentAsync(order, request);
        }

        // Cập nhật Delivered → update Shipment.DeliveredAt
        if (newStatus == OrderStatus.Delivered)
            await MarkShipmentDeliveredAsync(orderId);

        order.OrderStatus    = newStatus;
        order.HandledByStaffId = staffId;
        order.UpdatedAt      = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        await _repo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId         = orderId,
            Status          = newStatus,
            Note            = request.Note ?? $"Chuyển trạng thái sang {newStatus}.",
            ChangedByUserId = staffId,
            ChangedAt       = DateTime.UtcNow
        });

        // Reload để lấy Shipment mới nhất
        var updated = await _repo.GetByIdWithDetailsAsync(orderId)!;
        return MapToDetail(updated!);
    }

    // ── UC-35: Cancel + rollback stock ─────────────────────────────────────
    public async Task<OrderDetailResponse> CancelOrderAsync(
        int orderId, CancelOrderRequest request, int staffId)
    {
        var order = await _repo.GetByIdWithDetailsAsync(orderId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");

        if (!OrderStatus.IsCancellable(order.OrderStatus))
            throw new InvalidOperationException(
                $"Không thể hủy đơn ở trạng thái '{order.OrderStatus}'.");

        // Rollback stock cho mỗi order item
        foreach (var item in order.OrderItems)
        {
            var variant = await _repo.GetVariantByIdAsync(item.VariantId);
            if (variant != null)
            {
                variant.StockQuantity += item.Quantity;
                await _repo.UpdateVariantAsync(variant);
            }
        }

        order.OrderStatus    = OrderStatus.Cancelled;
        order.CancelReason   = request.CancelReason;
        order.CancelledAt    = DateTime.UtcNow;
        order.HandledByStaffId = staffId;
        order.UpdatedAt      = DateTime.UtcNow;

        await _repo.UpdateAsync(order);
        await _repo.AddStatusHistoryAsync(new OrderStatusHistory
        {
            OrderId         = orderId,
            Status          = OrderStatus.Cancelled,
            Note            = $"Hủy bởi Staff. Lý do: {request.CancelReason}",
            ChangedByUserId = staffId,
            ChangedAt       = DateTime.UtcNow
        });

        var updated = await _repo.GetByIdWithDetailsAsync(orderId)!;
        return MapToDetail(updated!);
    }

    // ── UC-19: Customer order history ──────────────────────────────────────
    public async Task<OrderListResponse> GetMyOrdersAsync(int userId, string? status, int page, int pageSize)
    {
        var (orders, total) = await _repo.GetByUserIdAsync(userId, status, page, pageSize);

        return new OrderListResponse
        {
            Orders = orders.Select(o => new OrderListItem
            {
                OrderId       = o.OrderId,
                OrderCode     = o.OrderCode,
                CustomerName  = o.RecipientName,
                CustomerPhone = o.RecipientPhone,
                TotalAmount   = o.TotalAmount,
                OrderStatus   = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                ItemCount     = o.OrderItems.Count,
                CreatedAt     = o.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }

    // ── UC-20 + UC-22: Customer order detail ────────────────────────────────
    public async Task<OrderDetailResponse> GetMyOrderDetailAsync(int orderId, int userId)
    {
        var order = await _repo.GetByIdAndUserIdAsync(orderId, userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{orderId}.");
        return MapToDetail(order);
    }

    // ── HELPERS ─────────────────────────────────────────────────────────────

    private async Task HandleShipmentAsync(Order order, UpdateOrderStatusRequest req)
    {
        var shipment = await _repo.GetShipmentByOrderIdAsync(order.OrderId);

        if (shipment == null)
        {
            await _repo.AddShipmentAsync(new Shipment
            {
                OrderId              = order.OrderId,
                CarrierName          = req.CarrierName!,
                TrackingNumber       = req.TrackingNumber,
                ShippingStatus       = "Shipping",
                ShippedAt            = DateTime.UtcNow,
                EstimatedDeliveryDate = req.EstimatedDeliveryDate,
                Note                 = req.Note
            });
        }
        else
        {
            shipment.CarrierName          = req.CarrierName!;
            shipment.TrackingNumber       = req.TrackingNumber;
            shipment.ShippingStatus       = "Shipping";
            shipment.ShippedAt            = DateTime.UtcNow;
            shipment.EstimatedDeliveryDate = req.EstimatedDeliveryDate;
            shipment.Note                 = req.Note;
            await _repo.UpdateShipmentAsync(shipment);
        }
    }

    private async Task MarkShipmentDeliveredAsync(int orderId)
    {
        var shipment = await _repo.GetShipmentByOrderIdAsync(orderId);
        if (shipment != null)
        {
            shipment.ShippingStatus = "Delivered";
            shipment.DeliveredAt    = DateTime.UtcNow;
            await _repo.UpdateShipmentAsync(shipment);
        }
    }

    private static OrderDetailResponse MapToDetail(Order o) => new()
    {
        OrderId        = o.OrderId,
        OrderCode      = o.OrderCode,
        CustomerId     = o.UserId,
        CustomerName   = o.User.FullName,
        CustomerEmail  = o.User.Email,
        RecipientName  = o.RecipientName,
        RecipientPhone = o.RecipientPhone,
        ShippingAddress = o.ShippingAddress,
        Province       = o.Province,
        District       = o.District,
        Ward           = o.Ward,
        SubTotal       = o.SubTotal,
        ShippingFee    = o.ShippingFee,
        DiscountAmount = o.DiscountAmount,
        TotalAmount    = o.TotalAmount,
        OrderStatus    = o.OrderStatus,
        PaymentStatus  = o.PaymentStatus,
        Note           = o.Note,
        CancelReason   = o.CancelReason,
        CancelledAt    = o.CancelledAt,
        HandledByStaff = o.HandledByStaff?.FullName,
        CreatedAt      = o.CreatedAt,
        UpdatedAt      = o.UpdatedAt,

        Items = o.OrderItems.Select(i => new OrderItemResponse
        {
            OrderItemId = i.OrderItemId,
            VariantId   = i.VariantId,
            ProductName = i.ProductName,
            SizeValue   = i.SizeValue,
            ColorName   = i.ColorName,
            Sku         = i.Variant?.Sku,
            UnitPrice   = i.UnitPrice,
            Quantity    = i.Quantity,
            LineTotal   = i.LineTotal
        }).ToList(),

        Shipment = o.Shipment == null ? null : new ShipmentResponse
        {
            ShipmentId           = o.Shipment.ShipmentId,
            CarrierName          = o.Shipment.CarrierName,
            TrackingNumber       = o.Shipment.TrackingNumber,
            ShippingStatus       = o.Shipment.ShippingStatus,
            ShippedAt            = o.Shipment.ShippedAt,
            EstimatedDeliveryDate = o.Shipment.EstimatedDeliveryDate,
            DeliveredAt          = o.Shipment.DeliveredAt,
            Note                 = o.Shipment.Note
        },

        StatusHistory = o.OrderStatusHistories.Select(h => new OrderStatusHistoryResponse
        {
            HistoryId  = h.HistoryId,
            Status     = h.Status,
            Note       = h.Note,
            ChangedBy  = h.ChangedByUser?.FullName,
            ChangedAt  = h.ChangedAt
        }).ToList()
    };
}
