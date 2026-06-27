using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly ShoeStoreDbContext _context;
    public BrandRepository(ShoeStoreDbContext context) => _context = context;

    public async Task<List<Brand>> GetAllAsync()
        => await _context.Brands.OrderBy(b => b.BrandName).ToListAsync();

    public async Task<Brand?> GetByIdAsync(int id)
        => await _context.Brands.FindAsync(id);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        => await _context.Brands.AnyAsync(b =>
            b.BrandName == name && (excludeId == null || b.BrandId != excludeId));

    public async Task<bool> HasProductsAsync(int id)
        => await _context.Products.AnyAsync(p => p.BrandId == id);

    public async Task AddAsync(Brand brand)
    {
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Brand brand)
    {
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
    }
}
