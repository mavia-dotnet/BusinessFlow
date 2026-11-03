# BusinessFlow — Business Management System

A business management web application built with **ASP.NET Core MVC, C#, Entity Framework Core, and SQL Server**.

BusinessFlow helps businesses manage customers, products, services, orders, invoices, and payments through a centralized web application.

## Live Demo

🌐 https://mavia13-001-site1.itempurl.com/

## Features

- **Authentication & Authorization** — ASP.NET Core Identity with role-based access.
- **Dashboard** — Business management dashboard.
- **Customer Management** — Manage customer records.
- **Product Management** — Manage business products.
- **Service Management** — Manage available services.
- **Service Requests** — Handle customer service requests.
- **Order Management** — Create and manage orders.
- **Invoice Management** — Generate and manage customer invoices.
- **Payment Management** — Record payments and track invoice payment status.
- **Customer Portal** — Customer dashboard, profile, orders, invoices, and payments.

## User Roles

- Admin
- Manager
- Staff
- Customer

Access to application features is controlled through role-based authorization.

## Technology Stack

| Technology | Purpose |
|---|---|
| C# | Application programming language |
| .NET 9 | Development platform |
| ASP.NET Core MVC | Web application framework |
| Entity Framework Core 9 | ORM and database access |
| SQL Server | Relational database |
| ASP.NET Core Identity | Authentication and authorization |
| Bootstrap | Responsive UI |
| HTML, CSS, JavaScript | Frontend development |
| Git & GitHub | Version control |

## Project Architecture

The application follows the ASP.NET Core MVC pattern:

- **Models** — Application entities and view models.
- **Views** — Razor views and user interfaces.
- **Controllers** — Handle HTTP requests and application workflows.
- **Data** — Database context and initialization.
- **Migrations** — Entity Framework Core database migrations.
- **wwwroot** — Static assets such as CSS, JavaScript, and libraries.

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server
- Visual Studio 2022 or compatible IDE
- Git

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/mavia-dotnet/BusinessFlow.git
   ```

2. Open the project folder:

   ```bash
   cd BusinessFlow
   ```

3. Restore dependencies:

   ```bash
   dotnet restore
   ```

4. Configure your database connection in `appsettings.json`.

   Use `appsettings.example.json` as the configuration template. Add your own database credentials locally. Never commit real credentials to GitHub.

5. Apply database migrations:

   ```bash
   dotnet ef database update
   ```

6. Run the application:

   ```bash
   dotnet run
   ```

## Database

BusinessFlow uses SQL Server with Entity Framework Core Code First migrations.

Database schema changes are maintained in the `Migrations` folder.

## Security

- Role-based authorization using ASP.NET Core Identity.
- Database credentials are excluded from the Git repository.
- Publishing profiles are excluded from version control.

## Repository

GitHub: https://github.com/mavia-dotnet/BusinessFlow

## Developer

**Mavia Abid**

GitHub: https://github.com/mavia-dotnet

---

*BusinessFlow — Manage Your Business Smarter.*