# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Fixed - Performance: Client-Side Filtering, N+1 Queries & Unbounded Loads (Session pw8mP)

#### Areas/Company/Models
- **CompanyEmployeeVM.cs**: Moved 11 `.Get()` validation queries to `Get(filter:)` — all uniqueness checks (name, email, mobile, NationalID, SequenceNumber+LaborOfficeID) now filter at DB level; restored `GetByID` for Settings lookup
- **CompanyProfileVM.cs**: Replaced 3x `.Get().Where().FirstOrDefault()` with `GetByID()` or `Get(filter:)` for Company and UserAccount lookups
- **RecurrenceTaskVM.cs**: Added company filter to `GetAllProjects()` and `GetAllEmployees()` — was loading entire tables unfiltered

#### Services
- **TasksService.cs**: Added `includeProperties: "Employee,Status"` to eliminate N+1 lazy-load queries in BriefTasks report
- **EmployeesReportService.cs**: Added `includeProperties: "Attendances"` to eliminate N+1 queries when counting attendance records
- **SharedService.cs**: Replaced `.AsEnumerable()` GroupBy (loaded all UserAccounts into memory) with direct `Company.Get(filter:)` query
- **AttendanceReportService.cs**: Added `Get(filter:)` for employee dropdown (`.ToList()` kept for `.ToString()` projection)

#### AppCode
- **Notification.cs**: Replaced unbounded `SIGNAL_R_SESSIONs.Get().ToList()` with filtered query using collection instance/type IDs; added empty collection guard

#### Services (continued)
- **TasksService.cs**: Added `Employee.Tasks` to includeProperties to prevent N+1 on `a.Employee.Tasks.Any(...)` check

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
