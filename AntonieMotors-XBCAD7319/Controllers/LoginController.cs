﻿using Microsoft.AspNetCore.Mvc;
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
            _authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("API_KEY_HERE"));
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
                var authLink = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
                var userId = authLink.User.LocalId;
                string businessId = BusinessID.businessId;

                // Check Employees under the specific business ID
                var employees = await _firebaseClient
                    .Child($"Users/{businessId}/Employees")
                    .OnceAsync<dynamic>();

                foreach (var employee in employees)
                {
                    if (employee.Key == userId || (employee.Object.id != null && employee.Object.id == userId))
                    {
                        isUserFound = true;
                        var role = employee.Object.role;

                        // Redirect based on role
                        HttpContext.Session.SetString("userId", authLink.User.LocalId);
                        HttpContext.Session.SetString("firebaseToken", authLink.FirebaseToken);

                        return role == "admin" || role == "owner"
                            ? RedirectToAction("Index", "Admin")
                            : RedirectToAction("UserLogin", new { error = "Access denied. Please speak to your Admin." });
                    }
                }

                // Check Customers under the specific business ID
                var customers = await _firebaseClient
                    .Child($"Users/{businessId}/Customers")
                    .OnceAsync<dynamic>();

                foreach (var customer in customers)
                {
                    if (customer.Key == userId || (customer.Object.CustomerID != null && customer.Object.CustomerID == userId))
                    {
                        isUserFound = true;
                        HttpContext.Session.SetString("userId", authLink.User.LocalId);
                        HttpContext.Session.SetString("firebaseToken", authLink.FirebaseToken);
                        return RedirectToAction("Index", "Customer");
                    }
                }

                TempData["Error"] = isUserFound ? "Invalid credentials." : "Login failed. User not found.";
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
    }
}
