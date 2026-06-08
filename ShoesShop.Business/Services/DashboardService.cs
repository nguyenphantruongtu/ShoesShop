using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Context;
using ShoesShop.Shared.DTOs.Dashboard;
using ShoesShop.Shared.DTOs.Order;

namespace ShoesShop.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly ShoeStoreDbContext _ctx;
    public DashboardService(ShoeStoreDbContext ctx) => _ctx = ctx;

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var now      = DateTime.UtcNow;
        var today    = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Chỉ tính đơn Delivered hoặc đã thanh toán
        var paidOrders = _ctx.Orders
            .Where(o => o.PaymentStatus == "Paid");

        var revenueToday = await paidOrders
            .Where(o => o.CreatedAt >= today)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var revenueWeek = await paidOrders
            .Where(o => o.CreatedAt >= weekStart)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var revenueMonth = await paidOrders
            .Where(o => o.CreatedAt >= monthStart)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Order counts by status
        var statusCounts = await _ctx.Orders
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int Count(string s) => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        var orderStats = new OrderStats
        {
            TotalOrders = statusCounts.Sum(x => x.Count),
            Pending     = Count(OrderStatus.Pending),
            Confirmed   = Count(OrderStatus.Confirmed),
            Preparing   = Count(OrderStatus.Preparing),
            Shipping    = Count(OrderStatus.Shipping),
            Delivered   = Count(OrderStatus.Delivered),
            Cancelled   = Count(OrderStatus.Cancelled)
        };

        // Revenue chart — last 14 days
        var chartStart = today.AddDays(-13);
        var chartData = await paidOrders
            .Where(o => o.CreatedAt >= chartStart)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date        = g.Key,
                Revenue     = g.Sum(o => o.TotalAmount),
                OrderCount  = g.Count()
            })
            .ToListAsync();

        var revenueChart = Enumerable.Range(0, 14)
            .Select(i =>
            {
                var date = chartStart.AddDays(i);
                var pt   = chartData.FirstOrDefault(x => x.Date == date);
                return new DailyRevenuePoint
                {
                    Date       = DateOnly.FromDateTime(date),
                    Revenue    = pt?.Revenue ?? 0,
                    OrderCount = pt?.OrderCount ?? 0
                };
            })
            .ToList();

        // Top 10 sản phẩm bán chạy (theo số lượng bán)
        var topProducts = await _ctx.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.PaymentStatus == "Paid")
            .GroupBy(oi => new { oi.Variant.ProductId, oi.ProductName })
            .Select(g => new TopProductItem
            {
                ProductId    = g.Key.ProductId,
                ProductName  = g.Key.ProductName,
                TotalSold    = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(10)
            .ToListAsync();

        return new DashboardSummaryResponse
        {
            Revenue = new RevenueStats
            {
                Today     = revenueToday,
                ThisWeek  = revenueWeek,
                ThisMonth = revenueMonth
            },
            Orders       = orderStats,
            RevenueChart = revenueChart,
            TopProducts  = topProducts
        };
    }
}
