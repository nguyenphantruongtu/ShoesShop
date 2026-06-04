using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int? OrderItemId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? ImageUrls { get; set; }

    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
