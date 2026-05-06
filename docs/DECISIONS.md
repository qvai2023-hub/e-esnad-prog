# Technical Decisions Log

## Decision Record Format
Each decision includes: Context, Decision, Rationale, and Consequences.

---

## DEC-NEW-PAGINATION: Server-side pagination on `/Company/Company/index` + 500-row safety cap everywhere else

**Date:** 2026-05-06
**Session:** WQ5V1
**Status:** Implemented
**Bug:** #36 Round 3

### Context
The Company tasks page (`/Company/Company/index`) was reported to take 80+ seconds to render (browser even crashed with `STATUS_ACCESS_VIOLATION` on the heaviest company). Earlier sprints had already added eager loading, throttled `UpdateTaskStatus()`, removed dead `ViewBag.statuse` queries, and confirmed `IX_Task_StatusID_CompanyID` / `IX_Task_EmpID_CompanyID` indexes exist on production. Performance was still bad.

Investigation found `CompanyTaskVM.Select()` was returning **every** task for the logged-in company; `WebGrid` then paginated client-side after the entire list was materialized. With thousands of tasks (and eager-loaded TaskTLogs joining further) the SQL alone took tens of seconds and the rest was wasted EF projection. The same "load-everything + WebGrid paginates" pattern existed on 8 other pages (Employee tasks, both dashboards, Common tasks, Projects, Company employees, Admin companies, Admin employees) — fast today only because their datasets are small.

### Decision
Two-pronged fix:

1. **Option A (proper pagination) on the bug page only** — `/Company/Company/index`.
   - Add `Models/PagedResult<T>` wrapper.
   - Add `CompanyTaskVM.SelectPaged(...)` returning `PagedResult<CompanyTaskVM>`. SQL gets `OFFSET .. FETCH` via `.Skip().Take()` on the IQueryable. A separate `.Count()` feeds the pager.
   - Keep existing `Select()` as a wrapper around `SelectPaged(page: 1, pageSize: 500)` — preserves backwards compatibility with `TaskController.FillDropDownLists` which legitimately wants a list of tasks for a dropdown (now safety-capped at 500).
   - Update controller, partial view, and Index view to flow `page` through.
   - Add a small jQuery event-delegated handler to make WebGrid pager clicks AJAX-aware — preserves the SPA feel after AJAX-loaded search/tab content.

2. **Option B (`.Take(500)` cap)** on the other 8 task/employee/project list pages — quick, safe insurance so the same bug can't surface elsewhere as those datasets grow.

### Rationale
- Proper pagination is the right architectural fix but costs ~3 hours per page and creates a tester regression cycle. Doing all 9 pages at once would be ~12 h of code + multi-day tester pass with high regression risk on a critical workflow.
- The bug page is the only one confirmed slow in the field today. Solving it in isolation gets the user-visible win shipped immediately.
- The `.Take(500)` cap on the rest is genuinely cheap (1 line per query site) and never harms a healthy company — typical companies have well under 500 active tasks/projects/employees per scope. If a page ever does grow past 500, it will silently drop oldest records, which is a strong signal to upgrade that page to Option A in a follow-up sprint rather than a hard outage.
- Removing the multi-employee filter (Sprint 3 T-08) was already done in commit `967f484` (March 18), so the Option A complexity around merging per-employee result lists is not a concern.

### Consequences

**Positive:**
- `/Company/Company/index` loads in <2 s regardless of company size.
- Safety net on 8 other pages — pathological cases (one company suddenly hits 5,000 tasks) won't bring the page down silently.
- Pager links work both for full page reload (initial Index) and for AJAX-loaded partials (after tab-click or search), via the new event-delegated click handler.

