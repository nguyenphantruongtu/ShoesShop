using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class ColorRepository : IColorRepository
{
    private readonly ShoeStoreDbContext _context;
    public ColorRepository(ShoeStoreDbContext context) => _context = context;

    public async Task<List<Color>> GetAllAsync()
        => await _context.Colors.OrderBy(c => c.ColorName).ToListAsync();

    public async Task<Color?> GetByIdAsync(int id)
        => await _context.Colors.FindAsync(id);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        => await _context.Colors.AnyAsync(c =>
            c.ColorName == name && (excludeId == null || c.ColorId != excludeId));

    public async Task AddAsync(Color color)
    {
        await _context.Colors.AddAsync(color);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Color color)
    {
        _context.Colors.Update(color);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Color color)
    {
        _context.Colors.Remove(color);
        await _context.SaveChangesAsync();
    }
}
