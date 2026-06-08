using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace ShoesShop.Data.Repositories;

public class OrderRepository : IOrderRepository
{
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
    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        // Đảm bảo Include đúng thực thể OrderItems để lấy danh sách sản phẩm cần hoàn kho
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }
    // 🌟 1. Hiện thực hàm GetByIdAsync kèm theo nạp sẵn danh sách sản phẩm (Include OrderItems)
    public async Task<Order?> GetByIdAsync(int orderId)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems) // Include để khi hủy đơn lấy được danh sách sản phẩm hoàn kho
                                        // .Include("OrderItems.ProductVariant") // Nếu có quan hệ để hoàn kho thì bỏ comment dòng này
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    // 🌟 2. Hiện thực hàm UpdateAsync để lưu trạng thái "Canceled" (Đã hủy)
    public async Task UpdateAsync(Order order)
    {
        _context.Set<Order>().Update(order);
        await Task.CompletedTask; // Hàm đồng bộ của EF Core, không cần gạch đỏ await
    }

    // 🌟 3. Hiện thực hàm GetOrdersByUserIdAsync lấy lịch sử đơn hàng của khách
    public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
    {
        return await _context.Set<Order>()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt) // Đơn hàng mới mua xếp lên hàng đầu
            .ToListAsync();
    }
}