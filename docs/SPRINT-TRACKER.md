# Sprint Tracker

## Current Sprint: Sprint 4 - Performance & Attachment Fixes

### Sprint Goal
Fix N+1 query performance issues, add eager loading, and preserve original file names for task attachments.

---

## Sprint 4 Task Status

| # | Task | Status | Session | Notes |
|---|------|--------|---------|-------|
| T-09b | Preserve original file names for attachments | Done | pw8mP | OriginalFileName in Areas2 TaskController, CompanyController, views (Areas v1 reverted) |
| PERF-01 | Fix N+1 queries in CompanyTaskVM (delay data) | Done | pw8mP | Batch dictionary lookup replaces per-task GetByID |
| PERF-02 | Add eager loading to CompanyTaskVM Select() | Done | pw8mP | includeProperties: Project,Status,Priority,TaskTLogs |
| PERF-03 | Add eager loading to EmployeeTaskListVM FillTasks() | Done | pw8mP | includeProperties: Project,Priority,Status,TaskTLogs,TaskTLogs.Status |
| PERF-04 | Fix redundant GetByID in ComapnyTaskDetailVM | Done | pw8mP | Use already-loaded task object |
| PERF-05 | Fix client-side filtering in ServiceManger | Done | pw8mP | Move filter into Get() call |
| PERF-06 | Fix 11 client-side filtering queries in CompanyEmployeeVM | Done | pw8mP | All uniqueness checks now use Get(filter:) |
| PERF-07 | Fix 3 client-side queries in CompanyProfileVM | Done | pw8mP | Replaced Get().Where() with GetByID / Get(filter:) |
| PERF-08 | Add eager loading to TasksService (BriefTasks report) | Done | pw8mP | includeProperties: Employee,Employee.Tasks,Status |
| PERF-09 | Add eager loading to EmployeesReportService | Done | pw8mP | includeProperties: Attendances |
| PERF-10 | Fix unbounded SIGNAL_R_SESSIONs load in Notification.cs | Done | pw8mP | Filter by collection instance/type IDs at DB level |
| PERF-11 | Fix client-side GroupBy in SharedService | Done | pw8mP | Replace AsEnumerable() with Company.Get(filter:) |
| PERF-12 | Add Get(filter:) to AttendanceReportService | Done | pw8mP | Server-side filter; .ToList() kept for .ToString() projection |
| PERF-13 | Add company scoping to RecurrenceTaskVM queries | Done | pw8mP | GetAllProjects and GetAllEmployees now filter by company |

| FIX-03 | Fix session timeout countdown display (00 : 2 → 00:02) | Done | eFE3C | Added dir="ltr" to `<strong id="inactivityCountdown">` in attendance-tracker.js line 285 |

---

## Sprint 5 — CHATBOT (شات بوت) ✅

| # | Task | Status | Session | Notes |
|---|------|--------|---------|-------|
| 1 | Updated welcome message text | Done | eFE3C | New 4-line Arabic text |
| 2 | Removed emoji icons from all quick button labels | Done | eFE3C | Text only, no emojis |
| 3 | Login tab now shows single button only (تغيير الإيميل) | Done | eFE3C | Removed خطأ في الدخول + نسيت كلمة المرور |
| 4 | Removed ▶ icon from video link answer | Done | eFE3C | URL only in telesak-qa.json |
| 5 | Merged login Q&A into one entry "مشكلة في تسجيل الدخول" | Done | eFE3C | Combined triggers from both entries |
| 6 | Updated WhatsApp number to 966568786846 | Done | eFE3C | wa.me/966568786846 |

### Sprint 5 Files Modified
- `EtaskMinstry/Scripts/telesak-chat.js` — welcome text, emojis, buttons, merged Q&A
- `EtaskMinstry/App_Data/telesak-qa.json` — merged login entries, removed video emoji, WhatsApp number

All changes tested and confirmed working ✅

---

## Sprint 4 Files Modified

### Session pw8mP
- `Areas2/Company/Models/CompanyTaskVM.cs` - Eager loading + batch delay data lookup
- `Areas2/Company/Models/ComapnyTaskDetailVM.cs` - Removed redundant GetByID
- `Areas2/Employee/Models/EmployeeTask/EmployeeTaskListVM.cs` - Eager loading
- `AppCode/ServiceManger.cs` - Server-side filtering for GetAllEmployees
- `Areas2/Company/Controllers/TaskController.cs` - OriginalFileName on upload
- `Areas2/Company/Controllers/CompanyController.cs` - OriginalFileName on re-assignment copy
- `Areas2/Company/Views/Company/EditTask.cshtml` - Display original file name
- `Areas2/Company/Views/Company/SaveData.cshtml` - Display original file name
- `Areas/Company/Models/CompanyEmployeeVM.cs` - 11 queries moved to server-side filtering
- `Areas/Company/Models/CompanyProfileVM.cs` - 3 queries moved to GetByID / Get(filter:)
- `Areas/Company/Models/RecurrenceTaskVM.cs` - Company-scoped project + employee queries
- `Services/TasksService.cs` - Eager loading for BriefTasks report
- `Services/EmployeesReportService.cs` - Eager loading for Attendances
- `Services/SharedService.cs` - DB-level company query replaces client-side GroupBy
- `Services/AttendanceReportService.cs` - Removed double materialization
- `AppCode/Notification.cs` - Filtered SignalR session query

---

## Previous Sprint: Sprint 3 - Bulk Operations & Filtering

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
| 3b | T-05: Add date range to CompanyTasks title header | Done | eFE3C | Title now shows "تقرير المهام — من X إلى Y", FooterDateRange hidden |
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
| 12 | Remove Arabic AM/PM (ص/م) from attendance times | Done | eFE3C | Removed ConvertTo12HourArabic() calls in AttendanceReportService.cs |
| 13 | Add ميلادي/هجري calendar toggle to AttendanceReport | Done | eFE3C | Dropdown + initCalendar() JS + Hijri date conversion in controller |
| 14 | Fix Attendance report layout to match CompanyTasks | Done | eFE3C | Logo added, blue title header, date range in title, removed duplicate date elements in Attendance.rdlc |

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
- [ ] Plan Sprint 5
- [x] Run Sprint 3 SQL scripts on database
- [x] Test bulk delete on جديدة tab
- [x] Test multi-employee filter with Chosen.js
- [ ] Verify eager loading doesn't change query results (compare task list pages)
- [ ] Verify original file names display correctly in EditTask and SaveData views
- [ ] Load test task list pages to confirm performance improvement
- [ ] Test employee CRUD validations (duplicate name/email/mobile/NationalID checks)
- [ ] Test company profile view/edit/change email/change address
- [ ] Test BriefTasks report and EmployeesReport with attendance counts
- [ ] Test notifications via SignalR (assign task, verify delivery)
- [ ] Test admin company dropdown (SharedService provider filter)
- [ ] Test attendance report employee dropdown
- [ ] Test recurrence task creation (verify projects/employees show only current company)
