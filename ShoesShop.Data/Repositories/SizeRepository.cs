using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class SizeRepository : ISizeRepository
{
    private readonly ShoeStoreDbContext _context;
    public SizeRepository(ShoeStoreDbContext context) => _context = context;

    public async Task<List<Size>> GetAllAsync()
        => await _context.Sizes.OrderBy(s => s.DisplayOrder).ThenBy(s => s.SizeValue).ToListAsync();

    public async Task<Size?> GetByIdAsync(int id)
        => await _context.Sizes.FindAsync(id);

    public async Task<bool> ValueExistsAsync(string value, int? excludeId = null)
        => await _context.Sizes.AnyAsync(s =>
            s.SizeValue == value && (excludeId == null || s.SizeId != excludeId));

    public async Task AddAsync(Size size)
    {
        await _context.Sizes.AddAsync(size);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Size size)
    {
        _context.Sizes.Update(size);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Size size)
    {
        _context.Sizes.Remove(size);
        await _context.SaveChangesAsync();
    }
}
