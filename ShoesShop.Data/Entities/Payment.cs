using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public int PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionCode { get; set; }

    public string? GatewayResponse { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
