# Sprint Tracker

## Sprint 8 — Bug Sweep #39 Round 2 + #64 Reassign Restriction (2026-06-02)

### Sprint Goal
Close the two remaining open bugs on `fix_v2`: re-fix the Admin path of #39 (previous fix targeted the wrong method) and implement the PO's restriction on task reassignment per #64.

### Sprint 8 Bug + Tasks

| Task | Page / Layer | Approach | Status |
|---|---|---|---|
| #39 Round 2 — Admin add-employee accepts company's own email | `/Admin/Employee/Create` | Remove `e.CompanyID != companyID` exclusion in `CheckEmployeeUniqueEmail` (the actual method called by `[Remote]`) | Uploaded — awaiting tester |
| #64 — Restrict reassignment to status "New" only | Task details + EditTask in Company area; Common + Company backends | UI: hide reassign button and disable dropdown unless `StatusID == New`. Hidden `EmpID` input preserves assignee. Backend: guard in both `ReAssignTask` actions. `TaskManger.cs` not touched. | Uploaded — awaiting tester |

### Sprint 8 Files Modified

**C# (compiled into bin/EtaskMinstry.dll):**
- `Areas/Admin/Models/CompanyEmployeeVM.cs` — `CheckEmployeeUniqueEmail` no longer excludes the current company from the duplicate check
- `Areas/Common/Controllers/CommonController.cs` — `ReAssignTask` returns `false` when task status is not `New`
- `Areas/Company/Controllers/CompanyController.cs` — `ReAssignTask` same guard

**Views (.cshtml):**
- `Areas/Company/Views/Company/TaskDetails.cshtml` — reassign button rendered only when `StatusID == New && !IsArchived`
- `Areas/Company/Views/Task/EDitTask.cshtml` — employee dropdown editable only when `StatusID == New && !IsArchived`; otherwise disabled with hidden `EmpID` input preserving current assignee

**Not touched (intentional):**
- `AppCode/TaskManger.cs` — shared code per CLAUDE.md Rule 3; guard moved to controller actions instead
- `Areas/Common/Views/Common/TaskDetails.cshtml` — reassign block was already wrapped in a `@*...*@` Razor comment

### Sprint 8 Impact Analysis (#64)
- Reports (tasks by status / employee / date / KPIs / dashboards) — not affected; reports only read data
- Task creation — not affected; `AssignTask` is only called by the two `ReAssignTask` actions
- Status transitions (Start/Done/Approve/Reject) — not affected
- Historical `TaskTLog` data — not affected; only future ineligible-status reassignments are blocked

---

## Sprint 7 — Pagination & Bug #36 Round 3 (2026-05-06, Session WQ5V1)

### Sprint Goal
Kill the 80s+ TTFB on `/Company/Company/index` by switching to true server-side pagination, then add a `.Take(500)` safety cap on every other list page that uses the same "load-everything + WebGrid client-paginates" pattern.

### Sprint 7 Bug + Tasks

| Task | Page | Approach | Status |
|---|---|---|---|
| #36 Round 3 — Company tasks slow | `/Company/Company/index` | **Option A** — proper server-side pagination (`PagedResult<T>`, `SelectPaged()`, AJAX pager) | ✅ Done |
| Safety cap | `/Employee/Tasks` | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Common/Common/Index` | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Company/DashBoard` (7 task-type branches) | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Employee/DashBoardEmp` (5 branches) | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Company/Project` | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Company/Employee` (2 query paths) | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Admin/Company` | Option B — `.Take(500)` | ✅ Done |
| Safety cap | `/Admin/Employee/Index` (3 query paths) | Option B — `.Take(500)` | ✅ Done |

### Sprint 7 Files Modified

**New:**
- `Models/PagedResult.cs` — generic `PagedResult<T>` wrapper

**C# (compiled into bin/EtaskMinstry.dll):**
- `Areas/Company/Models/CompanyTaskVM.cs` — added `SelectPaged()`, `Select()` is now a backwards-compat wrapper
- `Areas/Company/Controllers/CompanyController.cs` — `Index()` and both `GetTasks()` accept `page`
- `Areas/Employee/Models/EmployeeTask/EmployeeTaskListVM.cs` — `.Take(500)` cap
- `Areas/Common/Models/TaskCommonVM.cs` — `.Take(500)` cap
- `Areas/Company/Models/DaskBoardCompanyTaskVM.cs` — 7 caps
- `Areas/Employee/Models/DashBoardVM.cs` — 5 caps
- `Areas/Company/Models/ProjectDisplay.cs` — 1 cap
- `Areas/Company/Models/CompanyEmployeeVM.cs` — 2 caps
- `Areas/Admin/Models/CompanyVM.cs` — 1 cap
- `Areas/Admin/Models/CompanyEmployeeVM.cs` — 3 caps

