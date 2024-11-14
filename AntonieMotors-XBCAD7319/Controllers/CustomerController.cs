using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AntonieMotors_XBCAD7319.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FirebaseClient _database = new FirebaseClient("https://antonie-motors-default-rtdb.firebaseio.com/");
        private readonly FirebaseAuthProvider _authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyDJxhod4pFGkhUP_Hn3wHI2b3hOiI_dpiY"));

        public CustomerController()
        {
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
            //  customer.BusinessID = "28bc9a9b52a24d5b9579b5b48a75f4fc"; // Hardcoded business ID
            //  customer.BusinessID = "33a48a2ae69d46b4a4256c3811f8e57c"; // Perlas one 

            customer.BusinessID = BusinessID.businessId;

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


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QuoteGeneratorCust()
        {
            return View();
        }

        public IActionResult ViewCarStatus()
        {
            return View();
        }

        public async Task<IActionResult> ServiceHistory()
        {
            await getAllServices();

            return View();
        }



        //public IActionResult AnotherOption()
        //{
        //    return View();
        //}

        //public IActionResult Analytics()
        //{
        //    return View();
        //}

        //get list of Services
        private async Task getAllServices()
        {
            try
            {
                var services = await _database.Child($"Users/{BusinessID.businessId}/Services").OnceAsync<dynamic>();

                if (services == null || !services.Any())
                {
                    ViewBag.Services = new List<dynamic>();
                    return;
                }

                var serviceList = new List<dynamic>();

                foreach (var service in services)
                {
                    if (service.Object.custID == BusinessID.userId)
                    {
                        string vehicleModel = await fetchVehicleModel(service.Object.vehicleID);
                        string vehicleNumberPlate = await fetchVehicleNumPlate(service.Object.vehicleID);

                        string dateTakenIn = service.Object.dateTakenIn != null
                            ? DateTimeOffset.FromUnixTimeMilliseconds((long)service.Object.dateTakenIn).ToString("dd MMMM yyyy")
                            : "N/A";

                        string dateReturned = service.Object.dateReturned != null
                            ? DateTimeOffset.FromUnixTimeMilliseconds((long)service.Object.dateReturned).ToString("dd MMMM yyyy")
                            : "N/A";

                        serviceList.Add(new
                        {
                            Name = service.Object.name,
                            Status = service.Object.status,
                            Model = vehicleModel,
                            NumberPlate = vehicleNumberPlate,
                            DateTakenIn = dateTakenIn,
                            DateReturned = dateReturned,
                            TotalCost = service.Object.totalCost
                        }); 
                    }
                }

                ViewBag.Services = serviceList;
                Console.WriteLine($"Services fetched: {serviceList.Count}");
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
        }

        private async Task<string> fetchVehicleNumPlate(dynamic vehicleID)
        {
            try
            {
                return await _database.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleNumPlate").OnceSingleAsync<String>();
            }
            catch (Exception e)
            {
                return "Could not load vehicle data";
            }
        }

        private async Task<string> fetchVehicleModel(dynamic vehicleID)
        {
            string vehMakeModel = "Could not load vehicle data";

            try
            {
                string vehMake = await _database.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleMake").OnceSingleAsync<String>();
                string vehModel = await _database.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleModel").OnceSingleAsync<String>();

                vehMakeModel = vehMake + " " + vehModel;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

            return vehMakeModel;
        }

        private async Task<string> fetchCustName(dynamic custID)
        {
            string fullName = "Could not load customer data";

            try
            {
                string firstName = await _database.Child($"Users/{BusinessID.businessId}/Customers/{custID}/CustomerName").OnceSingleAsync<String>();
                string surname = await _database.Child($"Users/{BusinessID.businessId}/Customers/{custID}/CustomerSurname").OnceSingleAsync<String>();

                fullName = firstName + " " + surname;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

            return fullName;
        }
    }
}
