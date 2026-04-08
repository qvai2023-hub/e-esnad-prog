# CLAUDE.md - Project Instructions for Claude Code

## Development Workflow Rules (MANDATORY)

Every code change MUST follow this workflow — no exceptions:

### 1. Plan First — Never Code Without Approval
- Before making ANY code change, present a clear plan to the user that includes:
  - What will be changed and why
  - Which files will be affected
  - The approach and any trade-offs
- **Wait for the user's approval** before writing or modifying any code
- If the plan needs discussion, iterate on it until both sides agree

### 2. After Every Modification — Report Changed Files
- Immediately after completing a modification, provide a clear list of all changed files with:
  - Full file path
  - Brief description of what changed in each file
- This list is for the user to know exactly which files to upload/deploy

### 3. Provide Testing Instructions
- After every modification, include clear step-by-step instructions on how to test the change:
  - What to do (steps to reproduce/verify)
  - What the expected result should be
  - Any edge cases to check

### 4. Document Only After User Confirms Testing
- Do NOT update documentation (CHANGELOG, SPRINT-TRACKER, DECISIONS) until the user confirms the change has been tested and works
- Once the user confirms, update the relevant docs in `docs/`

### Workflow Summary
```
Plan → Discuss → Approve → Code → Report Files → Test Instructions → User Tests → Document
```

---

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

---

# Prompt Engineering Rules

These rules help Claude Code produce the best output with the fewest
API tokens — protecting quota and reducing cost.

---

## How to Write Prompts to Claude Code (For qv)

### Rule 1 — Always Name the File First
Before asking Claude to build anything, tell it which file to work on.

✅ GOOD:  "Open app/projects/page.js — add a search bar at the top"
❌ BAD:   "Add a search bar to the projects page"

Why: Without a filename, Claude reads multiple files to guess context.
That costs tokens before a single line of code is written.

---

### Rule 2 — Use the Phrase "Read CLAUDE.md first"
Start complex tasks with this exact phrase. It loads the design
system and coding standards in one shot.

✅ GOOD:  "Read CLAUDE.md first. Then build the notifications page."
❌ BAD:   "Build the notifications page" (Claude may ignore RTL rules)

---

### Rule 3 — Give Claude the Output Shape
Tell Claude exactly what you want returned.

✅ GOOD:  "Return only the updated function, not the full file"
✅ GOOD:  "Return the full file — I will replace mine"
❌ BAD:   (no instruction) — Claude guesses and may return too much

---

### Rule 4 — One Task Per Prompt
Split big requests into small steps. Each step uses fewer tokens
and is easier to review.

✅ GOOD:  Step 1: "Build the ProjectCard component (card only, no data)"
          Step 2: "Now connect it to the API call in lib/api.js"
❌ BAD:   "Build the projects page with cards, API, filtering, and RTL"

---

### Rule 5 — Reference Existing Files by Name
When a new file must match an existing pattern, name the example file.

✅ GOOD:  "Follow the same structure as app/intake/_components/IntakeForm.js"
❌ BAD:   "Follow the existing pattern" (Claude searches all files)

---

### Rule 6 — Use "Skip explanation" for Pure Code Tasks
When you just need the code, add this to the end of your prompt.
It removes the prose and saves 200–400 tokens per response.

✅ GOOD:  "Add loading spinner to the submit button. Skip explanation."
❌ BAD:   (no instruction) — Claude writes paragraphs before the code

---

### Rule 7 — Lock the Scope with "Do NOT touch other files"
Prevents Claude from refactoring files you did not ask about.

✅ GOOD:  "Add the filter tab to page.js only. Do NOT touch other files."
❌ BAD:   (no instruction) — Claude may rewrite components, layout, etc.

---

### Rule 8 — Bilingual String Shortcut
Instead of explaining the ar/en pattern every time, just write:

  "Use the standard TEXT constant pattern from CLAUDE.md"

Claude will apply the { ar: '...', en: '...' } object automatically.

---

### Rule 9 — For API + Page Tasks, Separate the Prompt
API route work and UI work use different mental models.
Split them or Claude mixes concerns.

✅ GOOD:  Prompt A: "Build POST /api/projects route in app/api/projects/route.js"
          Prompt B: "Build the UI in app/projects/page.js that calls that route"
❌ BAD:   "Build the projects API and the page that uses it"

---

### Rule 10 — The Golden Prompt Template

Copy this template for every new feature:

```
Read CLAUDE.md first.

Task: [what you want — one sentence]
File: [exact file path to create or edit]
Pattern to follow: [another file as reference, if any]
Output: [full file / updated function only / diff only]
Constraints: Do NOT touch other files. Skip explanation.
```

Example:
```
Read CLAUDE.md first.

Task: Add a status filter tab bar above the project cards
File: app/projects/page.js
Pattern to follow: app/intake/_components/IntakeForm.js (for RTL + TEXT constants)
Output: Full updated file
Constraints: Do NOT touch other files. Skip explanation.
```

---

## Token-Saving Checklist (run before every prompt)

- [ ] Did I name the exact file?
- [ ] Is this ONE task only?
- [ ] Did I say "Skip explanation" if I just need code?
- [ ] Did I say "Do NOT touch other files"?
- [ ] Did I reference an existing file as a pattern instead of re-explaining the rules?
