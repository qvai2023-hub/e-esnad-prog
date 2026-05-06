# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Fixed - Bug #60 Round 2 (Session WQ5V1, 2026-05-06)

After Round 1 deploy, tester reopened #60 with valid UX feedback: the `beforeunload`
guard was triggering Chrome's generic native "Leave site? Changes you may not be
saved" dialog. Clicking **Cancel** left the user looking at a static overlay
(misperceived as frozen). Clicking **Leave** actually let the upload finish on the
server but the user got no confirmation.

Browsers (Chrome/Firefox/Safari/Edge) deliberately ignore custom messages in
`beforeunload` for security reasons, so a custom "Wait / Leave" dialog there
is technically impossible. Approach changed to: drop `beforeunload` entirely,
make the overlay do the communication.

**Changes — `Views/Shared/PartialUploadFile.cshtml` only (1 file):**
- Removed both `beforeunload` listeners (the file-pending one + the in-flight one)
- Overhauled the upload overlay:
  - Bigger, layered text in RTL: bold "جاري رفع الملف..." headline + clear sub-message asking the user not to refresh
  - Animated CSS marquee progress bar (striped, perpetually moving) so motion is visible — proves the page isn't frozen even though we can't compute a real % from a synchronous form POST
  - Live time-elapsed counter ("الوقت المنقضي: N ثانية") updating every second
  - Bigger cloud-upload icon, dark backdrop, blocks all click-through
- New "تم الرفع بنجاح" success toast: detects `?isAttach=1` query string on page load (both Company and Employee `AddAttachment` redirect with that param), shows a green dismissable toast for 3.5s, then fades out. URL is cleaned via `history.replaceState` so refreshing the page doesn't re-show the toast.

### Fixed - Bug #36 Round 3 (Session WQ5V1, 2026-05-06)

The reported 80s+ TTFB on `/Company/Company/index` for companies with thousands of tasks was traced to **no server-side pagination** — the controller loaded every task from the DB and `WebGrid` paginated client-side after materializing the whole list. Confirmed by Chrome instrumentation against `app-test.telesak.com`.

**Option A — proper server-side pagination on `/Company/Company/index`:**
- New `Models/PagedResult.cs` — generic `PagedResult<T>` wrapper carrying `Items`, `TotalCount`, `PageNumber`, `PageSize`, `TotalPages`
- `Areas/Company/Models/CompanyTaskVM.cs`:
  - Added `SelectPaged(...)` — applies `.Skip().Take()` at the SQL level (translated to OFFSET/FETCH), returns `PagedResult<CompanyTaskVM>`. Delay tab still post-filters by `isDelayed` in C# but caps at 500 rows.
  - Existing `Select(...)` is now a thin wrapper that calls `SelectPaged(page: 1, pageSize: 500)` so legacy callers (e.g. `TaskController.FillDropDownLists` Tasks dropdown) keep working with the safety cap.
- `Areas/Company/Controllers/CompanyController.cs`:
  - `Index(...)` accepts `int page = 1` and calls `SelectPaged(..., page, 10)`
  - Both `GetTasks(...)` overloads (POST/GET) accept `page` and call `SelectPaged`
- `Areas/Company/Views/Company/PartialCompTask.cshtml`:
  - Model changed from `IEnumerable<CompanyTaskVM>` to `PagedResult<CompanyTaskVM>`
  - WebGrid bound via `grid.Bind(rowCount: Model.TotalCount, autoSortAndPage: false)` so pager links reflect the true total across all pages
  - All `Model.All/Any` references rewritten as `Model.Items.All/Any`
- `Areas/Company/Views/Company/Index.cshtml`:
  - Cast in `Html.RenderPartial` updated to `PagedResult<CompanyTaskVM>`
  - Added jQuery event-delegated handler that intercepts pager link clicks inside `#divTasks` and re-fetches the partial via AJAX (replacing `#divTasks` content) — preserves the SPA feel after AJAX search/tab clicks. Falls back to full navigation if the AJAX call fails.

