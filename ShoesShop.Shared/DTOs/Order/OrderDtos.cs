using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Order;

// ── STATUS CONSTANTS ───────────────────────────────────────────────────────────
public static class OrderStatus
{
    public const string Pending   = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Preparing = "Preparing";
    public const string Shipping  = "Shipping";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    /// <summary>Kiểm tra chuyển trạng thái hợp lệ (không tính Shipping — cần kiểm tra riêng)</summary>
    public static bool IsValidTransition(string current, string next) => (current, next) switch
    {
        (Pending,   Confirmed) => true,
        (Confirmed, Preparing) => true,
        (Preparing, Shipping)  => true,
        (Shipping,  Delivered) => true,
        _ => false
    };

    /// <summary>Trạng thái có thể bị hủy không</summary>
    public static bool IsCancellable(string status) =>
        status is Pending or Confirmed or Preparing;
}

// ── RESPONSE DTOs ──────────────────────────────────────────────────────────────

public class OrderListItem
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? HandledByStaff { get; set; }
}

public class OrderListResponse
{
    public List<OrderListItem> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class OrderItemResponse
{
    public int OrderItemId { get; set; }
    public int VariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public string SizeValue { get; set; } = null!;
    public string ColorName { get; set; } = null!;
    public string? Sku { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class ShipmentResponse
{
    public int ShipmentId { get; set; }
    public string CarrierName { get; set; } = null!;
    public string? TrackingNumber { get; set; }
    public string ShippingStatus { get; set; } = null!;
    public DateTime? ShippedAt { get; set; }
    public DateOnly? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Note { get; set; }
}

public class OrderStatusHistoryResponse
{
    public int HistoryId { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class OrderDetailResponse
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = null!;

    // Customer
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerEmail { get; set; } = null!;

    // Shipping
    public string RecipientName { get; set; } = null!;
    public string RecipientPhone { get; set; } = null!;
    public string ShippingAddress { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Status
    public string OrderStatus { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? HandledByStaff { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItemResponse> Items { get; set; } = new();
    public ShipmentResponse? Shipment { get; set; }
    public List<OrderStatusHistoryResponse> StatusHistory { get; set; } = new();
}

// ── REQUEST DTOs ───────────────────────────────────────────────────────────────

/// <summary>UC-33: Cập nhật trạng thái đơn hàng (Confirmed→Preparing→Shipping→Delivered)</summary>
public class UpdateOrderStatusRequest
{
    [Required]
    public string NewStatus { get; set; } = null!;

    public string? Note { get; set; }

    // UC-34: Bắt buộc khi NewStatus = "Shipping"
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public DateOnly? EstimatedDeliveryDate { get; set; }
}

/// <summary>UC-35: Hủy đơn + lý do</summary>
public class CancelOrderRequest
{
    [Required]
    [MaxLength(500)]
    public string CancelReason { get; set; } = null!;
}
