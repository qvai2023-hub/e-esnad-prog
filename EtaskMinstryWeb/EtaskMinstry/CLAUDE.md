# CLAUDE.md - Project Instructions for Claude Code

## Project Overview

**E-Esnad / Telesak** — A task management web application (E-Task Ministry) built with ASP.NET MVC 4 on .NET Framework 4.8. The application supports Arabic localization and both Hijri and Gregorian date systems.

## Tech Stack

- **Framework:** ASP.NET MVC 4 (.NET Framework 4.8)
- **ORM:** Entity Framework (Database-First via `.edmx` model in `TaskManagementModel/ETaskModel.edmx`)
- **Real-time:** SignalR 2.0
- **Reporting:** Microsoft ReportViewer (RDLC reports)
- **Authentication:** Custom session-based (no ASP.NET Identity)
- **Frontend:** jQuery 1.x, Bootstrap, Highcharts, FullCalendar, DataTables, Chosen.js, daterangepicker
- **Serialization:** Newtonsoft.Json 5.x
- **Language:** C# with Razor views (`.cshtml`)

## Project Structure

```
EtaskMinstryWeb/
├── EtaskMinstry/                  # Main MVC web project
│   ├── Areas/                     # MVC Areas (role-based modules)
│   │   ├── Admin/                 # Admin area (controllers, models, views)
│   │   ├── Common/                # Shared/common area
│   │   ├── Company/               # Company-level area
│   │   └── Employee/              # Employee-level area
│   ├── Areas2/                    # Secondary areas (same structure)
│   ├── AppCode/                   # Business logic & utilities
│   │   ├── TaskManger.cs          # Core task management logic
│   │   ├── TaskWorkflow.cs        # Task workflow/state transitions
│   │   ├── ServiceManger.cs       # External service integration
│   │   ├── QVSecurity.cs          # Security utilities
│   │   ├── Encryption.cs          # Encryption helpers
│   │   ├── FileManger.cs          # File upload/management
│   │   ├── Notification.cs        # Notification system
│   │   └── Mail.cs                # Email functionality
│   ├── Controllers/               # Root-level controllers
│   │   ├── BaseController.cs      # Base controller class
│   │   ├── SecurityController.cs  # Login/auth (default route)
│   │   ├── ReportsController.cs   # Report generation
│   │   └── ...
│   ├── Models/                    # View models and enums
│   ├── Views/                     # Razor views
│   ├── Services/                  # Service layer classes
│   ├── Scripts/                   # JavaScript files
│   ├── Content/                   # CSS and static assets
│   ├── App_Start/                 # Route, filter, WebAPI config
│   ├── App_GlobalResources/       # Localization resource files
│   ├── ReportsRDLC/               # RDLC report definitions
│   └── Web.config                 # App configuration
├── TaskManagementModel/           # Data access layer (EF Database-First)
│   ├── ETaskModel.edmx            # Entity Framework model
│   ├── ETaskModel.Context.cs      # DbContext
│   └── [Entity].cs                # Generated entity classes
└── EtaskMinstry-NewDesign.sln     # Solution file
```

## Architecture & Patterns

- **Areas-based routing:** Each user role (Admin, Company, Employee) has its own Area with separate controllers, models, and views
- **Each Area** has an `AreaRegistration.cs` file for route registration
- **Database-First EF:** Models are generated from `ETaskModel.edmx` — do NOT edit `.cs` files under `TaskManagementModel/` that are auto-generated (`.tt` files)
- **Session-based auth:** User data stored in `HttpContext.Session["User"]` via `MvcApplication.userData`
- **AppCode folder:** Contains core business logic (task management, workflows, notifications, security)
- **Services folder:** Higher-level service classes (`TasksService.cs`, `SharedService.cs`, etc.)

## Key Configuration

- **Connection string:** Named `ETaskEntities` in Web.config
- **WebAPI backend:** Configured via `WebAPIPath` in appSettings
- **Date system:** Controlled by `ISGreg` setting (1 = Gregorian, 0 = Hijri)
- **Application name:** Set via `ApplicationName` in appSettings (currently "Telesak")
- **Default route:** `Security/login` (SecurityController)

## Coding Conventions

- **Namespace:** `EtaskMinstry` (root), `EtaskMinstry.Areas.[AreaName].Controllers/Models`
- **Controller naming:** `[Name]Controller.cs`
- **File naming:** PascalCase for C# files
- **Views:** Organized by controller name under each area's `Views/` folder
- **JavaScript:** Stored in `Scripts/` with area-specific subfolders (e.g., `Scripts/Employee/`)
- **CSS:** Stored in `Content/Main/` and `Content/Site/`
- **Arabic UI:** The application uses Arabic text extensively — preserve Arabic strings and RTL layout conventions

## Important Notes

- Do NOT modify auto-generated Entity Framework files (`.Designer.cs`, files from `.tt` templates)
- When adding new entities, update the `.edmx` model first
- The `Web.config` contains environment-specific settings — be cautious with connection strings and server URLs
- The `BaseController.cs` likely contains shared logic — extend it for new controllers
- SignalR is used for real-time notifications (configured in `Startup.cs`)
- Report files are in `.rdlc` format under `ReportsRDLC/`
- The solution has config variants: `webesnad.config`, `webuat.config`, `webtele.config` at the repo root
