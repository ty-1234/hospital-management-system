# Hospital Management System

A full-stack ASP.NET Core hospital management system built with `.NET 9`, `MVC`, `Entity Framework Core`, `SQLite`, and `ASP.NET Identity`.
<img width="1915" height="923" alt="image" src="https://github.com/user-attachments/assets/75d79eae-52dd-4b16-90ea-af0253afc29c" />

## Features

- Authentication (Register/Login/Logout)
- Dashboard with key hospital metrics
- Department management
- Doctor management
- Patient management
- Appointment scheduling
- Billing and payment tracking
- Seeded default admin account
- REST-style API controllers for core modules

## Tech Stack

- ASP.NET Core MVC (`net9.0`)
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- Bootstrap + custom CSS

## Project Structure

- `HospitalManagementSystem.Core`:
  Domain entities and enums
- `HospitalManagementSystem.Infrastructure`:
  `ApplicationDbContext`, Identity user, seed data
- `HospitalManagementSystem.Web`:
  MVC controllers, API controllers, Razor views, static assets

## Prerequisites

- .NET SDK 9.0+
- PowerShell (Windows)

## Getting Started

1. Clone the repository:
```powershell
git clone https://github.com/ty-1234/hospital-management-system.git
cd hospital-management-system
```

2. Run the app:
```powershell
.\Start-HMS.ps1
```

The script stops any process already using the app ports and starts the web app on:

- `https://localhost:7099`
- `http://localhost:5099`

## Default Admin Login

- Email: `admin@hospital.local`
- Password: `Admin@12345`

## Database

- Provider: SQLite
- Connection string: `HospitalManagementSystem.Web/appsettings.json`
- Database file: `HospitalManagementSystem.Web/hospital_management.db`
- Seed data runs automatically on startup.

## Logging

- Development logging is configured to reduce EF Core SQL noise:
  - `Microsoft.EntityFrameworkCore.Database.Command = Warning`

## Common Commands

Build:
```powershell
dotnet build HospitalManagementSystem.sln
```

Run (manual, without helper script):
```powershell
dotnet run --project HospitalManagementSystem.Web/HospitalManagementSystem.Web.csproj --urls "https://localhost:7099;http://localhost:5099"
```

Trust HTTPS dev certificate (optional, one-time):
```powershell
dotnet dev-certs https --trust
```

## Notes

- If you see `address already in use`, another instance is already running on the same ports. Use `.\Start-HMS.ps1` to auto-resolve this.
- If browser assets look stale after UI updates, use hard refresh (`Ctrl+F5`).
