# Critical Security Batch 1 - Fixes and Test Cases

## Branch information

- Branch: `fix/critical-security-batch1`
- Remote tracking branch: `github/fix/critical-security-batch1`
- Base commit: `f4b82ac`
- Documented head: `b933eb9`
- Last reviewed: 2026-10-01

This document covers every commit on the branch after `f4b82ac`. It combines the fix inventory, deployment notes, test prerequisites, and regression/security test cases needed to validate the branch.

## Commit coverage

| Commit | Change | Test coverage |
|---|---|---|
| `944e635` | Remove anonymous database, workflow-test, and file-upload test endpoints | TC-01 to TC-03 |
| `71d6296` | Add missing authentication and role restrictions | TC-04 to TC-11 |
| `26813a8` | Reject path traversal in attachment file names | TC-12 to TC-16 |
| `f66d455` | Add ownership checks to generic delete action | TC-17 to TC-22 |
| `5a71dce` | Validate mobile API attachment size, extension, and file signature | TC-23 to TC-27 |
| `d7b6458` | Add company ownership checks to Company-area actions | TC-28 to TC-42 |
| `701b3c5` | Add task ownership checks to Employee-area actions | TC-43 to TC-50 |
| `04ff4c7` | Fix Task Statistics page null-checkbox crash | TC-51 to TC-52 |
| `60603a2` | Fix project filtering in `sp_CompanyTasks` | TC-53 to TC-56 |
| `de94410` | Respect Gregorian/Hijri setting in employee task report dates | TC-57 to TC-61 |
| `8f68965` | Block employees of inactive/deleted companies | TC-62 to TC-64 |
| `b933eb9` | Keep attendance open across reloads and redirects | TC-65 to TC-71 |

## Test prerequisites

Prepare the following records in a non-production database:

- One administrator account.
- Two active companies: Company A and Company B.
- One active employee in each company: Employee A and Employee B.
- One active project in each company: Project A and Project B.
- At least two tasks per company, including New, In Progress, and Done states.
- A task currently assigned to Employee A and a task previously assigned to Employee A but reassigned to another employee.
- One comment and one attachment on a task in each company.
- Tasks with Project A, Project B, and no project.
- Tasks with start and end dates suitable for both Gregorian and Hijri report filters.
- One inactive company and one deleted company, each with an otherwise active employee account.
- An employee with an open attendance row and a valid `LastHeartbeat` column.
- A valid small upload for every permitted extension, plus an oversized file, an unsupported file, and a renamed file whose content does not match its extension.

Use separate browser sessions for Admin, Company A, Company B, Employee A, Employee B, and anonymous access. Capture the response status and confirm that rejected operations do not change database records or files.

## Test cases

### Removed debug and test endpoints

| ID | Test | Expected result |
|---|---|---|
| TC-01 | Request `/DataBase`, `/DataBase/getTables`, and `/DataBase/getTableByID` anonymously and while authenticated. | Routes are unavailable; no database names, tables, or rows are exposed. |
| TC-02 | Request `/Test` and attempt to post to the former workflow action. | Route/action is unavailable and no task status changes. |
| TC-03 | Request `/Employee/TestFileUpload/Add` and attempt the former upload action. | Route/action is unavailable and no file is written. |

### Authentication and role restrictions

| ID | Test | Expected result |
|---|---|---|
| TC-04 | Open Company Profile and Recurrence Task URLs anonymously. | Redirect to login or authorization denial. |
| TC-05 | Open Company Profile and Recurrence Task URLs as an employee. | Access denied; employee cannot edit a company profile. |
| TC-06 | Open Employee Profile URLs as a company account. | Access denied; company cannot edit an employee profile through the Employee area. |
| TC-07 | Open Employee Performance and Employees reports anonymously, as a company, and as an employee. | Only Admin is allowed. |
| TC-08 | Open the Attendance report anonymously or as an employee. | Access denied or redirected to login. |
| TC-09 | Open the Attendance report as Company A while supplying Company B's ID. | Request is forced to Company A; Company B data is not returned. |
| TC-10 | Post `GetEmployees` as Company A with Company B's ID. | Only Company A employees are returned. Anonymous and employee calls are rejected. |
| TC-11 | Post `GetCompanies` as Company A or an employee, then as Admin. | Non-admin calls return 403; Admin receives the permitted company list. |

### Attachment path traversal

