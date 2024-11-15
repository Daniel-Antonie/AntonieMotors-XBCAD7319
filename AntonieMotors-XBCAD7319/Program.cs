using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AntonieMotors_XBCAD7319
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

           // var builder = WebApplication.CreateBuilder(args);

            // Load secrets.json
            builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

           // var app = builder.Build();


            // Create logger instance after building the app
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            try
            {
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

                logger.LogInformation("Firebase initialized successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Firebase.");
                throw; // Re-throw to allow the application to stop, or handle accordingly.
            }

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