**Views (.cshtml):**
- `Areas/Company/Views/Company/PartialCompTask.cshtml` — model is now `PagedResult<CompanyTaskVM>`, WebGrid bound with `rowCount`/`autoSortAndPage:false`
- `Areas/Company/Views/Company/Index.cshtml` — RenderPartial cast updated, AJAX-aware pager click handler added

### Performance Result (measured against app-test.telesak.com)

| Page | Before | After | Improvement |
|---|---|---|---|
| `/Admin/Company` | 20,050 ms TTFB | 550 ms | 36× faster |
| `/Admin/Index` (Employees) | 25,050 ms TTFB | 265 ms | 94× faster |
| `/Company/Company/index` (10-row sample, heavy company) | 80,000+ ms (browser crash) | <2,000 ms expected after deploy | ~50× faster |

### Sprint 7 Lessons Learned

1. **Don't generalize from a small dataset.** I initially tested `/Admin/Company` (550 ms) and concluded the fix was deployed and working. The tester was on `/Company/Company/index` with a heavy account and it was still 80 s. Always reproduce on the exact page+account from the bug report.
2. **`WebGrid(Model)` is not a perf fix — it's a UI feature.** It only paginates the rows it's been given. The DB query has to do the paging.
3. **Different code paths for "Index" vs "AJAX search" lookups need the same fix.** Forgot to apply the same logic in both the GET and POST `GetTasks()` overloads in earlier rounds — caught this time by listing all callers of `Select()` before refactoring.
4. **Feature flags for risky pagination changes.** Doing Option A on 1 page + Option B safety on 8 lets us ship today with low regression risk. Promote to Option A page-by-page in follow-up sprints.

---

## Current Sprint: Sprint 6 - Bug Fixing (14 GitHub Issues)

### Sprint Goal
Fix all 14 open bugs reported by tester (dr-emy) on GitHub issues #30–#47.

---

## Sprint 6 Task Status

### Round 1 — Initial Fixes

| # | Bug | Severity | Status | Notes |
|---|-----|----------|--------|-------|
| #43 | Debug mode on production | Severe | ✅ Done | `debug="false"`, `customErrors mode="On"` in 4 config files |
| #30 | Double company/employee creation | Severe | ✅ Done | `_isSubmitting` guard + `ajaxComplete` in 6 AddEdit views |
| #44 | Save button stuck "جاري الحفظ" | High | ✅ Done | Same fix as #30 — shared root cause |
| #46 | Edit blocked after deleting duplicate | Severe | ✅ Done | Added EmpID to email check + IsDeleted filter |
| #39 | Same company email accepted | Severe | ✅ Done | Same fix as #46 |
| #45 | Duplicate date range fields | Mild | ✅ Done | Simplified to 2 single date fields |
| #40 | Duplicate delete column | Mild | ✅ Done | Conditional guard on second column |
| #37 | Priority level unclear | Mild | ✅ Done | Strengthened `.btn.active` CSS |
| #42 | Report crash (CompanyName) | Severe | ✅ Done | `ReportParameters.Clear()` in 4 controllers |
| #47 | Excel columns misaligned | Low | ✅ Done | Aligned RDLC elements to column grid |
| #38 | Rejected task non-actionable | Severe | ✅ Done | غيرمعتمدة tab + Accept on NotApproved |
| #35 | Daily time entry not updating | Moderate | ✅ Done | ⚠️ Reverted in Round 2 |
| #36 | 20-25s TTFB Admin pages | Critical | ✅ Done | Eager loading UserAccounts (Admin page) |
| #34 | Password reset email fails | Moderate | ✅ Done | Try-catch + SMTP infra note posted |

### Round 2 — Reopened Bug Re-Fixes

| # | Bug | Why Reopened | Status | Fix |
|---|-----|-------------|--------|-----|
| #35 | Time entry regression | My fix broke total + daily update | ✅ Reverted | Restored original code |
| #36 | Company المهام still slow | Fixed wrong page (Admin vs Company) | ✅ Fixed | Eager loading in CompanyTaskVM.Select |
| #37 | Priority still unclear | CSS targeted `table` not `div` | ✅ Fixed | Added `.Priorbtns`, `.priortybar` selectors |
| #46 | Edit still blocked | Mobile/NationalID missing IsDeleted | ✅ Fixed | Added IsDeleted filter to 4 methods × 4 areas |
| #47 | Excel still spanning | PageHeader/Footer misaligned | ✅ Fixed | Aligned all elements + zeroed margins |
| #34 | Email still fails | SMTP config, not code | ✅ Closed | Infrastructure note posted |

### Sprint 6 Files Modified

