# TELE SAK - Technical Decisions

## Sprint 0 Decisions

---

### DEC-001: Separate Date from Time in Attendance

**Date:** 2026-03-08 | **Task:** A-02 | **Status:** Implemented

#### Context
The attendance report showed full datetime in both check-in and check-out columns, making the report cluttered.

#### Decision
Separate the date into its own column (`AttendanceDate`) and show only time (HH:mm) in check-in/check-out columns.

#### Rationale
- Reduces redundancy
- Cleaner report layout
- Easier to scan attendance times

#### Implementation
```sql
-- SP returns:
AttendanceDate  -- DATE only
CheckInTime     -- HH:mm only
CheckOutTime    -- HH:mm or "لم يُسجل"
```

---

### DEC-002: Dual Duration Format

**Date:** 2026-03-08 | **Task:** A-02 | **Status:** Implemented

#### Context
Duration was shown as `HH:MM:SS` string which cannot be summed for totals.

#### Decision
Store duration in two formats:
- `DurationMinutes` (INT) - for SUM calculations
- `DurationDisplay` (VARCHAR) - for display (H:MM format)

#### Rationale
- Enables SQL aggregation (SUM of minutes)
- Maintains readable display format
- Supports subtotals per employee

#### Implementation
```sql
DurationMinutes = DATEDIFF(MINUTE, CheckIn, CheckOut)
DurationDisplay = CAST(DurationMinutes/60 AS VARCHAR) + ':' + FORMAT(DurationMinutes%60, '00')
```

---

### DEC-003: Handle Missing Check-out

**Date:** 2026-03-08 | **Task:** A-02 | **Status:** Implemented

#### Context
When employee checks in but doesn't check out, the check-out field was empty.

#### Decision
Display "لم يُسجل" (Not Recorded) instead of blank.

#### Rationale
- Clear indication that check-out wasn't recorded
- Better user experience
- Distinguishes from data loading issues

#### Implementation
```sql
ISNULL(CheckOutTime, 'لم يُسجل')
```

---

### DEC-004: Employee Grouping in Report

**Date:** 2026-03-08 | **Task:** A-03 | **Status:** Implemented

#### Context
Employee name was repeated on every row in the attendance table.

#### Decision
Use RDLC TablixGroup to group records by employee with:
- Group Header: Employee name (bold, highlighted)
- Group Footer: Subtotals (days count + total hours)
- Row numbers reset per employee

#### Rationale
- Reduces visual clutter
- Groups related data together
- Professional report layout
- Enables per-employee subtotals

#### Implementation
```xml
<Group Name="EmployeeGroup">
  <GroupExpressions>
    <GroupExpression>=Fields!EmpID.Value</GroupExpression>
  </GroupExpressions>
</Group>
```

---

### DEC-005: Report Parameters for Date Range

**Date:** 2026-03-08 | **Task:** A-03 | **Status:** Implemented

#### Context
Report needed to display the selected date range in header.

#### Decision
Add three report parameters passed from Controller:
- `StartDate` - yyyy/MM/dd format
- `EndDate` - yyyy/MM/dd format
- `ReportPeriod` - Arabic month name(s)

#### Rationale
- Shows user the exact date range selected
- Arabic month names using `CultureInfo("ar-SA")`
- Consistent formatting

#### Implementation
```csharp
CultureInfo arCulture = new CultureInfo("ar-SA");
arCulture.DateTimeFormat.Calendar = new GregorianCalendar();
reportPeriod = fromDt.ToString("MMMM yyyy", arCulture);
```

---

### DEC-006: Professional Report Structure

**Date:** 2026-03-08 | **Task:** A-03 | **Status:** Implemented

#### Context
Report needed a professional, client-ready format.

#### Decision
Implement structured report:
- **Header:** Logo, company name, period, date range
- **Body:** 6 columns with employee grouping
- **Footer:** Total employees + total work days
- **Page Footer:** Print date + page numbers
- **Disclaimer:** Data source attribution

#### Color Scheme
- Primary: `#3d85c6` (Blue)
- Group Header: `#e6f2ff` (Light Blue)
- Group Footer: `#f5f5f5` (Light Gray)

#### Rationale
- Professional appearance
- Clear data organization
- Easy to print and export as PDF

