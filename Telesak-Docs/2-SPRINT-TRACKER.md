# TELE SAK - Sprint Tracker

## Current Status

```
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

### Sprint 0 Files

#### Application Files (To Deploy)

| # | File Path | Action | Priority |
|---|-----------|--------|----------|
| 1 | `EtaskMinstry/Controllers/AttendanceController.cs` | Upload | High |
| 2 | `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Upload | High |
| 3 | `TaskManagementModel/sp_Attendance_Result.cs` | Upload | High |

#### Documentation Files

| # | File Path | Action | Priority |
|---|-----------|--------|----------|
| 4 | `docs/README.md` | Upload | Low |
| 5 | `docs/CHANGELOG.md` | Upload | Low |
| 6 | `docs/SPRINT-TRACKER.md` | Upload | Low |
| 7 | `docs/DECISIONS.md` | Upload | Low |
| 8 | `Telesak-Docs/1-CHANGELOG.md` | Upload | Low |
| 9 | `Telesak-Docs/2-SPRINT-TRACKER.md` | Upload | Low |
| 10 | `Telesak-Docs/3-DECISIONS.md` | Upload | Low |

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
| Sprint 0 | 2026-03-08 | 2026-03-08 | Completed |

---

## Next Sprint (Sprint 1)

*To be planned*
