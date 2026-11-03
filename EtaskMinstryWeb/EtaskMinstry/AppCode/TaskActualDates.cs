using System;
using System.Linq;

namespace EtaskMinstry.AppCode
{
    /// <summary>
    /// When work on a task actually started and ended, read from its status history
    /// (TaskTLog). Task.StartDate / EndDate are the planned dates from the form and are
    /// never changed here.
    /// </summary>
    public static class TaskActualDates
    {
        /// <summary>
        /// First Inprogress row. Tasks moved Accepted -> Inprogress automatically before that
        /// change was logged (5b0d03d) have no such row; for those, once the task has clearly
        /// been worked on, the start is the later of the first acceptance and the planned
        /// start, since the automatic change needs both - but never after the first Done.
        /// </summary>
        public static DateTime? Start(TaskManagementModel.Task task)
        {
            var logs = task.TaskTLogs;
            var firstInprogress = logs.Where(l => l.StatusID == (int)TaskStatus.Inprogress)
                                      .Select(l => (DateTime?)l.CreatedDate).Min();
            if (firstInprogress.HasValue)
                return firstInprogress;

            bool workedOn = task.StatusID == (int)TaskStatus.Inprogress || task.StatusID == (int)TaskStatus.Done ||
                            task.StatusID == (int)TaskStatus.Approved || task.StatusID == (int)TaskStatus.NotAproved;
            var firstAccepted = logs.Where(l => l.StatusID == (int)TaskStatus.Accepted)
                                    .Select(l => (DateTime?)l.CreatedDate).Min();
            if (!workedOn || !firstAccepted.HasValue)
                return null;

            DateTime start = task.StartDate.HasValue && task.StartDate.Value > firstAccepted.Value
                ? task.StartDate.Value
                : firstAccepted.Value;
            var firstDone = logs.Where(l => l.StatusID == (int)TaskStatus.Done)
                                .Select(l => (DateTime?)l.CreatedDate).Min();
            if (firstDone.HasValue && start > firstDone.Value)
                start = firstAccepted.Value;
            return start;
        }

        /// <summary>
        /// Last Done row, while the task is finished (Done / Approved / NotApproved). A task
        /// reopened after Done has no end until it is finished again.
        /// </summary>
        public static DateTime? End(TaskManagementModel.Task task)
        {
            bool finished = task.StatusID == (int)TaskStatus.Done || task.StatusID == (int)TaskStatus.Approved ||
                            task.StatusID == (int)TaskStatus.NotAproved;
            if (!finished)
                return null;
            return task.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Done)
                                 .Select(l => (DateTime?)l.CreatedDate).Max();
        }

        /// <summary>Date in the site's calendar, plus the time: "dd/MM/yyyy HH:mm", or "-".</summary>
        public static string Format(DateTime? value)
        {
            if (!value.HasValue)
                return "-";
            string date = MvcApplication.IsGregDate ? value.Value.ToGregDatediff() : value.Value.ToHijriDate();
            return date + " " + value.Value.ToString("HH:mm");
        }
    }
}
