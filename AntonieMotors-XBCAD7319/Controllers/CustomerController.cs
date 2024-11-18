using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Claims;
using AntonieMotors_XBCAD7319.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;



        public CustomerController(IConfiguration configuration)
        {
            string apiKey = configuration["Firebase:ApiKey"];
            string databaseUrl = configuration["Firebase:DatabaseUrl"];

            // Initialize Firebase objects
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            _firebaseClient = new FirebaseClient(databaseUrl);

            TestFirebaseConnection();
        }

        public async Task<IActionResult> TestFirebaseConnection()
        {
            try
            {
                var data = await _firebaseClient
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
        public async Task<IActionResult> Register(CustomerModel customer, string customerPassword, string confirmPassword)
        {
            customer.BusinessID = BusinessID.businessId;

            // Validate password confirmation
            if (string.IsNullOrEmpty(customerPassword) || customerPassword != confirmPassword)
            {
                ModelState.AddModelError("CustomerPassword", "Passwords do not match.");
                return View(customer);
            }

            // Clear errors and validate the model (excluding CustomerPassword)
            ModelState.Clear();
            TryValidateModel(customer);

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            // Validate phone number format
            if (!Regex.IsMatch(customer.CustomerMobileNum, @"^\d{10}$"))
            {
                ModelState.AddModelError("CustomerMobileNum", "Mobile number must be exactly 10 digits.");
                return View(customer);
            }

            // Check for existing email in Firebase Database
            var existingCustomers = await _firebaseClient
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
                // Register user in Firebase Authentication and retrieve UID
                var auth = await _authProvider.CreateUserWithEmailAndPasswordAsync(
                    customer.CustomerEmail, customerPassword, displayName: customer.CustomerName);

                var firebaseToken = auth.FirebaseToken;

                if (string.IsNullOrEmpty(firebaseToken))
                {
                    throw new Exception("Authentication failed. Token is null or empty.");
                }

                // Assign UID to CustomerID
                customer.CustomerID = auth.User.LocalId;

                // Save customer to Firebase Database (excluding password)
                string path = $"Users/{customer.BusinessID}/Customers/{customer.CustomerID}";
                await _firebaseClient.Child(path).PutAsync(customer);

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

        public async Task<IActionResult> SendPasswordResetEmail()
        {
            await _authProvider.SendPasswordResetEmailAsync(BusinessID.email);
            Console.WriteLine("Email sent successfully");
            return View("Account");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                string userId = BusinessID.userId;
                string businessId = BusinessID.businessId;

                // Delete customer data from Firebase
                await _firebaseClient.Child($"Users/{businessId}/Customers/{userId}").DeleteAsync();

                return Logout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to delete account. Please try again.";
                return RedirectToAction("Account", "Customer");
            }
        }

        public IActionResult Account()
        {
            return View();
        }

        public IActionResult Logout()
        {
            // Log out the user
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            // Redirect to a confirmation page or homepage
            return RedirectToAction("Index", "Home");
        }

        //public IActionResult Analytics()
        //{
        //    return View();
        //}

        //get list of Services
        private async Task getAllServices()
        {
            try
            {
                var services = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Services").OnceAsync<dynamic>();

                if (services == null || !services.Any())
                {
                    ViewBag.Services = new List<dynamic>();
                    return;
                }

                var serviceList = new List<dynamic>();

                foreach (var service in services)
                {
                    if ((string)service.Object.custID == BusinessID.userId) // Ensure custID is a string
                    {
                        // Fetching vehicle data
                        string vehicleModel = await fetchVehicleModel((string)service.Object.vehicleID);
                        string vehicleNumberPlate = await fetchVehicleNumPlate((string)service.Object.vehicleID);

                        // Initialize date variables with "N/A"
                        string dateTakenIn = "N/A";
                        string dateReturned = "N/A";

                        // Handle dateTakenIn if it's not null and has a time field (Unix timestamp)
                        if (service.Object.dateReceived != null && service.Object.dateReceived.time != null)
                        {
                            long dateTakenInLong = (long)service.Object.dateReceived.time; // Get Unix timestamp
                            DateTime dateTakenInDateTime = DateTimeOffset.FromUnixTimeMilliseconds(dateTakenInLong).DateTime; // Convert to DateTime
                            dateTakenIn = dateTakenInDateTime.ToString("dd MMMM yyyy");
                        }

                        // Handle dateReturned if it's not null and has a time field (Unix timestamp)
                        if (service.Object.dateReturned != null && service.Object.dateReturned.time != null)
                        {
                            long dateReturnedLong = (long)service.Object.dateReturned.time; // Get Unix timestamp
                            DateTime dateReturnedDateTime = DateTimeOffset.FromUnixTimeMilliseconds(dateReturnedLong).DateTime; // Convert to DateTime
                            dateReturned = dateReturnedDateTime.ToString("dd MMMM yyyy");
                        }

                        // Handling totalCost
                        string totalCost = $"R {service.Object.totalCost}";

                        // Add the service to the list
                        serviceList.Add(new
                        {
                            Name = (string)service.Object.name, // Cast name to string
                            Status = (string)service.Object.status, // Cast status to string
                            Model = vehicleModel,
                            NumberPlate = vehicleNumberPlate,
                            DateTakenIn = dateTakenIn,
                            DateReturned = dateReturned,
                            TotalCost = totalCost
                        });
                    }
                }

                // Set services in ViewBag
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
                return await _firebaseClient.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleNumPlate").OnceSingleAsync<String>();
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
                string vehMake = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleMake").OnceSingleAsync<String>();
                string vehModel = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleModel").OnceSingleAsync<String>();

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
                string firstName = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Customers/{custID}/CustomerName").OnceSingleAsync<String>();
                string surname = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Customers/{custID}/CustomerSurname").OnceSingleAsync<String>();

                fullName = firstName + " " + surname;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

            return fullName;
        }

        [HttpPost]
        public async Task<IActionResult> SendQuoteRequest(QuoteRequestModel model)
        {
            model.CustomerId = BusinessID.userId;

            try
            {
                // Fetch the customer's name
                var customerName = await _firebaseClient
                    .Child($"Users/{BusinessID.businessId}/Customers/{model.CustomerId}/CustomerName")
                    .OnceSingleAsync<string>();

                if (!string.IsNullOrEmpty(customerName))
                {
                    model.CustomerName = customerName; // Populate the model with the customer's name

                    // Construct path to save under the customer's name
                    var path = $"Users/{BusinessID.businessId}/QuoteRequests";

                    // Save the quote request to Firebase
                    var result = await _firebaseClient
                        .Child(path)
                        .PostAsync(model);

                    if (result != null)
                    {
                        Console.WriteLine("Data saved successfully to Firebase.");
                        TempData["SuccessMessage"] = "Request sent successfully.";
                    }
                    else
                    {
                        Console.WriteLine("Failed to save data to Firebase.");
                        TempData["ErrorMessage"] = "Failed to send the request. Please try again.";
                    }
                }
                else
                {
                    Console.WriteLine("Customer name not found.");
                    TempData["ErrorMessage"] = "Failed to fetch customer details. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to Firebase: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while sending the request.";
            }

            // Clear model state to reset the form fields
            ModelState.Clear();
            return View("QuoteGeneratorCust");
        }
    }
}

public class QuoteRequestModel
{
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CarMake { get; set; }
    public string CarModel { get; set; }
    public string Description { get; set; }
    public string PhoneNumber { get; set; }
}
