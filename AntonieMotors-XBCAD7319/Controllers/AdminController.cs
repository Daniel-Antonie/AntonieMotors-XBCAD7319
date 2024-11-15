using AntonieMotors_XBCAD7319.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class AdminController : Controller
    {

        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;
        string businessId = BusinessID.businessId;



        public AdminController()
        {
            _authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("Firebase:ApiKey"));
            _firebaseClient = new FirebaseClient("Firebase:DatabaseUrl");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QuoteGenerator()
        {
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
        public IActionResult InventoryManagement()
        {
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
                    .Where(e => e.Object.ManagerID == managerId)
                    .Select(e => new EmployeeModel
                    {
                        EmployeeID = e.Key,
                        FirstName = e.Object.FirstName,
                        LastName = e.Object.LastName,
                        Email = e.Object.Email,
                        Phone = e.Object.Phone,
                        ManagerID = e.Object.ManagerID
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



        public async Task<IActionResult> EditEmployee(string id)
        {
            string businessId = BusinessID.businessId;

            // Fetch the employee details from Firebase
            var employee = await GetEmployeeByIdAsync(businessId, id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        [HttpPost]
        public async Task<IActionResult> EditEmployee(EmployeeModel model)
        {
            string businessId = BusinessID.businessId;

            try
            {
                // Update the employee data in Firebase
                await _firebaseClient
                    .Child($"Users/{businessId}/Employees/{model.EmployeeID}")
                    .PutAsync(model);

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
    }
}
