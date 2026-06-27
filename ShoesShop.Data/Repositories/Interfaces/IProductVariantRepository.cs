using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IProductVariantRepository
{
    Task<List<ProductVariant>> GetByProductIdAsync(int productId);
    Task<ProductVariant?> GetByIdAsync(int variantId);
    Task<ProductVariant?> GetByIdAndProductIdAsync(int variantId, int productId);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    Task<bool> VariantComboExistsAsync(int productId, int sizeId, int colorId, int? excludeId = null);
    Task AddAsync(ProductVariant variant);
    Task UpdateAsync(ProductVariant variant);
    Task DeleteAsync(ProductVariant variant);
}
