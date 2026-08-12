# TELE SAK - Sprint Tracker

## Current Status

```
Sprint 8: █████████████████░░░ ~86% — Slices 1–6 done; Slice 7 (Comments+Attachments) NOT built
Sprint 7: ████████████████████ 100% Complete
Sprint 6: ████████████████████ 100% Complete
Sprint 5: ████████████████████ 100% Complete
Sprint 4: ████████████████████ 100% Complete
Sprint 3: ████████████████████ 100% Complete
Sprint 1: ████████████████████ 100% Complete
Sprint 0: ████████████████████ 100% Complete
```

---

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

---
## Sprint 8: Mobile API Layer

Brief: a JSON Mobile API on top of the existing web (same project, same DB).
Six vertical slices, each independently testable. **Web behavior unchanged.**
Hard constraints honored: no async/await, no EF version upgrade, no
System.Text.Json, no new business logic in API code, only two new tables
(`MobileDeviceToken`, `RefreshToken`), and **exactly one line of new logic
in existing AppCode** (the FCM dispatch inside `NotificationHub.Send`).

### Tasks

| ID | Slice | Status | Date | Notes |
|----|-------|--------|------|-------|
| API-1 | Foundation (JWT, formatter, filters, 2 new tables) | ✅ Completed | 2026-05-11 | HS256 hand-rolled (no NuGet add) |
| API-2 | Auth (login/refresh/logout) + Me | ✅ Completed | 2026-05-12 | `AuthValidator` mirrors `LoginETask` queries without session writes |
| API-3 | Attendance (check-in/heartbeat/check-out/today) | ✅ Completed | 2026-05-12 | Reuses `UserAccountVM.Checkin` for parity with web auto-checkin at login |
| API-4 | Tasks (employee) + Projects + Employees pickers | ✅ Completed | 2026-05-12 | Calls `TaskManger.Emp*Task` (not `ChangeTaskStatus`) — brief reuse-map deviation, documented in DEC-016 |
| API-5 | Tasks (company create/approve/disapprove/delete) | ✅ Completed | 2026-05-12 | Same deviation as Slice 4 — calls `TaskManger.Company*Task` |
| API-6 | Notifications + FCM (single AppCode line) | ✅ Completed | 2026-05-12 | `FcmDispatcher.Dispatch` injected at the bottom of `NotificationHub.Send` — every existing notification trigger fans out to FCM automatically |
| API-7 | Comments + Attachments | ❌ NOT IMPLEMENTED | — | Documented as done on 2026-05-12 but never landed in code (no controller actions, DTOs, mapper methods, or routes exist). Design retained in `1-CHANGELOG.md` SLICE-7 and `4-MOBILE-API.md` §5.10/§5.11. Endpoints return 404. Corrected 2026-06-30. |
| API-8 | Internal API — Ops Portal attachment upload (`POST /api/internal/tasks/{id}/attachments`) | ✅ Completed | 2026-07-08 | Server-to-server, `X-Ops-Portal-Key` shared secret (not JWT). Reuses `TaskManger.AttachTaskFile`. Fix: synthetic per-request system `userData` so `LogTask`/`NotificationHub` don't NRE (DEC-040). Full spec: `5-OPS-PORTAL-INTERNAL-API.md`. Pending tester sign-off. |

### Progress

- [x] API-1: SQL migration, JWT services, filters, config, refresh-token cleanup job
- [x] API-2: AuthController, MeController, AuthValidator, 9 DTOs, error-code field on ApiResponse
- [x] API-3: AttendanceController, 3 DTOs, exception filter dev-mode toggle
- [x] API-4: TasksController (employee actions), ProjectsController, EmployeesController, TaskMapper, ProjectMapper, EmployeeMapper, 7 DTOs
- [x] API-5: TasksController extended (+create/approve/disapprove/delete), CreateTaskDto, verb-constrained routes
- [x] API-6: NotificationsController, DeviceTokensController, FcmDispatcher, DeviceTokenService, NotificationMapper, 2 DTOs, **single FCM line in AppCode/Notification.cs**
- [ ] API-7: Comments + Attachments — **NOT IMPLEMENTED** (documented in error; no code exists). Planned: TasksController +4 actions, 3 DTOs (CommentDto, AddCommentRequest, AttachmentDto), TaskMapper +2 methods, 4 new routes
- [x] API-8: Internal Ops Portal attachment upload — `InternalAttachmentsController`, `OpsAttachmentResultDto`, +1 route, `OpsPortalKey` config key, synthetic-context fix (DEC-040)

### Sprint 8 Files

#### Application Files (To Deploy)

| # | File Path | Slice | Priority |
|---|-----------|-------|----------|
| 1 | `EtaskMinstry/Api/**` (all new files under Api/) | 1–6 | High |
| 2 | `EtaskMinstry/App_Start/WebApiConfig.cs` | 1–6 | High |
| 3 | `EtaskMinstry/Global.asax.cs` | 1 | High |
| 4 | `EtaskMinstry/AppCode/Notification.cs` (single 1-line edit) | 6 | High |
| 5 | `EtaskMinstry/Web.config` (+ env variants) | 1, 3 | High |
| 6 | `EtaskMinstry/EtaskMinstry.csproj` | 1–6 | High |

