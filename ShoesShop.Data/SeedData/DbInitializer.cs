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

        await context.Database.EnsureCreatedAsync();

        if (await context.Roles.AnyAsync()) return;

        var roles = new List<Role>
        {
            new() { RoleName = "Admin", Description = "System administrator with full access" },
            new() { RoleName = "Staff", Description = "Store staff with order management access" },
            new() { RoleName = "Customer", Description = "Regular customer" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
