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
   This codebase has `Areas/` and `Areas2/` with mirrored code. A fix in `Areas/Company/Models/` must also be applied to `Areas2/Company/Models/`. Additionally, if a bug exists in the Company area validation, check the Admin area for the same pattern — they often share the same broken logic.

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

11. **Before changing any method — grep ALL callers across the entire codebase first.**
    Run a grep for the method name across all `.cs`, `.cshtml`, and `.js` files.
    List every file and line that calls this method. For each caller:
    - Does it depend on the current return type or behavior?
    - Will the fix break its flow?
    If ANY caller is in a different context (Dashboard, Report, Admin, Employee), STOP and trace
    that caller's full flow before touching the method. This rule prevented the #35 regression
    where `EmpUpdateDalyTaskTime` was changed without checking that `EmpUpdateTaskTime` was
    already handling the DB save.

12. **After finding one broken pattern — search the ENTIRE file for all similar patterns.**
    Do not commit after fixing the first match. Search the same file for every other method,
    query, or check that follows the same broken pattern and fix them all in one pass.
    Example: if `CheckForUniqueName` is missing `IsDeleted == false`, grep the whole file for
    every other `Check*` method and verify each one before committing.
    This rule would have caught the #46 partial fix (email fixed but Name/SequenceNumber missed).

13. **Before writing any fix — state the exact URL and confirm which controller handles it.**
    Open the `AreaRegistration.cs` for each area and trace the route.
    Never assume the URL maps to the area you expect — always verify.
    Example: `/Company/Company/index` → Area=Company, Controller=Company, not Admin.
    This rule would have caught the #36 Round 1 mistake (fixed Admin instead of Company).

14. **If the bug is a JS display issue — fix it in the JS callback, not in the server method.**
    Visual DOM issues belong in the frontend. If the display is stale after an AJAX call,
    the fix is: restructure the JS callbacks (sequential vs parallel, correct DOM selector).
    Never change a server method's return format to fix a visual bug — server methods are
    shared across multiple callers and changing their output breaks all of them.

15. **After fixing — walk through the tester's exact repro steps one by one.**
    For each step ask: "Does my fix change what happens at this step?"
    If you reach the final step and cannot say "yes, the tester will now see the correct result"
    with full confidence — do NOT commit. Investigate more.
    This applies even when the code change looks obviously correct.

16. **Areas2/ note — this project does NOT actively use the Areas2/ folder in production.**
    Do NOT apply fixes to `Areas2/` unless explicitly asked. All fixes go to `Areas/` only.

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