| ID | Test | Expected result |
|---|---|---|
| TC-12 | Download an attachment using `../../Web.config`, encoded traversal, `/`, or `\` path components from Company area. | 404/rejection; no file outside `/Upload/Task` is returned. |
| TC-13 | Repeat TC-12 from Employee area. | 404/rejection; no file outside `/Upload/Task` is returned. |
| TC-14 | Submit traversal values in Company task `RemovedFiles`. | No outside file is deleted and the request does not delete a database attachment belonging to another path. |
| TC-15 | Download a valid bare stored filename owned by the caller. | File downloads successfully. |
| TC-16 | Request a missing but syntactically safe filename. | 404 without exposing a physical path. |

### Generic delete ownership

| ID | Test | Expected result |
|---|---|---|
| TC-17 | Company A calls generic Delete for Company B's task. | Returns failure; task remains unchanged. |
| TC-18 | Company A calls generic Delete for Company B's project. | Returns failure; project remains unchanged. |
| TC-19 | Company A calls generic Delete for Company B's employee. | Returns failure; employee remains unchanged. |
| TC-20 | Company A deletes its own eligible task, project, and employee. | Existing business rules run and permitted deletes succeed. |
| TC-21 | Use the Dashboard delete action for Company A's own task and Company B's task. | Own task follows task-delete logic; Company B's task is rejected. |
| TC-22 | Employee or anonymous caller invokes generic Delete. | Rejected; no record is modified. |

### Mobile API attachment validation

| ID | Test | Expected result |
|---|---|---|
| TC-23 | Upload a multipart request whose declared size exceeds `maxfileSize + 64 KB`. | `400 FILE_TOO_LARGE`; request body is not persisted. |
| TC-24 | Upload an empty file or a file larger than `maxfileSize`. | `400 FILE_TOO_LARGE`; no attachment row or file is created. |
| TC-25 | Upload an unsupported extension such as `.html` or `.aspx`. | `400 INVALID_FILE_TYPE`; no file is written. |
| TC-26 | Rename invalid content to an allowed extension. | `400 INVALID_FILE_TYPE` because the file header does not match. |
| TC-27 | Upload a valid allowed file with a matching signature and legal size. | Upload succeeds and returns the new attachment normally. |

### Company-area ownership

| ID | Test | Expected result |
|---|---|---|
| TC-28 | Company A opens Company B's task details or extensions using its ID. | Redirect/404; no task data is rendered. |
| TC-29 | Company A edits Company B's task, including direct POST. | Rejected; task remains unchanged. |
| TC-30 | Company A assigns its task to Employee B or selects Project B. | Rejected; assignee/project remains unchanged. |
| TC-31 | Company A reassigns Company B's task or assigns its task to Employee B. | Returns failure; no reassignment log is created. |
| TC-32 | Company A pauses, resumes, approves, rejects, archives, or changes priority on Company B's task. | Every operation returns failure and makes no change. |
| TC-33 | Company A adds a comment to Company B's task or changes Company B's comment status. | Rejected; no comment is added or changed. |
| TC-34 | Company A uploads to Company B's task. | Redirect/rejection; no file or attachment row is created. |
| TC-35 | Company A downloads Company B's attachment by a valid stored filename. | 404; attachment contents are not disclosed. |
| TC-36 | Company A bulk-deletes a mixed list containing its task and Company B's task. | Only eligible Company A tasks are deleted; foreign task is counted as a failure. |
| TC-37 | Company A opens, edits, activates/deactivates, or rehires Employee B. | Rejected; Employee B is unchanged. |
| TC-38 | Company A opens, edits, archives, or lists tasks for Project B. | Rejected/empty response; Project B is unchanged and its tasks are not exposed. |
| TC-39 | Company A opens the Brief Tasks report while passing Company B's ID. | Report is scoped to Company A. |
| TC-40 | Repeat representative Company tests with valid Company A records. | Existing valid task, employee, project, comment, attachment, and report operations still work. |
| TC-41 | Send null, zero, duplicate, and missing IDs to ownership-protected actions. | Request fails safely without an unhandled exception or cross-tenant change. |
| TC-42 | Verify `RecordAccess.cs` is included in the deployed assembly/project. | Build succeeds and protected actions do not fail because the helper is missing. |

### Employee-area task ownership

| ID | Test | Expected result |
|---|---|---|
| TC-43 | Employee A opens Employee B's task details or extensions. | Redirect/404; no task details are disclosed. |
| TC-44 | Employee A starts or ends Employee B's task. | Returns failure; status and dates are unchanged. |
| TC-45 | Employee A changes daily time on Employee B's task. | Returns `Error`; no time/log row is changed. |
| TC-46 | Employee A adds or changes comments on Employee B's task. | Rejected; comments are unchanged. |
| TC-47 | Employee A uploads to or downloads from Employee B's task. | Upload is rejected and download returns 404. |
| TC-48 | Employee A reads a task previously assigned to them and recorded in `TaskTLog`. | Read-only visibility follows the existing history rule. |
| TC-49 | Employee A attempts a mutating operation after the task is reassigned away. | Mutation is rejected because Employee A is no longer the current assignee. |
| TC-50 | Employee A performs the same operations on a task currently assigned to them. | Valid operations continue to work according to task status rules. |

### Task Statistics null handling

| ID | Test | Expected result |
|---|---|---|
| TC-51 | Open Task Statistics from the menu with a plain GET and no `hdnWithoutProject` value. | Page loads without `Nullable object must have a value`. |
| TC-52 | Run statistics with and without the "without project" option. | Each option returns the correct task group and no regression is introduced. |

### Stored procedure project filtering

| ID | Test | Expected result |
|---|---|---|
| TC-53 | Run `sp_CompanyTasks` in include mode for Project A. | Only Project A tasks are returned; projectless tasks are excluded. |
| TC-54 | Run it with the explicit no-project selection (`-2`). | Only projectless tasks are returned. |
| TC-55 | Run it in exclude mode for Project A. | Project A tasks are excluded; other projects and projectless tasks follow the intended exclude-mode behavior. |
| TC-56 | Run it with no project filter. | All otherwise matching tasks are returned. |

### Employee task report calendar handling

| ID | Test | Expected result |
|---|---|---|
| TC-57 | Set `ISGreg=1`; filter by Gregorian start dates posted as `yyyy-MM-dd`. | Correct records are returned without conversion errors. |
| TC-58 | With `ISGreg=1`, use Gregorian dates containing `/`. | Correct records are returned. |
| TC-59 | With `ISGreg=1`, filter by task end-date range. | Correct records are returned and displayed in Gregorian format. |
| TC-60 | Set `ISGreg=0`; run equivalent Hijri start/end-date filters. | Existing Hijri behavior remains correct. |
| TC-61 | Run the report with all date fields empty. | Report loads without conversion errors and does not add an unintended date restriction. |

### Inactive company login

| ID | Test | Expected result |
|---|---|---|
| TC-62 | Log in as an active employee of an inactive company. | User is treated as stopped/redirected to the stopped-user page; no attendance check-in is created. |
| TC-63 | Repeat with a deleted company. | Access is blocked and no attendance check-in is created. |
| TC-64 | Log in as an active employee of an active company. | Login and attendance check-in continue to work. |

### Attendance across page unloads

| ID | Test | Expected result |
|---|---|---|
| TC-65 | Employee with open attendance reloads the page. | `BeaconCheckout` updates `LastHeartbeat`; `CheckOut` remains null. |
| TC-66 | Navigate Back, follow a link, or complete an action that redirects. | Attendance remains open and tracking continues on the next page. |
| TC-67 | Close one of multiple tabs while another tab remains active. | Attendance remains open; later heartbeats continue updating the row. |
| TC-68 | Stop all heartbeats and allow `AutoCheckoutJob` to pass the timeout. | Attendance is closed at the last activity/heartbeat time. |
| TC-69 | Employee A submits Employee B's `attendanceId` to `BeaconCheckout` or `InactivityCheckout`. | Request reports failure; Employee B's attendance is unchanged. |
| TC-70 | Submit a future last-activity time and then one earlier than check-in. | Stored heartbeat is clamped to now or check-in respectively. |
| TC-71 | Confirm inactivity checkout for Employee A's own open row. | Row closes using `LastHeartbeat`, and the active-attendance session flag is cleared. |

## Deployment requirements

- Deploy the rebuilt `EtaskMinstry.dll` containing `RecordAccess.cs` and all controller/model changes.
- Deploy the updated `Telesak-Docs/sp_CompanyTasks_update.sql` to the target database; the C# deployment alone does not apply this stored-procedure fix.
- Confirm the database has the `Attendance.LastHeartbeat` column before testing attendance behavior.
- Preserve environment-specific `Web.config` values and secrets; this branch's current local `Web.config` modification is not part of this test document.
- Back up the target database and `/Upload/Task` directory before deployment testing.

## Complete changed-file inventory

### Added

- `EtaskMinstryWeb/EtaskMinstry/AppCode/RecordAccess.cs`

### Modified

- `EtaskMinstryWeb/EtaskMinstry/Api/Controllers/TasksController.cs`
- `EtaskMinstryWeb/EtaskMinstry/AppCode/Extentions.cs`
- `EtaskMinstryWeb/EtaskMinstry/AppCode/TaskManger.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/CompanyController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/EmployeeController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/ProfileController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/ProjectController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/RecurrenceTaskController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/ReportController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Company/Controllers/TaskController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Employee/Controllers/EmployeeController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Employee/Controllers/ReportController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Employee/Controllers/TasksController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Employee/Models/EmployeeTask/EmployeeTaskListVM.cs`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/AttendanceController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/BaseController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/EmployeePerformanceReportController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/EmployeesReportController.cs`
- `EtaskMinstryWeb/EtaskMinstry/EtaskMinstry.csproj`
- `EtaskMinstryWeb/EtaskMinstry/Models/Login/UserAccountVM.cs`
- `Telesak-Docs/sp_CompanyTasks_update.sql`

### Removed

- `EtaskMinstryWeb/EtaskMinstry/Areas/Employee/Controllers/TestFileUploadController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Areas/Employee/Views/TestFileUpload/Add.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/DataBaseController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Controllers/TestController.cs`
- `EtaskMinstryWeb/EtaskMinstry/Views/DataBase/Index.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/DataBase/_TableView.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/DataBase/getTableByID.cshtml`
- `EtaskMinstryWeb/EtaskMinstry/Views/Test/Index.cshtml`

## Final sign-off checklist

- [ ] All 12 commits above are present in the deployed build.
- [ ] TC-01 through TC-71 have been executed and evidence attached.
- [ ] No cross-company data was returned or modified in negative tests.
- [ ] Valid same-company and same-employee workflows still work.
- [ ] The updated `sp_CompanyTasks` procedure was deployed and verified.
- [ ] Attendance reload, redirect, timeout, and ownership scenarios passed.
- [ ] No unexpected application errors were added to logs.
