using System;
using System.Collections.Generic;

namespace ShoesShop.Data.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int CategoryId { get; set; }

    public int BrandId { get; set; }

    public string? Description { get; set; }

    public string? ShortDescription { get; set; }

    public decimal BasePrice { get; set; }

    public decimal? SalePrice { get; set; }

    public string? Gender { get; set; }

    public string? Material { get; set; }

    public bool IsActive { get; set; }

    public bool IsFeatured { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
