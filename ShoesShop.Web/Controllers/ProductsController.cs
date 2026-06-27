using Microsoft.AspNetCore.Mvc;
using ShoesShop.Web.Services;
using System.Text.Json;

namespace ShoesShop.Web.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        private readonly ApiService _api;
        public ProductsController(ApiService api) => _api = api;

        // UC-02: Trang danh sách sản phẩm
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // UC-04: Trang chi tiết sản phẩm
        [HttpGet("Detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var productResp = await _api.GetAsync<JsonElement>($"/api/products/{id}");
            var reviewResp  = await _api.GetAsync<JsonElement>($"/api/products/{id}/reviews");

            if (productResp?.Data is null || productResp.Data.ValueKind == JsonValueKind.Undefined)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Product = productResp.Data;
            ViewBag.Reviews = reviewResp?.Data;
            return View();
        }
    }
}