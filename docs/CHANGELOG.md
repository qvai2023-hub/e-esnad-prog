# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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
