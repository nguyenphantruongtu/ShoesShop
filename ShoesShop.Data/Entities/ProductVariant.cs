using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class ProductVariant
{
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public int ColorId { get; set; }

    public string Sku { get; set; } = null!;

    public decimal? Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Color Color { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual Size Size { get; set; } = null!;
}
