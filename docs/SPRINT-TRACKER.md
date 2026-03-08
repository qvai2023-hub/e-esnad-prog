# Sprint Tracker

## Current Sprint: Attendance Report Improvements

### Sprint Goal
Improve the attendance report to be more readable and support data aggregation.

---

## Task Status

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
| 11 | Test and verify changes | Pending | - | User to test |

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

## Next Steps
- [ ] Test report with real data
- [x] Verify grouping by employee (Implemented in a03)
- [x] Confirm duration SUM works correctly (Implemented in a03)
- [x] Add total hours per employee (Implemented in a03)
