namespace ShoesShop.Shared.DTOs.Product;

public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Gender { get; set; }
    public string? Material { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public int VariantId { get; set; }
    public int SizeId { get; set; }
    public string SizeValue { get; set; } = null!;
    public int ColorId { get; set; }
    public string ColorName { get; set; } = null!;
    public string? HexCode { get; set; }
    public string SKU { get; set; } = null!;
    public decimal? Price { get; set; }
    public int StockQuantity { get; set; }
}