#### Database Migration (Manual)

| # | Action | Slice | Priority |
|---|--------|-------|----------|
| 1 | Run `Telesak-Docs/sql/mobile-api-tables.sql` on target DB | 1 | High |
|   | Creates `MobileDeviceToken` + `RefreshToken`. Idempotent (uses `IF NOT EXISTS`). | | |

#### Configuration (Manual)

| # | Action | Slice | Priority |
|---|--------|-------|----------|
| 1 | Generate a real 64+ char `JwtSecret` and paste into Web.config | 1 | High |
| 2 | Use DIFFERENT JwtSecret per environment (dev / UAT / Telesak prod / E-snad prod) | 1 | High |
| 3 | Set `FcmServerKey` to real Firebase Server Key (dev can leave placeholder) | 6 | Med |
| 4 | Set `ApiDetailedErrors=false` in prod configs (default already false in env variants) | 1 | High |
| 5 | Set a real `OpsPortalKey` (replace `REPLACE_WITH_OPS_PORTAL_SHARED_SECRET`) and share it with Ops Portal; different value per env. Endpoint stays 503-disabled until set | API-8 | High |
| 6 | Ensure Ops Portal writes/reads via this endpoint only — files land in the Telesak web-root `/Upload/Task/` (flat folder; DB stores bare filename, no path) | API-8 | High |

#### Tester / Mobile Dev Handoff

| # | Action | Slice | Priority |
|---|--------|-------|----------|
| 1 | Distribute `Telesak-Docs/4-MOBILE-API.md` to tester + mobile dev | All | High |
| 2 | Distribute `Telesak-Docs/postman/Telesak-MobileAPI.postman_collection.json` to tester | All | High |
| 3 | Tester sign-off: run all 6 Postman folders end-to-end + web regression | All | High |

---

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

---
## Sprint 7: Reports & Attendance Improvements

### Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| ATT-01 | Remove Arabic AM/PM (ص/م) from attendance times | ✅ Completed | eFE3C | Claude |
| ATT-02 | Add ميلادي/هجري calendar toggle to Attendance Report | ✅ Completed | eFE3C | Claude |
| ATT-03 | Fix Attendance report layout to match CompanyTasks | ✅ Completed | eFE3C | Claude |
| ATT-04 | Add Hijri date display to Attendance report data rows | ✅ Completed | eFE3C | Claude |
| T-05b | Add date range to CompanyTasks report title header | ✅ Completed | eFE3C | Claude |
| FIX-03 | Fix countdown display (00 : 2 → 00:02) | ✅ Completed | eFE3C | Claude |
| FIX-04 | Fix chat Q&A missing triggers | ✅ Completed | eFE3C | Claude |
| CHAT | Chat widget updates (welcome, emojis, login flow, WhatsApp) | ✅ Completed | eFE3C | Claude |

### Progress

- [x] ATT-01: Removed ConvertTo12HourArabic() — 24-hour format
- [x] ATT-02: Calendar dropdown + initCalendar() + Hijri conversion
- [x] ATT-03: Logo, blue title, removed duplicate elements
- [x] ATT-04: DisplayDate partial class + UmAlQuraCalendar in controller
- [x] T-05b: Title expression with date range, FooterDateRange hidden
- [x] FIX-03: dir="ltr" on countdown element
- [x] FIX-04: Added missing Q&A triggers for button texts
- [x] CHAT: Welcome text, emojis removed, login [نعم] flow, 👋 removed

### Sprint 7 Files

#### Application Files (To Deploy)

