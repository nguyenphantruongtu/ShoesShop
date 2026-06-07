using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Context;
using ShoesShop.Data.Interfaces;

namespace ShoesShop.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ShoeStoreDbContext _context;

    public ProductRepository(ShoeStoreDbContext context)
    {
        _context = context;
    }

    public IQueryable<Product> GetQueryable()
    {
        return _context.Products.AsQueryable();
    }

    // UC-01: Lấy sản phẩm nổi bật kèm hình ảnh
    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
    {
        return await _context.Products
            .Include(p => p.ProductImages)
            .Where(p => p.IsActive && p.IsFeatured)
            .ToListAsync();
    }

    // UC-03: Tìm kiếm theo keyword gần đúng
    public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Enumerable.Empty<Product>();

        return await _context.Products
            .Include(p => p.ProductImages)
            .Where(p => p.IsActive && p.ProductName.Contains(keyword))
            .ToListAsync();
    }

    // UC-04: Lấy chi tiết sản phẩm kèm slide ảnh, biến thể size, màu và số lượng tồn kho
    public async Task<Product?> GetProductDetailAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.ProductImages)
            .Include(p => p.ProductVariants).ThenInclude(v => v.Size)
            .Include(p => p.ProductVariants).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);
    }
}