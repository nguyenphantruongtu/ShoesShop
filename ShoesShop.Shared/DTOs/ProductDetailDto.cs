namespace ShoesShop.Shared.DTOs
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Slug { get; set; }
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