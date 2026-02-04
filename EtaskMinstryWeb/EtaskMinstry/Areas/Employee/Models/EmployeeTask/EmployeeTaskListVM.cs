using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementModel;
using EtaskMinstry;
using EtaskMinstry.Models.EmployeeTask;
using EtaskMinstry;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Models.EmployeeTask
{
    public class EmployeeTaskListVM
    {
        public int CurrentStatuse { get; set; }
        public bool isDelay { get; set; }
        public int NewCount { get; set; }
        public int EndedCount { get; set; }
        public List<EmployeeTaskVM> TaskList { get; set; }

        [ScaffoldColumn(false)]
        private UnitOfWork _unitOfWork;

        public EmployeeTaskListVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            TaskList = new List<EmployeeTaskVM>();
        }

        /// <summary>
        /// get employee tasks
        /// </summary>
        /// <param name="statuse"></param>
        /// <param name="title"></param>
        /// <param name="priorityId"></param>
        /// <param name="fDate"></param>
        /// <param name="tDate"></param>
        public void FillTasks(int statuse, string title, int priorityId, string fDate, string tDate, string FromEndDate, string ToEndDate)
        {

            //var tasks2 = _unitOfWork.TaskRepository.Get().ToList();

            //tasks2 = tasks2.Where(t => (t.StartDate == DateTime.Now.Date) && (t.StatusID == (int)TaskStatus.New || t.StatusID == (int)TaskStatus.Accepted)).ToList();
            //tasks2.ForEach(delegate(TaskManagementModel.Task t)
            //{
            //    var oldtask = t.Clone<TaskManagementModel.Task>();
            //    t.StatusID = (int)TaskStatus.Inprogress;
            //    //log.
            //   EtaskMinstry.AppCode.LogTask.Log(t, oldtask);
            //    StatusLog.LogStatuse(t.TaskID, t.StatusID, MvcApplication.userData.userId);
            //    _unitOfWork.TaskRepository.Update(t);
            //});

            //try
            //{
            //    _unitOfWork.Save();

            //}
            //catch { }


            CurrentStatuse = statuse;
            isDelay = CurrentStatuse == (int)TaskStatus.Delay ? true : false;
           
            int currentUser = MvcApplication.userData.isCompany ? 0 : MvcApplication.userData.userId;
            NewCount = _unitOfWork.TaskRepository.Get(t => t.StatusID == (int)TaskStatus.New && t.EmpID == currentUser && !t.IsDeleted).Count();
            EndedCount = _unitOfWork.TaskRepository.Get(t => t.StatusID == (int)TaskStatus.Done && t.EmpID == currentUser && !t.IsDeleted).Count();
            DateTime? fromDate = null, toDate = null;

            if (!string.IsNullOrEmpty(fDate))
                //fromDate = Convert.ToDateTime(fDate.ToGregExact());
                fromDate = (MvcApplication.IsGregDate ? fDate.ToGregExactformate() : Convert.ToDateTime(fDate.ToGregExact()));
            if (!string.IsNullOrEmpty(tDate))
                toDate = (MvcApplication.IsGregDate ? tDate.ToGregExactformate() : Convert.ToDateTime(tDate.ToGregExact()));
            //toDate = Convert.ToDateTime(fDate.ToGregExact());


            DateTime dtFromEndDate = new DateTime();
            DateTime dtToEndDate = new DateTime();

            if (!String.IsNullOrEmpty(FromEndDate))
                dtFromEndDate = (MvcApplication.IsGregDate ? FromEndDate.ToGregExactformate() : FromEndDate.ToGregExact());

            if (!String.IsNullOrEmpty(ToEndDate))
                dtToEndDate = (MvcApplication.IsGregDate ? ToEndDate.ToGregExactformate() : ToEndDate.ToGregExact());


            //check if the task assigned to the current employee or was assigned to him then reAssigned to another employee
            var tasks = _unitOfWork.TaskRepository.Get(filter: t => (t.EmpID == currentUser || t.TaskTLogs.Any(o => o.EmpID == currentUser))
                  && !t.IsDeleted
                  // && t.IsArchived != true // check that all tasks are not in archive.
                  && t.StatusID != (int)TaskStatus.Rejected
                  && (statuse == (int)TaskStatus.All ? statuse == (int)TaskStatus.All //if need all tasks 
                  : statuse == (int)TaskStatus.Delay ? true //get delay and not done tasks 
                                                            //get new or accepted task in the new tab
                  : statuse == (int)TaskStatus.New ? ((t.StatusID == (int)TaskStatus.New && t.EmpID == currentUser) || (t.StatusID == (int)TaskStatus.Accepted && t.StartDate > DateTime.Now && t.EmpID == currentUser))
                  //get tasks if it pending or reassigned to another employee
                  : statuse == (int)TaskStatus.Pending ? (t.StatusID == (int)TaskStatus.Pending || (t.TaskTLogs.FirstOrDefault(o => o.EmpID == currentUser) != null && t.TaskTLogs.FirstOrDefault(o => o.EmpID == currentUser).EmpID != t.TaskTLogs.OrderByDescending(o => o.TaskTLogID).FirstOrDefault().EmpID))
                  : t.StatusID == statuse)
                  && (priorityId == 0 || t.PriorityID == priorityId)
                  && (title == "" || t.Title.ToLower().Contains(title.Trim().ToLower())))
                  .Where(o => (fromDate == null || o.StartDate >= fromDate) && (toDate == null || o.StartDate <= toDate)
                   && (String.IsNullOrEmpty(FromEndDate) ||
                                                        o.EndDate.Value >= dtFromEndDate)

                                                    && (String.IsNullOrEmpty(ToEndDate) ||
                                                        o.EndDate.Value <= dtToEndDate)

                  ).OrderByDescending(t => t.TaskID).ToList();


            for (int i = 0; i < tasks.Count; i++)
            {

                if (tasks[i].StatusID == (int)TaskStatus.Pending)
                {
                    tasks = tasks.Where(t => (statuse == (int)TaskStatus.Delay ? t.isDelayed : true)).OrderByDescending(t => t.TaskID).ToList();
                }
            }


            // if task is pendding for emp1 and new and delay for emp2 .. then dont apear for emp1 in delay tab
            if (statuse == (int)TaskStatus.Delay)
            {
                tasks = tasks.Where(t => t.EmpID == currentUser).OrderByDescending(t => t.TaskID).ToList();
            }

            tasks.ForEach(delegate (TaskManagementModel.Task t)
        {
            var obj = new EmployeeTaskVM();
            var log = t.TaskTLogs.LastOrDefault(l => l.EmpID == EtaskMinstry.MvcApplication.userData.userId);
            obj.taskID = t.TaskID;
            obj.priorityId = t.PriorityID;
            obj.taskName = t.Title;
            obj.projectName = t.Project != null ? t.Project.Name : "";
            obj.priority = t.Priority.Name;
            obj.startDate = t.StartDate != null ? (MvcApplication.IsGregDate) ? t.StartDate.Value.ToGregArabicDate() : t.StartDate.Value.ToHijriArabicDate() : "";
            obj.endDate = t.EndDate != null ? (MvcApplication.IsGregDate) ? t.EndDate.Value.ToGregArabicDate() : t.EndDate.Value.ToHijriArabicDate() : "";
            obj.US_startDate = t.StartDate != null ? (MvcApplication.IsGregDate) ? t.StartDate.Value.ToGregArabicDate() : t.StartDate.Value.ToShortDateString() : "";
            obj.rowColor = t.Priority.Color;
            obj.actualTime = t.ActualTime == null ? 0 : t.ActualTime;
            obj.delaytime = t.delayTime;
            obj.isAccepted = t.StatusID == (int)TaskStatus.Accepted;

            obj.EndTaskClass = t.StatusID == (int)TaskStatus.Done ? "" : "end";
            obj.EndTaskSource = t.StatusID == (int)TaskStatus.Done ? "/Content/Main/images/Pend.png" : "/Content/Main/images/end-icon.png";
            if (t.EmpID == EtaskMinstry.MvcApplication.userData.userId)
            {
                obj.statusId = t.StatusID;
                obj.state = t.Status.Name;
            }
            else
            {
                // get last Employee log 
                // var log = t.TaskTLogs.LastOrDefault(l => l.EmpID == EtaskMinstry.MvcApplication.userData.userId);

                obj.statusId = log.StatusID;
                obj.state = log.Status.Name;

            }

            if (!(((statuse == (int)TaskStatus.Inprogress) && (log==null ||log.StatusID == (int)TaskStatus.Pending)) || ((statuse == (int)TaskStatus.Done) && (log == null || log.StatusID == (int)TaskStatus.Pending)) || ((statuse == (int)TaskStatus.Delay) && (t.isDelayed == false))))
               { TaskList.Add(obj); }

               //if (!(((statuse == (int)TaskStatus.Inprogress) && (log.StatusID == (int)TaskStatus.Pending)) || ((statuse == (int)TaskStatus.Done) && (log.StatusID == (int)TaskStatus.Pending)) ))
               //{ TaskList.Add(obj);}
               
            //new EmployeeTaskVM
          //  {
          //      taskID = t.TaskID,
          //      priorityId = t.PriorityID,
          //      taskName = t.Title,
          //      projectName = t.Project != null ? t.Project.Name : "",
          //      priority = t.Priority.Name,
          //      state = t.Status.Name,
          //      statusId = t.StatusID,
          //      startDate = t.StartDate != null ? t.StartDate.Value.ToHijriArabicDate() : "",
          //      endDate = t.EndDate != null ? t.EndDate.Value.ToHijriArabicDate() : "",
          //      US_startDate = t.StartDate != null ? t.StartDate.Value.ToShortDateString() : "",
          //      rowColor = t.Priority.Color,
          //      actualTime = t.ActualTime == null ? 0 : t.ActualTime,
          //      delaytime = t.delayTime,
          //      isAccepted = t.StatusID == (int)TaskStatus.Accepted,
          //      EndTaskClass = t.StatusID == (int)TaskStatus.Done ? "" : "end",
          //      EndTaskSource = t.StatusID == (int)TaskStatus.Done ? "/Content/Main/images/Pend.png" : "/Content/Main/images/end-icon.png"
         
          //  });
        });
        }

        /// <summary>
        /// for employee to accept or reject task
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="isAccept"></param>
        /// <returns></returns>
        public bool DeliverTask(int taskId, bool isAccept)
        {
            var result = false;
            var task = _unitOfWork.TaskRepository.GetByID(taskId);
            var oldtask = task.Clone<TaskManagementModel.Task>();
            task.StatusID = isAccept ? (int)TaskStatus.Accepted : (int)TaskStatus.Rejected;

            //log.
            EtaskMinstry.AppCode.LogTask.Log(task, oldtask);
            StatusLog.LogStatuse(taskId, task.StatusID);

            //Notification
            NotificationHub.Send(Users.Company(task.CompanyID),isAccept?NotificationType.AcceptTask:NotificationType.RejectTask,
                isAccept ? task.Title + " : تم قبول المهمة" : task.Title + " : تم رفض المهمة", "/Company/Company/TaskDetails?id=" + task.TaskID);
            

            if (task.StartDate <= DateTime.Now)
            {
                task.StatusID = (int)TaskStatus.Inprogress;

                //log.
                EtaskMinstry.AppCode.LogTask.Log(task, oldtask);
                StatusLog.LogStatuse(taskId, task.StatusID);
            }
            _unitOfWork.TaskRepository.Update(task);

            try
            {
                _unitOfWork.Save();
                result = true;
            }
            catch { }
            return result;
        }

        /// <summary>
        /// End Specific task
        /// convert its statuse to End.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool EndTask(int taskId)
        {
            var result = false;
            var task = _unitOfWork.TaskRepository.GetByID(taskId);
            var oldtask = task.Clone<TaskManagementModel.Task>();
            task.StatusID = (int)TaskStatus.Done;
            task.DeliverDate = DateTime.Now;
            task.EndDate = DateTime.Now;
            _unitOfWork.TaskRepository.Update(task);
            try
            {
                _unitOfWork.Save();
                result = true;

                //log.
                EtaskMinstry.AppCode.LogTask.Log(task, oldtask);
                StatusLog.LogStatuse(taskId, task.StatusID);

                //Notification
                NotificationHub.Send(Users.Company(task.CompanyID), NotificationType.EndTask,
                                    task.Title + " : تم انهاء المهمة " , "/Company/Company/TaskDetails?id=" + task.TaskID);
            }
            catch
            {
            }
            return result;
        }
    }
}