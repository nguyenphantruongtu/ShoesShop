using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly ShoeStoreDbContext _context;

    public AddressRepository(ShoeStoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Address>> GetByUserIdAsync(int userId)
        => await _context.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<Address?> GetByIdAsync(int addressId)
        => await _context.Addresses.FindAsync(addressId);

    public async Task<Address?> GetByIdAndUserIdAsync(int addressId, int userId)
        => await _context.Addresses
            .FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Address address)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
    }

    /// <summary>Bỏ default tất cả address của user trước khi set default mới</summary>
    public async Task ClearDefaultAsync(int userId)
    {
        await _context.Addresses
            .Where(a => a.UserId == userId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
    }
}
