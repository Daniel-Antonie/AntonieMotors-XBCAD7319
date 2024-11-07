using Microsoft.AspNetCore.Mvc;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult CustomerDashboard()
        {
            return View();
        }
    }
}
