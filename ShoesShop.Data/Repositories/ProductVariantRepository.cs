using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly ShoeStoreDbContext _context;
    public ProductVariantRepository(ShoeStoreDbContext context) => _context = context;

    public async Task<List<ProductVariant>> GetByProductIdAsync(int productId)
        => await _context.ProductVariants
            .Include(v => v.Size)
            .Include(v => v.Color)
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.Size.DisplayOrder)
            .ThenBy(v => v.Color.ColorName)
            .ToListAsync();

    public async Task<ProductVariant?> GetByIdAsync(int variantId)
        => await _context.ProductVariants
            .Include(v => v.Size)
            .Include(v => v.Color)
            .FirstOrDefaultAsync(v => v.VariantId == variantId);

    public async Task<ProductVariant?> GetByIdAndProductIdAsync(int variantId, int productId)
        => await _context.ProductVariants
            .Include(v => v.Size)
            .Include(v => v.Color)
            .FirstOrDefaultAsync(v => v.VariantId == variantId && v.ProductId == productId);

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
        => await _context.ProductVariants.AnyAsync(v =>
            v.Sku == sku && (excludeId == null || v.VariantId != excludeId));

    public async Task<bool> VariantComboExistsAsync(int productId, int sizeId, int colorId, int? excludeId = null)
        => await _context.ProductVariants.AnyAsync(v =>
            v.ProductId == productId && v.SizeId == sizeId && v.ColorId == colorId
            && (excludeId == null || v.VariantId != excludeId));

    public async Task AddAsync(ProductVariant variant)
    {
        await _context.ProductVariants.AddAsync(variant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductVariant variant)
    {
        _context.ProductVariants.Update(variant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProductVariant variant)
    {
        _context.ProductVariants.Remove(variant);
        await _context.SaveChangesAsync();
    }
}
