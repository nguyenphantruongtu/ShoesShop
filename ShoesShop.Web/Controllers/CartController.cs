using Microsoft.AspNetCore.Mvc;

namespace ShoesShop.Web.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}