using Microsoft.AspNetCore.Mvc;
using Firebase.Database; 
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;
using AntonieMotors_XBCAD7319.Models;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FirebaseClient _database;

        public CustomerController()
        {
            // Use FirebaseClient for FirebaseDatabase.net
            _database = new FirebaseClient("https://antonie-motors-default-rtdb.firebaseio.com/");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new CustomerModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(CustomerModel customer)
        {
            if (ModelState.IsValid)
            {
                // Generate a unique CustomerID using Guid
                customer.CustomerID = Guid.NewGuid().ToString();
                customer.CustomerAddedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                // Path to store customer data under the business ID
                string path = $"Users/{customer.BusinessID}/Customers/{customer.CustomerID}";

                // Save customer data to Firebase
                await _database
                    .Child(path)
                    .PutAsync(customer);

                return RedirectToAction("Success"); // Redirect to a success page or any other page
            }
            return View(customer);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
