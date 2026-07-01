using System.Collections.Generic;
using System.Linq;
using EtaskMinstry.Api.Dtos.Tasks;
using TaskManagementModel;

namespace EtaskMinstry.Api.Mapping
{
    /// <summary>
    /// EF Task entity â†’ API DTO. The ONLY place that touches both worlds.
    /// </summary>
    public static class TaskMapper
    {
        public static TaskListItemDto ToListItem(Task t)
        {
            if (t == null) return null;
            return new TaskListItemDto
            {
                TaskId = t.TaskID,
                Title = t.Title,
                StatusId = t.StatusID,
                StatusName = t.Status != null ? t.Status.Name : null,
                PriorityId = t.PriorityID,
                PriorityName = t.Priority != null ? t.Priority.Name : null,
                PriorityColor = t.Priority != null ? t.Priority.Color : null,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                ExpectedTime = t.ExpectedTime,
                ActualTime = t.ActualTime,
                ProjectName = t.Project != null ? t.Project.Name : null,
                // isDelayed is a client-side computed property â€” caller MUST
                // materialize before invoking this mapper.
                IsDelayed = SafeIsDelayed(t),
                IsAccepted = t.StatusID == (int)TaskStatus.Accepted
            };
        }

        public static TaskDetailDto ToDetail(Task t)
        {
            if (t == null) return null;

            var logs = new List<TaskStatusLogDto>();
            if (t.TaskTLogs != null)
            {
                // TaskTLog has no Employee nav property in the EDMX. We fill
                // EmpName only when the log's EmpID matches the task's current
                // assignee (the common case in practice). The rare cross-
                // employee case shows EmpId without EmpName.
                string currentAssigneeName = t.Employee != null ? t.Employee.Name : null;
                int? currentAssigneeId = t.EmpID;

                logs = t.TaskTLogs
                    .OrderBy(l => l.TaskTLogID)
                    .Select(l => new TaskStatusLogDto
                    {
                        LogId = l.TaskTLogID,
                        StatusId = l.StatusID,
                        StatusName = l.Status != null ? l.Status.Name : null,
                        EmpId = l.EmpID,
                        EmpName = (l.EmpID.HasValue && l.EmpID == currentAssigneeId)
                                    ? currentAssigneeName : null,
                        CreatedDate = l.CreatedDate,
                        TimeCount = l.TimeCount
                    })
                    .ToList();
            }

            return new TaskDetailDto
            {
                TaskId = t.TaskID,
                Title = t.Title,
                Description = t.Description,
                Summary = t.Summary,
                CreatedDate = t.CreatedDate,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                DeliverDate = t.DeliverDate,
                ExpectedTime = t.ExpectedTime,
                ActualTime = t.ActualTime,
                TimeUnitId = t.TimeUnitID,
                TimeUnitName = t.TimeUnit != null ? t.TimeUnit.Name : null,
                StatusId = t.StatusID,
                StatusName = t.Status != null ? t.Status.Name : null,
                PriorityId = t.PriorityID,
                PriorityName = t.Priority != null ? t.Priority.Name : null,
                PriorityColor = t.Priority != null ? t.Priority.Color : null,
                EmpId = t.EmpID,
                EmpName = t.Employee != null ? t.Employee.Name : null,
                CompanyId = t.CompanyID,
                CompanyName = t.Company != null ? t.Company.Name : null,
                ProjectId = t.ProjectID,
                ProjectName = t.Project != null ? t.Project.Name : null,
                IsArchived = t.IsArchived,
                IsRecurrence = t.IsRecurrence ?? false,
                IsDelayed = SafeIsDelayed(t),
                StatusLog = logs
            };
        }

        private static bool SafeIsDelayed(Task t)
        {
            if (t == null) return false;
            try
            {
                return t.isDelayed;
            }
            catch (System.NullReferenceException)
            {
                return false;
            }
        }
        public static TaskActionResultDto ToActionResult(Task t)
        {
            if (t == null) return null;
            return new TaskActionResultDto
            {
                TaskId = t.TaskID,
                NewStatusId = t.StatusID,
                NewStatusName = t.Status != null ? t.Status.Name : null
            };
        }
    }
}

