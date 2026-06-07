using Microsoft.EntityFrameworkCore.Storage;
using ShoesShop.Data.Context;
using ShoesShop.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShoesShop.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    // LƯU Ý: Thay 'AppDbContext' bằng tên DbContext thật của nhóm bạn (ví dụ: ShoesShopDbContext)
    private readonly ShoeStoreDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public OrderRepository(ShoeStoreDbContext context)
    {
        _context = context;
    }

    // Thực hiện thêm mới một đối tượng (Order hoặc OrderDetail) vào DB Context
    public async Task AddAsync<T>(T entity) where T : class
    {
        await _context.Set<T>().AddAsync(entity);
    }

    // Lưu tất cả các thay đổi từ RAM xuống Database thực tế
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // Kích hoạt Transaction (mở một phiên làm việc an toàn)
    public async Task BeginTransactionAsync()
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    // Chốt dữ liệu, lưu chính thức khi toàn bộ các bước thành công
    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // Hủy bỏ toàn bộ các bước đã làm trước đó nếu giữa chừng xảy ra lỗi
    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}