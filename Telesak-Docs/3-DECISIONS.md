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
