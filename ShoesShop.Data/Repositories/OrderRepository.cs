using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ShoeStoreDbContext _ctx;
    private IDbContextTransaction? _currentTransaction;

    public OrderRepository(ShoeStoreDbContext ctx) => _ctx = ctx;

    // ── Generic Add + SaveChanges (dùng cho CreateOrderAsync) ───────────────

    public async Task AddAsync<T>(T entity) where T : class
    {
        await _ctx.Set<T>().AddAsync(entity);
    }

    public async Task<int> SaveChangesAsync()
        => await _ctx.SaveChangesAsync();

    // ── Transaction management ───────────────────────────────────────────────

    public async Task BeginTransactionAsync()
        => _currentTransaction = await _ctx.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // ── UC-31: Danh sách đơn hàng (Staff) ───────────────────────────────────

    public async Task<(List<Order> Orders, int TotalCount)> GetPaginatedAsync(
        string? search, string? status, int page, int pageSize)
    {
        var query = _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.HandledByStaff)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.ProductImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.OrderStatus == status);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderCode.ToLower().Contains(kw) ||
                o.RecipientPhone.Contains(kw));
        }

        var total = await query.CountAsync();

        // Khi xem "Tất cả" (không lọc status): ưu tiên đơn Chờ xác nhận lên đầu để
        // staff xử lý trước, các đơn còn lại xếp theo ngày tạo mới nhất.
        var isViewingAll = string.IsNullOrWhiteSpace(status);
        var orderedQuery = isViewingAll
            ? query.OrderBy(o => o.OrderStatus == "Pending" ? 0 : 1)
                   .ThenByDescending(o => o.CreatedAt)
            : query.OrderByDescending(o => o.CreatedAt);

        var orders = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, total);
    }

    // ── UC-32: Chi tiết đơn hàng (full includes) ────────────────────────────

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        => await _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.HandledByStaff)
            .Include(o => o.Voucher)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.ProductImages)
            .Include(o => o.OrderStatusHistories.OrderBy(h => h.ChangedAt))
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task<Order?> GetByIdAsync(int orderId)
        => await _ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task UpdateAsync(Order order)
    {
        _ctx.Orders.Update(order);
        await _ctx.SaveChangesAsync();
    }

    // ── Status history ───────────────────────────────────────────────────────

    public async Task AddStatusHistoryAsync(OrderStatusHistory history)
    {
        await _ctx.OrderStatusHistories.AddAsync(history);
        await _ctx.SaveChangesAsync();
    }

    // ── Payment (ghi nhận phương thức + đánh dấu Paid) ───────────────────────

    public async Task<PaymentMethod?> GetPaymentMethodByNameAsync(string methodName)
        => await _ctx.PaymentMethods.FirstOrDefaultAsync(m => m.MethodName == methodName && m.IsActive);

    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        => await _ctx.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task UpdatePaymentAsync(Payment payment)
    {
        _ctx.Payments.Update(payment);
        await _ctx.SaveChangesAsync();
    }

    // ── Variant (dùng cho cancel rollback + create stock deduct) ────────────

    public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        => await _ctx.ProductVariants
            .Include(v => v.Product)
            .Include(v => v.Size)
            .Include(v => v.Color)
            .FirstOrDefaultAsync(v => v.VariantId == variantId);

    public async Task UpdateVariantAsync(ProductVariant variant)
    {
        _ctx.ProductVariants.Update(variant);
        await _ctx.SaveChangesAsync();
    }

    // ── UC-19: Lịch sử đơn hàng của Customer ────────────────────────────────

    public async Task<(List<Order> Orders, int TotalCount)> GetByUserIdAsync(
        int userId, string? status, int page, int pageSize)
    {
        var query = _ctx.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.ProductImages)
            .Where(o => o.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.OrderStatus == status);

        var total  = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, total);
    }

    // ── UC-20: Chi tiết đơn của Customer (ownership check) ──────────────────

    public async Task<Order?> GetByIdAndUserIdAsync(int orderId, int userId)
        => await _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.Voucher)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.ProductImages)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Reviews)
            .Include(o => o.OrderStatusHistories.OrderBy(h => h.ChangedAt))
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
}