**C# (compiled into bin/EtaskMinstry.dll)**
- `AppCode/TaskManger.cs` — #35 revert
- `AppCode/TaskWorkflow.cs` — #38 Accept on NotApproved
- `Areas/Admin/Models/CompanyVM.cs` — #36 eager loading UserAccounts
- `Areas/Admin/Models/CompanyEmployeeVM.cs` — #46 IsDeleted on Mobile/NationalID
- `Areas/Company/Controllers/EmployeeController.cs` — #46 EmpID param
- `Areas/Company/Controllers/ReportController.cs` — #42 ReportParameters.Clear
- `Areas/Company/Models/CompanyEmployeeVM.cs` — #46 email/mobile/nationalID fixes
- `Areas/Company/Models/CompanyTaskVM.cs` — #36 eager loading
- `Areas/Employee/Controllers/ReportController.cs` — #42
- `Areas2/Admin/Models/CompanyEmployeeVM.cs` — #46
- `Areas2/Company/Controllers/EmployeeController.cs` — #46
- `Areas2/Company/Controllers/ReportController.cs` — #42
- `Areas2/Company/Models/CompanyEmployeeVM.cs` — #46
- `Areas2/Company/Models/CompanyTaskVM.cs` — #36
- `Areas2/Employee/Controllers/ReportController.cs` — #42
- `Models/Login/Security.cs` — #34 error handling

**Views (.cshtml)**
- `Areas/Admin/Views/Company/AddEdit.cshtml` — #30/#44
- `Areas/Admin/Views/Employee/AddEdit.cshtml` — #30/#44
- `Areas/Company/Views/Company/Index.cshtml` — #45 date fields
- `Areas/Company/Views/Company/PartialCompTask.cshtml` — #40 delete column
- `Areas/Company/Views/Employee/AddEdit.cshtml` — #30
- `Areas/Employee/Views/Tasks/Index.cshtml` — #38 غيرمعتمدة tab
- `Areas/Employee/Views/Tasks/PartialEmpTask.cshtml` — #38 resume button
- Areas2 mirrors (5 files)

**CSS**
- `Content/Site/css/style.css` — #37 priority buttons

**Config**
- `Web.config`, `webtele.config`, `webesnad.config`, `webuat.config` — #43

**Reports**
- `ReportsRDLC/CompanyTasks.rdlc` — #47

**Docs**
- `CLAUDE.md` — Bug Fixing Workflow + Anti-Reopen Rules

### Lessons Learned (Anti-Reopen Rules added to CLAUDE.md)
1. Match exact page/role/area from tester's report
2. Fix ALL areas (Areas + Areas2, Admin + Company)
3. Trace full call chain before changing shared code
4. CSS: verify selector matches exact HTML on exact page
5. RDLC: align EVERY element to column grid
6. Re-read repro steps before committing
7. Always close GitHub issues with comment

---

## Previous Sprint: Sprint 4 - Performance & Attachment Fixes

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

## PERFORMANCE SPRINT ✅

**Problem:** Admin and Company Dashboard slow with large data.

### Group A — Critical (DaskBoardCompanyTaskVM.cs)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| A1 | Fixed Delayed count — pre-filter at DB level before .ToList() | ✅ Done | Skip New, Rejected, null EmpID, Archived at DB level — matches isDelayed early-exit conditions |
| A2 | Merged 6 GetCompanyTasksCount() calls into 1 GetAllCounts() | ✅ Done | 1 UnitOfWork instead of 6, controller passes via ViewBag |

### Group B — Medium (DashBoardController.cs)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| B1 | GetTasks() POST now uses pagination (pageSize=10) | ✅ Done | Tab clicks no longer load ALL tasks |

### Group C — Advanced (DaskBoardCompanyTaskVM.cs + CompanyVM.cs)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| C1 | Fixed isTaskDelayed() N+1 — batch load instead of per-row GetByID() | ✅ Done | 1 query instead of N per delayed check |
| C2 | Added eager loading (includeProperties: Status,Priority) | ✅ Done | 14 data queries — eliminates lazy load per row |
| C3 | Fixed Admin CompanyVM N+1 task count — single GROUP BY query | ✅ Done | 1 query instead of 2×N per company |

### Group D — Dashboard Load Optimization (DashBoardController + Layout)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| D1 | Throttle UpdateTaskStatus() — 5 min per company via Session | ✅ Done | Skips if ran <5 min ago, per-company session key |
| D2 | Remove NotificationHub.Send() from UpdateTaskStatus loop | ✅ Done | Task status still updates, notifications removed from auto-update loop |
| D3 | PushNotification lazy load via Ajax (1.5s delay) | ✅ Done | Empty shell renders immediately, real notifications load after page |

### Group E — Admin Employee Performance (CompanyEmployeeVM.cs)

