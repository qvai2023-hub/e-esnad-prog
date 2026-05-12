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

## Bug Fixing Workflow (MANDATORY)

Every bug fix MUST follow this workflow — no exceptions.
Four roles guide the process from report to resolution:

### 1. Bug Investigator

Before any fix, investigate and report:
- **Exact file + line:** Identify the precise location of the bug in code
- **Root cause:** Explain WHY the bug happens — not just the symptom
- **Blast radius:** List what else could break if this code is changed (other features, areas, shared logic)

### 2. Bug Analyst

Produce a diagnosis report that includes:
- **Severity:** Critical / High / Medium / Low (see reference table below)
- **Affected users:** Which roles are impacted (Admin, Company, Employee, all)
- **Fix risk level:**
  - **Safe** — isolated change, only affects the reported bug
  - **Risky** — touches shared code (BaseController, ServiceManger, TaskManger.cs, etc.)

### 3. Bug Fixer

Only after explicit approval:
- Apply a **minimal fix only** — solve the bug and nothing else
- Do NOT refactor nearby code
- Do NOT change any behavior outside the bug scope
- Do NOT add features, improvements, or "while I'm here" changes

### 4. Bug Verifier

After the fix is applied, provide:
- **Repro steps:** Exact steps to confirm the bug is gone
- **Regression checklist:** List of related features to re-test to ensure nothing else broke

### Bug Fixing Workflow Summary
```
Bug Report → Investigate → Diagnosis Report → Approve → Minimal Fix → Report Files → Repro Test → Regression Test → Document
```

### Golden Bug Prompt Template

Copy this template for every bug fix:

```
Read CLAUDE.md first.

BUG: [one sentence describing what is wrong]
Repro steps: [how to trigger it]
Expected: [what should happen]
Actual: [what happens instead]
Suspected file: [file path if known, or "unknown"]

Role: Bug Investigator — do NOT fix yet.
Output: Root cause analysis only. Wait for my approval.
```

### Bug Severity Reference

| Severity | Definition | Examples |
|----------|------------|----------|
| **Critical** | App crash / data loss / all users blocked | Server error page, database corruption, login broken |
| **High** | Feature broken for most users, no workaround | Save button stuck, report crashes, task workflow broken |
| **Medium** | Feature broken but workaround exists | Email not sending (user can reset manually), time entry mismatch |
| **Low** | Visual/cosmetic issue, no functional impact | Duplicate column, unclear priority indicator, misaligned report |

### Bug Fixing Rules

1. **Never fix more than what the bug report describes.**
   The bug report is the scope. If you notice other issues nearby, open a separate task — do not fix them in the same change.

2. **Never refactor while fixing a bug — open a separate task for that.**
   Bug fixes must be minimal and reviewable. Mixing refactoring with bug fixes makes it impossible to verify the fix in isolation.

3. **If the fix touches shared code — flag it as HIGH RISK.**
   Shared files include: `BaseController.cs`, `ServiceManger.cs`, `Notification.cs`, `TaskManger.cs`, `TaskWorkflow.cs`, `QvLib.cs`, `UnitOfWork.cs`, `GenericRepository.cs`.
   These files affect multiple features across all areas. Require **explicit approval** before proceeding with any change to these files.

### Anti-Reopen Rules (Lessons Learned)

These rules exist to prevent bugs from being reopened. Every rule was learned from a real reopen.

4. **Match the exact page, role, and area the tester reported.**
   Read the bug report repro steps carefully. If the tester says "company account المهام page", fix THAT page — not the Admin page, not the Employee page. Confirm: which URL? Which user role? Which area controller?

5. **Fix ALL areas — not just one.**
   If a bug exists in the Company area, check the Admin and Employee areas for the same pattern — they often share the same broken logic. `Areas2/` is NOT used in production — do NOT apply fixes there unless explicitly asked.

6. **Never change logic in shared code without tracing the full call chain.**
   Before modifying a method in `TaskManger.cs`, `TaskWorkflow.cs`, or any shared file:
   - Find ALL controllers that call this method
   - Find ALL JS/views that call those controllers
   - Understand the complete request flow (button click → JS → controller → method → DB)
   - If the method is called by multiple flows, your change may break one while fixing another

7. **CSS: verify the selector matches the exact HTML element on the exact page.**
   Before writing a CSS rule, inspect the actual HTML of the page the bug is on. Don't assume — check class names, parent elements, and whether the element is inside a `<table>`, `<div>`, or `<form>`. A selector like `table .btn.active` will NOT work if the button is inside a `<div>`.