**Trade-offs:**
- Bulk-delete "select all" on the New tab now selects only the **current page** (was: every loaded row, which was effectively everything). Tester verified — acceptable.
- WebGrid pager URL preserves QueryString, so paginating after a non-AJAX'd search continues to filter; paginating after an AJAX-only search does NOT preserve the filter (the URL never changed) — for now, search-then-paginate users will need to re-search. Documented for tester.
- The Delay tab still does C# post-filter by `isDelayed`, capped at 500 rows materialized. If a company has more than 500 accepted-and-delayed tasks at once (highly unusual), the oldest are dropped from the Delay tab.
- Pages 2/3 (Employee tasks, Common tasks) still do not have proper pagination — they have the safety cap. Promote to Option A in a follow-up sprint when those volumes grow.

### Implementation
See CHANGELOG.md "Bug #36 Round 3" for the full file list.

### Reference
- Branch: `claude/fix-git-bugs-WQ5V1`
- Bug: GitHub issue #36
- Performance verification: Chrome DevTools timing against `app-test.telesak.com`

---

## DEC-001: Separate Date from Time in Attendance

**Date:** 2026-03-08
**Session:** a02
**Status:** Implemented

### Context
The attendance report showed full datetime (e.g., `1/8/2026 7:07:16 PM`) in both check-in and check-out columns, making the report cluttered and hard to read.

### Decision
Separate the date into its own column and show only time (HH:mm) in check-in/check-out columns.

### Rationale
- Reduces redundancy (date shown once per row)
- Cleaner report layout
- Easier to scan attendance times

### Implementation
- SP: Added `AttendanceDate`, `CheckInTime`, `CheckOutTime` columns
- RDLC: New date column with `dd/MM/yyyy` format

---

## DEC-002: Dual Duration Format (Minutes + Display)

**Date:** 2026-03-08
**Session:** a02
**Status:** Implemented

### Context
Duration was shown as `HH:MM:SS` string which cannot be summed for totals.

### Decision
Store duration in two formats:
- `DurationMinutes` (INT) - for calculations and SUM
- `DurationDisplay` (VARCHAR) - for display (H:MM format)

### Rationale
- Enables aggregation (SUM of minutes)
- Maintains readable display format
- Supports future features like "total hours per employee"

### Implementation
- SP: Calculate minutes internally, format as `H:MM` for display
- Model: `DurationMinutes` (int?) + `DurationDisplay` (string)

---

## DEC-003: Handle Missing Check-out

**Date:** 2026-03-08
**Session:** a02
**Status:** Implemented

### Context
When employee checks in but doesn't check out, the check-out field was empty (NULL).

### Decision
Display "لم يُسجل" (Not Recorded) instead of empty/blank.

### Rationale
- Clear indication that check-out wasn't recorded
- Better user experience
- Distinguishes from data loading issues

### Implementation
- SP: `ISNULL(CheckOutTime, 'لم يُسجل')`

---

## DEC-004: Employee Name as Header

**Date:** 2026-03-08
**Session:** a02, a03
**Status:** Fully Implemented

### Context
Employee name was repeated on every row in the attendance table.

### Decision
Show employee name once as a group header, with their attendance records listed below.

### Rationale
- Reduces visual clutter
- Groups related data together
- More professional report layout

### Implementation
- RDLC: TablixGroup on EmpID field
- Group Header row with employee name (bold, highlighted)
- Group Footer row with subtotals
- Row number resets per employee using `RowNumber("EmployeeGroup")`

---

## DEC-005: Report Parameters for Date Range

**Date:** 2026-03-08
**Session:** a03
**Status:** Implemented

### Context
Report needed to display the selected date range and period in Arabic.

### Decision
Add three report parameters: StartDate, EndDate, ReportPeriod.

### Rationale
- Shows user the exact date range selected
- Arabic month names for better readability
- Consistent formatting across report

### Implementation
- Controller: Format dates and Arabic month names
- RDLC: Display parameters in report header

---

## DEC-006: Professional Report Structure

**Date:** 2026-03-08
**Session:** a03
**Status:** Implemented

### Context
Report needed a professional, client-ready format.

### Decision
Implement structured report with:
- Header: Logo, company name, period, date range
- Body: Grouped data by employee with 6 columns
- Footer: Totals and disclaimer

### Rationale
- Professional appearance
- Clear data organization
- Easy to print and export

