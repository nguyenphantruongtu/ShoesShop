using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ShoesShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Tạo một HttpClient để gửi request
        var client = _httpClientFactory.CreateClient();

        try
        {
            // 2. Gọi chính xác cổng port HTTPS của API mà bạn đã test trên Swagger (7214)
            var response = await client.GetAsync("https://localhost:7214/api/products/featured");

            if (response.IsSuccessStatusCode)
            {
                // 3. Đọc chuỗi JSON trả về từ API
                var jsonString = await response.Content.ReadAsStringAsync();

                // 4. Giải mã (Deserialize) chuỗi JSON đó thành Object C# dựa vào Khuôn mẫu ở tầng Shared
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProductDto>>>(jsonString, options);

                // 5. Truyền danh sách sản phẩm (Data) ra ngoài View HTML
                return View(result?.Data ?? new List<ProductDto>());
            }
        }
        catch (Exception ex)
        {
            // Nếu API chưa bật hoặc lỗi, ghi log và trả về danh sách rỗng để giao diện không bị sập
            ViewBag.ErrorMessage = "Không thể kết nối đến hệ thống Backend API.";
        }

        return View(new List<ProductDto>());
    }
}