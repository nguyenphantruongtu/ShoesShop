using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ShoeStoreDbContext _ctx;
    public CartRepository(ShoeStoreDbContext ctx) => _ctx = ctx;

    public async Task<Cart?> GetCartWithItemsByUserIdAsync(int userId)
        => await _ctx.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.ProductImages)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Size)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        => await _ctx.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Variant)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

    public async Task<Cart> CreateCartAsync(int userId)
    {
        var cart = new Cart
        {
            UserId    = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _ctx.Carts.AddAsync(cart);
        await _ctx.SaveChangesAsync();
        return cart;
    }

    public async Task AddItemAsync(CartItem item)
    {
        await _ctx.CartItems.AddAsync(item);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        _ctx.CartItems.Update(item);
        await _ctx.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _ctx.CartItems.Remove(item);
        await _ctx.SaveChangesAsync();
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = await _ctx.CartItems.Where(ci => ci.CartId == cartId).ToListAsync();
        _ctx.CartItems.RemoveRange(items);
        await _ctx.SaveChangesAsync();
    }
}
