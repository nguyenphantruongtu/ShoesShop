using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Helpers;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Context;
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Services;

public class ExportService : IExportService
{
    private static readonly IReadOnlyList<ExportTypeDto> ExportTypes =
    [
        new ExportTypeDto { Key = "products", Label = "Danh sách sản phẩm", SupportsDateRange = false, SupportsTop = false },
        new ExportTypeDto { Key = "orders", Label = "Danh sách đơn hàng", SupportsDateRange = true, SupportsTop = false },
        new ExportTypeDto { Key = "top-products", Label = "Top sản phẩm bán chạy", SupportsDateRange = true, SupportsTop = true },
        new ExportTypeDto { Key = "by-brand", Label = "Doanh thu theo thương hiệu", SupportsDateRange = true, SupportsTop = true },
        new ExportTypeDto { Key = "by-category", Label = "Doanh thu theo danh mục", SupportsDateRange = true, SupportsTop = true }
    ];

    private readonly ShoeStoreDbContext _context;
    private readonly IReportService _reportService;

    public ExportService(ShoeStoreDbContext context, IReportService reportService)
    {
        _context = context;
        _reportService = reportService;
    }

    public IReadOnlyList<ExportTypeDto> GetExportTypes() => ExportTypes;

    public async Task<(byte[] Content, string FileName)> ExportCsvAsync(string type, DateTime? from = null, DateTime? to = null, int top = 20)
    {
        var normalizedType = type.Trim().ToLowerInvariant();
        var (content, filePrefix) = normalizedType switch
        {
            "products" => (await ExportProductsAsync(), "san-pham"),
            "orders" => (await ExportOrdersAsync(from, to), "don-hang"),
            "top-products" => (await ExportTopProductsAsync(from, to, top), "top-san-pham"),
            "by-brand" => (await ExportByBrandAsync(from, to, top), "doanh-thu-thuong-hieu"),
            "by-category" => (await ExportByCategoryAsync(from, to, top), "doanh-thu-danh-muc"),
            _ => throw new ArgumentException($"Loại export không hợp lệ: {type}")
        };

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return (content, $"{filePrefix}_{timestamp}.csv");
    }

    private async Task<byte[]> ExportProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .OrderBy(p => p.ProductId)
            .Select(p => new
            {
                p.ProductId,
                p.ProductName,
                BrandName = p.Brand.BrandName,
                CategoryName = p.Category.CategoryName,
                p.BasePrice,
                p.SalePrice,
                p.Gender,
                p.Material,
                p.IsActive,
                p.IsFeatured,
                p.ViewCount,
                p.CreatedAt
            })
            .ToListAsync();

        return CsvBuilder.Build(
            ["Mã SP", "Tên sản phẩm", "Thương hiệu", "Danh mục", "Giá gốc", "Giá sale", "Giới tính", "Chất liệu", "Đang bán", "Nổi bật", "Lượt xem", "Ngày tạo"],
            products.Select(p => new object?[]
            {
                p.ProductId,
                p.ProductName,
                p.BrandName,
                p.CategoryName,
                p.BasePrice,
                p.SalePrice,
                p.Gender,
                p.Material,
                p.IsActive,
                p.IsFeatured,
                p.ViewCount,
                p.CreatedAt
            }));
    }

    private async Task<byte[]> ExportOrdersAsync(DateTime? from, DateTime? to)
    {
        var end = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var start = (from ?? end.AddMonths(-1)).ToUniversalTime();

        var orders = await _context.Orders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.OrderId,
                o.OrderCode,
                o.RecipientName,
                o.RecipientPhone,
                o.ShippingAddress,
                o.Province,
                o.District,
                o.Ward,
                o.SubTotal,
                o.ShippingFee,
                o.DiscountAmount,
                o.TotalAmount,
                o.OrderStatus,
                o.PaymentStatus,
                o.CreatedAt
            })
            .ToListAsync();

        return CsvBuilder.Build(
            ["Mã đơn", "Mã đơn hàng", "Người nhận", "SĐT", "Địa chỉ", "Tỉnh/TP", "Quận/Huyện", "Phường/Xã", "Tạm tính", "Phí ship", "Giảm giá", "Tổng tiền", "Trạng thái đơn", "Trạng thái TT", "Ngày tạo"],
            orders.Select(o => new object?[]
            {
                o.OrderId,
                o.OrderCode,
                o.RecipientName,
                o.RecipientPhone,
                o.ShippingAddress,
                o.Province,
                o.District,
                o.Ward,
                o.SubTotal,
                o.ShippingFee,
                o.DiscountAmount,
                o.TotalAmount,
                o.OrderStatus,
                o.PaymentStatus,
                o.CreatedAt
            }));
    }

    private async Task<byte[]> ExportTopProductsAsync(DateTime? from, DateTime? to, int top)
    {
        var items = await _reportService.GetTopSellingProductsAsync(top, from, to);

        return CsvBuilder.Build(
            ["Mã SP", "Tên sản phẩm", "Số lượng bán", "Doanh thu"],
            items.Select(p => new object?[]
            {
                p.ProductId,
                p.ProductName,
                p.TotalQuantity,
                p.TotalRevenue
            }));
    }

    private async Task<byte[]> ExportByBrandAsync(DateTime? from, DateTime? to, int top)
    {
        var items = await _reportService.GetRevenueByBrandAsync(from, to, top);

        return CsvBuilder.Build(
            ["Mã thương hiệu", "Thương hiệu", "Số lượng", "Doanh thu"],
            items.Select(p => new object?[]
            {
                p.GroupId,
                p.GroupName,
                p.TotalQuantity,
                p.TotalRevenue
            }));
    }

    private async Task<byte[]> ExportByCategoryAsync(DateTime? from, DateTime? to, int top)
    {
        var items = await _reportService.GetRevenueByCategoryAsync(from, to, top);

        return CsvBuilder.Build(
            ["Mã danh mục", "Danh mục", "Số lượng", "Doanh thu"],
            items.Select(p => new object?[]
            {
                p.GroupId,
                p.GroupName,
                p.TotalQuantity,
                p.TotalRevenue
            }));
    }
}
