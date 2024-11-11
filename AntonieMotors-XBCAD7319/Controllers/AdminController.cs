using Microsoft.AspNetCore.Mvc;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QuoteGenerator()
        {
            return View();
        }

        public IActionResult CustomerManagement()
        {
            return View();
        }

        public IActionResult VehicleManagement()
        {
            return View();
        }
        public IActionResult InventoryManagement()
        {
            return View();
        }

        public IActionResult Analytics()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }
    }
}
