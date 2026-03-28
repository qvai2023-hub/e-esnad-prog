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
