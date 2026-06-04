using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int VariantId { get; set; }

    public string ProductName { get; set; } = null!;

    public string SizeValue { get; set; } = null!;

    public string ColorName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ProductVariant Variant { get; set; } = null!;
}