---

---

## Sprint 1 Decisions

### DEC-007: Client-Side Attendance Tracking with Heartbeat

**Date:** 2026-03-09 | **Task:** ATT | **Status:** Implemented

#### Context
Employees could check in and forget to check out, leaving orphaned attendance sessions.

#### Decision
Implement a heartbeat-based attendance tracking system:
- Client sends periodic heartbeats via `attendance-tracker.js`
- `LastHeartbeat` column added to Attendance table
- Server-side `AutoCheckoutJob` checks for stale heartbeats

#### Rationale
- Ensures attendance records are always closed
- Non-intrusive to user experience
- Server-side fallback for closed browsers

#### Implementation
```javascript
// Client: periodic heartbeat
setInterval(() => fetch('/api/attendance/heartbeat'), HEARTBEAT_INTERVAL);
```
```csharp
// Server: auto-checkout stale sessions
var stale = db.Attendance.Where(a => a.LastHeartbeat < threshold);
```

---

### DEC-008: Inactivity Checkout with Modal

**Date:** 2026-03-09 | **Task:** ATT | **Status:** Implemented

#### Context
Users leaving browser open but inactive needed a checkout mechanism.

#### Decision
Show modal popup after 30 seconds inactivity with countdown, then auto-checkout and redirect to logout.

#### Rationale
- Gives user a chance to stay active
- Clear visual feedback before checkout
- Prevents false inactive checkouts

---

### DEC-009: Remove UserAgent Session Check

**Date:** 2026-03-09 | **Task:** ATT | **Status:** Implemented

#### Context
`UserAccountVM.cs` was checking UserAgent for session validation, causing false logouts on browser updates.

#### Decision
Remove UserAgent check from session validation.

#### Rationale
- UserAgent strings change with browser updates
- Was causing legitimate session invalidation
- Session token alone is sufficient

---

### DEC-010: Hijri/Gregorian Calendar Toggle

**Date:** 2026-03-16 | **Task:** T-02 | **Status:** Implemented

#### Context
Task report needed Hijri and Gregorian calendar support.

#### Decision
Add calendar type toggle in UI and `ConvertDate()` helper in controller.

#### Rationale
- Saudi business requirement for Hijri dates
- User flexibility to switch calendars
- `CultureInfo("ar-SA")` for Arabic formatting

#### Implementation
```csharp
// ConvertDate returns nullable DateTime
DateTime? hijriDate = ConvertDate(gregorianDate, "Hijri");
```

---

### DEC-011: Add Interaction Column (التفاعل)

**Date:** 2026-03-16 | **Task:** T-04 | **Status:** Implemented

#### Context
Task report lacked visibility into employee task interaction status.

#### Decision
Add التفاعل column to CompanyTasks report from updated `sp_CompanyTasks`.

#### Rationale
- Management needs task engagement visibility
- Color-coded: متفاعل (active) vs غير متفاعل (inactive)
- Integrated into existing report layout

#### Implementation
```xml
<!-- RDLC: Interaction column with conditional color -->
<TablixColumn>
  <TablixHeader>التفاعل</TablixHeader>
</TablixColumn>
```

---

---

## Sprint 4 Decisions

### DEC-012: Eager Loading via includeProperties for Task Lists

**Date:** 2026-03-18 | **Task:** PERF-01/02/03 | **Status:** Implemented

#### Context
Task list pages triggered N+1 lazy-loading queries for navigation properties (Project, Status, Priority, TaskTLogs). Each task caused additional DB roundtrips.

#### Decision
Add `includeProperties` parameter to `Get()` calls to eager-load all required navigation properties in a single query.

#### Rationale
- Eliminates hundreds of lazy-load queries per page load
- Already supported by the repository pattern
- Minimal code change, large performance gain

#### Implementation
```csharp
// CompanyTaskVM.Select()
_unitOfWork.TaskRepository.Get(filter: ..., includeProperties: "Project,Status,Priority,TaskTLogs")

// EmployeeTaskListVM.FillTasks()
_unitOfWork.TaskRepository.Get(filter: ..., includeProperties: "Project,Priority,Status,TaskTLogs,TaskTLogs.Status")
```

---

### DEC-013: Batch Dictionary Lookup for Task Delay Data

