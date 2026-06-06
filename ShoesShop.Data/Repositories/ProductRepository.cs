using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ShoeStoreDbContext _context;
    public ProductRepository(ShoeStoreDbContext context) => _context = context;

    public async Task<(List<Product> Products, int TotalCount)> GetPaginatedAsync(
        string? search, int? categoryId, int? brandId, bool? isActive, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages.Where(i => i.IsPrimary))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.ProductName.ToLower().Contains(s) ||
                p.Slug.ToLower().Contains(s));
        }
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (brandId.HasValue)    query = query.Where(p => p.BrandId == brandId);
        if (isActive.HasValue)   query = query.Where(p => p.IsActive == isActive);

        var total = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, total);
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.ProductId == id);

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
        => await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Size)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(p => p.ProductId == id);

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        => await _context.Products.AnyAsync(p =>
            p.Slug == slug && (excludeId == null || p.ProductId != excludeId));

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductImage?> GetImageByIdAsync(int imageId)
        => await _context.ProductImages.FindAsync(imageId);

    public async Task AddImageAsync(ProductImage image)
    {
        await _context.ProductImages.AddAsync(image);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteImageAsync(ProductImage image)
    {
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
    }

    public async Task ClearPrimaryImageAsync(int productId)
    {
        await _context.ProductImages
            .Where(i => i.ProductId == productId && i.IsPrimary)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsPrimary, false));
    }
}
