using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoesShop.Business.Interfaces;
using ShoesShop.Shared.DTOs.Cart;
using System.Security.Claims;

namespace ShoesShop.API.Controllers;

/// <summary>
/// F5. Shopping Cart
/// UC-13: POST   /api/cart/items          — Thêm sản phẩm vào giỏ (chọn variant + qty + check stock)
/// UC-14: GET    /api/cart                — Xem giỏ hàng + tổng tiền
///        PUT    /api/cart/items/{id}     — Cập nhật số lượng (quantity=0 → xóa item)
///        DELETE /api/cart/items/{id}     — Xóa item khỏi giỏ
/// </summary>
[ApiController]
[Route("api/cart")]
[Authorize(Roles = Roles.Customer)]
public class CartController : ControllerBase
{
    private readonly ICartService _service;
    public CartController(ICartService service) => _service = service;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>UC-14: Xem giỏ hàng của customer đang đăng nhập</summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _service.GetCartAsync(UserId);
        return Ok(ApiResponse<CartResponse>.Ok(result));
    }

    /// <summary>UC-13: Thêm sản phẩm vào giỏ. Nếu variant đã có → cộng dồn số lượng.</summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            var result = await _service.AddToCartAsync(UserId, request);
            return Ok(ApiResponse<CartResponse>.Ok(result, "Đã thêm vào giỏ hàng."));
        }
        catch (KeyNotFoundException ex)      { return NotFound(ApiResponse<CartResponse>.Fail(ex.Message)); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<CartResponse>.Fail(ex.Message)); }
        catch (ArgumentException ex)         { return BadRequest(ApiResponse<CartResponse>.Fail(ex.Message)); }
    }

    /// <summary>
    /// UC-14: Cập nhật số lượng item trong giỏ.
    /// Quantity = 0 → tự động xóa item khỏi giỏ.
    /// </summary>
    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var result = await _service.UpdateItemAsync(UserId, cartItemId, request);
            return Ok(ApiResponse<CartResponse>.Ok(result, "Đã cập nhật giỏ hàng."));
        }
        catch (KeyNotFoundException ex)      { return NotFound(ApiResponse<CartResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException)  { return Forbid(); }
        catch (InvalidOperationException ex) { return Conflict(ApiResponse<CartResponse>.Fail(ex.Message)); }
        catch (ArgumentException ex)         { return BadRequest(ApiResponse<CartResponse>.Fail(ex.Message)); }
    }

    /// <summary>UC-14: Xóa một item khỏi giỏ hàng</summary>
    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        try
        {
            var result = await _service.RemoveItemAsync(UserId, cartItemId);
            return Ok(ApiResponse<CartResponse>.Ok(result, "Đã xóa sản phẩm khỏi giỏ hàng."));
        }
        catch (KeyNotFoundException ex)     { return NotFound(ApiResponse<CartResponse>.Fail(ex.Message)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    /// <summary>
    /// Thêm mới: Endpoint để gộp giỏ hàng tạm từ Session vào Database sau khi Login thành công
    /// </summary>
    [HttpPost("merge")]
    public async Task<IActionResult> MergeCart([FromBody] List<AddToCartRequest> temporaryItems)
    {
        try
        {
            if (temporaryItems == null || !temporaryItems.Any())
            {
                var currentCart = await _service.GetCartAsync(UserId);
                return Ok(ApiResponse<CartResponse>.Ok(currentCart, "Không có sản phẩm tạm để gộp."));
            }

            CartResponse result = null;
            // Vòng lặp duyệt qua từng item tạm từ Session và gọi hàm AddToCart có sẵn của Service để tự động cộng dồn số lượng
            foreach (var item in temporaryItems)
            {
                result = await _service.AddToCartAsync(UserId, item);
            }

            return Ok(ApiResponse<CartResponse>.Ok(result, "Đã gộp giỏ hàng thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CartResponse>.Fail(ex.Message));
        }
    }
}