8. **RDLC: align EVERY element to the tablix column grid.**
   When fixing Excel export column spanning, check ALL elements — not just the body. PageHeader, PageFooter, and body items ALL contribute to the Excel column grid. Every element's `Left` and `Left + Width` must snap to a tablix column boundary. Also ensure `PageWidth` = `BodyWidth` + margins.

9. **After fixing, re-read the bug repro steps and mentally walk through the fix.**
   Ask yourself: "If the tester follows these exact steps, will my fix actually change what they see?" If you can't answer yes with confidence, investigate more before committing.

10. **Close every fixed bug on GitHub with a comment.**
    After committing and pushing a fix, ALWAYS:
    - Add a comment on the GitHub issue explaining the root cause, files changed, and test instructions
    - Close the issue with state `closed` and reason `completed`
    - If the fix requires infrastructure work (not code), leave a comment explaining what the team needs to do and close with a note

---

### Anti-Failure Rules for Bug Fixes

11. **Before touching any method — grep ALL callers first.**
    Run: grep -rn "MethodName" across the entire codebase.
    List every file that calls this method. For each caller,
    check if changing the return type or behavior will break it.
    If any caller is in a different context (Dashboard, Report,
    API), STOP and flag it before writing code.

12. **Before writing the fix — state the exact file path AND
    the exact URL that produces the bug.**
    Confirm: which area registration handles this URL?
    Open the AreaRegistration.cs for that area and verify the
    route. Never assume the URL maps to the area you expect.

13. **After finding one broken pattern — search for all
    similar patterns in the same file.**
    Example: found CheckForUniqueName missing IsDeleted?
    Search the ENTIRE file for every other Check* or Validate*
    method and verify each one. Do not commit until all
    variants are fixed.

14. **After writing the fix — walk through the exact repro
    steps from the bug report line by line.**
    Ask: "If the tester follows step 1, 2, 3 right now,
    what is different? Is the bug gone?" If you cannot answer
    YES with certainty, investigate more before committing.

15. **Never change a server method's return type or response
    format to fix a JS display bug.**
    If the bug is visual (DOM not updating), the fix belongs
    in the JS callback — not in the server method. Server
    methods are shared; JS callbacks are not.

16. **Before starting any fix — ask the user for explicit
    confirmation.**
    After delivering the diagnosis report, always output:

    ```
    Ready to fix Bug #[N].

    Fix summary: [one sentence describing what will change]
    Risk level: [Safe / Risky]
    Files that will change: [list]

    Shall I proceed with the fix?
    ```

    Do NOT write a single line of code until the user replies
    with explicit approval ("yes", "proceed", "go ahead").
    If the user says "investigate only", stop at the diagnosis
    report — never suggest a fix unless asked.

17. **After fixing — if ANY logic was changed, leave a
    dedicated tester warning on the GitHub issue.**
    A "logic change" means: a condition was added or removed,
    a method's behavior was altered, a workflow transition
    was modified, or a validation rule was changed.
    CSS-only, RDLC layout, or config-only fixes do NOT require
    this rule — Rule 10's standard comment is sufficient.

    If ANY logic changed, the GitHub comment MUST include
    this section written in the tester's language (Arabic
    if the tester writes in Arabic):

    ```
    ⚠️ تنبيه للمختبِر — تغيير في المنطق:

    ما الذي تغيّر في سلوك النظام:
    [اشرح بلغة بسيطة ماذا يفعل النظام الآن بشكل مختلف]

    ما يجب اختباره تحديداً بسبب هذا التغيير:
    1. [سيناريو الاختبار 1]
    2. [سيناريو الاختبار 2]

    ما يجب أن يبقى كما هو (فحص الانحدار):
    - [الميزة 1 التي يجب أن تعمل كالمعتاد]
    - [الميزة 2 التي يجب أن تعمل كالمعتاد]
    ```

---

### Pre-Fix Checklist (Mandatory Before Any Bug Fix)

Before writing any code for a bug fix, output this checklist
and wait for explicit user confirmation:

```
Before fixing #[N]:
✅ Exact URL from bug report: [url]
✅ Area/Controller confirmed by AreaRegistration: [file]
✅ All callers of the method being changed: [list]
✅ All similar patterns in the same file audited: [yes/no]
✅ Areas2 mirror checked: [yes/no]
Proceed?
```

