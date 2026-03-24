# TELE SAK - Sprint Tracker

## Current Status

```
Sprint 5: ████████████████████ 100% Complete
Sprint 4: ████████████████████ 100% Complete
Sprint 3: ████████████████████ 100% Complete
Sprint 1: ████████████████████ 100% Complete
Sprint 0: ████████████████████ 100% Complete
```

---

## Sprint 4: Performance & Attachment Fixes

### Performance Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| PERF-01 | Fix N+1 queries in CompanyTaskVM (delay data) | ✅ Completed | pw8mP | Claude |
| PERF-02 | Add eager loading to CompanyTaskVM Select() | ✅ Completed | pw8mP | Claude |
| PERF-03 | Add eager loading to EmployeeTaskListVM FillTasks() | ✅ Completed | pw8mP | Claude |
| PERF-04 | Fix redundant GetByID in ComapnyTaskDetailVM | ✅ Completed | pw8mP | Claude |
| PERF-05 | Fix client-side filtering in ServiceManger | ✅ Completed | pw8mP | Claude |
| PERF-06 | Fix 11 client-side filtering queries in CompanyEmployeeVM | ✅ Completed | pw8mP | Claude |
| PERF-07 | Fix 3 client-side queries in CompanyProfileVM | ✅ Completed | pw8mP | Claude |
| PERF-08 | Add eager loading to TasksService (BriefTasks report) | ✅ Completed | pw8mP | Claude |
| PERF-08b | Add Employee.Tasks to TasksService includeProperties | ✅ Completed | pw8mP | Claude |
| PERF-09 | Add eager loading to EmployeesReportService | ✅ Completed | pw8mP | Claude |
| PERF-10 | Fix unbounded SIGNAL_R_SESSIONs load in Notification.cs | ✅ Completed | pw8mP | Claude |
| PERF-11 | Fix client-side GroupBy in SharedService | ✅ Completed | pw8mP | Claude |
| PERF-12 | Add Get(filter:) to AttendanceReportService | ✅ Completed | pw8mP | Claude |
| PERF-13 | Add company scoping to RecurrenceTaskVM queries | ✅ Completed | pw8mP | Claude |

### Attachment Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| T-09b | Preserve original file names for attachments (Areas2 only) | ✅ Completed | pw8mP | Claude |
| FIX-01 | Revert T-09b code from Areas v1 CompanyController | ✅ Completed | pw8mP | Claude |
| FIX-02 | Restore .ToList() for .ToString() in AttendanceReport/SharedService | ✅ Completed | pw8mP | Claude |

### Progress

- [x] PERF-01: Batch delay data lookup
- [x] PERF-02: CompanyTaskVM eager loading
- [x] PERF-03: EmployeeTaskListVM eager loading
- [x] PERF-04: Remove redundant GetByID
- [x] PERF-05: Server-side filtering
- [x] PERF-06: CompanyEmployeeVM client-side filtering (11 queries)
- [x] PERF-07: CompanyProfileVM client-side queries (3 queries)
- [x] PERF-08: TasksService eager loading (Employee,Employee.Tasks,Status)
- [x] PERF-09: EmployeesReportService eager loading (Attendances)
- [x] PERF-10: Notification.cs unbounded session load
- [x] PERF-11: SharedService AsEnumerable GroupBy
- [x] PERF-12: AttendanceReportService Get(filter:) + .ToList() for .ToString()
- [x] FIX-01: Reverted T-09b from Areas v1 CompanyController
- [x] FIX-02: Restored .ToList() for .ToString() in AttendanceReport + SharedService
- [x] PERF-13: RecurrenceTaskVM company scoping
- [x] T-09b: Original file names

---

## Sprint 3: Bulk Operations & Filtering

### Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| T-07 | Bulk Delete Tasks (New status only, max 500) | ✅ Completed | haj1c | Claude |
| T-08 | Multi-Employee Filter (Chosen.js) | ✅ Completed | haj1c | Claude |
| T-09 | Attachment Table SQL Script | ✅ Completed | haj1c | Claude |
| T-10 | Performance Index on Task table | ✅ Completed | haj1c | Claude |

### Progress

- [x] T-07: Bulk Delete
- [x] T-08: Multi-Employee Filter
- [x] T-09: Attachment SQL Script
- [x] T-10: Performance Indexes

---

## Sprint 0: Attendance Report Improvements

### Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| A-01 | Analyze attendance report & document | Completed | a01 | Claude |
| A-02 | Update SP & model with new columns | Completed | a02 | User + Claude |
| A-03 | Rebuild RDLC with professional layout | Completed | a03 | Claude |

### Progress

- [x] A-01: Analysis & Documentation
- [x] A-02: Data Structure (SP + Model)
- [x] A-03: Report Layout (RDLC)

---

## Files to Upload

### Sprint 5 Files

#### Application Files (To Deploy)

