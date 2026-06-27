namespace ShoesShop.Shared.DTOs
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Slug { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string? Gender { get; set; }
        public string? Material { get; set; }

        // Danh sách hình ảnh của sản phẩm
        public List<string> ImageUrls { get; set; } = new();

        // Danh sách màu sắc khả dụng cho sản phẩm này
        public List<ColorDto> Colors { get; set; } = new();

        // Danh sách kích cỡ khả dụng cho sản phẩm này
        public List<SizeDto> Sizes { get; set; } = new();

        // Danh sách biến thể đầy đủ (để check tồn kho + add-to-cart)
        public List<VariantStockDto> Variants { get; set; } = new();
    }

    public class VariantStockDto
    {
        public int VariantId { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    public class ColorDto
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; } = null!;
        public string? HexCode { get; set; }
    }

    public class SizeDto
    {
        public int SizeId { get; set; }
        public string SizeName { get; set; } = null!;
    }
}