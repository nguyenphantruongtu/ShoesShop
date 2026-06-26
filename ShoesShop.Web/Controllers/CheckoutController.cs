using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApiService _api;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckoutController(ApiService api, IHttpContextAccessor httpContextAccessor)
        {
            _api = api;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            // Load saved addresses nếu user đã đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var resp = await _api.GetAsync<JsonElement>("/api/users/me/addresses");
                if (resp?.Data is JsonElement data
                    && data.ValueKind == JsonValueKind.Array)
                {
                    ViewBag.Addresses = data;
                }
            }

            // Truyền JWT vào view để JS dùng khi gọi /api/orders
            ViewBag.JwtToken = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken") ?? "";

            return View();
        }
    }
}
