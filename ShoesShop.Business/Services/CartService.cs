using Microsoft.EntityFrameworkCore;
using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Shared.DTOs.Cart;

namespace ShoesShop.Business.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;
    private readonly ShoeStoreDbContext _ctx;

    public CartService(ICartRepository cartRepo, ShoeStoreDbContext ctx)
    {
        _cartRepo = cartRepo;
        _ctx      = ctx;
    }

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        var cart = await _cartRepo.GetCartWithItemsByUserIdAsync(userId);
        return cart is null ? new CartResponse() : MapToResponse(cart);
    }

    public async Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request)
    {
        if (request.Quantity < 1)
            throw new ArgumentException("Số lượng phải ít nhất là 1.");

        var variant = await _ctx.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.VariantId == request.VariantId && v.IsActive)
            ?? throw new KeyNotFoundException("Không tìm thấy variant sản phẩm.");

        if (!variant.Product.IsActive)
            throw new InvalidOperationException("Sản phẩm không còn được bán.");

        var cart = await _cartRepo.GetCartWithItemsByUserIdAsync(userId)
                   ?? await _cartRepo.CreateCartAsync(userId);

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == request.VariantId);

        if (existingItem is not null)
        {
            var newQty = existingItem.Quantity + request.Quantity;
            if (newQty > variant.StockQuantity)
                throw new InvalidOperationException(
                    $"Không đủ hàng. Tồn kho hiện tại: {variant.StockQuantity}, trong giỏ: {existingItem.Quantity}.");

            existingItem.Quantity = newQty;
            await _cartRepo.UpdateItemAsync(existingItem);
        }
        else
        {
            if (request.Quantity > variant.StockQuantity)
                throw new InvalidOperationException(
                    $"Không đủ hàng. Tồn kho hiện tại: {variant.StockQuantity}.");

            var item = new CartItem
            {
                CartId    = cart.CartId,
                VariantId = request.VariantId,
                Quantity  = request.Quantity,
                AddedAt   = DateTime.UtcNow
            };
            await _cartRepo.AddItemAsync(item);
        }

        var updated = await _cartRepo.GetCartWithItemsByUserIdAsync(userId);
        return MapToResponse(updated!);
    }

    public async Task<CartResponse> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        if (request.Quantity < 0)
            throw new ArgumentException("Số lượng không hợp lệ.");

        var item = await _cartRepo.GetCartItemByIdAsync(cartItemId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm trong giỏ hàng.");

        if (item.Cart.UserId != userId)
            throw new UnauthorizedAccessException("Không có quyền thao tác giỏ hàng này.");

        if (request.Quantity == 0)
        {
            await _cartRepo.RemoveItemAsync(item);
        }
        else
        {
            if (request.Quantity > item.Variant.StockQuantity)
                throw new InvalidOperationException(
                    $"Không đủ hàng. Tồn kho hiện tại: {item.Variant.StockQuantity}.");

            item.Quantity = request.Quantity;
            await _cartRepo.UpdateItemAsync(item);
        }

        var cart = await _cartRepo.GetCartWithItemsByUserIdAsync(userId);
        return cart is null ? new CartResponse() : MapToResponse(cart);
    }

    public async Task<CartResponse> RemoveItemAsync(int userId, int cartItemId)
    {
        var item = await _cartRepo.GetCartItemByIdAsync(cartItemId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm trong giỏ hàng.");

        if (item.Cart.UserId != userId)
            throw new UnauthorizedAccessException("Không có quyền thao tác giỏ hàng này.");

        await _cartRepo.RemoveItemAsync(item);

        var cart = await _cartRepo.GetCartWithItemsByUserIdAsync(userId);
        return cart is null ? new CartResponse() : MapToResponse(cart);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepo.GetCartWithItemsByUserIdAsync(userId);
        if (cart is not null)
            await _cartRepo.ClearCartAsync(cart.CartId);
    }

    // ── Mapping ──────────────────────────────────────────────────────────────

    private static CartResponse MapToResponse(Cart cart)
    {
        var items = cart.CartItems.Select(MapItemToResponse).ToList();
        return new CartResponse
        {
            CartId      = cart.CartId,
            Items       = items,
            TotalItems  = items.Sum(i => i.Quantity),
            TotalAmount = items.Sum(i => i.SubTotal)
        };
    }

    private static CartItemResponse MapItemToResponse(CartItem ci)
    {
        var variant   = ci.Variant;
        var product   = variant.Product;
        var unitPrice = variant.Price ?? product.SalePrice ?? product.BasePrice;
        var image     = product.ProductImages
                            .FirstOrDefault(img => img.IsPrimary)
                        ?? product.ProductImages.FirstOrDefault();

        return new CartItemResponse
        {
            CartItemId    = ci.CartItemId,
            VariantId     = ci.VariantId,
            ProductId     = product.ProductId,
            ProductName   = product.ProductName,
            Slug          = product.Slug,
            SizeName      = variant.Size.SizeValue,
            ColorName     = variant.Color.ColorName,
            ImageUrl      = image?.ImageUrl,
            UnitPrice     = unitPrice,
            Quantity      = ci.Quantity,
            StockQuantity = variant.StockQuantity,
            SubTotal      = unitPrice * ci.Quantity
        };
    }
}
