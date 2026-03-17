# TELE SAK - Changelog

All notable changes to the TELE SAK project will be documented in this file.

---

## Sprint 0 - Attendance Report Improvements

### A-05: Report Header Redesign & Inactivity Timeout
**Status:** Completed
**Date Completed:** 2026-03-16

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Modified | Redesigned page header layout |
| `EtaskMinstry/Scripts/attendance-tracker.js` | Modified | Increased inactivity popup timeout |

#### Report Changes:
- Added "TELE SAK" app name (top right, blue #3d85c6)
- Company name below app name (right aligned)
- Report period/month (top left) - e.g., "يناير 2026"
- Report title centered
- Date range "من X إلى Y" below title
- Added separator line before table
- Removed duplicate title from body

#### Attendance Tracker Changes:
- Changed `INACTIVITY_TIMEOUT` to 2 minutes
- Employees now have 2 minutes to respond to inactivity warning before auto-checkout

---

### A-03: Professional Report Format
**Status:** Completed
**Date Completed:** 2026-03-08

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/Controllers/AttendanceController.cs` | Modified | Added report parameters (StartDate, EndDate, ReportPeriod) |
| `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Modified | Complete rebuild with professional layout |

#### Changes:
- Added 3 report parameters for date range display
- Rebuilt RDLC with 6 columns: م, التاريخ, اليوم, الحضور, الانصراف, عدد الساعات
- Employee grouping with Group Header/Footer
- Subtotals per employee (days + hours)
- Report footer with totals
- Page footer with print date and page numbers
- Blue color scheme (#3d85c6)
- RTL Arabic support

---

### A-02: Data Structure Improvements
**Status:** Completed
**Date Completed:** 2026-03-08

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `TaskManagementModel/sp_Attendance_Result.cs` | Modified | New columns for SP result |
| `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Modified | Updated field bindings |

#### Database Changes:
| Object | Type | Description |
|--------|------|-------------|
| `sp_Attendance` | Stored Procedure | Updated with new columns |

#### New SP Columns:
- `AttendanceDate` (DATE) - Separated date
- `DayName` (NVARCHAR) - Arabic day name
- `CheckInTime` (VARCHAR) - Time only (HH:mm)
- `CheckOutTime` (VARCHAR) - Time or "لم يُسجل"
- `DurationMinutes` (INT) - For aggregation
- `DurationDisplay` (VARCHAR) - Human readable (H:MM)

---

### A-01: Analysis & Documentation
**Status:** Completed
**Date Completed:** 2026-03-08

#### Files Created:
| File | Type | Description |
|------|------|-------------|
| `docs/README.md` | New | Project documentation |
| `docs/CHANGELOG.md` | New | Change log |
| `docs/SPRINT-TRACKER.md` | New | Sprint tracking |
| `docs/DECISIONS.md` | New | Technical decisions |

#### Analysis Results:
Identified 4 issues in the attendance report:
1. Empty check-out field instead of "لم يُسجل"
2. Date repeated in check-in/check-out
3. Duration not aggregatable (string format)
4. Employee name repeated on every row

---

## Database Change Summary

### Tables Modified
*None*

### Tables Created
*None (ActivityLog was created in earlier sprint)*

### Stored Procedures Modified
| SP Name | Changes |
|---------|---------|
| `sp_Attendance` | Added 6 new columns for improved report data |

### SQL Jobs
*None*

---

## Sprint 0 Summary

| Task | Status | Files | DB Changes |
|------|--------|-------|------------|
| A-01 | Completed | 4 new docs | None |
| A-02 | Completed | 2 modified | sp_Attendance updated |
| A-03 | Completed | 2 modified | None |

**Total Files:** 8 (4 new + 4 modified)
