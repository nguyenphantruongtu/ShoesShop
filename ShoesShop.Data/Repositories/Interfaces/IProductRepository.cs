using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IProductRepository
{
    // F4 Browse: OData support
    IQueryable<Product> GetQueryable();
    Task<IEnumerable<Product>> GetFeaturedProductsAsync();
    Task<IEnumerable<Product>> SearchProductsAsync(string keyword);

    Task<(List<Product> Products, int TotalCount)> GetPaginatedAsync(
        string? search, int? categoryId, int? brandId, bool? isActive, int page, int pageSize);
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithDetailsAsync(int id);   // includes images, variants, category, brand
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);

    // Images
    Task<ProductImage?> GetImageByIdAsync(int imageId);
    Task AddImageAsync(ProductImage image);
    Task DeleteImageAsync(ProductImage image);
    Task ClearPrimaryImageAsync(int productId);
}
