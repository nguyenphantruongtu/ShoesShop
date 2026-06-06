using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Context;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly ShoeStoreDbContext _context;

    public DashboardService(ShoeStoreDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetOverviewAsync()
    {
        var now = DateTime.UtcNow; // DB columns use sysutcdatetime

        // Define day bounds (UTC)
        var startOfToday = now.Date;
        var startOfWeek = startOfToday.AddDays(-6); // last 7 days including today
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var dto = new DashboardDto();

        // Orders considered paid (Payment.Status == "Completed" or Order.PaymentStatus == "Paid")
        // We'll use Orders table TotalAmount and CreatedAt. If Payments exist prefer Payment.PaidAt.

        // Revenue today
        dto.RevenueToday = await _context.Orders
            .Where(o => o.CreatedAt >= startOfToday && o.CreatedAt < startOfToday.AddDays(1) && o.PaymentStatus == "Paid")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        // Orders today
        dto.OrdersToday = await _context.Orders
            .Where(o => o.CreatedAt >= startOfToday && o.CreatedAt < startOfToday.AddDays(1))
            .CountAsync();

        // Week revenue / orders (last 7 days)
        dto.RevenueWeek = await _context.Orders
            .Where(o => o.CreatedAt >= startOfWeek && o.CreatedAt < startOfToday.AddDays(1) && o.PaymentStatus == "Paid")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        dto.OrdersWeek = await _context.Orders
            .Where(o => o.CreatedAt >= startOfWeek && o.CreatedAt < startOfToday.AddDays(1))
            .CountAsync();

        // Month revenue / orders
        dto.RevenueMonth = await _context.Orders
            .Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt < startOfMonth.AddMonths(1) && o.PaymentStatus == "Paid")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        dto.OrdersMonth = await _context.Orders
            .Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt < startOfMonth.AddMonths(1))
            .CountAsync();

        // Last 7 days chart data
        var labels = new List<string>();
        var data = new List<decimal>();
        for (int i = 6; i >= 0; i--)
        {
            var day = startOfToday.AddDays(-i);
            var next = day.AddDays(1);
            var dayRevenue = await _context.Orders
                .Where(o => o.CreatedAt >= day && o.CreatedAt < next && o.PaymentStatus == "Paid")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            labels.Add(day.ToString("MM/dd"));
            data.Add(dayRevenue);
        }

        dto.Last7DaysLabels = labels;
        dto.Last7DaysRevenue = data;

        return dto;
    }
}
