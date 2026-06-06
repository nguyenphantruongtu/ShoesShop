using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ShoeStoreDbContext _ctx;
    public OrderRepository(ShoeStoreDbContext ctx) => _ctx = ctx;

    public async Task<(List<Order> Orders, int TotalCount)> GetPaginatedAsync(
        string? search, string? status, int page, int pageSize)
    {
        var query = _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.HandledByStaff)
            .Include(o => o.OrderItems)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.OrderStatus == status);

        // Search by OrderCode or RecipientPhone
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderCode.ToLower().Contains(kw) ||
                o.RecipientPhone.Contains(kw));
        }

        var total = await query.CountAsync();

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, total);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        => await _ctx.Orders
            .Include(o => o.User)
            .Include(o => o.HandledByStaff)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
            .Include(o => o.Shipment)
            .Include(o => o.OrderStatusHistories.OrderBy(h => h.ChangedAt))
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task UpdateAsync(Order order)
    {
        _ctx.Orders.Update(order);
        await _ctx.SaveChangesAsync();
    }

    public async Task AddStatusHistoryAsync(OrderStatusHistory history)
    {
        await _ctx.OrderStatusHistories.AddAsync(history);
        await _ctx.SaveChangesAsync();
    }

    public async Task<Shipment?> GetShipmentByOrderIdAsync(int orderId)
        => await _ctx.Shipments.FirstOrDefaultAsync(s => s.OrderId == orderId);

    public async Task AddShipmentAsync(Shipment shipment)
    {
        await _ctx.Shipments.AddAsync(shipment);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateShipmentAsync(Shipment shipment)
    {
        _ctx.Shipments.Update(shipment);
        await _ctx.SaveChangesAsync();
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        => await _ctx.ProductVariants.FindAsync(variantId);

    public async Task UpdateVariantAsync(ProductVariant variant)
    {
        _ctx.ProductVariants.Update(variant);
        await _ctx.SaveChangesAsync();
    }
}
