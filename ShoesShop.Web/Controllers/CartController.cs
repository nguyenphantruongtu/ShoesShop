using Microsoft.AspNetCore.Mvc;
using ShoesShop.Shared.DTOs.Cart;
using System.Text.Json;

namespace ShoesShop.Web.Controllers;

public class CartController : Controller
{
    private const string CART_SESSION_KEY = "GuestCart";

    public IActionResult Index()
    {
        // Trả về giao diện giỏ hàng (Giao diện này sẽ dùng Ajax để gọi lấy dữ liệu hiển thị)
        return View();
    }

    [HttpPost]
    public IActionResult AddToCartLocal([FromBody] AddToCartRequest request)
    {
        // 1. Nếu khách ĐÃ ĐĂNG NHẬP -> Không lưu Session, trả về tín hiệu để Frontend gọi thẳng API gốc
        if (User.Identity?.IsAuthenticated == true)
        {
            return Json(new { isGuest = false });
        }

        // 2. Nếu khách CHƯA ĐĂNG NHẬP -> Xử lý lưu vào Session
        var sessionCart = HttpContext.Session.GetString(CART_SESSION_KEY);
        var cartItems = string.IsNullOrEmpty(sessionCart)
            ? new List<AddToCartRequest>()
            : JsonSerializer.Deserialize<List<AddToCartRequest>>(sessionCart) ?? new List<AddToCartRequest>();

        // Kiểm tra xem sản phẩm biến thể (ProductVariantId) này đã tồn tại trong giỏ hàng tạm chưa
        var existingItem = cartItems.FirstOrDefault(x => x.VariantId == request.VariantId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity; // Cộng dồn số lượng
        }
        else
        {
            cartItems.Add(request); // Thêm mới item vào danh sách tạm
        }

        // Lưu lại danh sách vào Session
        HttpContext.Session.SetString(CART_SESSION_KEY, JsonSerializer.Serialize(cartItems));

        return Json(new { isGuest = true, message = "Đã thêm vào giỏ hàng tạm thời (Session)." });
    }
}