using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
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
        private readonly FirebaseAuthProvider _authProvider;

        public CustomerController()
        {
            // Initialize FirebaseClient
            _database = new FirebaseClient("https://antonie-motors-default-rtdb.firebaseio.com/");
            _authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyDJxhod4pFGkhUP_Hn3wHI2b3hOiI_dpiY"));

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
            customer.BusinessID = "28bc9a9b52a24d5b9579b5b48a75f4fc"; // Hardcoded business ID

            // 1. Pre-validation: Clear errors and validate the initial fields except CustomerID
            ModelState.Clear();
            TryValidateModel(customer);

            // Password confirmation check
            if (string.IsNullOrEmpty(confirmPassword) || customer.CustomerPassword != confirmPassword)
            {
                ModelState.AddModelError("CustomerPassword", "Passwords do not match.");
            }

            // 4. Validate ModelState after setting required fields
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Model validation error: " + error.ErrorMessage);
                }
                return View(customer);
            }

            // Validate other fields before setting CustomerID
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Phone number validation
            if (!Regex.IsMatch(customer.CustomerMobileNum, @"^\d{10}$"))
            {
                ModelState.AddModelError("CustomerMobileNum", "Mobile number must be exactly 10 digits.");
                return View(customer);
            }

            // Check if email already exists in Firebase Database
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

            try
            {
                // 2. Register user in Firebase Authentication to get the UID
                var auth = await _authProvider.CreateUserWithEmailAndPasswordAsync(
                    customer.CustomerEmail, customer.CustomerPassword, displayName: customer.CustomerName);

                // Confirm authentication and assign UID to CustomerID
                var firebaseToken = auth.FirebaseToken;
                if (string.IsNullOrEmpty(firebaseToken))
                {
                    throw new Exception("Authentication failed. Token is null or empty.");
                }

                // Assign CustomerID from the UID
                customer.CustomerID = auth.User.LocalId;

                // 3. Re-validate ModelState with CustomerID set
                ModelState.Clear();
                TryValidateModel(customer);

                if (!ModelState.IsValid)
                {
                    // Return to view if validation fails after setting CustomerID
                    return View(customer);
                }

                // Set the date of registration
                customer.CustomerAddedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                // Define path to save the customer
                string path = $"Users/{customer.BusinessID}/Customers/{customer.CustomerID}";

                // 4. Save customer data to Firebase Database
                await _database
                    .Child(path)
                    .PutAsync(customer);

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create user or save customer data: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Failed to register the account. Please try again.");
            }

            return View(customer);
        }




        public IActionResult Success()
        {
            return View();
        }
    }
}
