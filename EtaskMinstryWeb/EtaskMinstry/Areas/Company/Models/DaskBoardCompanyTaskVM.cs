using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using EtaskMinstry;
using EtaskMinstry.AppCode;
using EtaskMinstry.App_Code;
using EtaskMinstry.Models.EmployeeTask;
using TaskManagementModel;

namespace EtaskMinstry.Models.Company
{
    public class DaskBoardCompanyTaskVM
    {
        #region"Properties"

       
        [ScaffoldColumn(false)]
        public int TaskID { get; set; }

      
        [LocalizedDisplayName("Task", NameResourceType = typeof (Resources.Company.CompanyTask))]
        public String Task { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.EmoloyeeTask.EmployeeTask))]
        public String Status { get; set; }

     
        public int StatusID { get; set; }

        public int PrioirtyID { get; set; }
   
        public String Prioirty { get; set; }

        public String EmpName { get; set; }

        public int? EmpID { get; set; }

        public DateTime? FinishDate { get; set; }



        public String ArabicStartDate { get; set; }

     
        public Boolean IsDelayed { get; set; }
        public string delayTime { get; set; }
        
      
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public String HijriStartDate { get; set; }

        public String HijriEndDate { get; set; }
        public String HijriFinishDate { get; set; }
        public string HijriRejectedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DashBoaedTaskType DBStatus { get; set; }
        public DateTime? SuspendedDate { get; set; }
        public string HijriSuspendedDate { get; set; }
        public double delayPercentage { get; set; }
        #endregion

        #region"Manage"

        private UnitOfWork _unitOfWork;

        public DaskBoardCompanyTaskVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }



        public List<DaskBoardCompanyTaskVM> SelectCompanyTasks(DashBoaedTaskType taskType)
        {
          
            UnitOfWork _unitOfWork =
               new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            //DateTime? PlusOneDay = DateTime.Now.AddDays(1);
            //DateTime? MinusOneDay = DateTime.Now.AddDays(-1);
            DateTime? DateNow = DateTime.Now.Date;

            // Get Company Employees .
            List<EmployeeProfile> CompanyEmployee = EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployee(MvcApplication.userData.userId);


            List<DaskBoardCompanyTaskVM> objTasks = new List<DaskBoardCompanyTaskVM>();
            switch( taskType)
            {
                    case DashBoaedTaskType.NeedAssign: // Need assign without Employees 
                    objTasks= _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && !t.EmpID.HasValue && !t.IsDeleted).Select(t => new DaskBoardCompanyTaskVM()
                                                        {
                                                            TaskID = t.TaskID,
                                                            Task = t.Title,                                                         
                                                            Status = t.Status.Name,
                                                            StatusID = t.StatusID,
                                                            PrioirtyID = t.PriorityID,
                                                            Prioirty = t.Priority.Name,
                                                            StartDate = t.StartDate.Value,
                                                            EndDate = t.EndDate.Value,
                                                            EmpID = t.EmpID,
                                                           // Get Finish Date .
                                                            FinishDate = t.DeliverDate                                                        
                                                        }).OrderByDescending(i => i.TaskID).ToList();

                    break;
                    case DashBoaedTaskType.Delayed: // Delayed Tasks
                     objTasks= _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                       ).Select(t => new DaskBoardCompanyTaskVM()
                                                        {
                                                            TaskID = t.TaskID,
                                                            Task = t.Title,
                                                            Status = t.Status.Name,
                                                            StatusID = t.StatusID,
                                                            PrioirtyID = t.PriorityID,
                                                            Prioirty = t.Priority.Name,
                                                            StartDate = t.StartDate.Value,
                                                            EndDate = t.EndDate.Value,
                                                            EmpID = t.EmpID,
                                                          // Get Finish Date .
                                                            FinishDate = t.DeliverDate                                                          
                                                        }).OrderByDescending(i => i.TaskID).ToList();

                       objTasks.ForEach(i => i.IsDelayed = isTaskDelayed(i));
                       objTasks = objTasks.Where(i => i.IsDelayed).ToList();

                    break;

                case DashBoaedTaskType.FinishToday: //EndDate =today
                    var obj = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                         &&   t.StatusID == (int)TaskStatus.Inprogress);
                             
                         objTasks= _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                             && t.EndDate == DateNow && t.StatusID == (int)TaskStatus.Inprogress)
                             .Select(t => new DaskBoardCompanyTaskVM()
                             {
                                 TaskID = t.TaskID,
                                 Task = t.Title,
                                 Status = t.Status.Name,
                                 StatusID = t.StatusID,
                                 PrioirtyID = t.PriorityID,
                                 Prioirty = t.Priority.Name,
                                 StartDate = t.StartDate.Value,
                                 EndDate = t.EndDate.Value,
                                 EmpID = t.EmpID.Value,
                                 // Get Finish Date .
                                 FinishDate = t.DeliverDate                                                    
                                                        }).OrderByDescending(i => i.TaskID).ToList();

                     

                    break;
                    case DashBoaedTaskType.Susspended: // SuspendedStatud
                         objTasks= _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                             && t.StatusID ==(int) TaskStatus.Pending)
                             .Select(t => new DaskBoardCompanyTaskVM()
                             {
                                 TaskID = t.TaskID,
                                 Task = t.Title,
                                 Status = t.Status.Name,
                                 StatusID = t.StatusID,
                                 PrioirtyID = t.PriorityID,
                                 Prioirty = t.Priority.Name,
                                 StartDate = t.StartDate.Value,
                                 EndDate = t.EndDate.Value,
                                 EmpID = t.EmpID.Value,
                                // Get Finish Date 
                                 SuspendedDate = t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault() == null ?
                                 DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                                 FinishDate = t.EndDate                                                    
                                                        }).OrderByDescending(i => i.TaskID).ToList();

                         objTasks.ForEach(
                     i => i.HijriSuspendedDate = (i.SuspendedDate != DateTime.MinValue) ? MvcApplication.IsGregDate ? i.SuspendedDate.Value.ToGregArabicDate() : i.SuspendedDate.Value.ToHijriArabicDate() : String.Empty);

                    objTasks.ForEach(
               i => i.HijriFinishDate = (i.FinishDate.HasValue) ? MvcApplication.IsGregDate ? i.FinishDate.Value.ToGregArabicDate() : i.FinishDate.Value.ToHijriArabicDate() : String.Empty);
                    break;
                    case DashBoaedTaskType.Empfinish:
                            objTasks= _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                             && t.StatusID ==(int) TaskStatus.Done)
                             .Select(t => new DaskBoardCompanyTaskVM()
                             {
                                 TaskID = t.TaskID,
                                 Task = t.Title,
                                 Status = t.Status.Name,
                                 StatusID = t.StatusID,
                                 PrioirtyID = t.PriorityID,
                                 Prioirty = t.Priority.Name,
                                 StartDate = t.StartDate.Value,
                                 EndDate = t.EndDate.Value,
                                 EmpID = t.EmpID.Value,
                                 // Get Finish Date .
                                 FinishDate = t.DeliverDate                                                    
                                                        }).OrderByDescending(i => i.TaskID).ToList();

                    objTasks.ForEach(
                        i => i.HijriFinishDate = (i.FinishDate.HasValue) ? MvcApplication.IsGregDate ? i.FinishDate.Value.ToGregArabicDate() : i.FinishDate.Value.ToHijriArabicDate() : String.Empty);

                    break;
                   
                    case DashBoaedTaskType.EmpReject:
                          objTasks= _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                             && t.StatusID ==(int) TaskStatus.Rejected)
                             .Select(t => new DaskBoardCompanyTaskVM()
                             {
                                 TaskID = t.TaskID,
                                 Task = t.Title,
                                 Status = t.Status.Name,
                                 StatusID = t.StatusID,
                                 PrioirtyID = t.PriorityID,
                                 Prioirty = t.Priority.Name,
                                 StartDate = t.StartDate.Value,
                                 EndDate = t.EndDate.Value,
                                 RejectedDate = t.TaskTLogs.Where(l=>l.StatusID== (int)TaskStatus.Rejected).OrderByDescending(s=>s.TaskTLogID).FirstOrDefault() == null ?
                                 DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Rejected).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                                 EmpID = t.EmpID.Value,
                                // Get Finish Date .
                                 FinishDate = t.DeliverDate                                                    
                                                        }).OrderByDescending(i => i.TaskID).ToList();

                          objTasks.ForEach(
                   i => i.HijriRejectedDate = (i.RejectedDate != DateTime.MinValue) ? MvcApplication.IsGregDate ? i.RejectedDate.Value.ToGregArabicDate() : i.RejectedDate.Value.ToHijriArabicDate() : String.Empty);
                          objTasks.ForEach(
                                           i => i.HijriStartDate = (i.StartDate.HasValue) ? MvcApplication.IsGregDate ? i.StartDate.Value.ToGregArabicDate() : i.StartDate.Value.ToHijriArabicDate() : String.Empty);
                    break;
                    case DashBoaedTaskType.New: // Need assign without Employees 
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.StatusID==(int)TaskStatus.New).Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID,
                            // Get Finish Date .
                            FinishDate = t.DeliverDate
                        }).OrderByDescending(i => i.TaskID).ToList();
                    objTasks.ForEach(
                                           i => i.HijriStartDate = (i.StartDate.HasValue) ? MvcApplication.IsGregDate ? i.StartDate.Value.ToGregArabicDate() : i.StartDate.Value.ToHijriArabicDate() : String.Empty);

                    objTasks.ForEach(
                                           i => i.HijriEndDate = (i.EndDate.HasValue) ? MvcApplication.IsGregDate ? i.EndDate.Value.ToGregArabicDate() : i.EndDate.Value.ToHijriArabicDate() : String.Empty);
               
                
                    break;


            }
            // Batch load delay data and employee names to avoid N+1 queries
            FillTaskMetadata(objTasks, taskType, _unitOfWork);
                    return objTasks;
        }

        /// <summary>
        /// Batch loads delay data and employee names for all tasks in a single query
        /// instead of N+1 individual GetByID calls per task.
        /// </summary>
        private void FillTaskMetadata(List<DaskBoardCompanyTaskVM> objTasks, DashBoaedTaskType taskType, UnitOfWork unitOfWork)
        {
            if (!objTasks.Any()) return;

            // Batch load task entities for delay info
            var taskIds = objTasks.Select(t => t.TaskID).ToList();
            var taskEntities = unitOfWork.TaskRepository.Get(t => taskIds.Contains(t.TaskID)).ToList();
            var taskDict = taskEntities.ToDictionary(t => t.TaskID);

            // Batch load employee names
            var empIds = objTasks.Where(t => t.EmpID.HasValue).Select(t => t.EmpID.Value).Distinct().ToList();
            var empDict = new Dictionary<int, string>();
            if (empIds.Any())
            {
                var employees = unitOfWork.Employee.Get(e => empIds.Contains(e.EmpID) && e.IsActive.HasValue && e.IsActive.Value).ToList();
                foreach (var emp in employees)
                {
                    if (!empDict.ContainsKey(emp.EmpID))
                        empDict[emp.EmpID] = emp.Name;
                }
            }

            foreach (var task in objTasks)
            {
                task.DBStatus = taskType;
                task.EmpName = task.EmpID.HasValue && empDict.ContainsKey(task.EmpID.Value) ? empDict[task.EmpID.Value] : "_";

                if (taskDict.ContainsKey(task.TaskID))
                {
                    var entity = taskDict[task.TaskID];
                    task.IsDelayed = entity.isDelayed;
                    task.delayTime = entity.delayTime != null ? entity.delayTime.Replace('-', ' ') : "";
                    task.delayPercentage = entity.DelayPercentage;
                }
            }
        }

        public List<DaskBoardCompanyTaskVM> SelectCompanyTasks(DashBoaedTaskType taskType, out int count, int page = 1, int pageSize = 10)
        {
            UnitOfWork _unitOfWork =
               new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            DateTime? DateNow = DateTime.Now.Date;
            List<DaskBoardCompanyTaskVM> objTasks = new List<DaskBoardCompanyTaskVM>();
            switch (taskType)
            {
                case DashBoaedTaskType.NeedAssign: // Need assign without Employees 
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && !t.EmpID.HasValue && !t.IsDeleted).Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID,
                            // Get Finish Date .
                            FinishDate = t.DeliverDate
                        }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                     && !t.EmpID.HasValue && !t.IsDeleted).Select(t => new DaskBoardCompanyTaskVM()
                     {
                         TaskID = t.TaskID,
                         Task = t.Title,
                         Status = t.Status.Name,
                         StatusID = t.StatusID,
                         PrioirtyID = t.PriorityID,
                         Prioirty = t.Priority.Name,
                         StartDate = t.StartDate.Value,
                         EndDate = t.EndDate.Value,
                         EmpID = t.EmpID,
                         // Get Finish Date .
                         FinishDate = t.DeliverDate
                     }).Count();

                    break;
                case DashBoaedTaskType.Delayed: // Delayed Tasks
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                      ).Select(t => new DaskBoardCompanyTaskVM()
                      {
                          TaskID = t.TaskID,
                          Task = t.Title,
                          Status = t.Status.Name,
                          StatusID = t.StatusID,
                          PrioirtyID = t.PriorityID,
                          Prioirty = t.Priority.Name,
                          StartDate = t.StartDate.Value,
                          EndDate = t.EndDate.Value,
                          EmpID = t.EmpID,
                          // Get Finish Date .
                          FinishDate = t.DeliverDate
                      }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                     ).Select(t => new DaskBoardCompanyTaskVM()
                     {
                         TaskID = t.TaskID,
                         Task = t.Title,
                         Status = t.Status.Name,
                         StatusID = t.StatusID,
                         PrioirtyID = t.PriorityID,
                         Prioirty = t.Priority.Name,
                         StartDate = t.StartDate.Value,
                         EndDate = t.EndDate.Value,
                         EmpID = t.EmpID,
                         // Get Finish Date .
                         FinishDate = t.DeliverDate
                     }).Count();
                    objTasks.ForEach(i => i.IsDelayed = isTaskDelayed(i));
                    objTasks = objTasks.Where(i => i.IsDelayed).ToList();

                    break;

                case DashBoaedTaskType.FinishToday: //EndDate =today
                    var obj = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                         && t.StatusID == (int)TaskStatus.Inprogress);

                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.EndDate == DateNow && t.StatusID == (int)TaskStatus.Inprogress)
                        .Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID.Value,
                            // Get Finish Date .
                            FinishDate = t.DeliverDate
                        }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.EndDate == DateNow && t.StatusID == (int)TaskStatus.Inprogress)
                        .Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID.Value,
                            // Get Finish Date .
                            FinishDate = t.DeliverDate
                        }).Count();

                    break;
                case DashBoaedTaskType.Susspended: // SuspendedStatud
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.StatusID == (int)TaskStatus.Pending)
                        .Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID.Value,
                            // Get Finish Date 
                            SuspendedDate = t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault() == null ?
                            DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                            FinishDate = t.EndDate
                        }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.StatusID == (int)TaskStatus.Pending)
                        .Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID.Value,
                            // Get Finish Date 
                            SuspendedDate = t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault() == null ?
                            DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                            FinishDate = t.EndDate
                        }).Count();
                    objTasks.ForEach(
                i => i.HijriSuspendedDate = (i.SuspendedDate != DateTime.MinValue) ? MvcApplication.IsGregDate ? i.SuspendedDate.Value.ToGregArabicDate() : i.SuspendedDate.Value.ToHijriArabicDate() : String.Empty);

                    objTasks.ForEach(
               i => i.HijriFinishDate = (i.FinishDate.HasValue) ? MvcApplication.IsGregDate ? i.FinishDate.Value.ToGregArabicDate() : i.FinishDate.Value.ToHijriArabicDate() : String.Empty);
                    break;
                case DashBoaedTaskType.Empfinish:
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                     && t.StatusID == (int)TaskStatus.Done)
                     .Select(t => new DaskBoardCompanyTaskVM()
                     {
                         TaskID = t.TaskID,
                         Task = t.Title,
                         Status = t.Status.Name,
                         StatusID = t.StatusID,
                         PrioirtyID = t.PriorityID,
                         Prioirty = t.Priority.Name,
                         StartDate = t.StartDate.Value,
                         EndDate = t.EndDate.Value,
                         EmpID = t.EmpID.Value,
                         // Get Finish Date .
                         FinishDate = t.DeliverDate
                     }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                     && t.StatusID == (int)TaskStatus.Done)
                     .Select(t => new DaskBoardCompanyTaskVM()
                     {
                         TaskID = t.TaskID,
                         Task = t.Title,
                         Status = t.Status.Name,
                         StatusID = t.StatusID,
                         PrioirtyID = t.PriorityID,
                         Prioirty = t.Priority.Name,
                         StartDate = t.StartDate.Value,
                         EndDate = t.EndDate.Value,
                         EmpID = t.EmpID.Value,
                         // Get Finish Date .
                         FinishDate = t.DeliverDate
                     }).Count();
                    objTasks.ForEach(
                        i => i.HijriFinishDate = (i.FinishDate.HasValue) ? MvcApplication.IsGregDate ? i.FinishDate.Value.ToGregArabicDate() : i.FinishDate.Value.ToHijriArabicDate() : String.Empty);

                    break;

                case DashBoaedTaskType.EmpReject:
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                       && t.StatusID == (int)TaskStatus.Rejected)
                       .Select(t => new DaskBoardCompanyTaskVM()
                       {
                           TaskID = t.TaskID,
                           Task = t.Title,
                           Status = t.Status.Name,
                           StatusID = t.StatusID,
                           PrioirtyID = t.PriorityID,
                           Prioirty = t.Priority.Name,
                           StartDate = t.StartDate.Value,
                           EndDate = t.EndDate.Value,
                           RejectedDate = t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Rejected).OrderByDescending(s => s.TaskTLogID).FirstOrDefault() == null ?
                           DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Rejected).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                           EmpID = t.EmpID.Value,
                           // Get Finish Date .
                           FinishDate = t.DeliverDate
                       }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                       && t.StatusID == (int)TaskStatus.Rejected)
                       .Select(t => new DaskBoardCompanyTaskVM()
                       {
                           TaskID = t.TaskID,
                           Task = t.Title,
                           Status = t.Status.Name,
                           StatusID = t.StatusID,
                           PrioirtyID = t.PriorityID,
                           Prioirty = t.Priority.Name,
                           StartDate = t.StartDate.Value,
                           EndDate = t.EndDate.Value,
                           RejectedDate = t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Rejected).OrderByDescending(s => s.TaskTLogID).FirstOrDefault() == null ?
                           DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Rejected).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                           EmpID = t.EmpID.Value,
                           // Get Finish Date .
                           FinishDate = t.DeliverDate
                       }).Count();

                    objTasks.ForEach(
             i => i.HijriRejectedDate = (i.RejectedDate != DateTime.MinValue) ? MvcApplication.IsGregDate ? i.RejectedDate.Value.ToGregArabicDate() : i.RejectedDate.Value.ToHijriArabicDate() : String.Empty);
                    objTasks.ForEach(
                                     i => i.HijriStartDate = (i.StartDate.HasValue) ? MvcApplication.IsGregDate ? i.StartDate.Value.ToGregArabicDate() : i.StartDate.Value.ToHijriArabicDate() : String.Empty);
                    break;
                case DashBoaedTaskType.New: // Need assign without Employees 
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.StatusID == (int)TaskStatus.New).Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID,
                            // Get Finish Date .
                            FinishDate = t.DeliverDate
                        }).OrderByDescending(i => i.TaskID).Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    count = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.StatusID == (int)TaskStatus.New).Select(t => new DaskBoardCompanyTaskVM()
                        {
                            TaskID = t.TaskID,
                            Task = t.Title,
                            Status = t.Status.Name,
                            StatusID = t.StatusID,
                            PrioirtyID = t.PriorityID,
                            Prioirty = t.Priority.Name,
                            StartDate = t.StartDate.Value,
                            EndDate = t.EndDate.Value,
                            EmpID = t.EmpID,
                            // Get Finish Date .
                            FinishDate = t.DeliverDate
                        }).Count();
                    objTasks.ForEach(
                                           i => i.HijriStartDate = (i.StartDate.HasValue) ? MvcApplication.IsGregDate ? i.StartDate.Value.ToGregArabicDate() : i.StartDate.Value.ToHijriArabicDate() : String.Empty);

                    objTasks.ForEach(
                                           i => i.HijriEndDate = (i.EndDate.HasValue) ? MvcApplication.IsGregDate ? i.EndDate.Value.ToGregArabicDate() : i.EndDate.Value.ToHijriArabicDate() : String.Empty);


                    break;
                default:
                    // Fallback for unknown task types
                    count = 0;
                    objTasks = new List<DaskBoardCompanyTaskVM>();
                    break;

            }
            // Batch load delay data and employee names to avoid N+1 queries
            FillTaskMetadata(objTasks, taskType, _unitOfWork);
            return objTasks;
        }

        public static  int  GetCompanyTasksCount( DashBoaedTaskType taskType,DateTime dtDashBoardDate)
        {

            UnitOfWork _unitOfWork =
               new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //DateTime? PlusOneDay = DateTime.Now.AddDays(1);
            //   DateTime? MinusOneDay = DateTime.Now.AddDays(-1);
            DateTime? DateNow = DateTime.Now.Date;
           int iTasksCount  =0;
            switch (taskType)
            {
                case DashBoaedTaskType.NeedAssign: // Need assign without Employees 
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && !t.EmpID.HasValue && !t.IsDeleted).Count();

                    break;
                case DashBoaedTaskType.Delayed: // Delayed Tasks - filter on DB side
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                      && t.isDelayed).Count();
                    break;
                case DashBoaedTaskType.FinishToday: //EndDate =today
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.EndDate == DateNow && t.StatusID == (int)TaskStatus.Inprogress).Count();                       
                    break;
                case DashBoaedTaskType.Susspended: // SuspendedStatud
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                        && t.StatusID == (int)TaskStatus.Pending).Count();                        
                    break;
                case DashBoaedTaskType.Empfinish:
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                     && t.StatusID == (int)TaskStatus.Done).Count();                   
                   break;
                case DashBoaedTaskType.EmpReject:
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                       && t.StatusID == (int)TaskStatus.Rejected).Count();
                      
                    break;
                case DashBoaedTaskType.New:
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId
                       && t.StatusID ==(int)TaskStatus.New).Count();

                    break;


            }

            return iTasksCount;
        }


        public static bool isTaskDelayed(DaskBoardCompanyTaskVM task)
        {
            UnitOfWork _unitOfWork =
                 new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
    
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).isDelayed;

        }


        public static string TaskDelayTime(DaskBoardCompanyTaskVM task)
        {
            UnitOfWork _unitOfWork =
                 new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            return _unitOfWork.TaskRepository.GetByID(task.TaskID).delayTime.Replace('-',' ');

        }

        public static double DelayPercentage(DaskBoardCompanyTaskVM task)
        {
            UnitOfWork _unitOfWork =
                 new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            return _unitOfWork.TaskRepository.GetByID(task.TaskID).DelayPercentage;

        }
     
        #endregion

    }
}
