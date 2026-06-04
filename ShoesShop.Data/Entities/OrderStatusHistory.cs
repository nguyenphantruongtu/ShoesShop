using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class OrderStatusHistory
{
    public int HistoryId { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public int? ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual User? ChangedByUser { get; set; }

    public virtual Order Order { get; set; } = null!;
}
