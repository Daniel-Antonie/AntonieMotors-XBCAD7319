using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin.Database;
using System.Threading.Tasks;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class CustomerController : Controller
    {
       

        public class CustomerController : Controller
        {
            private readonly FirebaseDatabase _database;

            public CustomerController()
            {
                _database = FirebaseDatabase.DefaultInstance;
            }

            [HttpGet]
            public IActionResult Register()
            {
                return View(new CustomerData());
            }

            [HttpPost]
            public async Task<IActionResult> Register(CustomerData customer)
            {
                if (ModelState.IsValid)
                {
                    customer.CustomerID = _database.RootReference.Push().Key;
                    customer.CustomerAddedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                    // Path to store customer data under the business ID
                    string path = $"Users/{customer.BusinessID}/Customers/{customer.CustomerID}";

                    // Save customer data to Firebase
                    await _database.GetReference(path).SetRawJsonValueAsync(Newtonsoft.Json.JsonConvert.SerializeObject(customer));

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
}
