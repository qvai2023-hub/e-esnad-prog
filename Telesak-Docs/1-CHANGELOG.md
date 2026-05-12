# TELE SAK - Changelog

All notable changes to the TELE SAK project will be documented in this file.

---

## Sprint 8 - Mobile API Layer

**Status:** Completed (pending tester sign-off)
**Date Completed:** 2026-05-12

A JSON Mobile API layer was added to serve a future mobile app. Same project,
same solution, same database. **Web behavior is unchanged.** Full reference
for testers + mobile devs: see `Telesak-Docs/4-MOBILE-API.md`.

### SLICE-1: Foundation — JWT, formatter, filters, two new tables

| File | Type | Description |
|------|------|-------------|
| `Api/Configuration/ApiJsonFormatter.cs` | New | camelCase + ISO 8601 + drops XML. Web API only; MVC `Json()` unaffected |
| `Api/Configuration/ApiCorsConfig.cs` | New | CORS via `DelegatingHandler` (no NuGet add) |
| `Api/Filters/JwtAuthorizeAttribute.cs` | New | Validates Bearer JWT + populates `MvcApplication.userData` from claims |
| `Api/Filters/ApiResponseFilter.cs` | New | Wraps raw DTOs in `ApiResponse` envelope |
| `Api/Filters/ApiExceptionFilter.cs` | New | Sanitized Arabic 500s; honors `ApiDetailedErrors=true` for dev |
| `Api/Dtos/Common/{ApiResponse,PagedResponse,ErrorItem}.cs` | New | Standard envelope shapes |
| `Api/Services/JwtIssuer.cs` | New | HS256 access token + 64-byte refresh token (no NuGet add — hand-rolled HMACSHA256) |
| `Api/Services/JwtValidator.cs` | New | Constant-time signature check, issuer + exp validation |
| `Api/Services/RefreshTokenService.cs` | New | SHA-256 hashed refresh tokens via direct ADO.NET (EDMX unchanged) |
| `Api/Services/RefreshTokenCleanupJob.cs` | New | Daily timer-based purge, mirrors `AutoCheckoutJob` lifecycle |
| `App_Start/WebApiConfig.cs` | Modified | Added `/api/v1/` convention route, installed JSON formatter + CORS + filters |
| `Global.asax.cs` | Modified | Start/stop `RefreshTokenCleanupJob` (2 lines) |
| `Web.config` (+ `webesnad.config`, `webuat.config`, `webtele.config`) | Modified | +5 appSettings keys: `FcmServerKey`, `JwtSecret`, `JwtIssuer`, `JwtAccessExpiryMinutes`, `JwtRefreshExpiryDays` (+`ApiDetailedErrors` in dev) |
| `Telesak-Docs/sql/mobile-api-tables.sql` | New | Creates `MobileDeviceToken` + `RefreshToken` (additive only, idempotent) |

### SLICE-2: Auth + Me

| File | Type | Description |
|------|------|-------------|
| `Api/Controllers/AuthController.cs` | New | POST `/auth/login` `/auth/refresh` `/auth/logout` |
| `Api/Controllers/MeController.cs` | New | GET `/me`, POST `/me/change-password` |
| `Api/Services/AuthValidator.cs` | New | Mirrors `LoginETask` EF queries; does NOT write to session |
| `Api/Dtos/Auth/{LoginRequest,RefreshRequest,LogoutRequest,ChangePasswordRequest,DeviceInfoDto,TokenResponse,UserSummaryDto,MeResponse}.cs` | New | Request + response shapes |
| `Api/Dtos/Common/ApiResponse.cs` | Modified | Added optional `Code` field for machine-readable error codes |
| `App_Start/WebApiConfig.cs` | Modified | Added explicit `/api/v1/me` route |

### SLICE-3: Attendance

| File | Type | Description |
|------|------|-------------|
| `Api/Controllers/AttendanceController.cs` | New | POST `check-in`, `heartbeat`, `check-out`; GET `today` |
| `Api/Dtos/Attendance/{CheckInResponse,HeartbeatResponse,TodayAttendanceDto}.cs` | New | Response shapes |
| `Api/Dtos/Auth/MeResponse.cs` | Modified | Uses moved `TodayAttendanceDto` namespace |
| `Api/Controllers/MeController.cs` | Modified | Uses moved namespace + sets `IsOpen` |
| `Api/Filters/ApiExceptionFilter.cs` | Modified | Honors `ApiDetailedErrors` (dev visibility) |