**Date:** 2026-03-18 | **Task:** PERF-01 | **Status:** Implemented

#### Context
`CompanyTaskVM.Select()` called `isTaskDelayed(t)` and `Delaytime(t)` per task inside a `ForEach`. Each method called `GetByID()`, causing N extra DB queries.

#### Decision
Batch-load all task delay data into a `Dictionary<int, {isDelayed, delayTime, DelayPercentage}>` and perform O(1) lookups.

#### Rationale
- Replaces N queries with 1 query
- Dictionary lookups are O(1)
- No schema changes needed

#### Implementation
```csharp
var taskDelayData = _unitOfWork.TaskRepository
    .Get(filter: t => taskIds.Contains(t.TaskID))
    .ToDictionary(t => t.TaskID, t => new { t.isDelayed, t.delayTime, t.DelayPercentage });
```

---

### DEC-014: Server-Side Filtering for GetAllEmployees

**Date:** 2026-03-18 | **Task:** PERF-05 | **Status:** Implemented

#### Context
`ServiceManger.GetAllEmployees()` loaded ALL employees then filtered client-side with `.Where()`.

#### Decision
Pass filter into `Get(filter:)` so EF translates it to SQL WHERE clause.

#### Rationale
- Avoids loading entire employee table into memory
- Reduces data transfer and memory usage
- Single-line fix

#### Implementation
```csharp
// Before: Get().Where(e => e.company_Id == companyId)
// After:  Get(filter: e => e.company_Id == companyId)
```

---

### DEC-015: Server-Side Filtering for CompanyEmployeeVM Validation Queries

**Date:** 2026-03-18 | **Task:** PERF-06 | **Status:** Implemented

#### Context
All 11 validation/lookup methods in `CompanyEmployeeVM` called `.Get()` with no filter, loading ALL employees into memory before filtering client-side.

#### Decision
Pass filter expressions directly into `Get(filter:)` for all 11 methods.

#### Rationale
- Every employee form submit triggered full table scans
- Single-line fixes with no behavioral change
- Query execution moves from C# to SQL WHERE clause

---

### DEC-016: Replace Unbounded Session Load in NotificationHub

**Date:** 2026-03-18 | **Task:** PERF-10 | **Status:** Implemented

#### Context
`NotificationHub.Send()` loaded ALL SignalR sessions into memory, then filtered client-side.

#### Decision
Filter at DB level using `InstanceID` and `UserTypeID` values from the notification collection with `.Contains()` (translates to SQL IN).

#### Rationale
- Scales better as connected users grow
- Avoids loading entire session table per notification

---

### DEC-017: Company-Scoped Queries in RecurrenceTaskVM

**Date:** 2026-03-18 | **Task:** PERF-13 | **Status:** Implemented

#### Context
`GetAllProjects()` and `GetAllEmployees()` loaded ALL records without company filter — cross-tenant data exposure in a multi-tenant system.

#### Decision
Add `CompanyID == MvcApplication.userData.userId` filter, plus `IsDeleted == false` for employees.

#### Rationale
- Security: prevents cross-tenant data exposure
- Performance: only loads relevant records

---

### DEC-018: Remove AsEnumerable() Client-Side GroupBy in SharedService

**Date:** 2026-03-18 | **Task:** PERF-11 | **Status:** Implemented

#### Context
`GetCompaniesByproviderId()` loaded all UserAccounts, forced client-side execution with `.AsEnumerable()`, then did GroupBy.

#### Decision
Replace with `Company.Get(filter:)` using `.Any()` subquery to check matching UserAccounts.

#### Rationale
- GroupBy was only used to get distinct companies — a Company query does this directly
- `.Any()` translates to efficient SQL EXISTS

---

## Sprint 7 Decisions

### DEC-029: DisplayDate Partial Class for Hijri Report Data

**Date:** 2026-03-30 | **Task:** ATT-04 | **Status:** Implemented

#### Context
Attendance report data rows showed Gregorian dates even when Hijri calendar was selected. The `AttendanceDate` field is `DateTime?` from the stored procedure. RDLC cannot conditionally format dates to Hijri using simple expressions.

