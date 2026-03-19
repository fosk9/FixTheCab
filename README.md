# PMPRacing

PMPRacing is an ASP.NET Core 8 Web Application designed to manage different aspects of a racing or automotive service business. The application provides dedicated portals and role-based access for various user types including Admins, Managers, Cashiers, and Mechanics.

## 🚀 Features

- **Role-Based Portals**: Dedicated controllers and views for different user groups:
  - `AdminsController`: Administrative tools and system oversight.
  - `ManagersController`: Management of business operations.
  - `CashiersController`: Handling of transactions and payments.
  - `MechanicsController`: Management of technical tasks and work orders.
- **Real-Time Updates**: Utilizes SignalR (`AdminAccountsHub`) for real-time notifications and administrative updates.
- **Authentication & Security**: 
  - Cookie-based authentication with 8-hour sliding expiration.
  - Secure password hashing using `BCrypt.Net-Next`.
- **Database**: Entity Framework Core with SQL Server for robust data management and migrations.
- **Email Services**: Built-in support for SMTP email services and APIs for notifications (e.g., OTP emails).
- **Caching**: Leverages `MemoryCache` for transient data storage such as OTP validation.

## 🛠️ Technology Stack

- **Framework**: .NET 8 (ASP.NET Core MVC)
- **Database ORM**: Entity Framework Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Real-Time Framework**: SignalR
- **Security**: Cookie Authentication, BCrypt.Net-Next (v4.1.0)
- **Frontend**: Razor Views, static files hosted in `wwwroot`.

## ⚙️ Configuration Setup

Configure your environment settings in `appsettings.json` or `appsettings.Development.json`:

1. **Database Connection**: 
   Ensure `ConnectionStrings:DefaultConnection` correctly points to your SQL Server instance.
2. **Email Settings**:
   Configure the `Email` section in the settings to enable the `SmtpEmailService`.

## 🏃 Getting Started

1. **Clone the repository.**
2. **Setup the Database:**
   Run Entity Framework Core migrations to set up your SQL Server database schemas:
   ```bash
   dotnet ef database update
   ```
3. **Run the Application:**
   ```bash
   dotnet run
   ```
4. **Access the App:**
   The default route directs to the login page: `/Accounts/Login`.

## 📂 Project Structure Overview

- `Controllers/`: Handles incoming HTTP requests and routing logic for the various portals.
- `Models/` & `ViewModels/`: Entity models for EF Core shaping the business logic, and view models for handling data transfer to Razor views.
- `Views/`: Razor pages and UI templates.
- `Hubs/`: SignalR hubs for pushing real-time events.
- `Services/`: Core business logic modules including Email service providers.
- `wwwroot/`: Static web assets containing CSS, JS, and image files.
