using System.ComponentModel.DataAnnotations;

namespace ShoesShop.Shared.DTOs.Catalog;

// --- PRODUCT ---
public class CreateProductRequest
{
    [Required][MaxLength(200)] public string ProductName { get; set; } = null!;
    [MaxLength(220)] public string? Slug { get; set; }           // auto-gen if empty
    [Required] public int CategoryId { get; set; }
    [Required] public int BrandId { get; set; }
    public string? Description { get; set; }
    [MaxLength(500)] public string? ShortDescription { get; set; }
    [Required][Range(0, 999_999_999)] public decimal BasePrice { get; set; }
    [Range(0, 999_999_999)] public decimal? SalePrice { get; set; }
    [MaxLength(10)] public string? Gender { get; set; }
    [MaxLength(100)] public string? Material { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;

    /// <summary>Danh sách ảnh — ít nhất 1 ảnh phải là primary</summary>
    public List<ProductImageInput> Images { get; set; } = new();
}

public class UpdateProductRequest
{
    [Required][MaxLength(200)] public string ProductName { get; set; } = null!;
    [MaxLength(220)] public string? Slug { get; set; }
    [Required] public int CategoryId { get; set; }
    [Required] public int BrandId { get; set; }
    public string? Description { get; set; }
    [MaxLength(500)] public string? ShortDescription { get; set; }
    [Required][Range(0, 999_999_999)] public decimal BasePrice { get; set; }
    [Range(0, 999_999_999)] public decimal? SalePrice { get; set; }
    [MaxLength(10)] public string? Gender { get; set; }
    [MaxLength(100)] public string? Material { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
}

public class ProductImageInput
{
    [Required][MaxLength(500)] public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}

public class AddProductImageRequest
{
    [Required][MaxLength(500)] public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}

public class ProductImageResponse
{
    public int ImageId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ProductListItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductListResponse
{
    public List<ProductListItem> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ProductDetailResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
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
    public List<ProductImageResponse> Images { get; set; } = new();
    public List<VariantResponse> Variants { get; set; } = new();
}

// --- VARIANT ---
public class CreateVariantRequest
{
    [Required] public int SizeId { get; set; }
    [Required] public int ColorId { get; set; }
    [MaxLength(100)] public string? Sku { get; set; }       // auto-gen if empty
    [Range(0, 999_999_999)] public decimal? Price { get; set; }
    [Required][Range(0, int.MaxValue)] public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateVariantRequest
{
    [MaxLength(100)] public string? Sku { get; set; }
    [Range(0, 999_999_999)] public decimal? Price { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateStockRequest
{
    [Required][Range(0, int.MaxValue)] public int StockQuantity { get; set; }
}

public class VariantResponse
{
    public int VariantId { get; set; }
    public int ProductId { get; set; }
    public int SizeId { get; set; }
    public string SizeValue { get; set; } = null!;
    public int ColorId { get; set; }
    public string ColorName { get; set; } = null!;
    public string? HexCode { get; set; }
    public string Sku { get; set; } = null!;
    public decimal? Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}