#### Decision
Add a `DisplayDate` string property via partial class (`sp_Attendance_Result.Partial.cs`). The controller populates it using `UmAlQuraCalendar` or Gregorian based on `calendarType`, then the RDLC displays the pre-formatted string.

#### Rationale
- Partial class doesn't touch auto-generated EF code
- Controller-level conversion matches the project's existing pattern
- Same `UmAlQuraCalendar` approach used in CompanyTasks ReportController

---

### DEC-030: Calendar Toggle via Dropdown (Not Global Setting)

**Date:** 2026-03-30 | **Task:** ATT-02 | **Status:** Implemented

#### Context
AttendanceReport used the global `MvcApplication.IsGregDate` setting to decide calendar type. This meant the user couldn't choose — it was fixed system-wide.

#### Decision
Added a dropdown selector (ميلادي/هجري) to the report page, same pattern as CompanyTasks `TaskReportPreperation.cshtml`. The JS `initCalendar()` function reinitializes the datepicker when the user switches.

#### Rationale
- Matches existing CompanyTasks pattern exactly
- Gives users per-report control over calendar type
- Global ISGreg setting still used as default elsewhere

---

### DEC-031: Login Chat Intermediate [نعم] Step

**Date:** 2026-03-30 | **Task:** CHAT | **Status:** Implemented

#### Context
On the login page, clicking "مشكلة في تسجيل الدخول" showed the answer directly. The desired UX was a conversational flow.

#### Decision
Added an intermediate step: bot asks "مرحبا! هل تواجه مشكلة في تسجيل الدخول؟" with a [نعم] button. Clicking [نعم] shows the answer and removes the button.

#### Rationale
- More conversational and less abrupt
- Matches the reference design's interaction pattern
- Button is implemented as a `.tlsk-chip` element with delegated click handler

---

## Sprint 6 Decisions

### DEC-026: Q&A Keyword Match Before AI Fallback

**Date:** 2026-03-25 | **Task:** T-15 | **Status:** Implemented

#### Context
A chat assistant was needed to help employees with common questions about Telesak.

#### Decision
Two-tier response strategy:
1. First try keyword matching against `App_Data/telesak-qa.json` (triggers array, case-insensitive substring)
2. If no match, fall back to Claude Haiku API with user context in system prompt

#### Rationale
- Q&A file gives instant, deterministic answers for known questions — no API cost
- AI fallback handles open-ended questions naturally
- Q&A file is editable by admin without code changes
- Response includes `source: "qa"|"ai"` so the frontend can distinguish

---

### DEC-027: Self-Contained Chat Widget (JS + Injected CSS)

**Date:** 2026-03-25 | **Task:** T-15 | **Status:** Implemented

#### Context
The chat widget needed to work across all 3 layout files without external CSS dependencies.

#### Decision
All styles are injected via a `<style>` tag from within `telesak-chat.js`. No separate CSS file.

#### Rationale
- Single file to deploy — just add one `<script>` tag to each layout
- No CSS bundle changes or build step needed
- Styles are scoped by `#tlsk-` prefixed IDs/classes to avoid conflicts
- Easier to maintain — widget is fully self-contained

---

### DEC-028: Category Tabs for Quick Buttons

**Date:** 2026-03-25 | **Task:** T-15 | **Status:** Implemented

#### Context
17 quick buttons across 5 topics would clutter the UI if shown all at once.

#### Decision
Add 6 category tabs (الكل + 5 categories) that filter the quick buttons. Each button has a `cat` property. "الكل" shows all.

#### Rationale
- Users can quickly find relevant questions by category
- Reduces visual clutter
- Familiar tab/filter UX pattern

---

## Sprint 5 Decisions

### DEC-019: Remove End Date Filter from Report Pages

**Date:** 2026-03-24 | **Task:** T-10 | **Status:** Implemented

#### Context
Report preparation pages had 4 date fields (Start Date From/To + End Date From/To) which appeared as duplicates to users.

#### Decision
Remove the "تاريخ النهاية" (End Date) row entirely. Keep only "تاريخ البداية" (Start Date) From/To.

