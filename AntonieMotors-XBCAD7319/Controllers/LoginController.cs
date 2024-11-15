using Microsoft.AspNetCore.Mvc;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class LoginController : Controller
    {
        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;

        public LoginController()
        {
            _authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyDJxhod4pFGkhUP_Hn3wHI2b3hOiI_dpiY"));
            _firebaseClient = new FirebaseClient("https://antonie-motors-default-rtdb.firebaseio.com/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin(string email, string password)
        {
            bool isUserFound = false;

            try
            {
                // Authenticate user with Firebase
                var auth = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
                var userId = auth.User.LocalId;
                BusinessID.userId = userId;

                // Hardcoded business ID
                // string businessId = "33a48a2ae69d46b4a4256c3811f8e57c";
                string businessId = BusinessID.businessId;

                // Check Employees under the specific business ID
                var employees = await _firebaseClient
                    .Child($"Users/{businessId}/Employees")
                    .OnceAsync<dynamic>();

                foreach (var employee in employees)
                {
                    // Check if the employee ID matches the authenticated user ID
                    if (employee.Key == userId || (employee.Object.id != null && employee.Object.id == userId))
                    {
                        isUserFound = true;
                        var role = employee.Object.role;

                        // Redirect based on role
                        if (role == "admin")
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (role == "employee")
                        {
                            TempData["Error"] = "Access denied. Please speak to your Admin.";
                            return RedirectToAction("UserLogin");
                        }
                    }
                }

                // Check Customers under the specific business ID
                var customers = await _firebaseClient
                    .Child($"Users/{businessId}/Customers")
                    .OnceAsync<dynamic>();

                foreach (var customer in customers)
                {
                    // Check if the customer ID matches the authenticated user ID
                    if (customer.Key == userId || (customer.Object.CustomerID != null && customer.Object.CustomerID == userId))
                    {
                        isUserFound = true;
                        return RedirectToAction("Index", "Customer");
                    }
                }

                // Set error message if user is not found in either Employees or Customers
                TempData["Error"] = isUserFound
                    ? "Invalid credentials. Please check your login details."
                    : "Login failed. User not found.";
            }
            catch
            {
                TempData["Error"] = "Login failed. Please try again.";
            }

            return RedirectToAction("UserLogin");
        }



        [HttpGet]
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