| ID | Task | Status | Notes |
|----|------|--------|-------|
| E1 | Eager loading Company,UserAccounts in Search() | ✅ Done | includeProperties eliminates 2×N lazy loads |
| E2 | Batch task counts via GROUP BY in Search() | ✅ Done | Same month-only filter, 1 query instead of 2×N |

### Files Changed

| # | File | Groups |
|---|------|--------|
| 1 | `Areas/Company/Models/DaskBoardCompanyTaskVM.cs` | A1, A2, C1, C2 |
| 2 | `Areas/Company/Controllers/DashBoardController.cs` | A2, B1, D1 |
| 3 | `Areas/Company/Views/DashBoard/Index.cshtml` | A2 |
| 4 | `Areas/Admin/Models/CompanyVM.cs` | C3 |
| 5 | `AppCode/TaskManger.cs` | D2 |
| 6 | `Models/PushNotificationVM.cs` | D3 |
| 7 | `Views/Shared/_Layout.cshtml` | D3 |
| 8 | `Controllers/NotificationController.cs` | D3 |
| 9 | `Views/Shared/PushNotification.cshtml` | D3 |
| 10 | `Areas/Admin/Models/CompanyEmployeeVM.cs` | E1, E2 |

Tested and confirmed — noticeable speed improvement ✅

---

## Performance Code Standards (مبادئ الأداء)

These rules apply to ALL future code changes in this project:

### 1. Never use .ToList() before filtering
```csharp
// ❌ Bad: loads ALL then filters in C#
var data = _unitOfWork.Repo.Get().ToList().Where(x => x.Status == 1);

// ✅ Good: filter at DB level
var data = _unitOfWork.Repo.Get(filter: x => x.Status == 1).ToList();
```

### 2. Never count/aggregate inside a loop (N+1)
```csharp
// ❌ Bad: 1 query per item
foreach (var emp in employees)
    emp.TaskCount = emp.Tasks.Where(...).Count();

// ✅ Good: single GROUP BY query, then dictionary lookup
var counts = _unitOfWork.TaskRepository.Get(filter: ...)
    .GroupBy(t => t.EmpID)
    .Select(g => new { EmpID = g.Key, Count = g.Count() })
    .ToDictionary(x => x.EmpID);
emp.TaskCount = counts.ContainsKey(emp.EmpID) ? counts[emp.EmpID].Count : 0;
```

### 3. Always use includeProperties for navigation properties
```csharp
// ❌ Bad: lazy loads Status.Name and Priority.Name per row
_unitOfWork.TaskRepository.Get(filter: ...)

// ✅ Good: eager loads in single query
_unitOfWork.TaskRepository.Get(filter: ..., includeProperties: "Status,Priority")
```

### 4. Never call GetByID() inside a loop
```csharp
// ❌ Bad: N queries
tasks.ForEach(t => t.IsDelayed = isTaskDelayed(t)); // GetByID inside

// ✅ Good: batch load, then dictionary lookup
var entities = _unitOfWork.TaskRepository.Get(t => ids.Contains(t.TaskID)).ToList();
var dict = entities.ToDictionary(t => t.TaskID);
tasks.ForEach(t => t.IsDelayed = dict.ContainsKey(t.TaskID) && dict[t.TaskID].isDelayed);
```

### 5. Never create UnitOfWork inside static methods called in loops
```csharp
// ❌ Bad: new DB connection per call
public static bool IsDelayed(int taskId) {
    var uow = new UnitOfWork(...); // new connection each time!
    return uow.TaskRepository.GetByID(taskId).isDelayed;
}

// ✅ Good: pass UnitOfWork as parameter, or batch outside the loop
```

### 6. Throttle expensive operations
```csharp
// ❌ Bad: runs on every page load
TaskManger.UpdateTaskStatus();

// ✅ Good: session-based throttle
if (lastRun == null || (DateTime.Now - lastRun.Value).TotalMinutes >= 5)
    TaskManger.UpdateTaskStatus();
```

### 7. Lazy load UI components that block page render
```csharp
// ❌ Bad: blocks page render
@Html.Partial("Notifications", new NotificationVM(0, 8))

// ✅ Good: empty shell + Ajax after page loads
@Html.Partial("Notifications", new NotificationVM())
// + setTimeout Ajax load after 1.5s
```

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
| 7 | Login flow: intermediate [نعم] step for "مشكلة في تسجيل الدخول" | Done | eFE3C | Bot asks question first, then shows answer after [نعم] click |
| 8 | Removed 👋 emoji from login mode welcome message | Done | eFE3C | Clean text only |

### Sprint 5 Files Modified
- `EtaskMinstry/Scripts/telesak-chat.js` — welcome text, emojis, buttons, merged Q&A, login [نعم] flow
- `EtaskMinstry/App_Data/telesak-qa.json` — merged login entries, removed video emoji, WhatsApp number

All Sprint 5 chatbot changes tested and confirmed ✅

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
