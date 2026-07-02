using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface IVoucherRepository
{
    Task<List<Voucher>> GetAllAsync();
    Task<Voucher?> GetByIdAsync(int voucherId);
    Task<Voucher?> GetByCodeAsync(string code);
    Task<bool> CodeExistsAsync(string code);
    Task AddAsync(Voucher voucher);
    Task UpdateAsync(Voucher voucher);
    Task DeleteAsync(Voucher voucher);

    /// <summary>Số đơn hàng (không tính đơn đã hủy) mà user đã dùng voucher này — dùng để áp UsageLimitPerUser</summary>
    Task<int> CountUserUsageAsync(int userId, int voucherId);
}
