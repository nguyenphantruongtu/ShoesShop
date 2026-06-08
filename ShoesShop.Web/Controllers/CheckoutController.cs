using Microsoft.AspNetCore.Mvc;

namespace ShoesShop.Web.Controllers
{
    public class CheckoutController : Controller
    {
        [HttpGet("Checkout")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
