using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Dtos.Tasks;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Api.Mapping;
using EtaskMinstry.AppCode;
using LogTask = EtaskMinstry.AppCode.LogTask;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API task endpoints. URL prefix: /api/v1/tasks
    ///   GET  /api/v1/tasks?status=new|inprogress|done|all&amp;page=1&amp;pageSize=20
    ///   GET  /api/v1/tasks/{id}
    ///   POST /api/v1/tasks/{id}/accept     (Employee)
    ///   POST /api/v1/tasks/{id}/reject     (Employee)
    ///   POST /api/v1/tasks/{id}/complete   (Employee)
    ///   POST /api/v1/tasks/{id}/time       (Employee)
    ///
    /// Slice 4 = employee actions only. Slice 5 will add company-side
    /// endpoints (create, approve, disapprove, delete).
    ///
    /// Employee actions call the SAME TaskManger.Emp*Task methods the web's
    /// Areas/Employee/Controllers/TasksController calls, so notification
    /// side effects (NotificationHub.Send → FCM in Slice 6) fire identically
    /// for web and mobile users.
    /// </summary>
    [JwtAuthorize]
    public class TasksController : ApiController
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        // ──────────────────────────── GET /api/v1/tasks ────────────────────────────
        [HttpGet]
        public HttpResponseMessage List(string status = "all", int page = 1, int pageSize = DefaultPageSize)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            // Base filter: not deleted, not rejected, scoped to the user's role.
            int empId = userData.userId;
            int companyId = userData.CompanyId ?? 0;
            bool isCompany = userData.isCompany;

            int? statusFilter = MapStatusParam(status);

            // Build the EF query — kept simple per Mobile API rules (no
            // attempt to replicate the web's reassignment-history view).
            // Company sees all tasks in their company; Employee sees only
            // tasks currently assigned to them.
            var query = uow.TaskRepository.Get(
                filter: t => !t.IsDeleted
                             && t.StatusID != (int)TaskStatus.Rejected
                             && (isCompany ? t.CompanyID == companyId : t.EmpID == empId)
                             && (!statusFilter.HasValue || t.StatusID == statusFilter.Value),
                orderBy: q => q.OrderByDescending(t => t.TaskID),
                includeProperties: "Status,Priority,Project");

            // Push pagination to SQL.
            int totalCount = query.Count();
            var materialized = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // isDelayed is a client-side computed property — only safe after .ToList().
            var dtos = materialized.Select(TaskMapper.ToListItem).ToList();

            return Request.CreateResponse(HttpStatusCode.OK,
                PagedResponse.Build(dtos, page, pageSize, totalCount));
        }

        // ──────────────────────────── GET /api/v1/tasks/{id} ───────────────────────
        [HttpGet]
        public HttpResponseMessage Detail(int id)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var task = uow.TaskRepository.Get(
                filter: t => t.TaskID == id && !t.IsDeleted,
                includeProperties: "Status,Priority,Project,Company,Employee,TimeUnit,TaskTLogs,TaskTLogs.Status"
            ).FirstOrDefault();

            if (task == null)
                return NotFound("لم يتم العثور على المهمة", "TASK_NOT_FOUND");

            // Authorization: Employee can only see their own tasks (current
            // assignee, or anywhere in their TaskTLogs). Company can see any
            // task in their company.
            if (!CanRead(task, userData))
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("غير مصرح بعرض هذه المهمة", "TASK_FORBIDDEN"));

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(TaskMapper.ToDetail(task)));
        }

        // ──────────────────────────── POST /api/v1/tasks/{id}/accept ───────────────
        [HttpPost]
        [ActionName("accept")]
        public HttpResponseMessage Accept(int id)
        {
            return InvokeEmployeeAction(id, taskId =>
            {
                bool changed = TaskManger.EmpAcceptTask(taskId);
                return changed;
            }, expectedFromStatus: TaskStatus.New, actionLabel: "قبول");
        }

        // ──────────────────────────── POST /api/v1/tasks/{id}/reject ───────────────
        [HttpPost]
        [ActionName("reject")]
        public HttpResponseMessage Reject(int id)
        {
            return InvokeEmployeeAction(id, taskId =>
            {
                bool changed = TaskManger.EmpRejectTask(taskId);
                return changed;
            }, expectedFromStatus: TaskStatus.New, actionLabel: "رفض");
        }

        // ──────────────────────────── POST /api/v1/tasks/{id}/complete ─────────────
        [HttpPost]
        [ActionName("complete")]
        public HttpResponseMessage Complete(int id)
        {
            return InvokeEmployeeAction(id, taskId =>
            {
                bool changed = TaskManger.EmpFinishTask(taskId);
                return changed;
            }, expectedFromStatus: TaskStatus.Inprogress, actionLabel: "إنهاء");
        }

        // ──────────────────────────── POST /api/v1/tasks/{id}/time ─────────────────
        [HttpPost]
        [ActionName("time")]
        public HttpResponseMessage UpdateTime(int id, UpdateTimeDto request)
        {
            if (request == null || request.Time < 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("القيمة المرسلة غير صحيحة", "INVALID_REQUEST"));

            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للموظفين فقط", "EMPLOYEE_ONLY"));

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var task = uow.TaskRepository.GetByID(id);
            if (task == null || task.IsDeleted)
                return NotFound("لم يتم العثور على المهمة", "TASK_NOT_FOUND");
            if (task.EmpID != userData.userId)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("هذه المهمة ليست مسندة إليك", "TASK_NOT_ASSIGNED"));

            // Wraps TaskManger.EmpUpdateTaskTime exactly — same signature and
            // same side effects (TaskTLog row + ActualTime recompute +
            // notification to employee+company).
            TaskManger.EmpUpdateTaskTime(id, request.Time);

            // Re-fetch to return current state.
            var freshUow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var fresh = freshUow.TaskRepository.Get(
                filter: t => t.TaskID == id,
                includeProperties: "Status").FirstOrDefault();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(TaskMapper.ToActionResult(fresh), "تم تحديث الوقت"));
        }

        // ──────────────────────────── POST /api/v1/tasks (Company) ─────────────────
        [HttpPost]
        public HttpResponseMessage Create(CreateTaskDto request)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (!userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للشركات فقط", "COMPANY_ONLY"));

            if (request == null
                || string.IsNullOrWhiteSpace(request.Title)
                || request.PriorityId <= 0
                || request.EmpId <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("بيانات الطلب غير صحيحة", "INVALID_REQUEST"));
            }

            int companyId = userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            // Authorization: the assignee must belong to the calling company.
            var emp = uow.Employee.Get(filter: e => e.EmpID == request.EmpId).FirstOrDefault();
            if (emp == null || emp.CompanyID != companyId || emp.IsActive != true || emp.IsDeleted == true)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("الموظف غير صالح", "INVALID_EMPLOYEE"));
            }

            // ProjectId optional, but if provided must belong to this company.
            if (request.ProjectId.HasValue && request.ProjectId.Value > 0)
            {
                var project = uow.ProjectRepository.Get(
                    filter: p => p.ProjectID == request.ProjectId.Value && !p.IsDeleted
                ).FirstOrDefault();
                if (project == null || project.CompanyID != companyId)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        ApiResponse.Fail("المشروع غير صالح", "INVALID_PROJECT"));
                }
            }

            // Mirrors the inline insert pattern from
            // Areas/Company/Models/TaskAddEdit.cs::Save() lines 141-200 —
            // identical fields, identical NEW status, identical notification,
            // identical TaskTLog row. No view-model date parsing because the
            // mobile API receives ISO DateTime values directly.
            var task = new Task
            {
                CompanyID = companyId,
                CreatedDate = DateTime.Now,
                Title = request.Title,
                Description = request.Description,
                Summary = request.Summary,
                BriefTaskName = request.BriefTaskName,
                PriorityID = request.PriorityId,
                EmpID = request.EmpId,
                ProjectID = request.ProjectId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ExpectedTime = request.ExpectedTime ?? 0,
                TimeUnitID = request.TimeUnitId,
                IsArchived = false,
                IsDeleted = false,
                StatusID = (int)TaskStatus.New
            };
            uow.TaskRepository.Insert(task);
            uow.Save();

            // Same notification pattern the web sends — Slice 6's FCM line
            // inside NotificationHub.Send will piggyback automatically.
            NotificationHub.Send(
                Users.Employee(request.EmpId),
                NotificationType.NewTask,
                "تم اسناد المهمة ' " + task.Title + " ' لك ",
                "/Employee/Tasks/TaskDetails/" + task.TaskID);

            LogTask.Log(task, null);
            uow.TaskStatuseLog.Insert(new TaskTLog
            {
                TaskID = task.TaskID,
                CreatedDate = DateTime.Now,
                EmpID = task.EmpID,
                StatusID = (int)TaskStatus.New
            });
            uow.Save();

            return Request.CreateResponse(HttpStatusCode.Created,
                ApiResponse.Ok(new TaskActionResultDto
                {
                    TaskId = task.TaskID,
                    NewStatusId = task.StatusID,
                    NewStatusName = "جديدة"
                }, "تم إنشاء المهمة"));
        }

        // ──────────────────────────── POST /api/v1/tasks/{id}/approve (Company) ────
        [HttpPost]
        [ActionName("approve")]
        public HttpResponseMessage Approve(int id)
        {
            return InvokeCompanyAction(id, TaskManger.CompanyAcceptTask,
                expectedFromStatus: TaskStatus.Done, actionLabel: "اعتماد");
        }

        // ──────────────────────────── POST /api/v1/tasks/{id}/disapprove (Company) ─
        [HttpPost]
        [ActionName("disapprove")]
        public HttpResponseMessage Disapprove(int id)
        {
            return InvokeCompanyAction(id, TaskManger.CompanyRejectTask,
                expectedFromStatus: TaskStatus.Done, actionLabel: "رفض");
        }

        // ──────────────────────────── DELETE /api/v1/tasks/{id} (Company) ──────────
        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (!userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للشركات فقط", "COMPANY_ONLY"));

            int companyId = userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var task = uow.TaskRepository.GetByID(id);
            if (task == null || task.IsDeleted)
                return NotFound("لم يتم العثور على المهمة", "TASK_NOT_FOUND");
            if (task.CompanyID != companyId)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("غير مصرح بحذف هذه المهمة", "TASK_FORBIDDEN"));

            // Mirrors web single-delete at Areas/Company/Models/CompanyTaskVM.cs:413.
            // Log first (with isDeleted=true flag), then flip the bit, then save.
            LogTask.Log(task, null, true);
            task.IsDeleted = true;
            uow.TaskRepository.Update(task);
            uow.Save();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(new TaskActionResultDto
                {
                    TaskId = task.TaskID,
                    NewStatusId = task.StatusID,
                    NewStatusName = task.Status != null ? task.Status.Name : null
                }, "تم حذف المهمة"));
        }

        // ──────────────────────────── helpers ────────────────────────────

        private HttpResponseMessage InvokeEmployeeAction(
            int taskId,
            System.Func<int, bool> action,
            TaskStatus expectedFromStatus,
            string actionLabel)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للموظفين فقط", "EMPLOYEE_ONLY"));

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var task = uow.TaskRepository.GetByID(taskId);
            if (task == null || task.IsDeleted)
                return NotFound("لم يتم العثور على المهمة", "TASK_NOT_FOUND");
            if (task.EmpID != userData.userId)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("هذه المهمة ليست مسندة إليك", "TASK_NOT_ASSIGNED"));

            // Guard against invalid transitions before delegating.
            // EmpAcceptTask/EmpRejectTask always mutate; only Complete requires
            // Inprogress. The status check below mirrors what the workflow
            // logic enforces in TaskWorkflow.cs.
            bool legalFromCurrent = IsLegalTransition(task.StatusID, expectedFromStatus);
            if (!legalFromCurrent)
                return Request.CreateResponse(HttpStatusCode.Conflict,
                    ApiResponse.Fail("لا يمكن تنفيذ هذا الإجراء على حالة المهمة الحالية", "INVALID_STATE"));

            bool ok = action(taskId);
            if (!ok)
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    ApiResponse.Fail("تعذر تنفيذ الإجراء", "ACTION_FAILED"));

            // Reload to return fresh status.
            var fresh = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString)
                .TaskRepository.Get(filter: t => t.TaskID == taskId, includeProperties: "Status")
                .FirstOrDefault();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(TaskMapper.ToActionResult(fresh), "تم " + actionLabel + " المهمة"));
        }

        private HttpResponseMessage InvokeCompanyAction(
            int taskId,
            System.Func<int, bool> action,
            TaskStatus expectedFromStatus,
            string actionLabel)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (!userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للشركات فقط", "COMPANY_ONLY"));

            int companyId = userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var task = uow.TaskRepository.GetByID(taskId);
            if (task == null || task.IsDeleted)
                return NotFound("لم يتم العثور على المهمة", "TASK_NOT_FOUND");
            if (task.CompanyID != companyId)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("غير مصرح بهذا الإجراء على المهمة", "TASK_FORBIDDEN"));

            if (!IsLegalTransition(task.StatusID, expectedFromStatus))
                return Request.CreateResponse(HttpStatusCode.Conflict,
                    ApiResponse.Fail("لا يمكن تنفيذ هذا الإجراء على حالة المهمة الحالية", "INVALID_STATE"));

            bool ok = action(taskId);
            if (!ok)
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    ApiResponse.Fail("تعذر تنفيذ الإجراء", "ACTION_FAILED"));

            var fresh = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString)
                .TaskRepository.Get(filter: t => t.TaskID == taskId, includeProperties: "Status")
                .FirstOrDefault();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(TaskMapper.ToActionResult(fresh), "تم " + actionLabel + " المهمة"));
        }

        private static bool IsLegalTransition(int currentStatusId, TaskStatus expectedFromStatus)
        {
            switch (expectedFromStatus)
            {
                case TaskStatus.New:
                    return currentStatusId == (int)TaskStatus.New;
                case TaskStatus.Inprogress:
                    return currentStatusId == (int)TaskStatus.Inprogress
                        || currentStatusId == (int)TaskStatus.Accepted;
                case TaskStatus.Done:
                    // Approve / Disapprove are valid only from Done state.
                    return currentStatusId == (int)TaskStatus.Done;
                default:
                    return true;
            }
        }

        private static bool CanRead(Task task, EtaskMinstry.Models.UserData userData)
        {
            if (userData.isCompany)
                return task.CompanyID == (userData.CompanyId ?? userData.userId);

            // Employee: assigned now OR in history (matches the web list rule).
            if (task.EmpID == userData.userId) return true;
            if (task.TaskTLogs != null && task.TaskTLogs.Any(l => l.EmpID == userData.userId))
                return true;
            return false;
        }

        private static int? MapStatusParam(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return null;
            switch (status.Trim().ToLowerInvariant())
            {
                case "all":         return null;
                case "new":         return (int)TaskStatus.New;
                case "inprogress":  return (int)TaskStatus.Inprogress;
                case "done":        return (int)TaskStatus.Done;
                case "accepted":    return (int)TaskStatus.Accepted;
                case "approved":    return (int)TaskStatus.Approved;
                case "notapproved": return (int)TaskStatus.NotAproved;
                case "pending":     return (int)TaskStatus.Pending;
                default:            return null;   // ignore unknown values
            }
        }

        private HttpResponseMessage NotFound(string message, string code)
        {
            return Request.CreateResponse(HttpStatusCode.NotFound, ApiResponse.Fail(message, code));
        }
    }
}