### SLICE-4: Tasks (Employee) + Projects + Employees pickers

| File | Type | Description |
|------|------|-------------|
| `Api/Controllers/TasksController.cs` | New | GET list (paged), GET detail, POST `accept`/`reject`/`complete`/`time` |
| `Api/Controllers/ProjectsController.cs` | New | GET `/projects` — both roles |
| `Api/Controllers/EmployeesController.cs` | New | GET `/employees` — Company only |
| `Api/Dtos/Tasks/{TaskListItemDto,TaskDetailDto,TaskStatusLogDto,UpdateTimeDto,TaskActionResultDto}.cs` | New | Task DTOs |
| `Api/Dtos/Projects/ProjectListItemDto.cs` | New | |
| `Api/Dtos/Employees/EmployeeListItemDto.cs` | New | |
| `Api/Mapping/{TaskMapper,ProjectMapper,EmployeeMapper}.cs` | New | Entity → DTO (only place that touches both) |
| `App_Start/WebApiConfig.cs` | Modified | +5 explicit routes for tasks/projects/employees |

### SLICE-5: Tasks (Company)

| File | Type | Description |
|------|------|-------------|
| `Api/Dtos/Tasks/CreateTaskDto.cs` | New | Body for POST /tasks |
| `Api/Controllers/TasksController.cs` | Modified | +`Create`, `Approve`, `Disapprove`, `Delete` actions + helpers |
| `App_Start/WebApiConfig.cs` | Modified | Split `/api/v1/tasks` into GET/POST verb-constrained routes; added DELETE `/api/v1/tasks/{id}` |

### SLICE-6: Notifications + FCM (single AppCode line)

| File | Type | Description |
|------|------|-------------|
| `Api/Controllers/NotificationsController.cs` | New | GET list, POST `{id}/read`, POST `read-all` |
| `Api/Controllers/DeviceTokensController.cs` | New | POST register, DELETE `{token}` unregister |
| `Api/Dtos/Notifications/{NotificationDto,DeviceTokenRequest}.cs` | New | Notification + device-token shapes |
| `Api/Mapping/NotificationMapper.cs` | New | NotificationCollection → DTO |
| `Api/Services/DeviceTokenService.cs` | New | Direct-SQL CRUD on `MobileDeviceToken` |
| `Api/Services/FcmDispatcher.cs` | New | Fire-and-forget FCM HTTP POST. Auto-deactivates stale tokens |
| **`AppCode/Notification.cs`** | **Modified** | **+1 line** inside `NotificationHub.Send` after the existing SignalR call — calls `FcmDispatcher.Dispatch` (try/catch wrapped). This is the **only** AppCode change in the whole Mobile API project |
| `Api/Controllers/AuthController.cs` | Modified | `Logout` honors `request.DeviceToken` and deactivates the token |
| `App_Start/WebApiConfig.cs` | Modified | +5 routes for notifications + device-tokens |

### Migration & Configuration (Manual)

| # | Action | Slice | Priority |
|---|--------|-------|----------|
| 1 | Run `Telesak-Docs/sql/mobile-api-tables.sql` on the target DB | 1 | High |
| 2 | Generate a fresh 64+ char random `JwtSecret` and paste into Web.config | 1 | High |
| 3 | Paste real Firebase server key into `FcmServerKey` (dev can leave placeholder; dispatcher silently no-ops) | 6 | Med |
| 4 | Import `Telesak-Docs/postman/Telesak-MobileAPI.postman_collection.json` | 1–6 | High (testers) |
| 5 | Distribute `Telesak-Docs/4-MOBILE-API.md` to mobile dev + tester | 1–6 | High |

### Documentation

