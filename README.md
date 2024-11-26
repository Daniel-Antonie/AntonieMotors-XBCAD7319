Antonie Motors Web Application

Overview
The Antonie Motors Web Application is designed to streamline business operations and enhance customer experience. It provides role-based access control (RBAC) where:
•	Admins (Owners) have access to business and employee management tools, analytics, and service records.
•	Customers have access to their personal profiles, service updates, and payment history.
This web application is built using ASP.NET Core MVC with Firebase for backend services and database storage.

Key Features
1. Role-Based Access Control (RBAC)
•	Admins: Access employee data, business metrics, and all customer records.
•	Customers: Access only personal profiles, vehicle service updates, and payment receipts.
2. Login and Registration
•	Customers can register and log in to view service updates, payment history, and receipts.
•	Admins log in to manage the business, employees, and services as well as vehicles, invoices, quotes, vehicles and customer related data.
3. Admin Dashboard
•	View and manage client profiles and service records.
•	Manage employees, including schedules, roles, and assignments.
•	Manage Vehicles and customer information.
4. Business Information Hub (Landing Page)
•	Display details about Antonie Motors, including types of services, contact info, and locations.
5. Analytics Dashboard (Admin Only)
•	Track metrics such as services completed, pending, and service duration.
6. Customer Vehicle Tracking (Customers Only)
•	Real-time updates on vehicle service status, including milestones like “In Progress,” “Awaiting Parts,” or “Completed.”
7. Service History, Quotations, and Receipts
•	Track vehicle service history and generate/view quotations and receipts.

System Requirements
Software
•	IDE: Visual Studio 2022 or later.
•	Framework: .NET 8.
•	Database: Firebase (Realtime Database, Storage, and Authentication).
•	Version Control: Git.
Hardware
•	Processor: Minimum Dual-Core 2.0 GHz.
•	RAM: 4GB or higher (8GB recommended).
•	Storage: At least 500 MB free disk space.
•	OS: Windows 10 or later.

Setup Instructions
Step 1: Clone the Repository
1	Open Visual Studio.
2	Navigate to File > Clone Repository.
3	Copy and paste the repository URL:

https://github.com/Daniel-Antonie/AntonieMotors-XBCAD7319.git  

4	Select the desired local directory and click Clone.

Step 2: Restore Dependencies
1	Open the cloned solution in Visual Studio.
2	Go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution.
3	Install all missing dependencies.

Step 3: Configure Firebase
1	Navigate to appsettings.json.
2	Add your Firebase credentials:json

3 Copy code


       {  
	    "Firebase": {  
	        "ApiKey": "<YourApiKey>",  
	        "AuthDomain": "<YourAuthDomain>",  
	        "DatabaseURL": "<YourDatabaseURL>",  
	        "ProjectId": "<YourProjectId>",  
	        "StorageBucket": "<YourStorageBucket>"  
	    }  
	}  	


Step 4: Run the Application
1	Set the project as the startup project.
2	Press Ctrl+F5 to run the application.
3	Open the browser and navigate to http://localhost:<port>/.


Contributors
•	Perla Jbara
•	Daniel Antonie
•	Gabriella Janssen
•	Mauro Coelho
•	Lee Knowles


For any questions, feel free to contact us via the repository issues section.