---

### Magic Phrases — Force Correct Behavior at Key Moments

| When | Phrase to add to prompt |
|------|------------------------|
| Before fixing shared code | "Before changing this method, grep ALL callers across the entire codebase and list them." |
| Before fixing an area bug | "State the exact URL from the bug report and confirm which AreaRegistration.cs handles it before touching any file." |
| When fixing a Check* method | "After fixing this method, search the same file for ALL other Check* or Validate* methods and verify each one has the same fix." |
| After writing the fix | "Walk through the tester's exact repro steps one by one and explain what is different after your fix." |
| When the fix seems done | "List every file this change could affect, including Areas2 mirrors. Have you checked all of them?" |

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
│   ├── Api/                       # Mobile API layer (NEW — see "Mobile API Layer" section)
│   │   ├── Configuration/         # JSON formatter + CORS for /api/v1/
│   │   ├── Controllers/           # JWT-gated thin controllers
│   │   ├── Dtos/                  # Hand-written POCOs (never EF entities)
│   │   ├── Filters/               # JwtAuthorize, ApiResponse wrapper, ApiException
│   │   ├── Mapping/               # entity → DTO mappers
│   │   └── Services/              # JWT issue/validate, refresh tokens, FCM
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

- `Areas2/` folder exists in the repo but is NOT used in production — all fixes go to `Areas/` only
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

---

# Mobile API Layer

Added in 2026 to serve a future mobile app. All Mobile API code lives under
`EtaskMinstry/Api/` and ships in the SAME project as the web. The web app
and Mobile API share the SAME database and the SAME `AppCode` / `Services`
business logic.

## Non-negotiable rules for the Mobile API

1. **Sync only** — no `async`/`await`. Matches the rest of the project.
2. **EF6 Database-First stays untouched** — do not edit `.edmx` or any
   auto-generated entity files. Direct ADO.NET (`SqlConnection`,
   parameterized SQL) is the approved escape hatch for the two new
   tables (`MobileDeviceToken`, `RefreshToken`).
3. **Newtonsoft.Json 5.x only** — never `System.Text.Json`.
4. **NO new business logic in Mobile API code.** API controllers MUST
   call existing AppCode methods (`TaskWorkflow.ChangeTaskStatus`,
   `TaskManger.EmpUpdateTaskTime`, `UserAccountVM.Checkin`, etc.).
5. **NO modifications to existing tables.** The Mobile API adds exactly
   two tables: `MobileDeviceToken` and `RefreshToken`. See
   `Telesak-Docs/sql/mobile-api-tables.sql`.
6. **NO changes to existing controllers, views, AppCode, Services, or
   Areas — except** one explicitly-allowed FCM line inside
   `AppCode/Notification.cs` (Slice 6). Anything else must be done as a
   wrapper or new method beside the existing one.
7. **Web behavior MUST remain unchanged.** Every regression in the web
   means stop, revert, redesign.

## Reuse, don't rewrite — overwrite vs. extension policy

When the Mobile API needs a variant of an existing method:

- ✅ **Call existing method directly** if no variation is needed.
- ✅ **Write a new method beside the old one** (same file or
  `Api/Services/`) when you need a variant. Naming convention:
  `LoginETaskForApi`, `CheckinForApi`, etc.
- ✅ **Trigger from a shared call site** (e.g. one FCM line inside
  `NotificationHub.Send` benefits both web and API).
- ❌ **DO NOT overwrite an existing method body.** This is a HIGH-RISK
  shared-code change per bug-fix Rule 3 and requires explicit user
  approval with rationale.

If a wrapper would mean copy-pasting more than ~15 lines of business
logic, stop and surface it as an open question — that's the line where
"wrapper" becomes "fork".

## Architecture

```
Mobile Client
     ↓ HTTPS + JWT Bearer
┌──────────────────────────────────────────┐
│ Api/Controllers/   ← THIN (30-50 lines)  │
│  - extract JWT claims via JwtAuthorize    │
│  - call existing AppCode/Services         │
│  - map entity → DTO via Api/Mapping/      │
│  - return ApiResponse<T> / PagedResponse  │
└──────────────────────────────────────────┘
     ↓ direct C# calls (no HTTP)
┌──────────────────────────────────────────┐
│ AppCode/, Services/, Models/   (UNCHANGED)│
└──────────────────────────────────────────┘
     ↓
SQL Server (same DB as web)
```