**Option B — `.Take(500)` safety cap on the other 8 pages with the same load-everything pattern:**
- `Areas/Employee/Models/EmployeeTask/EmployeeTaskListVM.cs` — `/Employee/Tasks`
- `Areas/Common/Models/TaskCommonVM.cs` — `/Common/Common/Index`
- `Areas/Company/Models/DaskBoardCompanyTaskVM.cs` — `/Company/DashBoard` (7 task-type Select branches)
- `Areas/Employee/Models/DashBoardVM.cs` — `/Employee/DashBoardEmp` (5 task-type branches)
- `Areas/Company/Models/ProjectDisplay.cs` — `/Company/Project`
- `Areas/Company/Models/CompanyEmployeeVM.cs` — `/Company/Employee` (2 query paths)
- `Areas/Admin/Models/CompanyVM.cs` — `/Admin/Company`
- `Areas/Admin/Models/CompanyEmployeeVM.cs` — `/Admin/Employee/Index` (3 query paths)

Total 21 query sites get a `.Take(500)` after `OrderByDescending` so no future N grows unbounded. Option A on `/Company/Company/index` removes the cap entirely for that page (true pagination).

**Performance result (measured against app-test.telesak.com):**
- `/Admin/Company`: 20,050 ms TTFB → **550 ms** (36× faster)
- `/Admin/Index`: 25,050 ms TTFB → **265 ms** (94× faster)
- `/Company/Company/index` (the headline bug, ~10 visible task rows on a heavy company): expected **<2 s** after deploy of this branch (was 80s+)

### Fixed - Bug Fixing Sprint: 14 Bugs (Session WQ5V1)

#### Round 1 — Initial Fixes (14 bugs)