#### Rationale
- User reported as confusing/duplicate
- Start Date range is sufficient for most report use cases
- Backend SP still accepts the parameters (they'll be null)

---

### DEC-020: Hijri Month Names for Statistics Filter

**Date:** 2026-03-24 | **Task:** T-11 | **Status:** Implemented

#### Context
TasksByMonthsChart uses Hijri months on the X-axis. Month filter dropdown needed matching names.

#### Decision
Use Hijri month names (محرم through ذو الحجة) instead of Gregorian months, matching the chart axis labels.

#### Rationale
- Consistency with chart display
- The data is already grouped by Hijri months

---

### DEC-021: BiDi Fix via CSS Class Instead of Inline Styles

**Date:** 2026-03-24 | **Task:** T-12 | **Status:** Implemented

#### Context
Mixed Arabic/English task names displayed in wrong reading order.

#### Decision
Add `.bidi-text { unicode-bidi: plaintext; }` CSS class and wrap task name elements with `<span class="bidi-text">`. Deferred RDLC report BiDi to future task.

#### Rationale
- CSS class is reusable across all views
- `unicode-bidi: plaintext` lets the browser determine text direction automatically
- RDLC requires different approach (XML-level changes)

---

### DEC-022: Batch FillTaskMetadata Instead of Per-Task GetByID

**Date:** 2026-03-24 | **Task:** T-13 | **Status:** Implemented

#### Context
Dashboard `SelectCompanyTasks()` called `isTaskDelayed()`, `TaskDelayTime()`, `DelayPercentage()`, and `GetEmplyeeName()` per task — each creating a new UnitOfWork and calling GetByID.

#### Decision
Replace with single `FillTaskMetadata()` method that batch-loads all task entities and employee names into dictionaries, then performs O(1) lookups.

#### Rationale
- Reduces N*4 DB queries to 2 queries (tasks + employees)
- `isDelayed` is a computed property — cannot be used in LINQ-to-Entities WHERE clause
- Same pattern as Sprint 4 DEC-013 for CompanyTaskVM

---

### DEC-024: RDLC Report Language for BiDi Text

**Date:** 2026-03-24 | **Task:** T-12b | **Status:** Implemented

#### Context
CompanyTasks RDLC report displayed mixed Arabic/English task names in wrong reading order (e.g. "ChatGPT" appeared at the wrong position). The web pages rendered correctly using CSS `unicode-bidi: plaintext`, but RDLC requires XML-level configuration.

#### Decision
Change the report `<Language>` from `en-US` to `ar-SA` and add `<Direction>RTL</Direction>` to task column paragraphs.

#### Rationale
- RDLC uses the report Language to determine base paragraph direction for BiDi text
- `en-US` forces LTR base direction, causing incorrect mixed text ordering
- `ar-SA` sets RTL as default, matching the web page behavior
- `<Direction>RTL</Direction>` on individual paragraphs provides explicit control

---

### DEC-025: Ignore Activity Events While Inactivity Modal Is Shown

**Date:** 2026-03-24 | **Task:** T-14 | **Status:** Implemented

#### Context
The inactivity warning modal closed immediately when the employee moved the mouse to click a button, because `mousemove` was one of the tracked activity events that triggered `updateLastActivity()`, which called `hideInactivityModal()`.

#### Decision
Return early from `updateLastActivity()` when `state.isModalShown` is true. Only the modal's own buttons ("Continue Work" / "End Work") can dismiss it.

#### Rationale
- Mouse movement to interact with the modal is not a "resume work" signal
- Users need to make an explicit choice (continue or end work)
- The countdown timer still runs while the modal is shown

---

### DEC-023: Only Modify Areas/ Folder, Not Areas2/

**Date:** 2026-03-24 | **Task:** All | **Status:** Documented

#### Context
Project has both Areas/ and Areas2/ folders with similar views. Areas2 is excluded from the VS project.

#### Decision
Only apply code changes to Areas/ folder. Areas2 is not part of the active application.

#### Rationale
- Areas2 is excluded from .csproj
- Confirmed by user via Visual Studio Solution Explorer screenshot

---

---

## Sprint 8 Decisions (Mobile API)

---

### DEC-032: Hand-rolled HS256 JWT (no NuGet add)

**Date:** 2026-05-11 | **Slice:** 1 | **Status:** Implemented

#### Context
The project's installed `Microsoft.AspNet.WebApi 4.0.20710.0` corresponds to
WebApi 1.0 (MVC 4 era). Modern JWT libraries (`System.IdentityModel.Tokens.Jwt`)
target WebApi 2 / .NET 4.5.1+. Adding the library would pull a graph of
transitive deps and modify `packages.config`.

#### Decision
Implement HS256 sign + validate manually in `Api/Services/JwtIssuer.cs` and
`Api/Services/JwtValidator.cs` using `System.Security.Cryptography.HMACSHA256`
+ Newtonsoft for the JSON payload. Total ~120 LOC, fully under our control.

#### Rationale
- Zero new dependencies; no risk to existing web behavior
- Constant-time signature comparison (security)
- Easier to audit for a single environment
- Avoids the trap of upgrading WebApi → MVC 5 implicitly

#### Trade-off
- We're responsible for getting the crypto and base64-url-encoding right
- No built-in support for nested tokens / signed assertions (we don't need them)

---

### DEC-033: Direct ADO.NET for the two new tables

**Date:** 2026-05-11 | **Slice:** 1, 6 | **Status:** Implemented

#### Context
The Mobile API adds two tables (`RefreshToken`, `MobileDeviceToken`) but the
brief forbids EF version upgrades or `.edmx` model changes.

#### Decision
Both tables are accessed via direct `SqlConnection` + parameterized SQL inside
`RefreshTokenService.cs` and `DeviceTokenService.cs`. The EDMX model stays
untouched. Pattern mirrors `AttendanceController.LogActivity` which already
uses direct SQL for the `LastHeartbeat` column (also unmapped in EDMX).

#### Rationale
- Avoids EDMX regen risk
- Tables are simple enough that EF would add overhead without benefit
- Same connection-string-derivation trick as `AutoCheckoutJob`

#### Trade-off
- No automatic change tracking — but these tables don't need it
- Future devs touching these tables must use the service layer, not EF

---

### DEC-034: Single AppCode line for FCM fan-out

**Date:** 2026-05-12 | **Slice:** 6 | **Status:** Implemented

#### Context
Every notification in the web (employee accept/reject/complete, time update,
company approve/disapprove, task create, reassign, etc.) already calls
`NotificationHub.Send`. To fan out to FCM we could either inject calls in
each AppCode method, or hook the single sink point.

#### Decision
Inject one call to `FcmDispatcher.Dispatch(...)` at the bottom of
`NotificationHub.Send` (`AppCode/Notification.cs`), after the existing
SignalR `HubContext.Clients.Clients(...).SendGeneralNotification` line. The
call is wrapped in a same-line `try { ... } catch { }` so any FCM failure
cannot disrupt the web's SignalR path.

#### Rationale
- Every existing trigger fans out to FCM automatically — no duplicated logic
- Honors the brief's "only one new line in existing AppCode" constraint
- Slice 6 controllers stay simple; they never touch FCM directly
- Wrapped in try/catch — preserves Risk R9 mitigation

#### Trade-off
- The line is in HIGH-RISK shared code per CLAUDE.md Rule 3
- Mitigated by: try/catch + fire-and-forget dispatch + Risk R4 thread pool

---

### DEC-035: TaskManger.Emp*Task / Company*Task (brief reuse-map deviation)

**Date:** 2026-05-12 | **Slices:** 4, 5 | **Status:** Implemented

#### Context
The master brief's Section 12 reuse map listed:
`Task accept → TaskWorkflow.ChangeTaskStatus(Accept)` (and similar for reject,
complete, approve, disapprove). On inspection, the web's actual employee /
company controllers call:
- `TaskManger.EmpAcceptTask`, `EmpRejectTask`, `EmpFinishTask`
- `TaskManger.CompanyAcceptTask`, `CompanyRejectTask`

These methods set the new status, log, AND **send `NotificationHub.Send`
notifications internally**. `TaskWorkflow.ChangeTaskStatus` does not.

#### Decision
Mobile API task endpoints call the `TaskManger.Emp*Task` / `Company*Task`
methods (matching what the web's `Employee.TasksController` /
`Company.CompanyController` actually do) — NOT `TaskWorkflow.ChangeTaskStatus`.

#### Rationale
- Mirrors web behavior exactly — same status changes, same notifications,
  same TaskTLog rows for both web and mobile users
- Slice 6 FCM fan-out works automatically (notifications fire from inside
  `Emp*Task` / `Company*Task`)
- Avoids dual sources of truth for "what happens on accept"

#### Trade-off
- Deviates from the brief's reuse map (flagged in plan delivery + in CLAUDE.md
  Slice 4/5 notes)

---

### DEC-036: Convention routing (WebApi 1 has no attribute routing)

**Date:** 2026-05-12 | **Slice:** 1 | **Status:** Implemented

#### Context
The brief described `[RoutePrefix("api/v1/...")]` + `[Route(...)]` attribute
routing. WebApi attribute routing was added in WebApi 2 (System.Web.Http 5.x).
This project has WebApi 1 (4.0.20710). Initial attempt to use `[RoutePrefix]`
in `PingApiController` produced compile errors `CS0246: RoutePrefix could
not be found`.

#### Decision
Use convention routing in `App_Start/WebApiConfig.cs`:
- Generic: `api/v1/{controller}/{action}/{id}` (id optional)
- Specific: explicit `MapHttpRoute` entries for `/api/v1/me`, `/api/v1/tasks`,
  `/api/v1/tasks/{id}`, `/api/v1/tasks/{id}/{action}`, `/api/v1/projects`,
  `/api/v1/employees`, `/api/v1/notifications/{id}/read`,
  `/api/v1/notifications/read-all`, `/api/v1/device-tokens/{token}`.
- HTTP-verb constraints via `HttpMethodConstraint` to split GET/POST/DELETE
  cleanly (e.g. GET `/tasks` → List, POST `/tasks` → Create).
- Controller class names drop the "Api" suffix so the `{controller}` token
  matches the URL segment (`AuthController` not `AuthApiController`).

#### Rationale
- No NuGet upgrade required; no risk to existing web behavior
- Existing convention route `api/{controller}/{id}` is left intact
- More verbose `WebApiConfig.cs` is a worthwhile trade for zero deps

#### Trade-off
- Future devs adding endpoints must remember to add the explicit route
- Action names appear in URLs (e.g. `/tasks/{id}/accept`)

---

### DEC-037: AuthValidator wrapper instead of overwriting LoginETask

**Date:** 2026-05-12 | **Slice:** 2 | **Status:** Implemented

#### Context
The web's `UserAccountVM.LoginETask(username, password)` validates credentials
AND mutates `MvcApplication.userData` (session) AND auto-checks-in employees.
The Mobile API needs validation only (session is replaced by JWT) but should
keep the auto-checkin (Q1 = a).

#### Decision
Create `Api/Services/AuthValidator.cs` that mirrors the EF queries of
`LoginETask` (UserAccount lookup with hashed password + tenant check via
`IsApplicationTelesak()` + Employee/Company active state) WITHOUT writing to
the session. Auto-checkin is invoked explicitly from `AuthController.Login`
by calling `new UserAccountVM().Checkin(empId)` (same method the web uses).

#### Rationale
- Preserves `LoginETask` unchanged (HIGH-RISK shared code per CLAUDE.md Rule 3)
- API auth path is isolated — no shared mutation surface
- Auto-checkin behavior identical to web (calls the existing `Checkin` method)

#### Trade-off
- Duplicates a handful of EF query lines — below the 15-line "wrapper vs fork"
  threshold documented in CLAUDE.md

---

### DEC-038: Fire-and-forget FCM via ThreadPool (no async/await)

**Date:** 2026-05-12 | **Slice:** 6 | **Status:** Implemented

#### Context
CLAUDE.md mandates sync-only code. FCM HTTP calls take 50–500ms — blocking
the web/API request inside `NotificationHub.Send` would regress every page
that triggers a notification (Risk R4).

#### Decision
`FcmDispatcher.Dispatch` snapshots inputs as plain values, then queues a
`ThreadPool.QueueUserWorkItem` worker that performs synchronous
`HttpWebRequest` POSTs to FCM. The dispatch call returns immediately. All
exceptions inside the worker are caught and logged to Debug.

#### Rationale
- Honors no-async constraint
- Original request returns within microseconds of the dispatch call
- A failing FCM endpoint cannot stall web pages or API responses

#### Trade-off
- ThreadPool work item is fire-and-forget — no guaranteed retry
- If the worker pool is exhausted, dispatch may be delayed (acceptable;
  push delivery is best-effort by design)

---

### DEC-039: ApiDetailedErrors dev-only flag

**Date:** 2026-05-12 | **Slice:** 3 | **Status:** Implemented

#### Context
The brief required sanitized 500 responses (Arabic message only, no stack
trace). During Slice-2 testing of `/me`, a 500 was returned with no
indication of root cause — guessing burned time.

#### Decision
Add an opt-in `<add key="ApiDetailedErrors" value="true" />` Web.config key.
When `true`, `ApiExceptionFilter` includes the exception type + message
(plus inner exception) in the response body. Default is `false` (sanitized).
Always logs to `System.Diagnostics.Debug.WriteLine` regardless.

#### Rationale
- Diagnostics in dev without touching production behavior
- Same toggle pattern as `customErrors mode=on/off`

#### Trade-off
- Must remember to set `false` in prod configs (default is false in env
  variants `webesnad.config` / `webuat.config` / `webtele.config`)

---

### DEC-040: Synthetic per-request system actor for the internal (key-auth) attachment API

**Date:** 2026-07-08 | **Task:** API-8 | **Status:** Implemented

#### Context
`POST /api/internal/tasks/{id}/attachments` (Ops Portal, authenticated by the
`X-Ops-Portal-Key` shared secret, not JWT) reuses `TaskManger.AttachTaskFile`.
That method calls `LogTask.LogAddAttachment` (→ `LogTaskSingleValue`, which reads
`MvcApplication.userData.isCompany`) and `NotificationHub.Send` (reads
`MvcApplication.userData` at the actor-skip guard). A key-authenticated request
never populates `MvcApplication.userData` (that's `JwtAuthorizeAttribute`'s job on
`/api/v1`), so it is `null` → `NullReferenceException` thrown before `Save()`,
surfaced as `500 SAVE_FAILED`.

#### Decision
Install a synthetic per-request "system" `UserData` (`userId=0`, `isCompany=false`,
`CompanyId=task.CompanyID`) in `InternalAttachmentsController` immediately before the
`AttachTaskFile` call, and clear it in a `finally`. No shared-code changes.

#### Rationale
- `MvcApplication.userData` is stored **per request** (HttpContext Session/Items, see
  `Global.asax.cs`), **not** a process-wide static — so a synthetic value cannot leak
  into any concurrent request. This is what makes the "set a context" approach safe,
  contrary to its initial reputation.
- `userId=0` matches no real employee/company id, so `NotificationHub`'s actor-skip
  guard lets **both** the employee and company notifications through (correct for a
  system-originated upload). Log records `IsFromCompany=false` (agreed system value).
- Touches **only** the internal controller — zero edits to shared `TaskManger` /
  `LogTask` / `Notification`, honoring the shared-code-caution rules in `CLAUDE.md`.

#### Alternatives rejected
- **Internal-safe sibling methods** (`AttachTaskFileInternal` + a `NotificationHub`
  variant without the actor-skip): would duplicate >15 lines of notification fan-out
  logic and edit three shared files — crosses the "fork vs wrap" threshold in `CLAUDE.md`.
- **Null-guarding the shared method bodies**: forbidden (overwriting shared method
  bodies is HIGH-RISK) and would change behavior for every existing caller.

#### Follow-up
- Temporary `[InternalAttachments]` `Debug.WriteLine` diagnostics were added in both
  `SAVE_FAILED` catch blocks to separate disk-write failures from DB/notify failures —
  remove (or route to the permanent logger) after tester sign-off.
- Orphan file written before a failed insert is now deleted in the failure path.

---

## Architecture Decisions

### ADR-001: RDLC for Reports

**Status:** Existing (inherited)

#### Decision
Use RDLC (Report Definition Language Client) for all reports.

#### Rationale
- Native .NET integration
- Client-side rendering
- PDF/Excel export support
- No SQL Server Reporting Services required

---

### ADR-002: ReportAgent Static Class

**Status:** Existing (inherited)

#### Decision
Use `ReportAgent` static class to pass data between Controller and Report.aspx.

#### Rationale
- Simple data transfer mechanism
- Supports DataSources and Parameters
- Existing pattern in codebase
