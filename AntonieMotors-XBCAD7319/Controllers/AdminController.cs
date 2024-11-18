using AntonieMotors_XBCAD7319.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class AdminController : Controller
    {

        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;
        string businessId = BusinessID.businessId;



        public AdminController(IConfiguration configuration)
        {
            string apiKey = configuration["Firebase:ApiKey"];
            string databaseUrl = configuration["Firebase:DatabaseUrl"];

            // Initialize Firebase objects
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            _firebaseClient = new FirebaseClient(databaseUrl);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> QuoteGenerator()
        {
            await fetchQuoteRequests();
            return View();
        }

        public IActionResult CustomerManagement()
        {
            return View();
        }

        public IActionResult VehicleManagement()
        {
            return View();
        }
        public async Task<IActionResult> InventoryManagement()
        {
            await fetchInventory();
            return View();
        }

        public async Task<IActionResult> EmployeeManagement()
        {
            string businessId = BusinessID.businessId; // Replace with the logged-in user's business ID
            string managerId = BusinessID.userId; // Replace with logged-in user's manager ID

            Console.WriteLine($"BusinessID: {businessId}, ManagerID: {managerId}");

            var employees = await GetEmployeesAsync(businessId, managerId);

            return View(employees);
        }


        private async Task<List<EmployeeModel>> GetEmployeesAsync(string businessId, string managerId)
        {
            try
            {
                // Fetch employees under the businessId
                var employees = await _firebaseClient
                    .Child($"Users/{businessId}/Employees")
                    .OnceAsync<EmployeeModel>();

                Console.WriteLine($"Fetched {employees.Count} employees from Firebase.");

                // Filter employees by managerId
                var filteredEmployees = employees
                    .Where(e => e.Object.managerID == managerId)
                    .Select(e => new EmployeeModel
                    {
                        EmployeeID = e.Key,
                        firstName = e.Object.firstName,
                        lastName = e.Object.lastName,
                        email = e.Object.email,
                        phone = e.Object.phone,
                        managerID = e.Object.managerID
                    })
                    .ToList();

                Console.WriteLine($"Filtered {filteredEmployees.Count} employees for ManagerID: {managerId}");
                return filteredEmployees;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error fetching employees: {ex.Message}");
                return new List<EmployeeModel>();
            }
        }



        [HttpPost]
        public async Task<IActionResult> EditEmployee(EmployeeModel model, IFormFile ProfileImage)
        {
            string businessId = BusinessID.businessId;

            try
            {
                // Fetch existing employee data to avoid overwriting other properties
                var existingEmployee = await GetEmployeeByIdAsync(businessId, model.EmployeeID);
                if (existingEmployee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found!";
                    return RedirectToAction("EmployeeManagement");
                }

                // Update properties from the form model
                existingEmployee.firstName = model.firstName;
                existingEmployee.lastName = model.lastName;
                existingEmployee.email = model.email;
                existingEmployee.phone = model.phone;
                existingEmployee.managerID = model.managerID;



                // Update the employee data in Firebase with the modified data
                await _firebaseClient
                    .Child($"Users/{businessId}/Employees/{model.EmployeeID}")
                    .PutAsync(existingEmployee);

                TempData["SuccessMessage"] = "Employee details updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating employee: {ex.Message}";
            }

            return RedirectToAction("EmployeeManagement");
        }



        private async Task<EmployeeModel> GetEmployeeByIdAsync(string businessId, string employeeId)
        {
            try
            {
                var employee = await _firebaseClient
                    .Child($"Users/{businessId}/Employees/{employeeId}")
                    .OnceSingleAsync<EmployeeModel>();

                return employee;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employee: {ex.Message}");
                return null;
            }
        }



        public async Task<IActionResult> AnalyticsAsync()
        {
            //fetch analytics data to display
            ViewBag.NumServicesCompleted = null;
            ViewBag.NumServicesPending = null; //im including both "not started" and "busy" services here

            await getServicesAnalytics();

            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        //fetches analytics data for services
        private async Task getServicesAnalytics()
        {
            //counting completed services
            try
            {
                // Retrieve all services under the given path
                var services = await _firebaseClient
                    .Child($"Users/{businessId}/Services")
                    .OnceAsync<dynamic>();

                // Count services with status set to "Completed"
                int completedServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() == "Completed");

                ViewBag.NumServicesCompleted = completedServicesCount;


                //counting busy services
                int busyServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() == "Busy");

                ViewBag.NumServicesBusy = busyServicesCount;

                //counting not started services
                int notStartedServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() == "Not Started");

                ViewBag.NumServicesNotStarted = notStartedServicesCount;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

        }

        private async Task fetchQuoteRequests()
        {
            try
            {
                var quoteRequestsResponse = await _firebaseClient
                    .Child($"Users/{BusinessID.businessId}/Customers")
                    .OnceAsync<dynamic>();

                var quoteRequestsList = new List<dynamic>();

                foreach (var customer in quoteRequestsResponse)
                {
                    var customerQuotes = customer.Object.QuoteRequests;

                    if (customerQuotes != null)
                    {
                        foreach (var quoteRequest in customerQuotes)
                        {
                            quoteRequestsList.Add(new
                            {
                                CustomerName = customer.Object.CustomerName,
                                CarMake = quoteRequest.Value.CarMake,
                                CarModel = quoteRequest.Value.CarModel,
                                Description = quoteRequest.Value.Description,
                                PhoneNumber = quoteRequest.Value.PhoneNumber,
                                CustomerId = customer.Key // Customer ID is the key here
                            });
                        }
                    }
                }

                ViewBag.quoteRequests = quoteRequestsList;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDone(string customerId)
        {
            try
            {
                // Define the path to the customer's quote requests
                var path = $"Users/{BusinessID.businessId}/Customers/{customerId}/QuoteRequests";

                // Delete the customer's quote request
                await _firebaseClient.Child(path).DeleteAsync();

                // Refresh the list of quote requests after deletion
                await fetchQuoteRequests();

                TempData["SuccessMessage"] = "Quote request marked as done.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while marking as done: {ex.Message}";
            }

            return RedirectToAction("QuoteGenerator");
        }

        private async Task fetchInventory()
        {
            try
            {
                var inventoryResponse = await _firebaseClient.Child($"Users/{BusinessID.businessId}/parts").OnceAsync<dynamic>();

                var inventoryList = new List<dynamic>();

                foreach (var part in inventoryResponse)
                {
                    inventoryList.Add(new
                    {
                        partName = part.Object.partName,
                        costPrice = part.Object.costPrice,
                        stockCount = part.Object.stockCount
                    });
                }

                ViewBag.inventory = inventoryList;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
        }

        public IActionResult Logout()
        {
            // Log out the user
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            // Redirect to a confirmation page or homepage
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SendPasswordResetEmail()
        {
            await _authProvider.SendPasswordResetEmailAsync(BusinessID.email);
            Console.WriteLine("Email sent successfully");
            return RedirectToAction("Account", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                string userId = BusinessID.userId;
                string businessId = BusinessID.businessId;

                // Delete customer data from Firebase
                await _firebaseClient.Child($"Users/{businessId}/Employees/{userId}").DeleteAsync();

                return Logout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to delete account. Please try again.";
                return RedirectToAction("Account", "Admin");
            }
        }

        public IActionResult Account()
        {
            return View();
        }
    }
}
