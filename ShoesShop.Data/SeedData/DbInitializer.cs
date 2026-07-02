using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;

namespace ShoesShop.Data.SeedData;

public static class DbInitializer
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ShoeStoreDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new() { RoleName = "Admin",    Description = "System administrator with full access" },
                new() { RoleName = "Staff",    Description = "Store staff with order management access" },
                new() { RoleName = "Customer", Description = "Regular customer" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        if (!await context.PaymentMethods.AnyAsync())
        {
            var methods = new List<PaymentMethod>
            {
                new() { MethodName = "COD",   Description = "Thanh toán khi nhận hàng", IsActive = true },
                new() { MethodName = "PayOS", Description = "Thanh toán trực tuyến qua PayOS (QR / chuyển khoản)", IsActive = true }
            };
            await context.PaymentMethods.AddRangeAsync(methods);
            await context.SaveChangesAsync();
        }

        await SeedShowcaseReviewsAsync(context);
    }

    /// <summary>
    /// Seed một số đánh giá "demo" đã được duyệt sẵn để trang sản phẩm có nội dung
    /// hiển thị cho khách/guest ngay cả trên DB mới chưa có đơn hàng thật.
    /// Các review này không gắn OrderItemId (không phải đơn hàng thật) — vẫn được
    /// đánh dấu IsApproved = true vì là nội dung showcase đã được duyệt trước.
    /// Chỉ chạy khi bảng Reviews đang trống — không đụng tới dữ liệu review thật.
    /// </summary>
    private static async Task SeedShowcaseReviewsAsync(ShoeStoreDbContext context)
    {
        if (await context.Reviews.AnyAsync()) return;

        var products = await context.Products.OrderBy(p => p.ProductId).Take(8).Select(p => p.ProductId).ToListAsync();
        var customers = await context.Users
            .Where(u => u.Role.RoleName == "Customer")
            .OrderBy(u => u.UserId)
            .Take(6)
            .Select(u => u.UserId)
            .ToListAsync();

        if (products.Count == 0 || customers.Count == 0) return;

        var comments = new[]
        {
            "Giày đẹp, đúng size, đóng gói cẩn thận. Sẽ ủng hộ shop tiếp!",
            "Chất lượng tốt so với giá tiền, đi êm chân.",
            "Giao hàng nhanh, giày y hình, rất ưng ý.",
            "Form giày chuẩn, màu sắc lên đẹp hơn cả ảnh.",
            "Đóng gói kỹ, giày mang thoải mái cả ngày.",
            "Sản phẩm ổn, đáng tiền, sẽ mua lại.",
            "Chất liệu êm, phù hợp đi chơi lẫn đi làm.",
            "Giày nhẹ, thoáng khí, rất phù hợp mùa hè."
        };

        var random = new Random(20260702);
        var reviews = new List<Review>();

        for (int i = 0; i < products.Count; i++)
        {
            reviews.Add(new Review
            {
                ProductId  = products[i],
                UserId     = customers[i % customers.Count],
                Rating     = 4 + random.Next(0, 2), // 4 hoặc 5 sao — showcase nội dung tích cực
                Comment    = comments[i % comments.Length],
                IsApproved = true,
                CreatedAt  = DateTime.UtcNow.AddDays(-random.Next(1, 30))
            });
        }

        await context.Reviews.AddRangeAsync(reviews);
        await context.SaveChangesAsync();
    }
}
