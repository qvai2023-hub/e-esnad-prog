using System.Configuration;
using System.Linq;
using TaskManagementModel;

namespace EtaskMinstry.AppCode
{
    /// <summary>
    /// Ownership checks for web actions that receive a record id from the request.
    /// The area authorize attributes only check the caller's role, and the ids are plain
    /// sequential integers, so without these a company could read or change another
    /// company's tasks and employees, and an employee another employee's tasks.
    /// All checks read the current caller from MvcApplication.userData.
    /// </summary>
    public static class RecordAccess
    {
        private static UnitOfWork NewUnitOfWork()
        {
            return new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        private static int? CurrentCompanyId()
        {
            var user = MvcApplication.userData;
            return (user != null && user.isCompany) ? user.userId : (int?)null;
        }

        private static int? CurrentEmployeeId()
        {
            var user = MvcApplication.userData;
            return (user != null && !user.isCompany && user.UserTypeId == (int)LoggedUserType.Employee) ? user.userId : (int?)null;
        }

        #region Company

        public static bool CompanyOwnsTask(int taskId)
        {
            int? companyId = CurrentCompanyId();
            if (companyId == null) return false;
            var task = NewUnitOfWork().TaskRepository.GetByID(taskId);
            return task != null && task.CompanyID == companyId.Value;
        }

        /// <summary>True when every id belongs to the caller's company.</summary>
        public static bool CompanyOwnsTasks(int[] taskIds)
        {
            int? companyId = CurrentCompanyId();
            if (companyId == null) return false;
            if (taskIds == null || taskIds.Length == 0) return true;
            var ids = taskIds.Distinct().ToList();
            int owned = NewUnitOfWork().TaskRepository.Get(filter: t => ids.Contains(t.TaskID) && t.CompanyID == companyId.Value).Count();
            return owned == ids.Count;
        }

        public static bool CompanyOwnsEmployee(int? empId)
        {
            int? companyId = CurrentCompanyId();
            if (companyId == null || !empId.HasValue) return false;
            var emp = NewUnitOfWork().Employee.GetByID(empId.Value);
            return emp != null && emp.CompanyID == companyId.Value;
        }

        public static bool CompanyOwnsProject(int? projectId)
        {
            int? companyId = CurrentCompanyId();
            if (companyId == null || !projectId.HasValue) return false;
            var project = NewUnitOfWork().ProjectRepository.GetByID(projectId.Value);
            return project != null && project.CompanyID == companyId.Value;
        }

        public static bool CompanyOwnsComment(int commentId)
        {
            var comment = NewUnitOfWork().TaskCommentRepository.GetByID(commentId);
            return comment != null && CompanyOwnsTask(comment.TaskID);
        }

        /// <summary>fileName is the stored name (dbo.Attachment.FileName).</summary>
        public static bool CompanyOwnsAttachment(string fileName)
        {
            int? companyId = CurrentCompanyId();
            if (companyId == null || string.IsNullOrEmpty(fileName)) return false;
            return NewUnitOfWork().AttachmentRepository
                .Get(filter: a => a.FileName == fileName && a.Task.CompanyID == companyId.Value)
                .Any();
        }

        #endregion

        #region Employee

        /// <summary>
        /// Same visibility rule as TaskManger.IsValidTask for employees: current assignee,
        /// or the task appears in the employee's status history (reassigned away).
        /// </summary>
        public static bool EmployeeCanSeeTask(int? taskId)
        {
            int? empId = CurrentEmployeeId();
            if (empId == null || !taskId.HasValue) return false;
            var uow = NewUnitOfWork();
            var task = uow.TaskRepository.GetByID(taskId.Value);
            if (task == null || task.IsDeleted) return false;
            return task.EmpID == empId.Value
                   || uow.TaskStatuseLog.Get(filter: l => l.TaskID == taskId.Value && l.EmpID == empId.Value).Any();
        }

        /// <summary>Changes to a task are only for its current assignee.</summary>
        public static bool EmployeeIsAssignee(int taskId)
        {
            int? empId = CurrentEmployeeId();
            if (empId == null) return false;
            var task = NewUnitOfWork().TaskRepository.GetByID(taskId);
            return task != null && !task.IsDeleted && task.EmpID == empId.Value;
        }

        public static bool EmployeeCanSeeComment(int commentId)
        {
            var comment = NewUnitOfWork().TaskCommentRepository.GetByID(commentId);
            return comment != null && EmployeeCanSeeTask(comment.TaskID);
        }

        /// <summary>fileName is the stored name (dbo.Attachment.FileName).</summary>
        public static bool EmployeeCanSeeAttachment(string fileName)
        {
            if (CurrentEmployeeId() == null || string.IsNullOrEmpty(fileName)) return false;
            var attachment = NewUnitOfWork().AttachmentRepository.Get(filter: a => a.FileName == fileName).FirstOrDefault();
            return attachment != null && EmployeeCanSeeTask(attachment.TaskID);
        }

        #endregion
    }
}