| # | File Path | Task | Priority |
|---|-----------|------|----------|
| 1 | `EtaskMinstry/Areas/Company/Views/Report/TaskReportPreperation.cshtml` | T-10 | High |
| 2 | `EtaskMinstry/Areas/Employee/Views/Report/TaskReportPreperation.cshtml` | T-10 | High |
| 3 | `EtaskMinstry/Areas/Company/Views/Report/EmployeeReport.cshtml` | T-10 | High |
| 4 | `EtaskMinstry/Areas/Company/Views/Report/TasksByMonthsChart.cshtml` | T-11 | High |
| 5 | `EtaskMinstry/Areas/Company/Controllers/ReportController.cs` | T-11 | High |
| 6 | `EtaskMinstry/Areas/Company/Models/YearlyTasksChart.cs` | T-11 | High |
| 7 | `EtaskMinstry/Content/Main/Developers.css` | T-12 | High |
| 8 | `EtaskMinstry/Areas/Company/Views/Company/PartialCompTask.cshtml` | T-12 | High |
| 9 | `EtaskMinstry/Areas/Company/Views/DashBoard/PartialDBCompTask.cshtml` | T-12 | High |
| 10 | `EtaskMinstry/Areas/Employee/Views/Tasks/PartialEmpTask.cshtml` | T-12 | High |
| 11 | `EtaskMinstry/Areas/Common/Views/Common/PartialTaskCommon.cshtml` | T-12 | High |
| 12 | `EtaskMinstry/Areas/Company/Models/DaskBoardCompanyTaskVM.cs` | T-13 | High |
| 13 | `EtaskMinstry/Views/Shared/_Layout.cshtml` | T-13 | Medium |
| 14 | `EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml` | T-13 | Medium |

#### Database Scripts (Optional)

| # | Script | Action | Priority |
|---|--------|--------|----------|
| 1 | `Telesak-Docs/Sprint5_SQL_Indexes.sql` | Execute in SSMS (optional) | Low |

### Sprint 4 Files

#### Application Files (To Deploy)

| # | File Path | Action | Priority |
|---|-----------|--------|----------|
| 1 | `EtaskMinstry/Areas2/Company/Models/CompanyTaskVM.cs` | Upload | High |
| 2 | `EtaskMinstry/Areas2/Company/Models/ComapnyTaskDetailVM.cs` | Upload | High |
| 3 | `EtaskMinstry/Areas2/Employee/Models/EmployeeTask/EmployeeTaskListVM.cs` | Upload | High |
| 4 | `EtaskMinstry/AppCode/ServiceManger.cs` | Upload | High |
| 5 | `EtaskMinstry/Areas2/Company/Controllers/TaskController.cs` | Upload | High |
| 6 | `EtaskMinstry/Areas2/Company/Controllers/CompanyController.cs` | Upload | High |
| 7 | `EtaskMinstry/Areas2/Company/Views/Company/EditTask.cshtml` | Upload | Medium |
| 8 | `EtaskMinstry/Areas2/Company/Views/Company/SaveData.cshtml` | Upload | Medium |
| 9 | `EtaskMinstry/Areas/Company/Models/CompanyEmployeeVM.cs` | Upload | High |
| 10 | `EtaskMinstry/Areas/Company/Models/CompanyProfileVM.cs` | Upload | High |
| 11 | `EtaskMinstry/Areas/Company/Models/RecurrenceTaskVM.cs` | Upload | High |
| 12 | `EtaskMinstry/Services/TasksService.cs` | Upload | High |
| 13 | `EtaskMinstry/Services/EmployeesReportService.cs` | Upload | High |
| 14 | `EtaskMinstry/Services/SharedService.cs` | Upload | High |
| 15 | `EtaskMinstry/Services/AttendanceReportService.cs` | Upload | High |
| 16 | `EtaskMinstry/AppCode/Notification.cs` | Upload | High |
| 17 | `EtaskMinstry/Areas/Company/Controllers/CompanyController.cs` | Upload | High |

### Sprint 3 Files

#### Application Files (To Deploy)

| # | File Path | Action | Priority |
|---|-----------|--------|----------|
| 1 | `EtaskMinstry/Areas/Company/Controllers/CompanyController.cs` | Upload | High |
| 2 | `EtaskMinstry/Areas/Company/Views/Company/PartialCompTask.cshtml` | Upload | High |
| 3 | `EtaskMinstry/Areas/Company/Views/Company/Index.cshtml` | Upload | High |

#### Database Scripts (To Execute)

| # | Script | Action | Priority |
|---|--------|--------|----------|
| 1 | `docs/Sprint3_SQL_Scripts.sql` | Execute in SSMS | High |

### Sprint 1 Files

#### Application Files (To Deploy)