## Roles & identity model

The Mobile API serves two roles only — the same two the web app serves
to end users:

| JWT claim `userTypeId` | LoggedUserType enum | Role     | Notes |
|------------------------|--------------------|----------|-------|
| `2`                    | Employee           | Employee | Task lifecycle: accept, reject, complete, log time |
| `3`                    | Company            | Company  | Task creation, approve, disapprove, soft-delete; sees employee list |

Admin (`userTypeId = 1`) is **NOT exposed via the Mobile API** — admin
functions stay on the web. Anyone attempting to log in to the API with
an admin account is rejected as 401.

JWT claim set (RFC 7519):

| Claim         | Type       | Meaning |
|---------------|-----------|---------|
| `sub`         | string    | user ID (employee ID or company ID) |
| `userTypeId`  | int       | 2 = Employee, 3 = Company |
| `companyId`   | int?      | the user's company ID (employee → their employer's id; company → self) |
| `name`        | string    | display name |
| `iss`         | string    | "Telesak" / "E-snad" (per env) |
| `iat`, `exp`  | long      | Unix-seconds issued-at / expiry |
| `jti`         | string    | random id, prevents token caching |

`JwtAuthorizeAttribute` validates the token and populates
`MvcApplication.userData` from these claims, so any downstream call
into AppCode that reads `MvcApplication.userData` (e.g.
`NotificationHub.Send`'s sender-skip guard) continues to work
identically for both web (session) and API (JWT) callers.

## Standard response envelope

Every Mobile API endpoint returns one of:

```json
{ "success": true, "data": <obj|array|null>, "message": "", "errors": [] }
```

Paginated lists also include `page`, `pageSize`, `totalCount`, `totalPages`.
Errors are Arabic-only. Field names are camelCase (Newtonsoft
`CamelCasePropertyNamesContractResolver` applied to the Web API
JsonFormatter only — MVC's `Json()` action result stays PascalCase).

Dates are ISO 8601 Gregorian (`2026-05-11T14:30:00Z`). Hijri display
is the mobile client's responsibility.

## Routes & versioning

- All Mobile API routes are under `/api/v1/`.
- Routes are declared via attribute routing
  (`[RoutePrefix("api/v1/...")]` + `[Route("...")]`). Attribute routing
  is enabled in `App_Start/WebApiConfig.cs`; the existing convention
  route `api/{controller}/{id}` is kept intact for any non-versioned
  Web API endpoints.

## Performance standards (same as web)

All 7 performance rules from this CLAUDE.md apply to API code:
no `.ToList()` before filtering, always use `includeProperties`, no N+1
in loops, no `GetByID()` inside loops, `isDelayed` rule, `GetByID()` does
not support `includeProperties`, named lambda parameters
(`Get(filter: ...)`).

## What is OUT OF SCOPE for the Mobile API

Do NOT add: reports / PDF / RDLC endpoints, bulk operations, forgot
password / reset password endpoints, admin endpoints, activity log /
audit feed endpoints, file upload endpoints, offline / sync logic,
Hijri date conversion in API responses, bilingual error messages
(Arabic only), TenantId concept (single tenant), Swagger in
production (optional in dev only).

## Routing model

WebApi version in this project is 1.0 (`Microsoft.AspNet.WebApi 4.0.20710.0`).
Attribute routing (`[RoutePrefix]` / `[Route]`) was added in WebApi 2 and is
NOT available. The Mobile API therefore uses **convention routing** in
`App_Start/WebApiConfig.cs`:

```
api/v1/me                                  → MeController.Get (explicit)
api/v1/{controller}/{action}/{id}          → action-named convention
api/{controller}/{id}                      → existing default route, untouched
```

Implications when adding a new endpoint:

- Controller class names drop the "Api" suffix so `{controller}` matches the
  desired URL segment: `AuthController` not `AuthApiController` (file name
  may keep "Api" but class name must match the route token).
- The action name appears in the URL: `POST /api/v1/auth/login` →
  `AuthController.Login`. For dashed paths use `[ActionName("change-password")]`.
- For ergonomic URLs without an action segment (like `/api/v1/me`), add an
  explicit `MapHttpRoute` BEFORE the generic action route.

## Files added in Slice 1 (foundation)

- `Api/Configuration/{ApiJsonFormatter,ApiCorsConfig}.cs`
- `Api/Filters/{JwtAuthorizeAttribute,ApiResponseFilter,ApiExceptionFilter}.cs`
- `Api/Dtos/Common/{ApiResponse,PagedResponse,ErrorItem}.cs`
- `Api/Services/{JwtIssuer,JwtValidator,RefreshTokenService,RefreshTokenCleanupJob}.cs`
- `Telesak-Docs/sql/mobile-api-tables.sql`  *(creates `MobileDeviceToken`, `RefreshToken`)*

Existing files edited in Slice 1 (additive only):
- `App_Start/WebApiConfig.cs` — add `/api/v1/` convention route + camelCase formatter + register API filters
- `Global.asax.cs` — start/stop `RefreshTokenCleanupJob`
- `Web.config` + `webesnad.config` + `webuat.config` + `webtele.config` — add 5 appSettings keys (`FcmServerKey`, `JwtSecret`, `JwtIssuer`, `JwtAccessExpiryMinutes`, `JwtRefreshExpiryDays`)

## Files added in Slice 6 (notifications + FCM)

- `Api/Controllers/NotificationsController.cs` — `GET /api/v1/notifications`, `POST /api/v1/notifications/{id}/read`, `POST /api/v1/notifications/read-all`
- `Api/Controllers/DeviceTokensController.cs` — `POST /api/v1/device-tokens`, `DELETE /api/v1/device-tokens/{token}`
- `Api/Dtos/Notifications/{NotificationDto,DeviceTokenRequest}.cs`
- `Api/Mapping/NotificationMapper.cs`
- `Api/Services/DeviceTokenService.cs` — direct ADO.NET CRUD over `MobileDeviceToken`
- `Api/Services/FcmDispatcher.cs` — fire-and-forget FCM HTTP POST

Existing files edited in Slice 6:
- `AppCode/Notification.cs` — **one line** inside `NotificationHub.Send` (after the existing `HubContext.Clients.Clients(...)` SignalR call) calling `FcmDispatcher.Dispatch`. Wrapped in try/catch so any FCM failure cannot disrupt the web's SignalR path.
- `Api/Controllers/AuthController.cs` — `Logout` now consumes `request.DeviceToken` and calls `DeviceTokenService.Unregister`
- `App_Start/WebApiConfig.cs` — +5 routes (notifications list, read-all, {id}/read, device-tokens POST, device-tokens DELETE)

## Slice 6 — FCM flow notes

- **Single injection point in existing AppCode.** All notification triggers
  in the web (TaskManger.EmpAcceptTask, EmpRejectTask, EmpFinishTask,
  EmpUpdateTaskTime, CompanyAcceptTask, CompanyRejectTask, ReassignTask,
  TaskAddEdit.Save, etc.) ALREADY call `NotificationHub.Send` internally.
  Adding `FcmDispatcher.Dispatch(...)` once at the end of that method means
  EVERY existing notification trigger automatically fans out to FCM —
  zero changes to any controller, no `Api/Services/TaskNotifier` wrapper,
  no duplicated business logic. This is the single brief-sanctioned edit
  to existing AppCode.
- **Fire-and-forget.** `FcmDispatcher.Dispatch` snapshots the inputs as
  plain values, then queues a `ThreadPool.QueueUserWorkItem` worker. The
  original web/API request returns immediately (Risk R4 mitigation).
- **No async/await.** Per CLAUDE.md sync-only rule, FCM HTTP is via
  `HttpWebRequest` synchronously inside the worker thread. The worker
  thread itself is the asynchronicity boundary.
- **Stale-token cleanup.** FCM legacy API returns `NotRegistered` /
  `InvalidRegistration` / `MismatchSenderId` for expired or wrong-app
  tokens. The dispatcher parses the response body for those substrings
  and calls `DeviceTokenService.MarkInvalid` to flip `IsActive=0`. Token
  is never deleted (audit trail kept).
- **Server key.** `FcmServerKey` Web.config app setting. When the value
  is missing or starts with `REPLACE_`, the dispatcher silently no-ops
  — keeps dev environments unblocked without producing noisy errors.
- **Idempotent device-token register.** Re-registering the same token
  updates `LastSeenDate` (and reactivates if it was disabled by a prior
  FCM error). One user can hold many active tokens (phone + tablet).
- **Notifications endpoints reuse existing data.** No change to
  `NotificationCollection` schema; the controller reads rows scoped to
  `(InstanceID, UserTypeID)` exactly the way `NotificationHub.Get`
  already does. Mark-as-read flips `IsSeen=1` and sets `SeenDate=now`
  — matches the column semantics the web's UI assumes.
- **Per-user scoping** is enforced on read/unregister so a compromised
  token from one account can't disable another account's pushes
  (covered by `DeviceTokenService.Unregister(userId, token)` rather than
  by token value alone).
- **Logout side effect:** `/auth/logout` now accepts `{ refreshToken,
  deviceToken? }`. If `deviceToken` is present we deactivate it so push
  stops going to a logged-out phone.
- **Error codes (Slice 6 additions):**
  | Code | When | HTTP |
  |---|---|---|
  | `NOTIFICATION_NOT_FOUND` | mark-read on an unknown id | 404 |
  | `NOTIFICATION_FORBIDDEN` | mark-read on someone else's notification | 403 |
  | `INVALID_REQUEST` | device-token register/unregister with empty token | 400 |

## Files added in Slice 5 (company task flow)

- `Api/Dtos/Tasks/CreateTaskDto.cs` — body for POST /api/v1/tasks

Existing files edited in Slice 5:
- `Api/Controllers/TasksController.cs` — added `Create`, `Approve`, `Disapprove`, `Delete` methods + `InvokeCompanyAction` helper + `IsLegalTransition` Done case
- `App_Start/WebApiConfig.cs` — split `/api/v1/tasks` into GET+POST verb-constrained routes; added DELETE `/api/v1/tasks/{id}` route

## Slice 5 — company task flow notes

- **Deviation from brief reuse map (same pattern as Slice 4):** brief said
  `Approve → ChangeTaskStatus(Approve)` / `Disapprove → ChangeTaskStatus(Disapprove)`.
  The web actually calls `TaskManger.CompanyAcceptTask` (line 514) and
  `TaskManger.CompanyRejectTask` (line 359). Those methods send
  `NotificationHub.Send` notifications internally; ChangeTaskStatus does not.
  TasksController calls the `TaskManger.Company*Task` methods for behavior
  parity with web (and so Slice 6 FCM works automatically).
- **Create flow is inlined** in `TasksController.Create` (~30 lines). The web's
  `Areas/Company/Models/TaskAddEdit.Save()` mixes view-model concerns
  (Hijri↔Greg string parsing) that don't apply to the API. The inline copy
  uses the same Task field assignments, same `StatusID = New`, same
  `NotificationHub.Send` call, same `TaskTLog` insert, same `LogTask.Log`.
  Right at the ~15-line "fork vs wrap" threshold from CLAUDE.md — flagged in
  the controller comment.
- **Soft delete** mirrors `Areas/Company/Models/CompanyTaskVM.cs:413`:
  `LogTask.Log(task, null, true)` → `task.IsDeleted = true` → Save.
  Single-task only — no bulk delete (per brief).
- **Per-task authorization (company):**
  - Approve / Disapprove / Delete: `task.CompanyID == userData.userId`.
  - Create: assignee `Employee.CompanyID` must equal caller's company.
  - Project (if provided): `Project.CompanyID` must equal caller's company.
- **Routing:** `/api/v1/tasks` is now split by HTTP verb via
  `HttpMethodConstraint`. GET → `List`, POST → `Create`. Same trick on
  `/api/v1/tasks/{id}` for GET (`Detail`) vs DELETE (`Delete`). The
  `/api/v1/tasks/{id}/{action}` route handles all named POST actions
  (accept, reject, complete, time, approve, disapprove).
- **Error codes (Slice 5 additions):**
  | Code | When | HTTP |
  |---|---|---|
  | `COMPANY_ONLY` | Employee tried a Company-only endpoint | 403 |
  | `INVALID_REQUEST` | missing title / empId / priorityId on Create | 400 |
  | `INVALID_EMPLOYEE` | assignee not in caller's company, or inactive/deleted | 400 |
  | `INVALID_PROJECT` | project not in caller's company | 400 |
  | `INVALID_STATE` | approve/disapprove against a non-Done task | 409 |

## Files added in Slice 4 (employee task flow + projects + employees pickers)

- `Api/Controllers/TasksController.cs` — list, detail, accept, reject, complete, time (Employee actions only; Slice 5 adds company-side)
- `Api/Controllers/ProjectsController.cs` — GET `/api/v1/projects` (both roles)
- `Api/Controllers/EmployeesController.cs` — GET `/api/v1/employees` (Company only)
- `Api/Dtos/Tasks/{TaskListItemDto,TaskDetailDto,TaskStatusLogDto,UpdateTimeDto,TaskActionResultDto}.cs`
- `Api/Dtos/Projects/ProjectListItemDto.cs`
- `Api/Dtos/Employees/EmployeeListItemDto.cs`
- `Api/Mapping/{TaskMapper,ProjectMapper,EmployeeMapper}.cs`

Existing files edited in Slice 4:
- `App_Start/WebApiConfig.cs` — added 5 explicit routes for tasks (list, detail, action) + projects + employees

## Slice 4 — task flow notes

- **Deviation from brief's reuse map:** the web's employee accept/reject/
  complete actions actually call `TaskManger.EmpAcceptTask`, `EmpRejectTask`,
  `EmpFinishTask` (lines 749, 811, 852 of `AppCode/TaskManger.cs`), NOT
  `TaskWorkflow.ChangeTaskStatus` as the brief listed. The `TaskManger.Emp*Task`
  methods send `NotificationHub.Send` notifications internally; ChangeTaskStatus
  does not. To mirror web behavior exactly (and to let Slice 6's FCM line
  piggyback automatically), `TasksController` calls the `TaskManger.Emp*Task`
  methods. Same reuse policy as everywhere else — no duplicated business logic.
- **Per-task authorization:**
  - Employee can act on (accept/reject/complete/time) only tasks where
    `task.EmpID == userData.userId`.
  - Employee can READ tasks where they are the current assignee OR they
    appear in `TaskTLogs` (matches web list rule for reassignment history).
  - Company can read tasks in their company.
- **State guards:** controller pre-checks `task.StatusID` against the
  expected source status before invoking `TaskManger.Emp*Task`. Returns
  409 `INVALID_STATE` on illegal transitions (e.g. complete on a `New` task).
  This mirrors what the workflow refuses internally — surfaces a clean
  4xx instead of a silent no-op.
- **`isDelayed` is client-side:** `Task.isDelayed` is a partial-class
  property — not safe inside an EF query. List endpoint materializes with
  `.ToList()` THEN maps, per the CLAUDE.md performance rule.
- **Pagination:** default 20, max 100, server clamps page<1 to 1, pageSize>max
  to max. `.Skip/.Take` applied on IQueryable BEFORE materialization — counts
  via `.Count()` against the same IQueryable, pushed to SQL.
- **Status filter param:** lowercase strings — `all`, `new`, `inprogress`,
  `done`, `accepted`, `approved`, `notapproved`, `pending`. Unknown values
  are treated as `all`.
- **Error codes (Slice 4):**
  | Code | When | HTTP |
  |---|---|---|
  | `TASK_NOT_FOUND` | task id missing or soft-deleted | 404 |
  | `TASK_FORBIDDEN` | caller cannot read this task | 403 |
  | `TASK_NOT_ASSIGNED` | employee tries to mutate a task not assigned to them | 403 |
  | `EMPLOYEE_ONLY` | Company hits an Employee-only endpoint | 403 |
  | `COMPANY_ONLY` | Employee hits a Company-only endpoint (e.g. `/employees`) | 403 |
  | `INVALID_STATE` | workflow refused the transition | 409 |
  | `ACTION_FAILED` | `TaskManger.Emp*Task` returned false | 500 |

## Files added in Slice 3 (attendance)

- `Api/Controllers/AttendanceController.cs` — POST `check-in`, `heartbeat`, `check-out`; GET `today`
- `Api/Dtos/Attendance/{CheckInResponse,HeartbeatResponse,TodayAttendanceDto}.cs`

Existing files edited in Slice 3:
- `Api/Dtos/Auth/MeResponse.cs` — moved TodayAttendanceDto to `Api/Dtos/Attendance/` namespace
- `Api/Controllers/MeController.cs` — uses new TodayAttendanceDto namespace + sets `IsOpen` flag
- `Api/Filters/ApiExceptionFilter.cs` — optional detailed errors via Web.config `ApiDetailedErrors=true` (DEV ONLY)
- `Web.config` — added `ApiDetailedErrors` key (default true in dev)

## Slice 3 — attendance flow notes

- **Employee-only:** `RequireEmployee()` guard returns 403 `EMPLOYEE_ONLY` for
  Company users on `/check-in`, `/heartbeat`, `/check-out`. `/today` returns
  `{ hasAttendance: false }` for Company (mirrors web's
  `AttendanceController.GetCurrentAttendance`).
- **Check-in is idempotent:** if today's latest Attendance row is still open
  (CheckOut null), it is returned with `isNew=false`. Otherwise a new row is
  created by calling `new UserAccountVM().Checkin(empId)` — same call the
  web's `LoginETask` line 135 makes, so any side effects (LastHeartbeat seed,
  Session["HasActiveAttendance"]) match the web exactly.
- **Heartbeat uses direct SQL** on `Attendance.LastHeartbeat`. The column is
  `[NotMapped]` on the EF entity (see `Attendance.Partial.cs`), so we cannot
  set it via EF. Mirrors `AttendanceController.LogActivity` exactly.
- **Check-out** uses the EF entity (CheckOut column IS mapped).
- **Error codes:**
  | Code | When | HTTP |
  |---|---|---|
  | `EMPLOYEE_ONLY` | Company user hits an employee-only attendance endpoint | 403 |
  | `NO_ACTIVE_ATTENDANCE` | heartbeat / check-out called without an open row today | 404 |
  | `CHECKIN_FAILED` | check-in tried to create a row but post-insert lookup found none | 500 |

## Files added in Slice 2 (auth + me)

- `Api/Controllers/AuthController.cs` — POST `/api/v1/auth/login`, `/refresh`, `/logout`
- `Api/Controllers/MeController.cs` — GET `/api/v1/me`, POST `/api/v1/me/change-password`
- `Api/Services/AuthValidator.cs` — credentials + tenant + active-account check (no session writes)
- `Api/Dtos/Auth/{LoginRequest,RefreshRequest,LogoutRequest,ChangePasswordRequest,DeviceInfoDto,TokenResponse,UserSummaryDto,TodayAttendanceDto,MeResponse}.cs`

Existing files edited in Slice 2:
- `App_Start/WebApiConfig.cs` — explicit `/api/v1/me` route
- `Api/Dtos/Common/ApiResponse.cs` — optional `Code` field for machine-readable error codes (`INVALID_CREDENTIALS`, `ACCOUNT_STOPPED`, `INVALID_REFRESH`, `REUSE_DETECTED`)

Removed in Slice 2:
- `Api/Controllers/PingController.cs` (was a temporary smoke-test endpoint from Slice 1)

## Slice 2 — auth flow notes

- **Login reuse policy:** `AuthValidator` mirrors the EF queries of
  `Models/Login/UserAccountVM.LoginETask` but does NOT write to
  `MvcApplication.userData`. The web `LoginETask` writes the session as a
  side effect of validation — we want validation only. The class-level
  encryption + tenant check + IsActive/IsDeleted gates are exactly the
  same, just packaged into a method that returns a result instead of
  mutating state.
- **Auto check-in at login (Q1=a):** `AuthController.Login` calls
  `new UserAccountVM().Checkin(empId)` for employees, mirroring the web's
  `LoginETask` line 135 behavior. Failures are swallowed so login succeeds
  even if attendance insert fails.
- **`MvcApplication.userData` for API:** set by `JwtAuthorizeAttribute` on
  every authenticated request from JWT claims. Endpoints that downstream
  read `userData` (e.g. `UserAccountVM.ChangePassowrd` reads
  `userData.userId`) work unchanged.
- **Refresh-token reuse detection:** if a refresh token marked `IsRevoked=1`
  is presented, every active refresh token for that user is revoked
  (`RevokeAllForUser(userId, "Compromised")`). The mobile app must force
  re-login after a `REUSE_DETECTED` response.
- **Error codes** (in `ApiResponse.code`):
  | Code | When | HTTP |
  |---|---|---|
  | `INVALID_REQUEST` | malformed body / missing fields | 400 |
  | `INVALID_CREDENTIALS` | wrong username / password / tenant mismatch | 401 |
  | `INVALID_REFRESH` | refresh token unknown or expired | 401 |
  | `REUSE_DETECTED` | revoked refresh token replayed (all user tokens revoked) | 401 |
  | `ACCOUNT_STOPPED` | employee/company inactive or deleted | 403 |
  | `INVALID_OLD_PASSWORD` | change-password called with wrong old password | 400 |
