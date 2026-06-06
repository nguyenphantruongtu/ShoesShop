using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Context;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Services;

public class ReportService : IReportService
{
    private readonly ShoeStoreDbContext _context;

    public ReportService(ShoeStoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int top = 10, DateTime? from = null, DateTime? to = null)
    {
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = (from ?? end.AddMonths(-1)).ToUniversalTime();

        var query = _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end && oi.Order.PaymentStatus == "Paid")
            .GroupBy(oi => new { oi.Variant.Product.ProductId, oi.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.LineTotal),
                ImageUrl = _context.ProductImages.Where(pi => pi.ProductId == g.Key.ProductId && pi.IsPrimary).Select(pi => pi.ImageUrl).FirstOrDefault()
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(top);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<RevenueGroupDto>> GetRevenueByBrandAsync(DateTime? from = null, DateTime? to = null, int top = 20)
    {
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = (from ?? end.AddMonths(-1)).ToUniversalTime();

        var query = _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end && oi.Order.PaymentStatus == "Paid")
            .GroupBy(oi => new { oi.Variant.Product.Brand.BrandId, oi.Variant.Product.Brand.BrandName })
            .Select(g => new RevenueGroupDto
            {
                GroupId = g.Key.BrandId,
                GroupName = g.Key.BrandName,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(top);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<RevenueGroupDto>> GetRevenueByCategoryAsync(DateTime? from = null, DateTime? to = null, int top = 20)
    {
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = (from ?? end.AddMonths(-1)).ToUniversalTime();

        var query = _context.OrderItems
            .Where(oi => oi.Order.CreatedAt >= start && oi.Order.CreatedAt <= end && oi.Order.PaymentStatus == "Paid")
            .GroupBy(oi => new { oi.Variant.Product.Category.CategoryId, oi.Variant.Product.Category.CategoryName })
            .Select(g => new RevenueGroupDto
            {
                GroupId = g.Key.CategoryId,
                GroupName = g.Key.CategoryName,
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(top);

        return await query.ToListAsync();
    }
}