| # | File Path | Action | Priority |
|---|-----------|--------|----------|
| 1 | `EtaskMinstry/ReportsRDLC/CompanyTasks.rdlc` | Upload | High |
| 2 | `EtaskMinstry/Views/Report/TaskReportPreperation.cshtml` | Upload | High |
| 3 | `EtaskMinstry/Controllers/AttendanceController.cs` | Upload | High |
| 4 | `EtaskMinstry/Scripts/attendance-tracker.js` | Upload | High |
| 5 | `EtaskMinstry/Services/AutoCheckoutJob.cs` | Upload | High |
| 6 | `EtaskMinstry/CustomAttrbutes/EmployeeAuthorize.cs` | Upload | High |
| 7 | `EtaskMinstry/Models/Login/UserAccountVM.cs` | Upload | High |
| 8 | `EtaskMinstry/Global.asax.cs` | Upload | High |
| 9 | `EtaskMinstry/Views/Shared/_Layout.cshtml` | Upload | High |
| 10 | `EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml` | Upload | High |
| 11 | `EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml` | Upload | High |
| 12 | `EtaskMinstry/Views/Shared/Modal.cshtml` | Upload | High |
| 13 | `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Upload | Medium |
| 14 | `TaskManagementModel/Attendance.Partial.cs` | Upload | High |
| 15 | `TaskManagementModel/sp_Attendance_Result.cs` | Upload | High |

#### Database Scripts (To Execute)

| # | Script | Action | Priority |
|---|--------|--------|----------|
| 1 | `sp_CompanyTasks` (updated) | Execute in SSMS | High |
| 2 | `TaskManagementModel/Migrations/AddLastHeartbeat.sql` | Execute in SSMS | High |

#### Maintenance Scripts

| # | Script | Action | Priority |
|---|--------|--------|----------|
| 1 | `scripts/check_isTelesak_consistency.sql` | Execute as needed | Low |
| 2 | `scripts/fix_useraccount_companyid_mismatch.sql` | Execute as needed | Low |

### Sprint 0 Files

#### Application Files (To Deploy)

| # | File Path | Action | Priority |
|---|-----------|--------|----------|
| 1 | `EtaskMinstry/Controllers/AttendanceController.cs` | Upload | High |
| 2 | `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Upload | High |
| 3 | `TaskManagementModel/sp_Attendance_Result.cs` | Upload | High |

#### Database Scripts (To Execute)

| # | Script | Action | Priority |
|---|--------|--------|----------|
| 1 | `sp_Attendance` (updated) | Execute in SSMS | High |

---

## Issues Resolved

| # | Issue | Solution | Task |
|---|-------|----------|------|
| 1 | Empty check-out shows blank | Display "لم يُسجل" | A-02 |
| 2 | Date in check-in/out fields | Separate AttendanceDate column | A-02 |
| 3 | Duration not summable | DurationMinutes (INT) | A-02 |
| 4 | Employee name repeated | Employee grouping | A-03 |

---

## Sprint History

| Sprint | Start | End | Status |
|--------|-------|-----|--------|
| Sprint 5 | 2026-03-24 | 2026-03-24 | Completed |
| Sprint 4 | 2026-03-18 | 2026-03-18 | Completed |
| Sprint 3 | 2026-03-17 | 2026-03-17 | Completed |
| Sprint 1 | 2026-03-09 | 2026-03-16 | Completed |
| Sprint 0 | 2026-03-08 | 2026-03-08 | Completed |

---

## Sprint 1: Task Report & Attendance Tracking

### Task Report Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| T-02 | Calendar Type Toggle (Hijri/Gregorian) | ✅ Completed | haj1c | Claude |
| T-04 | Add Interaction Column (التفاعل) | ✅ Completed | haj1c | Claude |
| T-05 | Report Header Modification | ✅ Completed | haj1c | Claude |
| FIX | CompanyTasks.rdlc XML fix (missing closing tag) | ✅ Completed | haj1c | Claude |

### Attendance Tracking Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| ATT-01 | Client-side attendance tracker (JS) | ✅ Completed | earlier | Claude |
| ATT-02 | Server-side auto-checkout job | ✅ Completed | earlier | Claude |
| ATT-03 | Fix EmployeeAuthorize redirect bug | ✅ Completed | earlier | Claude |
| ATT-04 | Remove UserAgent session check | ✅ Completed | earlier | Claude |
| ATT-05 | Fix modal display issues | ✅ Completed | earlier | Claude |
| ATT-06 | Layout integration (tracker + modal) | ✅ Completed | earlier | Claude |
| ATT-07 | Inactivity popup timeout (30s) | ✅ Completed | earlier | Claude |
| ATT-08 | Attendance report header update | ✅ Completed | earlier | Claude |

### Progress

- [x] T-02: Calendar Type Toggle
- [x] T-04: Interaction Column
- [x] T-05: Report Header
- [x] FIX: CompanyTasks.rdlc XML fix
- [x] ATT: Attendance Tracking System

---

## Sprint 5: UX & Performance (T-10 + T-11 + T-12 + T-13)

### Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| T-10 | Remove Duplicate Date Filters (إزالة تكرار فلاتر التاريخ) | ✅ Completed | eFE3C | Claude |
| T-11 | Add Month Filter in Statistics (فلتر الشهر في الإحصائيات) | ✅ Completed | eFE3C | Claude |
| T-12 | Fix BiDi (Arabic/English Mixed Text) | ✅ Completed | eFE3C | Claude |
| T-13 | Performance Improvements (تحسين الأداء) | ✅ Completed | eFE3C | Claude |

### Progress

- [x] T-10: Remove duplicate date filters
- [x] T-11: Add month filter in statistics
- [x] T-12: Fix BiDi mixed text
- [x] T-13: Performance improvements
