using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public string OrderCode { get; set; } = null!;

    public int UserId { get; set; }

    public string RecipientName { get; set; } = null!;

    public string RecipientPhone { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Ward { get; set; } = null!;

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public int? VoucherId { get; set; }

    public string? Note { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public int? HandledByStaffId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? CancelReason { get; set; }

    public virtual User? HandledByStaff { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
