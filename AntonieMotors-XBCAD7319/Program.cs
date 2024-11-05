using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;

namespace AntonieMotors_XBCAD7319
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Retrieve the Firebase key path from environment variables
            var firebaseKeyPath = builder.Configuration["FIREBASE_KEY_PATH"];

            if (string.IsNullOrEmpty(firebaseKeyPath) || !File.Exists(firebaseKeyPath))
            {
                throw new FileNotFoundException("Firebase service account key file not found at the specified path: " + firebaseKeyPath);
            }

            // Initialize Firebase with the service account key
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebaseKeyPath)
            });


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