| File | Type | Description |
|------|------|-------------|
| `Telesak-Docs/4-MOBILE-API.md` | New | Single reference doc for testers + mobile devs. Auth flow, endpoint reference, error codes, environment setup, testing checklist |
| `Telesak-Docs/postman/Telesak-MobileAPI.postman_collection.json` | New | Postman v2.1 collection covering all 6 slices with auto-token capture |
| `EtaskMinstry/CLAUDE.md` | Modified | Added "Mobile API Layer" section + per-slice notes + error code tables |

---

## Sprint 7 - Reports & Attendance Improvements

**Status:** Completed
**Date Completed:** 2026-03-30

### ATT-01: Remove Arabic AM/PM from Attendance Times

| File | Type | Description |
|------|------|-------------|
| `Services/AttendanceReportService.cs` | Modified | Removed `ConvertTo12HourArabic()` calls on CheckInTime/CheckOutTime — times now display in 24-hour format |

### ATT-02: Add ميلادي/هجري Calendar Toggle to Attendance Report

| File | Type | Description |
|------|------|-------------|
| `Views/Attendance/AttendanceReport.cshtml` | Modified | Added calendar type dropdown + `initCalendar()` JS function |
| `Controllers/AttendanceController.cs` | Modified | Added `calendarType` parameter, Hijri date conversion via `QvLib.QVUtil.Date.hijritodate()` |

### ATT-03: Fix Attendance Report Layout to Match CompanyTasks

| File | Type | Description |
|------|------|-------------|
| `ReportsRDLC/Attendance.rdlc` | Modified | Added logo Image, blue title header with date range, deleted HeaderAppName "TELE SAK" text, deleted HeaderDateRange, deleted HeaderMonth, reduced header gap |

### ATT-04: Add Hijri Date Display to Attendance Report

| File | Type | Description |
|------|------|-------------|
| `TaskManagementModel/sp_Attendance_Result.Partial.cs` | New | Partial class adds `DisplayDate` string property |
| `TaskManagementModel/TaskManagementModel.csproj` | Modified | Included new partial class file |
| `Controllers/AttendanceController.cs` | Modified | Populates `DisplayDate` using `UmAlQuraCalendar` (Hijri) or Gregorian based on calendarType; header dates also use conditional formatting |
| `ReportsRDLC/Attendance.rdlc` | Modified | Added `DisplayDate` field, changed date column to use it; declared `CalendarType` parameter |

### T-05b: Add Date Range to CompanyTasks Report Title

| File | Type | Description |
|------|------|-------------|
| `ReportsRDLC/CompanyTasks.rdlc` | Modified | Title expression now shows "تقرير المهام — من X إلى Y"; FooterDateRange hidden |

### FIX-03: Fix Session Timeout Countdown Display

| File | Type | Description |
|------|------|-------------|
| `Scripts/attendance-tracker.js` | Modified | Added `dir="ltr"` to countdown element — fixes "00 : 2" → "00:02" in RTL |

### FIX-04: Fix Chat Q&A Missing Triggers

| File | Type | Description |
|------|------|-------------|
| `App_Data/telesak-qa.json` | Modified | Added "ملف داخل المهمة" and "مهمة لا تظهر" as triggers |

### CHAT Updates

| File | Type | Description |
|------|------|-------------|
| `Scripts/telesak-chat.js` | Modified | Updated welcome text, removed emojis from buttons, merged login Q&A, login [نعم] intermediate step, removed 👋, WhatsApp number |
| `App_Data/telesak-qa.json` | Modified | Merged login entries, removed video emoji, updated WhatsApp, updated attachment answer |

---

## Sprint 6 - AI Chat Assistant (T-15)

**Status:** Completed
**Date Completed:** 2026-03-25

### T-15: AI Chat Assistant (المساعد الذكي)

Added a floating chat widget with Q&A keyword matching and Claude AI fallback.

#### Backend — ChatController

| File | Type | Description |
|------|------|-------------|
| `Controllers/ChatController.cs` | New | Inherits BaseController, [HttpPost] Send() action |

- Reads user info from `MvcApplication.userData` (userName, userId, isCompany)
- Tries keyword match against `App_Data/telesak-qa.json` triggers array
- Falls back to Claude Haiku API (`claude-haiku-4-5-20251001`) with Arabic system prompt
- API key from `Web.config` appSettings `ClaudeApiKey`
- Returns `{ success, answer, source: "qa"|"ai" }`
- Async with graceful Arabic error messages