| # | File Path | Task | Priority |
|---|-----------|------|----------|
| 1 | `EtaskMinstry/Services/AttendanceReportService.cs` | ATT-01 | High |
| 2 | `EtaskMinstry/Views/Attendance/AttendanceReport.cshtml` | ATT-02 | High |
| 3 | `EtaskMinstry/Controllers/AttendanceController.cs` | ATT-02/03/04 | High |
| 4 | `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | ATT-03/04 | High |
| 5 | `TaskManagementModel/sp_Attendance_Result.Partial.cs` | ATT-04 | High |
| 6 | `TaskManagementModel/TaskManagementModel.csproj` | ATT-04 | High |
| 7 | `EtaskMinstry/ReportsRDLC/CompanyTasks.rdlc` | T-05b | High |
| 8 | `EtaskMinstry/Scripts/attendance-tracker.js` | FIX-03 | Medium |
| 9 | `EtaskMinstry/App_Data/telesak-qa.json` | FIX-04/CHAT | High |
| 10 | `EtaskMinstry/Scripts/telesak-chat.js` | CHAT | High |

---

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

---
## Sprint 6: AI Chat Assistant

### Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| T-15 | AI Chat Assistant (المساعد الذكي) | ✅ Completed | eFE3C | Claude |

### Progress

- [x] T-15: ChatController (backend Q&A + Claude API)
- [x] T-15: telesak-chat.js (frontend floating widget)
- [x] T-15: Include script in all 3 layouts
- [x] T-15: Web.config ClaudeApiKey appSetting

### Sprint 6 Files

#### Application Files (To Deploy)

| # | File Path | Task | Priority |
|---|-----------|------|----------|
| 1 | `EtaskMinstry/Controllers/ChatController.cs` | T-15 | High |
| 2 | `EtaskMinstry/Scripts/telesak-chat.js` | T-15 | High |
| 3 | `EtaskMinstry/Views/Shared/_Layout.cshtml` | T-15 | High |
| 4 | `EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml` | T-15 | High |
| 5 | `EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml` | T-15 | High |
| 6 | `EtaskMinstry/Web.config` | T-15 | High |

#### Configuration (Manual)

| # | Action | Priority |
|---|--------|----------|
| 1 | Create `App_Data/` folder on server | High |
| 2 | Upload `telesak-qa.json` to `App_Data/` | High |
| 3 | Replace `REPLACE_WITH_YOUR_KEY` in Web.config with real Claude API key | High |

---

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

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

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

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

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

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

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

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
| 15 | `EtaskMinstry/ReportsRDLC/CompanyTasks.rdlc` | T-12b | High |
| 16 | `EtaskMinstry/Scripts/attendance-tracker.js` | T-14 | High |

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

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

---
## Issues Resolved

| # | Issue | Solution | Task |
|---|-------|----------|------|
| 1 | Empty check-out shows blank | Display "لم يُسجل" | A-02 |
| 2 | Date in check-in/out fields | Separate AttendanceDate column | A-02 |
| 3 | Duration not summable | DurationMinutes (INT) | A-02 |
| 4 | Employee name repeated | Employee grouping | A-03 |

---

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

---
## Sprint History

| Sprint | Start | End | Status |
|--------|-------|-----|--------|
| Sprint 7 | 2026-03-30 | 2026-03-30 | Completed |
| Sprint 6 | 2026-03-25 | 2026-03-25 | Completed |
| Sprint 5 | 2026-03-24 | 2026-03-24 | Completed |
| Sprint 4 | 2026-03-18 | 2026-03-18 | Completed |
| Sprint 3 | 2026-03-17 | 2026-03-17 | Completed |
| Sprint 1 | 2026-03-09 | 2026-03-16 | Completed |
| Sprint 0 | 2026-03-08 | 2026-03-08 | Completed |

---

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

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

## Sprint 9 - Bug Closeout (#66, #69) (2026-07-27)

### Sprint Goal

Close the two currently open GitHub bugs: task deletion false-success for non-New statuses and unreliable employee attendance/session recovery after browser close.

| # | Bug | Fix | Status |
|---|-----|-----|--------|
| #66 | Task deletion returns 200 but non-New task remains | `CompanyTaskVM.Delete` now soft-deletes any company-owned non-deleted task, and layout delete modals show success/failure from the returned boolean. | Fixed - build passed |
| #69 | Browser close does not always trigger logout/session update | Employee layouts always load `attendance-tracker.js`; tracker marks new browser client sessions; `AttendanceController.GetCurrentAttendance` closes stale rows at DB `LastHeartbeat` and creates a fresh row when needed. | Fixed - build passed |

### Files Modified

- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Models/CompanyTaskVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Views/Company/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Scripts/attendance-tracker.js`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_Layout.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml`

### Verification

- MSBuild `EtaskMinstry-NewDesign.sln` in `E:\work\2026\ai_vibe\Esnad_ai` succeeded on 2026-07-27 with 0 errors.
- Remaining warning: existing `System.Web.Http.WebHost` reference warning in `TaskManagementModel`.

---
## Sprint 5: UX & Performance (T-10 + T-11 + T-12 + T-13)

### Tasks

| ID | Task | Status | Session | Assignee |
|----|------|--------|---------|----------|
| T-10 | Remove Duplicate Date Filters (إزالة تكرار فلاتر التاريخ) | ✅ Completed | eFE3C | Claude |
| T-11 | Add Month Filter in Statistics (فلتر الشهر في الإحصائيات) | ✅ Completed | eFE3C | Claude |
| T-12 | Fix BiDi (Arabic/English Mixed Text) | ✅ Completed | eFE3C | Claude |
| T-12b | Fix BiDi in CompanyTasks RDLC Report | ✅ Completed | eFE3C | Claude |
| T-13 | Performance Improvements (تحسين الأداء) | ✅ Completed | eFE3C | Claude |
| T-14 | Fix Inactivity Modal Closing on Mouse Move | ✅ Completed | eFE3C | Claude |

### Progress

- [x] T-10: Remove duplicate date filters
- [x] T-11: Add month filter in statistics
- [x] T-12: Fix BiDi mixed text
- [x] T-12b: Fix BiDi in CompanyTasks RDLC report (Language ar-SA + RTL direction)
- [x] T-13: Performance improvements
- [x] T-14: Fix inactivity modal closing on mouse move
