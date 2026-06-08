using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Json;
using System; 
using ShoesShop.Shared.DTOs;

namespace ShoesShop.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Inject HttpClient để gọi API lấy danh sách đơn hàng từ Backend
        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // URL: https://localhost:7161/Order/History
        public async Task<IActionResult> History()
        {
            // Khách hàng mặc định 'duy pham' để test kết nối dữ liệu
            int userId = 1;
            var client = _httpClientFactory.CreateClient();

            // Gọi lên API Backend (Cổng 7214)
            var response = await client.GetAsync($"https://localhost:7214/api/orders/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                // Sử dụng ApiResponse và OrderDto được cấu hình tập trung trong dự án Shared
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<OrderDto>>>();
                if (result != null && result.Success && result.Data != null)
                {
                    return View(result.Data);
                }
            }

            return View(new List<OrderDto>());
        }
    }
}