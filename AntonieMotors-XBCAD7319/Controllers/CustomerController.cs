using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AntonieMotors_XBCAD7319.Models;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FirebaseClient _database;

        public CustomerController()
        {
            // Initialize FirebaseClient
            _database = new FirebaseClient("https://antonie-motors-default-rtdb.firebaseio.com/");
            TestFirebaseConnection();
        }

        public async Task<IActionResult> TestFirebaseConnection()
        {
            try
            {
                var data = await _database
                    .Child("Users")
                    .OnceAsync<object>();

                if (data != null)
                {
                    return Content("Connection successful and data retrieved.");
                }
                else
                {
                    return Content("Connection successful but no data found.");
                }
            }
            catch (Exception ex)
            {
                return Content($"Connection failed: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new CustomerModel());
        }
        [HttpPost]
        public async Task<IActionResult> Register(CustomerModel customer, string confirmPassword)
        {
            // 1. Set BusinessID and CustomerID early
            customer.BusinessID = "28bc9a9b52a24d5b9579b5b48a75f4fc"; // Hardcoded business ID
            customer.CustomerID = Guid.NewGuid().ToString();

            // 2. Clear ModelState errors for BusinessID and CustomerID after assigning values
            ModelState.Clear();
            TryValidateModel(customer);

            // 3. Confirm Password Validation
            if (string.IsNullOrEmpty(confirmPassword) || customer.CustomerPassword != confirmPassword)
            {
                ModelState.AddModelError("CustomerPassword", "Passwords do not match.");
            }

            // 4. Validate ModelState after setting required fields
            if (!ModelState.IsValid)
            {
                // Log model validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Model validation error: " + error.ErrorMessage);
                }
                return View(customer);
            }

            // 5. Validate phone number (only digits, 10 characters)
            if (!Regex.IsMatch(customer.CustomerMobileNum, @"^\d{10}$"))
            {
                ModelState.AddModelError("CustomerMobileNum", "Mobile number must be exactly 10 digits.");
                return View(customer);
            }

            // 6. Check for email uniqueness
            var existingCustomers = await _database
                .Child("Users")
                .Child(customer.BusinessID)
                .Child("Customers")
                .OnceAsync<CustomerModel>();

            if (existingCustomers.Any(c => c.Object.CustomerEmail == customer.CustomerEmail))
            {
                ModelState.AddModelError("CustomerEmail", "Email already exists.");
                return View(customer);
            }

            // 7. Set additional required values for the database
            customer.CustomerAddedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string path = $"Users/{customer.BusinessID}/Customers/{customer.CustomerID}";

            // 8. Save to Firebase
            try
            {
                Console.WriteLine($"Saving Customer to Firebase at path: {path}");

                await _database
                    .Child(path)
                    .PutAsync(customer);

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save customer to Firebase: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Failed to save customer data. Please try again.");
            }

            return View(customer);
        }



        public IActionResult Success()
        {
            return View();
        }
    }
}
