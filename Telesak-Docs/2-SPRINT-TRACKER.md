# TELE SAK - Sprint Tracker

## Current Status

```
Sprint 1: ████████████████████ 100% Complete
Sprint 0: ████████████████████ 100% Complete
```

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

## Next Sprint (Sprint 2)

*To be planned*
