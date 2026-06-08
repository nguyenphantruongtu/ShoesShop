using ShoesShop.Shared.DTOs.Cart;

namespace ShoesShop.Business.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(int userId);
    Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request);
    Task<CartResponse> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task<CartResponse> RemoveItemAsync(int userId, int cartItemId);
    Task ClearCartAsync(int userId);
}
