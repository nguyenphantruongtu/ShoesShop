// ShoesShop.Business/Interfaces/IProductService.cs
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Interfaces;

public interface IProductService
{
    // UC-01: Trang chủ – sản phẩm nổi bật
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();

    // UC-02: Danh sách sản phẩm (OData fallback / non-OData consumers)
    Task<ProductListResponse> GetListAsync(
        string? search,
        string? categorySlug,
        string? brandSlug,
        bool? isActive,
        int page,
        int pageSize);

    // UC-03: Tìm kiếm theo keyword
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);

    // UC-04: Chi tiết sản phẩm
    Task<ProductDetailDto?> GetProductDetailAsync(int productId);
}