# Sprint Tracker

## Current Sprint: Sprint 3 - Bulk Operations & Filtering

### Sprint Goal
Add bulk task deletion, multi-employee filtering, and database performance improvements.

---

## Sprint 3 Task Status

| # | Task | Status | Session | Notes |
|---|------|--------|---------|-------|
| T-07 | Bulk Delete Tasks (New status only, max 500) | Done | haj1c | Checkbox column on جديدة tab + confirmation modal + "تم الحذف بنجاح" |
| T-08 | Multi-Employee Filter (Chosen.js) | Done | haj1c | Multi-select dropdown + per-employee DB query + merge results |
| T-09 | Attachment Table SQL Script | Done | haj1c | SQL script provided, no binary column, files stay on disk |
| T-10 | Performance Index on Task table | Done | haj1c | IX_Task_StatusID_CompanyID + IX_Task_EmpID_CompanyID |

---

## Sprint 3 Files Modified

### Session haj1c
- `Areas/Company/Controllers/CompanyController.cs` - BulkDelete action + multi-employee GetTasks
- `Areas/Company/Views/Company/PartialCompTask.cshtml` - Checkbox column (New tab only) + select-all JS
- `Areas/Company/Views/Company/Index.cshtml` - Bulk delete toolbar + Chosen.js multi-select + JS handlers
- `docs/Sprint3_SQL_Scripts.sql` - New: SQL scripts for indexes (T-10) and Attachment verification (T-09)

### Database (User to Run)
- `docs/Sprint3_SQL_Scripts.sql` - Performance indexes

---

## Previous Sprint: Sprint 1 - Task Report & Attendance Tracking

### Sprint Goal
Improve task report (CompanyTasks) with interaction column and calendar toggle, plus build attendance tracking system.

---

## Sprint 1 Task Status

| # | Task | Status | Session | Notes |
|---|------|--------|---------|-------|
| 1 | Calendar Type Toggle (Hijri/Gregorian) T-02 | Done | haj1c | ConvertDate + UI toggle |
| 2 | Add Interaction Column (التفاعل) T-04 | Done | haj1c | SP + RDLC updated |
| 3 | Report Header Modification T-05 | Done | haj1c | Redesigned layout |
| 4 | Update sp_CompanyTasks stored procedure | Done | haj1c | User updated in SSMS |
| 5 | Fix CompanyTasks.rdlc XML (missing closing tag) | Done | haj1c | Was causing ReportProcessingException |
| 6 | Fix CS0266 nullable DateTime returns | Done | haj1c | Added .Value to ConvertDate() |
| 7 | Restore truncated embedded images | Done | haj1c | Base64 image data restored |
| 8 | Add AllowBlank to report parameters | Done | haj1c | Prevents null parameter errors |
| 9 | Build attendance-tracker.js | Done | earlier | Client-side heartbeat + auto-checkout |
| 10 | Build AutoCheckoutJob.cs | Done | earlier | Server-side orphaned session cleanup |
| 11 | Fix EmployeeAuthorize redirect bug | Done | earlier | Was always redirecting to StopedUser |
| 12 | Remove UserAgent session check | Done | earlier | Was causing false session invalidation |
| 13 | Fix modal display issues | Done | earlier | Backdrop + z-index |
| 14 | Integrate tracker in layouts | Done | earlier | _Layout, _LayoutNewDesign, _LayoutNoSearch |
| 15 | Increase inactivity popup timeout | Done | earlier | Changed to 30 seconds |
| 16 | Update Attendance report header | Done | earlier | Company name & date below logo |

---

## Previous Sprint: Sprint 0 - Attendance Report Improvements

### Sprint Goal
Improve the attendance report to be more readable and support data aggregation.

---

## Sprint 0 Task Status

| # | Task | Status | Session | Notes |
|---|------|--------|---------|-------|
| 1 | Analyze current attendance report | Done | a01 | Identified 4 issues |
| 2 | Update sp_Attendance stored procedure | Done | a02 | User updated in SSMS |
| 3 | Update sp_Attendance_Result.cs model | Done | a02 | New columns added |
| 4 | Update Attendance.rdlc report fields | Done | a02 | Fields updated |
| 5 | Update report layout (date column) | Done | a02 | Employee -> Date |
| 6 | Add employee name header | Done | a02 | Below company name |
| 7 | Rebuild RDLC with professional layout | Done | a03 | Full grouping + styling |
| 8 | Add report parameters (dates) | Done | a03 | StartDate, EndDate, Period |
| 9 | Add employee grouping with subtotals | Done | a03 | Days + Hours per employee |
| 10 | Add report totals footer | Done | a03 | Total employees + days |
| 11 | Test and verify changes | Done | - | Verified by user |

---

## Issues Addressed

| # | Issue | Solution | Status |
|---|-------|----------|--------|
| 1 | Check-in without Check-out shows empty | SP returns "لم يُسجل" | Done |
| 2 | Date repeated in check-in/check-out | Separate AttendanceDate column | Done |
| 3 | Duration not aggregatable | DurationMinutes (INT) + DurationDisplay | Done |
| 4 | Employee name repeated every row | Employee as group header | Done |

---

## Files Modified

### Session a03
- `EtaskMinstry/Controllers/AttendanceController.cs` - Added report parameters
- `EtaskMinstry/ReportsRDLC/Attendance.rdlc` - Complete rebuild

### Session a02
- `TaskManagementModel/sp_Attendance_Result.cs`
- `EtaskMinstry/ReportsRDLC/Attendance.rdlc`

### Database (User Modified)
- `sp_Attendance` stored procedure

---

## Sprint 1 Files Modified

### Session haj1c (Task Report)
- `EtaskMinstry/ReportsRDLC/CompanyTasks.rdlc` - Redesigned + XML fix
- `EtaskMinstry/Views/Report/TaskReportPreperation.cshtml` - Calendar toggle
- `EtaskMinstry/Controllers/AttendanceController.cs` - Hijri/Gregorian + fixes
- `Telesak-Docs/sp_CompanyTasks_update.sql` - SP update script

### Earlier Sessions (Attendance Tracking)
- `EtaskMinstry/Scripts/attendance-tracker.js` - New
- `EtaskMinstry/Services/AutoCheckoutJob.cs` - New
- `TaskManagementModel/Attendance.Partial.cs` - New
- `TaskManagementModel/Migrations/AddLastHeartbeat.sql` - New
- `EtaskMinstry/CustomAttrbutes/EmployeeAuthorize.cs` - Fixed
- `EtaskMinstry/Models/Login/UserAccountVM.cs` - Fixed
- `EtaskMinstry/Global.asax.cs` - Modified
- `EtaskMinstry/Views/Shared/_Layout.cshtml` - Modified
- `EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml` - Modified
- `EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml` - Modified
- `EtaskMinstry/Views/Shared/Modal.cshtml` - Fixed
- `EtaskMinstry/ReportsRDLC/Attendance.rdlc` - Header update

### Database (User Modified)
- `sp_CompanyTasks` stored procedure

---

## Next Steps
- [ ] Plan Sprint 4
- [x] Run Sprint 3 SQL scripts on database
- [x] Test bulk delete on جديدة tab
- [x] Test multi-employee filter with Chosen.js
