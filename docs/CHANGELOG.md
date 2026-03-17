# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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