#### Frontend — Chat Widget

| File | Type | Description |
|------|------|-------------|
| `Scripts/telesak-chat.js` | New | Self-contained floating chat widget (JS + injected CSS) |
| `Views/Shared/_Layout.cshtml` | Modified | Added script tag before `</body>` |
| `Views/Shared/_LayoutNewDesign.cshtml` | Modified | Added script tag before `</body>` |
| `Views/Shared/_LayoutNoSearch.cshtml` | Modified | Added script tag before `</body>` |

Widget features:
- Blue theme (#3D85C6) floating button (bottom-left), toggles to X when open
- RTL Arabic UI with header avatar "ت", status "متاح الآن · يرد فوراً"
- Welcome card with greeting and "تواصل مع الدعم" WhatsApp button
- 6 category tabs: الكل, تسجيل الدخول, المهام, الملفات, الحضور, التقارير
- 17 categorized quick buttons with emoji icons
- Bot messages with "ت" avatar circle and HH:MM timestamp
- Typing indicator (3 animated dots) while waiting for response
- Reset chat button (refresh icon) clears conversation
- Keeps last 6 conversation turns in memory as history
- Sends AntiForgeryToken header if available on page

#### Configuration

| File | Type | Description |
|------|------|-------------|
| `Web.config` | Modified | Added `ClaudeApiKey` appSetting |
| `App_Data/telesak-qa.json` | New (pending) | Q&A keyword matching file |

---

## Sprint 5 - UX Fixes & Performance (T-10, T-11, T-12, T-13)

**Status:** Completed
**Date Completed:** 2026-03-24

### T-10: Remove Duplicate Date Filters (إزالة تكرار فلاتر التاريخ)

Removed "تاريخ النهاية" (End Date) filter row from report preparation pages. Only "تاريخ البداية" (Start Date) From/To remains.

| File | Description |
|------|-------------|
| `Areas/Company/Views/Report/TaskReportPreperation.cshtml` | Removed تاريخ النهاية HTML + JS |
| `Areas/Employee/Views/Report/TaskReportPreperation.cshtml` | Removed تاريخ النهاية HTML + JS |
| `Areas/Company/Views/Report/EmployeeReport.cshtml` | Removed تاريخ النهاية HTML + JS |

### T-11: Add Month Filter in Statistics (فلتر الشهر في الإحصائيات)

Added Hijri month dropdown filter next to the year filter on the TasksByMonthsChart page.

| File | Description |
|------|-------------|
| `Areas/Company/Views/Report/TasksByMonthsChart.cshtml` | Added month `<select>` dropdown (محرم - ذو الحجة) |
| `Areas/Company/Controllers/ReportController.cs` | Added `Month` parameter to `TasksByMonthsChart()` action |
| `Areas/Company/Models/YearlyTasksChart.cs` | Added `iMonth` param to `GetTasks()` — filters by specific Hijri month |

### T-12: Fix BiDi (Arabic/English Mixed Text)

Added `unicode-bidi: plaintext` CSS class to fix mixed Arabic/English task name display order.

| File | Description |
|------|-------------|
| `Content/Main/Developers.css` | Added `.bidi-text` CSS class |
| `Areas/Company/Views/Company/PartialCompTask.cshtml` | Wrapped task name in `<span class="bidi-text">` |
| `Areas/Company/Views/DashBoard/PartialDBCompTask.cshtml` | Wrapped task name in `<span class="bidi-text">` |
| `Areas/Employee/Views/Tasks/PartialEmpTask.cshtml` | Wrapped task name in `<span class="bidi-text">` |
| `Areas/Common/Views/Common/PartialTaskCommon.cshtml` | Wrapped task name in `<span class="bidi-text">` |

### T-12b: Fix BiDi in CompanyTasks RDLC Report

Fixed mixed Arabic/English text order in the CompanyTasks RDLC report and widened the task column.

| File | Description |
|------|-------------|
| `ReportsRDLC/CompanyTasks.rdlc` | Changed report `Language` from `en-US` to `ar-SA` for correct RTL base direction |
| `ReportsRDLC/CompanyTasks.rdlc` | Added `<Direction>RTL</Direction>` to task column header and data paragraphs |
| `ReportsRDLC/CompanyTasks.rdlc` | Widened task column from 1.5in to 2.3in; reduced other columns to fit |

### T-14: Fix Inactivity Modal Closing on Mouse Move

Fixed bug where the inactivity warning popup closed immediately when the employee moved the mouse to click a button.

| File | Description |
|------|-------------|
| `Scripts/attendance-tracker.js` | `updateLastActivity()` now ignores activity events while modal is shown — only modal buttons can dismiss it |

### T-13: Performance Improvements (تحسين الأداء)

Batched N+1 queries in Dashboard and removed duplicate jQuery loading.

| File | Description |
|------|-------------|
| `Areas/Company/Models/DaskBoardCompanyTaskVM.cs` | Replaced 4x per-task GetByID + GetEmplyeeName with batch `FillTaskMetadata()` |
| `Views/Shared/_Layout.cshtml` | Removed duplicate `jquery-1.7.1.min.js` (jquery.js already loaded) |
| `Views/Shared/_LayoutNewDesign.cshtml` | Removed duplicate `jquery-1.7.1.min.js` |

### Database Scripts (Optional)

| Script | Description |
|--------|-------------|
| `Telesak-Docs/Sprint5_SQL_Indexes.sql` | Suggested indexes: `IX_Task_CompanyID_IsDelayed`, `IX_Task_CompanyID_IsDeleted` |

---

## Sprint 4 - Performance & Attachment Fixes

### PERF: N+1 Query Fixes, Eager Loading & Client-Side Filtering Fixes
**Status:** Completed
**Date Completed:** 2026-03-18

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/Areas2/Company/Models/CompanyTaskVM.cs` | Modified | Added includeProperties to Select() + batch delay data lookup |
| `EtaskMinstry/Areas2/Company/Models/ComapnyTaskDetailVM.cs` | Modified | Removed redundant GetByID in ProgressbarPercentage() |
| `EtaskMinstry/Areas2/Employee/Models/EmployeeTask/EmployeeTaskListVM.cs` | Modified | Added includeProperties to FillTasks() |
| `EtaskMinstry/AppCode/ServiceManger.cs` | Modified | Server-side filtering for GetAllEmployees() |
| `EtaskMinstry/Areas/Company/Models/CompanyEmployeeVM.cs` | Modified | 11 queries moved from client-side to DB-level filtering |
| `EtaskMinstry/Areas/Company/Models/CompanyProfileVM.cs` | Modified | 3 queries replaced with GetByID / Get(filter:) |
| `EtaskMinstry/Areas/Company/Models/RecurrenceTaskVM.cs` | Modified | Company-scoped GetAllProjects + GetAllEmployees |
| `EtaskMinstry/Services/TasksService.cs` | Modified | Added includeProperties: Employee,Employee.Tasks,Status |
| `EtaskMinstry/Services/EmployeesReportService.cs` | Modified | Added includeProperties: Attendances |
| `EtaskMinstry/Services/SharedService.cs` | Modified | Replaced AsEnumerable() GroupBy with Company.Get(filter:) |
| `EtaskMinstry/Services/AttendanceReportService.cs` | Modified | Added Get(filter:) for employee dropdown; .ToList() kept for .ToString() |
| `EtaskMinstry/AppCode/Notification.cs` | Modified | Filtered SignalR session query at DB level + empty collection guard |
| `EtaskMinstry/Areas/Company/Controllers/CompanyController.cs` | Modified | Reverted T-09b code (APIs only exist in Areas2) |

#### Changes:
- CompanyTaskVM: Eager load Project, Status, Priority, TaskTLogs in task list query
- CompanyTaskVM: Batch dictionary lookup for isDelayed, delayTime, DelayPercentage (replaces per-task GetByID)
- EmployeeTaskListVM: Eager load Project, Priority, Status, TaskTLogs, TaskTLogs.Status
- ComapnyTaskDetailVM: Use already-loaded task object instead of re-querying
- ServiceManger: Move company filter into Get() call (DB-level instead of client-side)
- CompanyEmployeeVM: All 11 uniqueness/lookup queries now filter at DB level via Get(filter:)
- CompanyProfileVM: CompanyDetails, ChangeEmail, ChangeAddress use GetByID / Get(filter:)
- RecurrenceTaskVM: GetAllProjects and GetAllEmployees now scoped to current company
- TasksService: BriefTasks report eager-loads Employee, Employee.Tasks, and Status navigation properties
- EmployeesReportService: Employee report eager-loads Attendances for count
- SharedService: Eliminated AsEnumerable() that forced client-side GroupBy of all UserAccounts
- AttendanceReportService: Employee dropdown uses Get(filter:) at DB level (.ToList() kept for .ToString())
- CompanyController (Areas v1): Reverted T-09b original filename code (SanitizeFileName, AttachmentDisplay.OriginalFileName only exist in Areas2)
- Notification.cs: SignalR session lookup filtered by collection instance/type IDs at DB level

---

### T-09b: Preserve Original File Names for Attachments
**Status:** Completed
**Date Completed:** 2026-03-18

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/Areas2/Company/Controllers/TaskController.cs` | Modified | Store OriginalFileName on upload |
| `EtaskMinstry/Areas2/Company/Controllers/CompanyController.cs` | Modified | Preserve OriginalFileName on task re-assignment copy |
| `EtaskMinstry/Areas2/Company/Views/Company/EditTask.cshtml` | Modified | Display original file name |
| `EtaskMinstry/Areas2/Company/Views/Company/SaveData.cshtml` | Modified | Display original file name |

#### Changes:
- Task attachments now preserve and display the user's original file name
- GUID-based disk name still used for storage (no collision risk)
- Original name carried forward when tasks are re-assigned and attachments are copied

---

## Sprint 3 - Bulk Operations & Filtering

### T-07/T-08/T-09/T-10: Bulk Delete, Multi-Employee Filter, SQL Scripts
**Status:** Completed
**Date Completed:** 2026-03-17

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/Areas/Company/Controllers/CompanyController.cs` | Modified | BulkDelete action + multi-employee GetTasks |
| `EtaskMinstry/Areas/Company/Views/Company/PartialCompTask.cshtml` | Modified | Checkbox column (New tab only) + select-all JS |
| `EtaskMinstry/Areas/Company/Views/Company/Index.cshtml` | Modified | Bulk delete toolbar + Chosen.js multi-select |
| `docs/Sprint3_SQL_Scripts.sql` | New | Performance indexes + Attachment table verification |

#### Changes:
- T-07: Bulk delete for tasks with New status only (max 500), with confirmation modal
- T-08: Multi-employee filter using Chosen.js multi-select dropdown
- T-09: SQL verification script for attachment table (files stay on disk)
- T-10: Performance indexes IX_Task_StatusID_CompanyID and IX_Task_EmpID_CompanyID

---

## Sprint 1 - Task Report & Attendance Tracking

### T-05/T-04: Task Report Redesign (CompanyTasks)
**Status:** Completed
**Date Completed:** 2026-03-16

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/ReportsRDLC/CompanyTasks.rdlc` | Modified | Redesigned layout, added interaction column, fixed XML |
| `EtaskMinstry/Views/Report/TaskReportPreperation.cshtml` | Modified | Added calendar type toggle UI |
| `EtaskMinstry/Controllers/AttendanceController.cs` | Modified | Hijri/Gregorian support, nullable DateTime fix |

#### Database Changes:
| Object | Type | Description |
|--------|------|-------------|
| `sp_CompanyTasks` | Stored Procedure | Updated with interaction column |

#### Changes:
- Added التفاعل (Interaction) column to task report (T-04)
- Modified report header layout (T-05)
- Added Hijri/Gregorian calendar toggle (T-02)
- Fixed missing `</Report>` closing tag causing ReportProcessingException
- Fixed CS0266 nullable DateTime error
- Restored truncated embedded images
- Added AllowBlank to report parameters

---

### ATT: Attendance Tracking System
**Status:** Completed
**Date Completed:** 2026-03-16

#### Files Created:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/Scripts/attendance-tracker.js` | New | Client-side attendance tracker |
| `EtaskMinstry/Services/AutoCheckoutJob.cs` | New | Server-side auto-checkout job |
| `TaskManagementModel/Attendance.Partial.cs` | New | Partial class for heartbeat |
| `TaskManagementModel/Migrations/AddLastHeartbeat.sql` | New | DB migration script |
| `scripts/check_isTelesak_consistency.sql` | New | Maintenance script |
| `scripts/fix_useraccount_companyid_mismatch.sql` | New | Maintenance script |

#### Files Modified:
| File | Type | Description |
|------|------|-------------|
| `EtaskMinstry/CustomAttrbutes/EmployeeAuthorize.cs` | Modified | Fixed StopedUser redirect bug |
| `EtaskMinstry/Models/Login/UserAccountVM.cs` | Modified | Removed UserAgent session check |
| `EtaskMinstry/Global.asax.cs` | Modified | Auto-checkout job registration |
| `EtaskMinstry/Views/Shared/_Layout.cshtml` | Modified | Tracker integration |
| `EtaskMinstry/Views/Shared/_LayoutNewDesign.cshtml` | Modified | Tracker integration |
| `EtaskMinstry/Views/Shared/_LayoutNoSearch.cshtml` | Modified | Tracker integration |
| `EtaskMinstry/Views/Shared/Modal.cshtml` | Modified | Fixed backdrop + z-index |
| `EtaskMinstry/ReportsRDLC/Attendance.rdlc` | Modified | Header layout update |

#### Bug Fixes:
- EmployeeAuthorize always redirecting to StopedUser
- UserAgent check causing session validation issues
- Duplicate checkout calls
- Inactivity checkout countdown errors
- Custom modal display issues

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

## Sprint 4 Summary

| Task | Status | Files | DB Changes |
|------|--------|-------|------------|
| T-09b | Completed | 4 modified | None |
| PERF-01 | Completed | 1 modified (CompanyTaskVM) | None |
| PERF-02 | Completed | 1 modified (CompanyTaskVM) | None |
| PERF-03 | Completed | 1 modified (EmployeeTaskListVM) | None |
| PERF-04 | Completed | 1 modified (ComapnyTaskDetailVM) | None |
| PERF-05 | Completed | 1 modified (ServiceManger) | None |
| PERF-06 | Completed | 1 modified (CompanyEmployeeVM) | None |
| PERF-07 | Completed | 1 modified (CompanyProfileVM) | None |
| PERF-08 | Completed | 1 modified (TasksService) | None |
| PERF-09 | Completed | 1 modified (EmployeesReportService) | None |
| PERF-10 | Completed | 1 modified (Notification.cs) | None |
| PERF-11 | Completed | 1 modified (SharedService) | None |
| PERF-12 | Completed | 1 modified (AttendanceReportService) | None |
| PERF-13 | Completed | 1 modified (RecurrenceTaskVM) | None |

**Total Files:** 16 modified (12 unique for perf + 4 for T-09b attachments)

---

## Sprint 3 Summary

| Task | Status | Files | DB Changes |
|------|--------|-------|------------|
| T-07 | Completed | 3 modified | None |
| T-08 | Completed | 2 modified | None |
| T-09 | Completed | 1 new (SQL) | Verification only |
| T-10 | Completed | 1 new (SQL) | 2 indexes to create |

**Total Files:** 4 (1 new SQL + 3 modified)

---

## Sprint 1 Summary

| Task | Status | Files | DB Changes |
|------|--------|-------|------------|
| T-02 | Completed | 2 modified | None |
| T-04 | Completed | 1 modified | sp_CompanyTasks updated |
| T-05 | Completed | 1 modified | None |
| ATT | Completed | 4 new + 8 modified | AddLastHeartbeat migration |
| FIX | Completed | 1 modified | None (CompanyTasks.rdlc XML fix) |

**Total Files:** 14 (6 new + 11 modified)

---

## Sprint 0 Summary

| Task | Status | Files | DB Changes |
|------|--------|-------|------------|
| A-01 | Completed | 4 new docs | None |
| A-02 | Completed | 2 modified | sp_Attendance updated |
| A-03 | Completed | 2 modified | None |

**Total Files:** 8 (4 new + 4 modified)