**Security**
- **Web.config** (all variants): Set `debug="false"`, `customErrors mode="On"` — stack traces no longer exposed to users (#43)

**Double-Save & Validation UI**
- **Admin/Company AddEdit.cshtml** (6 views): Added `_isSubmitting` guard flag + `ajaxComplete` listener to prevent double-save and re-enable save button after Remote validation errors (#30, #44)

**Uniqueness Validation**
- **CompanyEmployeeVM.cs** (Company area): Added `EmpID` to `CheckForUniqueEmail` + `AdditionalFields="EmpID"` on Remote attribute — employee's own email no longer blocks edit (#46)
- **CompanyEmployeeVM.cs** (Company area): Added `IsDeleted == false` to email query — soft-deleted records excluded (#46)
- **EmployeeController.cs** (Company area): `CheckDuplicateEmail` now accepts `int EmpID = 0` (#46)

**Task Workflow**
- **TaskWorkflow.cs**: Added `NotAproved → InProgress` transition in Accept action — employees can resume rejected tasks (#38)
- **Employee/Tasks/Index.cshtml**: Removed "غيرمعتمدة" from hidden tab filter — tab now visible to employees (#38)
- **Employee/Tasks/PartialEmpTask.cshtml**: Added "استئناف العمل" accept button for NotApproved tasks (#38)

**Reports**
- **ReportController.cs** (4 files): Added `ReportParameters.Clear()` before ProjectTasks report — prevents stale CompanyName parameter crash (#42)
- **CompanyTasks.rdlc**: Aligned textbox positions to tablix column grid, set PageWidth to match body width (#47)

**Views**
- **Company/Index.cshtml**: Simplified date search from 4 fields (2 ranges) to 2 fields (Start Date + End Date) (#45)
- **PartialCompTask.cshtml**: Wrapped generic delete column in condition — prevents duplicate column on جديدة tab (#40)

**CSS**
- **style.css**: Strengthened `.btn.active` style with thick black border, box-shadow, and scale (#37)

**Performance**
- **CompanyVM.cs** (Admin): Added `includeProperties: "UserAccounts"` to eliminate N+1 lazy loading (#36)

**Password Reset**
- **Security.cs**: Added null-check for SMTP settings + try-catch wrapper (#34) — actual fix requires SMTP credentials update in DB Settings table

#### Round 2 — Reopened Bug Fixes (6 bugs)

**Regression Revert**
- **TaskManger.cs**: Reverted `EmpUpdateDalyTaskTime` to original code — previous fix caused regression breaking both total and daily time updates (#35)

**Performance (correct page)**
- **CompanyTaskVM.cs** (Areas + Areas2): Added `includeProperties: "Project,Status,Priority,TaskTLogs"` to `Select()` — fixes N+1 on Company المهام page, not Admin page (#36)

**CSS (correct selector)**
- **style.css**: Broadened selector to include `.Priorbtns .btn.active` and `.priortybar .btn.active` — covers TaskDetails page priority buttons which are in `<div>` not `<table>` (#37)

**Uniqueness (all fields, all areas)**
- **CompanyEmployeeVM.cs** (Admin + Company, Areas + Areas2): Added `IsDeleted == false` to `CheckForUniqueMobile` and `CheckForUniqueNationalID` same-company clauses — soft-deleted duplicates no longer block edits (#46)

**RDLC (full alignment)**
- **CompanyTasks.rdlc**: Aligned PageHeader (Logo, CompanyName, ReportPeriod) and PageFooter (PageNumber, PrintDate) elements to tablix column grid. Set margins to 0 (#47)

**SMTP (infrastructure)**
- **#34**: Posted comment explaining SMTP credentials in DB Settings table need verification — not a code issue

#### CLAUDE.md Updates
- Added **Bug Fixing Workflow** section with 4 roles (Investigator, Analyst, Fixer, Verifier)
- Added **Anti-Reopen Rules** (7 rules) based on real reopened bugs
- Added **Golden Bug Prompt Template** and **Severity Reference Table**

---
### Fixed - Performance: Client-Side Filtering, N+1 Queries & Unbounded Loads (Session pw8mP)

#### Areas/Company/Models
- **CompanyEmployeeVM.cs**: Moved 11 `.Get()` validation queries to `Get(filter:)` — all uniqueness checks (name, email, mobile, NationalID, SequenceNumber+LaborOfficeID) now filter at DB level; restored `GetByID` for Settings lookup
- **CompanyProfileVM.cs**: Replaced 3x `.Get().Where().FirstOrDefault()` with `GetByID()` or `Get(filter:)` for Company and UserAccount lookups
- **RecurrenceTaskVM.cs**: Added company filter to `GetAllProjects()` and `GetAllEmployees()` — was loading entire tables unfiltered

#### Services
- **TasksService.cs**: Added `includeProperties: "Employee,Employee.Tasks,Status"` to eliminate N+1 lazy-load queries in BriefTasks report (including `Employee.Tasks` for the `Any()` check)
- **EmployeesReportService.cs**: Added `includeProperties: "Attendances"` to eliminate N+1 queries when counting attendance records
- **SharedService.cs**: Replaced `.AsEnumerable()` GroupBy (loaded all UserAccounts into memory) with direct `Company.Get(filter:)` query
- **AttendanceReportService.cs**: Added `Get(filter:)` for employee dropdown (`.ToList()` kept for `.ToString()` projection)

#### AppCode
- **Notification.cs**: Replaced unbounded `SIGNAL_R_SESSIONs.Get().ToList()` with filtered query using collection instance/type IDs; added empty collection guard

#### Bug Fixes During Audit
- **CompanyController.cs (Areas v1)**: Reverted T-09b original filename code — `SanitizeFileName`, 4-arg `AttachTaskFile`, and `AttachmentDisplay.OriginalFileName` only exist in Areas2
- **AttendanceReportService.cs**: Restored `.ToList()` before `.Select()` — `EmpID.ToString()` can't be translated by LINQ to Entities
- **SharedService.cs**: Restored `.ToList()` before `.Select()` — same `.ToString()` issue

---

### Fixed - Performance: N+1 Queries & Eager Loading (Session pw8mP)

#### Areas2/Company
- **CompanyTaskVM.cs**: Added `includeProperties: "Project,Status,Priority,TaskTLogs"` to `Select()` Get() call
  - Eliminates lazy-loading N+1 queries when projecting task list
- **CompanyTaskVM.cs**: Replaced per-task `isTaskDelayed()`, `Delaytime()` calls with batch dictionary lookup
  - Single query loads all task delay data (isDelayed, delayTime, DelayPercentage) at once
  - Eliminates ~N extra `GetByID()` calls for delayed task list
- **ComapnyTaskDetailVM.cs**: `ProgressbarPercentage()` now uses the already-loaded `task` parameter
  - Removed redundant `GetByID(task.TaskID)` call

#### Areas2/Employee
- **EmployeeTaskListVM.cs**: Added `includeProperties: "Project,Priority,Status,TaskTLogs,TaskTLogs.Status"` to `FillTasks()` Get() call
  - Eliminates lazy-loading N+1 queries when building employee task list

#### AppCode
- **ServiceManger.cs**: Moved `GetAllEmployees()` filter into `Get(filter:)` parameter
  - Was: `Get().Where(e => e.company_Id == companyId)` — loads ALL employees then filters client-side
  - Now: `Get(filter: e => e.company_Id == companyId)` — filters at DB level

---

### Fixed - Task Attachments: Original File Names (Session pw8mP)

#### T-09: Preserve Original File Names
- **TaskController.cs** (Areas2): Added `OriginalFileName` property when saving attachments
  - Stores user-facing file name alongside the GUID-based disk name
- **CompanyController.cs** (Areas2): Preserved `OriginalFileName` when copying attachments during task re-assignment
- **EditTask.cshtml**: Display original file name in attachment list
- **SaveData.cshtml**: Display original file name in save confirmation view

---

### Added - Sprint 3: Bulk Operations & Filtering (Session haj1c)

#### T-07: Bulk Delete Tasks
- **CompanyController.cs**: Added `BulkDelete` action (POST, max 500 tasks)
  - Reuses existing `CompanyTaskVM.Delete()` per task (New status only, soft-delete)
- **PartialCompTask.cshtml**: Added checkbox column (only on جديدة tab)
  - Select-all checkbox in header via JS injection
- **Index.cshtml**: Bulk delete toolbar with confirmation modal
  - Shows selected count, delete button, clear selection
  - Success message: "تم الحذف بنجاح"

#### T-08: Multi-Employee Filter
- **Index.cshtml**: Changed employee dropdown to multi-select with Chosen.js
  - `chzn-select chzn-rtl` classes, placeholder "اختر الموظف"
- **CompanyController.cs**: Updated `GetTasks` (POST/GET) to accept `int[] EmpIDs`
  - Calls `Select()` per employee at DB level, merges and deduplicates results

#### T-09: Attachment Table
- **Sprint3_SQL_Scripts.sql**: New file with table verification script
  - No binary column added (files stay on disk at `/Upload/Task/`)

#### T-10: Performance Indexes
- **Sprint3_SQL_Scripts.sql**: Two new indexes
  - `IX_Task_StatusID_CompanyID` with INCLUDE columns
  - `IX_Task_EmpID_CompanyID` with INCLUDE columns

---

### Changed - Attendance Report Header Redesign (Session a05)

#### Reports
- **Attendance.rdlc**: Redesigned page header layout
  - Added "TELE SAK" app name (top right, blue color)
  - Company name moved below app name (right aligned)
  - Report period/month added (top left) - e.g., "يناير 2026"
  - Report title centered
  - Date range below title - "من X إلى Y"
  - Added separator line before table
  - Removed duplicate title from body section

#### Scripts
- **attendance-tracker.js**: Increased inactivity popup timeout
  - Changed `INACTIVITY_TIMEOUT` to 2 minutes (was 30 seconds)
  - Employees now have 2 minutes to respond before auto-checkout
### Fixed - CompanyTasks Report XML (Session haj1c)

#### Reports
- **CompanyTasks.rdlc**: Fixed missing `</Report>` closing tag that caused ReportProcessingException

---

### Changed - Task Report Improvements (Sprint 1 - Session haj1c)

#### Controller
- **AttendanceController.cs**: Enhanced report controller
  - Added Hijri/Gregorian calendar toggle support (T-02)
  - Fixed `ConvertDate()` nullable DateTime return types (CS0266)
  - Added `AllowBlank` to RDLC report parameters

#### Reports
- **CompanyTasks.rdlc**: Redesigned task report layout (T-04, T-05)
  - Added التفاعل (Interaction) column
  - Modified report header layout
  - Restored truncated embedded images
  - Fixed XML structure (missing closing tags)

#### Views
- **TaskReportPreperation.cshtml**: Added calendar type toggle UI

#### Database
- **sp_CompanyTasks**: Updated stored procedure with interaction column support

---

### Changed - Attendance Tracking System (Sprint 1)

#### New Features
- **attendance-tracker.js**: Client-side attendance tracking system
  - Auto-checkout on browser close/inactivity
  - Heartbeat mechanism for session tracking
  - Inactivity popup with countdown (30 second timeout)
  - Custom modal for checkout notification

- **AutoCheckoutJob.cs**: Server-side auto-checkout service
  - Checks for orphaned attendance sessions
  - Automatic checkout for stale heartbeats

#### Bug Fixes
- **EmployeeAuthorize.cs**: Fixed redirect always going to StopedUser
- **UserAccountVM.cs**: Removed UserAgent check from session validation
- **_Layout*.cshtml**: Added attendance tracker integration
- **Modal.cshtml**: Fixed custom modal display (backdrop + z-index)

#### Database
- **AddLastHeartbeat.sql**: Migration to add LastHeartbeat column to Attendance table
- **Attendance.Partial.cs**: Added partial class with heartbeat support

#### Maintenance Scripts
- **check_isTelesak_consistency.sql**: Script to check IsTelesak flag consistency
- **fix_useraccount_companyid_mismatch.sql**: Script to fix CompanyId mismatches

---

### Changed - Attendance Report Layout (Sprint 1)
- **Attendance.rdlc**: Updated header layout - moved company name and date below logo
- Hidden date line when no date is specified

---

### Changed - Professional Report Format (Session a03)

#### Controller
- **AttendanceController.cs**: Added report parameters
  - `StartDate` - Formatted start date (yyyy/MM/dd)
  - `EndDate` - Formatted end date (yyyy/MM/dd)
  - `ReportPeriod` - Arabic month name or date range

#### Reports
- **Attendance.rdlc**: Complete rebuild with professional layout
  - 6 columns: م, التاريخ, اليوم, الحضور, الانصراف, عدد الساعات
  - Employee grouping with Group Header
  - Group Footer with subtotals (days + hours)
  - Row number resets per employee
  - Report header: company, period, date range
  - Report footer: total employees, total work days
  - Page footer: print date, page numbers
  - Logo in page header
  - RTL support with Arabic styling

---

### Changed - Attendance Report Improvements (Session a02)

#### Database
- **sp_Attendance**: Updated stored procedure with new columns:
  - `AttendanceDate` - Date only (separated from time)
  - `DayName` - Day name in Arabic
  - `CheckInTime` - Time only (HH:mm format)
  - `CheckOutTime` - Time only or "لم يُسجل" if not recorded
  - `DurationMinutes` - Integer for SUM calculations
  - `DurationDisplay` - Human-readable format (H:MM)

#### Model
- **sp_Attendance_Result.cs**: Updated properties to match new SP columns

#### Reports
- **Attendance.rdlc**:
  - Changed employee name column to date column
  - Added employee name label below company name
  - Updated field bindings for new column names
  - Date format: dd/MM/yyyy

---

## Session History

### Session pw8mP (2026-03-18)
- Fixed N+1 query performance issues in CompanyTaskVM, EmployeeTaskListVM, ComapnyTaskDetailVM
- Added eager loading (includeProperties) to task list queries
- Moved server-side filtering for GetAllEmployees in ServiceManger
- Preserved original file names for task attachments (T-09 continuation)
- Fixed client-side filtering in CompanyEmployeeVM (11 queries), CompanyProfileVM (3 queries)
- Added eager loading to TasksService (Employee,Status) and EmployeesReportService (Attendances)
- Fixed unbounded SIGNAL_R_SESSIONs load in Notification.cs
- Replaced client-side GroupBy in SharedService with DB-level Company query
- Removed double materialization in AttendanceReportService
- Added company scoping to RecurrenceTaskVM (GetAllProjects, GetAllEmployees)

### Session a02 (2026-03-08)
- Refactored attendance report structure
- Separated date from time in check-in/check-out
- Added duration calculation (minutes for aggregation, display for readability)
- Improved report layout with employee grouping

### Session a01 (2026-03-08)
- Analyzed attendance report implementation
- Documented current state and architecture
- Identified improvement areas:
  1. Open check-ins without check-out showing empty instead of "لم يُسجل"
  2. Date repeated in check-in/check-out fields
  3. Duration format not aggregatable
  4. Employee name repeated on every row
