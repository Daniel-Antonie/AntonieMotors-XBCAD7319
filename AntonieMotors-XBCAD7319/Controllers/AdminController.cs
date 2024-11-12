using Firebase.Auth;
using Firebase.Database;
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
            _authProvider = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyDJxhod4pFGkhUP_Hn3wHI2b3hOiI_dpiY"));
            _firebaseClient = new FirebaseClient("https://antonie-motors-default-rtdb.firebaseio.com/");
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


                //counting pending services
                int nonCompletedServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() != "Completed");

                ViewBag.NumServicesPending = nonCompletedServicesCount;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
            
        }
    }
}
