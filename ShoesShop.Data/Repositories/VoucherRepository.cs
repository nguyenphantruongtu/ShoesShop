using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class VoucherRepository : IVoucherRepository
{
    private readonly ShoeStoreDbContext _ctx;
    public VoucherRepository(ShoeStoreDbContext ctx) => _ctx = ctx;

    public async Task<List<Voucher>> GetAllAsync()
        => await _ctx.Vouchers.OrderByDescending(v => v.CreatedAt).ToListAsync();

    public async Task<Voucher?> GetByIdAsync(int voucherId)
        => await _ctx.Vouchers.FindAsync(voucherId);

    public async Task<Voucher?> GetByCodeAsync(string code)
        => await _ctx.Vouchers.FirstOrDefaultAsync(v => v.Code == code.ToUpper());

    public async Task<bool> CodeExistsAsync(string code)
        => await _ctx.Vouchers.AnyAsync(v => v.Code == code.ToUpper());

    public async Task AddAsync(Voucher voucher)
    {
        await _ctx.Vouchers.AddAsync(voucher);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(Voucher voucher)
    {
        _ctx.Vouchers.Update(voucher);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Voucher voucher)
    {
        _ctx.Vouchers.Remove(voucher);
        await _ctx.SaveChangesAsync();
    }

    public async Task<int> CountUserUsageAsync(int userId, int voucherId)
        => await _ctx.Orders.CountAsync(o =>
            o.UserId == userId && o.VoucherId == voucherId && o.OrderStatus != "Cancelled");
}