### Implementation
- Complete RDLC rebuild with new structure
- Blue color scheme (#3d85c6)
- RTL support for Arabic text

---

---

## Sprint 1 Decisions

### DEC-007: Client-Side Attendance Tracking with Heartbeat

**Date:** 2026-03-09
**Session:** earlier
**Status:** Implemented

### Context
Employees could check in and forget to check out, leaving orphaned attendance sessions.

### Decision
Implement a heartbeat-based attendance tracking system:
- Client sends periodic heartbeats via `attendance-tracker.js`
- `LastHeartbeat` column added to Attendance table
- Server-side `AutoCheckoutJob` checks for stale heartbeats and auto-checks out

### Rationale
- Ensures attendance records are always closed
- Non-intrusive to user experience
- Server-side fallback for closed browsers

### Implementation
- JS: `attendance-tracker.js` with periodic heartbeat API calls
- C#: `AutoCheckoutJob.cs` runs on app startup
- SQL: `AddLastHeartbeat.sql` migration

---

### DEC-008: Inactivity Checkout with Modal Notification

**Date:** 2026-03-09
**Session:** earlier
**Status:** Implemented

### Context
Users leaving browser open but inactive needed a checkout mechanism.

### Decision
Show a modal popup after 30 seconds of inactivity with countdown timer, then auto-checkout and redirect to logout.

### Rationale
- Gives user a chance to stay active
- Clear visual feedback before checkout
- Prevents false inactive checkouts

---

### DEC-009: Remove UserAgent from Session Validation

**Date:** 2026-03-09
**Session:** earlier
**Status:** Implemented

### Context
`UserAccountVM.cs` was checking UserAgent string for session validation, causing false session invalidation when browser updates changed the agent string.

### Decision
Remove UserAgent check from session validation.

### Rationale
- UserAgent strings change frequently with browser updates
- Was causing legitimate users to be logged out
- Session token alone is sufficient for validation

---

### DEC-010: Hijri/Gregorian Calendar Toggle for Reports

**Date:** 2026-03-16
**Session:** haj1c
**Status:** Implemented

### Context
Task report needed to support both Hijri and Gregorian calendar date display.

### Decision
Add a calendar type toggle in `TaskReportPreperation.cshtml` and use `ConvertDate()` helper in the controller.

### Rationale
- Saudi business requirement for Hijri dates
- User flexibility to switch between calendars
- `CultureInfo("ar-SA")` for Arabic formatting

---

### DEC-011: Add Interaction Column to Task Report

**Date:** 2026-03-16
**Session:** haj1c
**Status:** Implemented

### Context
Task report lacked visibility into employee task interaction status.

### Decision
Add التفاعل (Interaction) column to CompanyTasks report, sourced from updated `sp_CompanyTasks` stored procedure.

### Rationale
- Management needs to see task engagement at a glance
- Color-coded: متفاعل (active) vs غير متفاعل (inactive)
- Integrated into existing report structure

---

---

## Sprint 4 Decisions

### DEC-012: Eager Loading via includeProperties for Task Lists

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
Task list pages (`CompanyTaskVM.Select()`, `EmployeeTaskListVM.FillTasks()`) were triggering N+1 lazy-loading queries for navigation properties like Project, Status, Priority, and TaskTLogs. Each task rendered caused additional DB roundtrips.

### Decision
Add `includeProperties` parameter to `Get()` calls to eager-load required navigation properties in a single query.

### Rationale
- Eliminates hundreds of lazy-load queries per page load
- `includeProperties` is already supported by the repository's `Get()` method
- No schema or model changes required
- Minimal code change with large performance gain

### Implementation
- `CompanyTaskVM`: `includeProperties: "Project,Status,Priority,TaskTLogs"`
- `EmployeeTaskListVM`: `includeProperties: "Project,Priority,Status,TaskTLogs,TaskTLogs.Status"`

---

### DEC-013: Batch Dictionary Lookup for Task Delay Data

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
In `CompanyTaskVM.Select()`, the `else` branch (delayed tasks) called `isTaskDelayed(t)` and `Delaytime(t)` per task inside `ForEach`. Each method called `GetByID()`, causing N extra queries.

### Decision
Fetch all needed task delay data (isDelayed, delayTime, DelayPercentage) in a single batch query using a `Dictionary<int, ...>` keyed by TaskID, then perform O(1) lookups inside the loop.

### Rationale
- Replaces N queries with 1 query
- Dictionary lookups are O(1)
- No changes to DB schema or stored procedures
- Same data, dramatically fewer roundtrips

### Implementation
```csharp
var taskIds = objTasks.Select(t => t.TaskID).ToList();
var taskDelayData = _unitOfWork.TaskRepository
    .Get(filter: t => taskIds.Contains(t.TaskID))
    .ToDictionary(t => t.TaskID, t => new { t.isDelayed, t.delayTime, t.DelayPercentage });
// Then: taskDelayData[t.TaskID].isDelayed instead of isTaskDelayed(t)
```

---

### DEC-014: Server-Side Filtering for GetAllEmployees

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
`ServiceManger.GetAllEmployees()` called `_unitOfWork.Employees.Get()` (no filter), loading ALL employees into memory, then filtered with `.Where(e => e.company_Id == companyId)` client-side.

### Decision
Pass the filter directly into `Get(filter:)` so EF translates it to a SQL WHERE clause.

### Rationale
- Avoids loading entire employee table into memory
- Significantly reduces data transfer and memory usage
- Single-line fix with no behavioral change

---

### DEC-015: Server-Side Filtering for All Uniqueness Checks in CompanyEmployeeVM

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
All uniqueness validation methods in `CompanyEmployeeVM` (CheckForUniqueName, CheckForUniqueEmail, CheckForUniqueMobile, CheckForUniqueNationalID, CheckForUniquSequenceNumberLaborOfficeID, CheckForDeletedEmpNationlID) called `.Get()` with no filter, loading ALL employees into memory and filtering client-side with `.Count()` or `.Where().FirstOrDefault()`. Same pattern in `GetEmpIDUserAccount`, `UpdateEmp`, `UpdateRehireDeletedEmployee`.

### Decision
Pass filter expressions directly into `Get(filter:)` so EF translates them to SQL WHERE clauses. For simple PK lookups, use `GetByID()`.

### Rationale
- Every employee form submit was loading the entire Employee table
- 11 separate methods affected — each one a full table scan
- No behavioral change; only query execution location changes from client to DB

---

### DEC-016: Replace Unbounded Session Load in NotificationHub

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
`NotificationHub.Send()` loaded ALL SignalR sessions (`SIGNAL_R_SESSIONs.Get().ToList()`) into memory, then filtered client-side to find matching recipients. As connected users grow, this becomes a significant bottleneck.

### Decision
Filter at DB level using the known `InstanceID` and `UserTypeID` values from the notification collection.

### Rationale
- Avoids loading entire session table on every notification
- Scales better as active user count grows
- Uses `.Contains()` which EF translates to SQL IN clause

---

### DEC-017: Company-Scoped Queries in RecurrenceTaskVM

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
`GetAllProjects()` and `GetAllEmployees()` in `RecurrenceTaskVM` loaded ALL records from the Projects and Employee tables without any company filter. In a multi-tenant system, this means every company sees every other company's projects and employees.

### Decision
Add `CompanyID == MvcApplication.userData.userId` filter to both queries, and `IsDeleted == false` filter to employee query.

### Rationale
- Security: prevents cross-tenant data exposure
- Performance: only loads relevant records
- Correctness: dropdowns should only show current company's data

---

### DEC-018: Remove AsEnumerable() Client-Side GroupBy in SharedService

**Date:** 2026-03-18
**Session:** pw8mP
**Status:** Implemented

### Context
`GetCompaniesByproviderId()` loaded all UserAccount records, called `.AsEnumerable()` (forcing client-side execution), then did a `.GroupBy()` on CompanyID + Company.Name. This loaded the entire UserAccounts table into memory.

### Decision
Replace with a direct `Company.Get(filter:)` query that uses `.Any()` subquery to check if matching UserAccounts exist, eliminating the GroupBy entirely.

### Rationale
- GroupBy was only used to get distinct companies — a Company query does this directly
- Avoids loading entire UserAccounts table
- `.Any()` subquery translates to efficient SQL EXISTS

---

---

## Sprint 6 Decisions

### DEC-019: Revert EmpUpdateDalyTaskTime — Read-Only Display Method

**Date:** 2026-04-20
**Session:** WQ5V1
**Status:** Implemented

### Context
Bug #35 reported daily time entry not updating. I added `_unitOfWork.Save()` and `Update()` calls to `EmpUpdateDalyTaskTime()`, which caused a regression: both total AND daily time stopped updating.

### Decision
Revert to original code. `EmpUpdateDalyTaskTime()` is a read-only display helper — it returns a value for the UI but does not save to DB. The actual saving is done by `EmpUpdateTaskTime()` (a separate endpoint).

### Rationale
- Changing a read-only method to write broke the save flow
- `EmpUpdateTaskTime` handles all DB writes for time entries
- Two methods writing to the same data creates race conditions
- Minimal fix principle: revert the regression, investigate separately

---

### DEC-020: Anti-Reopen Rules in CLAUDE.md

**Date:** 2026-04-20
**Session:** WQ5V1
**Status:** Implemented

### Context
5 out of 14 fixed bugs were reopened by the tester. Root causes: wrong page fixed (#36), wrong CSS selector (#37), incomplete area coverage (#46), incomplete RDLC alignment (#47), and regression from logic change in shared code (#35).

### Decision
Added 7 Anti-Reopen Rules to CLAUDE.md Bug Fixing Workflow to prevent recurrence.

### Rationale
- Each rule maps to a real reopened bug
- Rules enforce verification before committing
- Shared code changes now require explicit call-chain tracing
- CSS/RDLC changes require element-level verification
- GitHub issue closure with comments is now mandatory

---

### DEC-021: Eager Loading in CompanyTaskVM for Company المهام Page

**Date:** 2026-04-20
**Session:** WQ5V1
**Status:** Implemented

### Context
Bug #36 was reopened because the original fix targeted Admin pages (`CompanyVM.Select`, `CompanyEmployeeVM.Search`). The Company account's المهام page uses `CompanyTaskVM.Select()` which lazy-loads `Project`, `Status`, `Priority`, and `TaskTLogs` per row.

### Decision
Add `includeProperties: "Project,Status,Priority,TaskTLogs"` to the `TaskRepository.Get()` call in `CompanyTaskVM.Select()`.

### Rationale
- Same N+1 pattern as Admin pages but different code path
- 4 navigation properties × N tasks = 4N extra queries eliminated
- Consistent with PERF-02 pattern already applied to Areas2
- Minimal change: single parameter addition

---

### DEC-022: IsDeleted Filter on All Uniqueness Checks

**Date:** 2026-04-20
**Session:** WQ5V1
**Status:** Implemented

### Context
Bug #46 was reopened because only the Email check was fixed. `CheckForUniqueMobile` and `CheckForUniqueNationalID` had the same bug: the same-company clause was missing `IsDeleted == false`, so soft-deleted duplicates still blocked edits.

### Decision
Add `&& e.IsDeleted == false` to the same-company clause in `CheckForUniqueMobile` and `CheckForUniqueNationalID` across all 4 code locations (Admin Areas, Admin Areas2, Company Areas, Company Areas2).

### Rationale
- All uniqueness checks must consistently exclude soft-deleted records
- Remote validation fires for ALL fields on form submit — one broken check blocks the entire save
- Fix must cover all areas (Areas + Areas2) and all roles (Admin + Company)

---

## Architecture Decisions

### ADR-001: RDLC for Reports

**Status:** Existing

### Decision
Use RDLC (Report Definition Language Client) for all reports.

### Rationale
- Native .NET integration
- Client-side rendering
- PDF/Excel export support
- No Report Server required
