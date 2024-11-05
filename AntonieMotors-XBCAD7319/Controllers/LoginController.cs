using Microsoft.AspNetCore.Mvc;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult UserLogin()
        {
            return View();
        }

        public IActionResult CustomerLoginSuccess()
        {
            return View();
        }
    }
}
