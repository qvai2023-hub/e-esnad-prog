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
