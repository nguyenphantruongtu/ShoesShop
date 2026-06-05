using Microsoft.AspNetCore.Mvc;

namespace ShoesShop.Web.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        // UC-02: Trang danh sách sản phẩm (Sẽ dùng JS/OData gọi sang API sau)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // UC-04: Trang chi tiết sản phẩm (Ví dụ: /products/5)
        [HttpGet("{id}")]
        public IActionResult Detail(int id)
        {
            // Tạm thời truyền Id qua ViewBag để ngoài View sử dụng
            ViewBag.ProductId = id;
            return View();
        }
    }
}