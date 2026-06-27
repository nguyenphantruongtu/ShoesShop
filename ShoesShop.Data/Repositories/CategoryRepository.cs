using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ShoeStoreDbContext _context;
    public CategoryRepository(ShoeStoreDbContext context) => _context = context;

    public async Task<List<Category>> GetAllAsync()
        => await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .OrderBy(c => c.ParentCategoryId)
            .ThenBy(c => c.CategoryName)
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id)
        => await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

    public async Task<Category?> GetByIdWithChildrenAsync(int id)
        => await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        => await _context.Categories.AnyAsync(c =>
            c.Slug == slug && (excludeId == null || c.CategoryId != excludeId));

    public async Task<bool> HasChildrenAsync(int id)
        => await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);

    public async Task<bool> HasProductsAsync(int id)
        => await _context.Products.AnyAsync(p => p.CategoryId == id);

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
