using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartWithItemsByUserIdAsync(int userId);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task<Cart> CreateCartAsync(int userId);
    Task AddItemAsync(CartItem item);
    Task UpdateItemAsync(CartItem item);
    Task RemoveItemAsync(CartItem item);
    Task ClearCartAsync(int cartId);
}
