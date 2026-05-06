using EtaskMinstry.AppCode;
using EtaskMinstry.App_Code;
using EtaskMinstry.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementModel;
using EtaskMinstry;

namespace EtaskMinstry.Models.Employee
{
    public class DashBoardVM
    {
        
        [ScaffoldColumn(false)]
        public int taskID { get; set; }

         
        [ScaffoldColumn(false)]
        public int priorityId { get; set; }

        [ScaffoldColumn(false)]
        public int statusId { get; set; }

        public bool IsSusbended { get; set; }
        [LocalizedDisplayName("taskName", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string taskName { get; set; }

        [LocalizedDisplayName("projectName", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string projectName { get; set; }

        [LocalizedDisplayName("priority", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string priority { get; set; }

        [LocalizedDisplayName("priority", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string rowColor { get; set; }

        [LocalizedDisplayName("state", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string state { get; set; }

        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public DateTime? startDate { get; set; }

        public DateTime? stopDate { get; set; }
        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string US_startDate { get; set; }

        [LocalizedDisplayName("EndDate", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public DateTime? endDate { get; set; }

        [LocalizedDisplayName("isAccepted", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public bool isAccepted { get; set; }

        public int? EmpID { get; set; }
        public Boolean IsDelayed { get; set; }
        public Boolean IsDeleted { get; set; }
        [LocalizedDisplayName("DelayTime", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string delaytime { get; set; }

        public String HijriStartDate { get; set; }

        public String HijriEndDate { get; set; }

        public String HijriStopDate { get; set; }

        public string EndTaskClass{ get; set; }
        public string EndTaskSource { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public String Status { get; set; }


        [LocalizedDisplayName("actualTime", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public decimal? actualTime { get; set; }
        public ICollection<TaskTLog> SpendTimeList { get; set; }
        public double SpendTime { get; set; }
        public string DilverTime { get; set; }
        public string TasktodayTime { get; set; }
        public List<TaskTimeDetails> TimeDetails { get; set; }
        public DashBoaedTaskType DBStatus { get; set; }
        
         private UnitOfWork _unitOfWork;

         public DashBoardVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

         public List<DashBoardVM> Select(int iEmpolyee, DashBoaedTaskType? type)
        {
            iEmpolyee = MvcApplication.userData.userId;
            //DateTime? PlusOneDay = DateTime.Now.AddDays(1);
            //DateTime? MinusOneDay = DateTime.Now.AddDays(-1);
            DateTime? DateNow = DateTime.Now.Date;
           List<DashBoardVM> objTasks = new List<DashBoardVM>();

           int currentUser = MvcApplication.userData.isCompany ? 0 : MvcApplication.userData.userId;
            switch (type)
            {
                case DashBoaedTaskType.NewAssigned: 
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && t.StatusID == (int)TaskStatus.New && !t.IsDeleted).Select(t => new DashBoardVM()
                        {
                            taskID = t.TaskID,
                            taskName = t.Title,
                            startDate = t.StartDate.HasValue ? t.StartDate.Value : new Nullable<DateTime>(),
                            endDate = t.EndDate.HasValue ? t.EndDate.Value : new Nullable<DateTime>(),
                            statusId = t.StatusID,
                            EmpID=t.EmpID
                            
                            
                        }).OrderByDescending(i => i.taskID).Take(500).ToList(); // Bug #36 — Option B safety cap

                    // Arabic End Date .                     
                    objTasks.ForEach(
                        i =>
                        i.HijriEndDate = (i.endDate.HasValue) ? MvcApplication.IsGregDate ? i.endDate.Value.ToGregArabicDate() : i.endDate.Value.ToHijriArabicDate() : String.Empty);

                    // Arabic Start Date .
                    objTasks.ForEach(
                                i =>
                                i.HijriStartDate = (i.startDate.HasValue) ? MvcApplication.IsGregDate ? i.startDate.Value.ToGregArabicDate() : i.startDate.Value.ToHijriArabicDate() : String.Empty);


                    break;

                case DashBoaedTaskType.Susspended:
                    
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && t.StatusID == (int)TaskStatus.Pending 
                    || (t.TaskTLogs.FirstOrDefault(o => o.EmpID == currentUser) != null && t.TaskTLogs.FirstOrDefault(o => o.EmpID == currentUser).EmpID != t.TaskTLogs.OrderByDescending(o => o.TaskTLogID).FirstOrDefault().EmpID) && !t.IsDeleted).Select(t => new DashBoardVM()
                    {
                        taskID = t.TaskID,
                        taskName = t.Title,
                        startDate = t.StartDate.HasValue ? t.StartDate.Value : new Nullable<DateTime>(),
                        endDate = t.EndDate.HasValue ? t.EndDate.Value : new Nullable<DateTime>(),
                        statusId = t.StatusID,
                        EmpID = t.EmpID,
                        IsSusbended=true,

                        //stopDate = t.TaskTLogs.FirstOrDefault().CreatedDate,
                        // Get Finish Date 
                        stopDate = t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault() == null ?
                        DateTime.MinValue : t.TaskTLogs.Where(l => l.StatusID == (int)TaskStatus.Pending).OrderByDescending(s => s.TaskTLogID).FirstOrDefault().CreatedDate,
                        

                    }).OrderByDescending(i => i.taskID).Take(500).ToList(); // Bug #36 — Option B safety cap


                    objTasks.ForEach(
                i =>
                i.HijriStopDate = (i.stopDate.HasValue) ? MvcApplication.IsGregDate ? i.stopDate.Value.ToGregArabicDate() : i.stopDate.Value.ToHijriArabicDate() : String.Empty);


                    break;

                case DashBoaedTaskType.FinishToday:
                    DateTime today = DateTime.Today; 
                    DateTime tomorrow = today.AddDays(1);
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && t.EndDate >= today && t.EndDate < tomorrow && t.StatusID == (int)TaskStatus.Inprogress && !t.IsDeleted).Select(t => new DashBoardVM()
                    {
                        taskID = t.TaskID,
                        taskName = t.Title,
                        startDate = t.StartDate.HasValue ? t.StartDate.Value : new Nullable<DateTime>(),
                        endDate = t.EndDate.HasValue ? t.EndDate.Value : new Nullable<DateTime>(),
                        statusId = t.StatusID,
                        SpendTimeList=t.TaskTLogs,
                        SpendTime = t.TaskTLogs.Count(),
                        EmpID = t.EmpID,
                        actualTime = t.ActualTime
                    }).OrderByDescending(i => i.taskID).Take(500).ToList(); // Bug #36 — Option B safety cap

                    //Calculate Spending Time
                    var finishIds = objTasks.Select(t => t.taskID).ToList();
                    var finishDict = _unitOfWork.TaskRepository.Get(t => finishIds.Contains(t.TaskID)).ToList().ToDictionary(t => t.TaskID);
                    for (int j = 0; j < objTasks.Count(); j++)
                    {
                        objTasks[j].TimeDetails = new List<TaskTimeDetails>();

                        var objTask = finishDict.ContainsKey(objTasks[j].taskID) ? finishDict[objTasks[j].taskID] : null;
                        if (objTask == null)
                            return null;

                        else
                        {
                            var allEmp = objTask.TaskTLogs.GroupBy(e => e.EmpID);
                            var allDates = objTask.TaskTLogs.GroupBy(d => d.CreatedDate.Date);
                            var emp = new List<TaskTimeDetails>();
                            foreach (var item in allDates)
                            {
                                var lst = item.GroupBy(e => e.EmpID);
                                foreach (var itemlst in lst)
                                {
                                    var lastDate = itemlst.LastOrDefault(t => t.TimeCount != null);
                                    if (lastDate != null)
                                    {
                                        emp.Add(new TaskTimeDetails()
                                        {
                                            LogTime = ((int)lastDate.TimeCount).ToString(),
                                            isToday = lastDate.CreatedDate.Date == DateTime.Today.Date,
                                            LogDate = lastDate.CreatedDate.Date.ToString(),
                                            empId = lastDate.EmpID.Value
                                        });
                                    }

                                }

                            }

                            var hash = new List<TaskTimeDetails>();
                            foreach (var item in emp)
                            {

                                hash.Add(new TaskTimeDetails()
                                {
                                    LogTime = item.LogTime,
                                    isToday = item.isToday,
                                    LogDate = item.LogDate,
                                    empId = item.empId
                                });
                            }
                            Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();


                            var groupbyDate = hash.GroupBy(s => s.LogDate).ToList();

                            foreach (var item in groupbyDate)
                            {
                                decimal totalTimeCountSameDay = 0;
                                var lstSimillarDates = hash.FindAll(a => a.LogDate == item.Key);

                                foreach (var date in lstSimillarDates)
                                {

                                    totalTimeCountSameDay += decimal.Parse(date.LogTime);
                                }

                                if (!uniqueTimeLog.ContainsKey(item.Key))
                                    uniqueTimeLog.Add(item.Key, new TaskTimeDetails()
                                    {

                                        LogTime = totalTimeCountSameDay == 0 ? item.ToList()[0].LogTime : totalTimeCountSameDay.ToString(),
                                        isToday = item.ToList()[0].isToday,
                                        LogDate = (MvcApplication.IsGregDate) ? item.Key.ToGregArabicDate() : item.Key.ToHijriArabicDate()
                                    });

                            }
                            objTasks[j].TimeDetails.AddRange(uniqueTimeLog.Values);
                            objTasks[j].TasktodayTime = objTasks[j].TimeDetails.FirstOrDefault(t => t.isToday) == null ? "0" : objTasks[j].TimeDetails.FirstOrDefault(t => t.isToday).LogTime.Split(' ')[0];
                        }

                    }//for objtask

                    // Arabic Start Date .
                    objTasks.ForEach(
                        i =>
                        i.HijriStartDate = (i.startDate.HasValue) ? MvcApplication.IsGregDate ? i.startDate.Value.ToGregArabicDate() : i.startDate.Value.ToHijriArabicDate() : String.Empty);

                    objTasks.ForEach(
                      i =>
                      i.HijriEndDate = (i.endDate.HasValue) ? MvcApplication.IsGregDate ? i.endDate.Value.ToGregArabicDate() : i.endDate.Value.ToHijriArabicDate() : String.Empty);

                    break;

                case DashBoaedTaskType.Delayed:
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && !t.IsDeleted && t.StatusID != (int)TaskStatus.Rejected).ToList().Select(t => new DashBoardVM()
                    {
                        taskID = t.TaskID,
                        taskName = t.Title,
                        startDate = t.StartDate.HasValue ? t.StartDate.Value :new Nullable<DateTime>() ,
                        endDate = t.EndDate.HasValue ? t.EndDate.Value : new Nullable<DateTime>(),
                        statusId = t.StatusID,
                        EmpID = t.EmpID,
                        actualTime = t.ActualTime,
                          Status = t.Status.Name

                    }).OrderByDescending(i => i.taskID).Take(500).ToList(); // Bug #36 — Option B safety cap

                    // Arabic Start Date .
                    objTasks.ForEach(
                        i =>
                        i.HijriStartDate = (i.startDate.HasValue) ? MvcApplication.IsGregDate ? i.startDate.Value.ToGregArabicDate() : i.startDate.Value.ToHijriArabicDate() : String.Empty);

                    // Arabic End Date .            

                    objTasks.ForEach(
                        i =>
                        i.HijriEndDate = (i.endDate.HasValue) ? MvcApplication.IsGregDate ? i.endDate.Value.ToGregArabicDate() : i.endDate.Value.ToHijriArabicDate() : String.Empty);

                    //delaytime

                    //objTasks.ForEach(
                    //    i =>
                    //    i.delaytime = ((i.startDate.HasValue) && (i.endDate.HasValue))?(i.endDate - i.startDate): TimeSpan.Zero);
                    // Batch load entities for isDelayed + delayTime evaluation (avoids N+1 per task)
                    var delayIds = objTasks.Select(t => t.taskID).ToList();
                    var delayEntities = _unitOfWork.TaskRepository.Get(t => delayIds.Contains(t.TaskID)).ToList();
                    var empDelayedIds = new HashSet<int>(delayEntities.Where(t => t.isDelayed).Select(t => t.TaskID));
                    var empDelayDict = delayEntities.ToDictionary(t => t.TaskID, t => t.delayTime);
                    objTasks.ForEach(i => i.delaytime = empDelayDict.ContainsKey(i.taskID) ? empDelayDict[i.taskID] != null ? empDelayDict[i.taskID].Replace('-', ' ') : null : null);
                    objTasks.ForEach(i => i.IsDelayed = empDelayedIds.Contains(i.taskID));
                    objTasks = objTasks.Where(i => i.IsDelayed).ToList();

                    break;

                case DashBoaedTaskType.Doing:
                   // DateTime dt= DateTime.Now.Date;
                    //&& (t.EndDate <dt || t.EndDate> dt)
                    objTasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && t.StatusID == (int)TaskStatus.Inprogress && !t.IsDeleted).Select(t => new DashBoardVM()
                    {
                        taskID = t.TaskID,
                        taskName = t.Title,
                        startDate = t.StartDate.Value,
                        endDate = t.EndDate.Value,
                        statusId = t.StatusID,
                        SpendTimeList = t.TaskTLogs,
                        SpendTime = t.TaskTLogs.Count(),
                        EmpID = t.EmpID,
                        actualTime=t.ActualTime
                    }).OrderByDescending(i => i.taskID).Take(500).ToList(); // Bug #36 — Option B safety cap

                    objTasks.ForEach(i => i.TimeDetails = new List<TaskTimeDetails>());
                   
                    //Calculate Spending Time
                    for (int j = 0; j < objTasks.Count(); j++)
                    {
                        objTasks[j].TimeDetails = new List<TaskTimeDetails>();

                        var objTask = _unitOfWork.TaskRepository.GetByID(objTasks[j].taskID);
                        if (objTask == null)
                            return null;

                        else
                        {
                            var allEmp = objTask.TaskTLogs.GroupBy(e => e.EmpID);
                            var allDates = objTask.TaskTLogs.GroupBy(d => d.CreatedDate.Date);
                            var emp = new List<TaskTimeDetails>();
                            foreach (var item in allDates)
                            {
                                var lst = item.GroupBy(e => e.EmpID);
                                foreach (var itemlst in lst)
                                {
                                    var lastDate = itemlst.LastOrDefault(t => t.TimeCount != null);
                                    if (lastDate != null)
                                    {
                                        emp.Add(new TaskTimeDetails()
                                        {
                                            LogTime = ((int)lastDate.TimeCount).ToString(),
                                            isToday = lastDate.CreatedDate.Date == DateTime.Today.Date,
                                            LogDate = lastDate.CreatedDate.Date.ToString(),
                                            empId = lastDate.EmpID.Value
                                        });
                                    }

                                }

                            }

                            var hash = new List<TaskTimeDetails>();
                            foreach (var item in emp)
                            {

                                hash.Add(new TaskTimeDetails()
                                {
                                    LogTime = item.LogTime,
                                    isToday = item.isToday,
                                    LogDate = item.LogDate,
                                    empId = item.empId
                                });
                            }
                            Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();


                            var groupbyDate = hash.GroupBy(s => s.LogDate).ToList();

                            foreach (var item in groupbyDate)
                            {
                                decimal totalTimeCountSameDay = 0;
                                var lstSimillarDates = hash.FindAll(a => a.LogDate == item.Key);

                                foreach (var date in lstSimillarDates)
                                {

                                    totalTimeCountSameDay += decimal.Parse(date.LogTime);
                                }

                                if (!uniqueTimeLog.ContainsKey(item.Key))
                                    uniqueTimeLog.Add(item.Key, new TaskTimeDetails()
                                    {

                                        LogTime = totalTimeCountSameDay == 0 ? item.ToList()[0].LogTime : totalTimeCountSameDay.ToString(),
                                        isToday = item.ToList()[0].isToday,
                                        LogDate = (MvcApplication.IsGregDate) ? item.Key.ToGregArabicDate() : item.Key.ToHijriArabicDate()
                                    });

                            }
                            objTasks[j].TimeDetails.AddRange(uniqueTimeLog.Values);
                            objTasks[j].TasktodayTime = objTasks[j].TimeDetails.FirstOrDefault(t => t.isToday) == null ? "0" : objTasks[j].TimeDetails.FirstOrDefault(t => t.isToday).LogTime.Split(' ')[0];
                        }
                        
                    }//for objtask

                        //double x = 0;
                        //foreach (var item in objTasks[j].SpendTimeList.Where(a => a.CreatedDate.Date == DateTime.Now.Date.Date))
                        //{

                        //    x += Convert.ToDouble(item.TimeCount);
                        //}
                        //objTasks[j].SpendTime = x;
                    

                    //objTasks.TasktodayTime = objTasks.TimeDetails.FirstOrDefault(t => t.isToday) == null ? "0" : objTasks.TimeDetails.FirstOrDefault(t => t.isToday).LogTime.Split(' ')[0];
                   
                // Arabic End Date .                     
                    objTasks.ForEach(
                        i =>
                        i.HijriEndDate = (i.endDate.HasValue) ? MvcApplication.IsGregDate ? i.endDate.Value.ToGregArabicDate() : i.endDate.Value.ToHijriArabicDate() : String.Empty);

                    // Arabic Start Date .
                    objTasks.ForEach(
                        i =>
                        i.HijriStartDate = (i.startDate.HasValue) ? MvcApplication.IsGregDate ? i.startDate.Value.ToGregArabicDate() : i.startDate.Value.ToHijriArabicDate() : String.Empty);

                    break;
            }

            return objTasks;
        }

         public static bool isTaskDelayed(DashBoardVM task)
         {
             UnitOfWork _unitOfWork =
                  new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

             return _unitOfWork.TaskRepository.GetByID(task.taskID).isDelayed;

         }

        
        public static  int  GetEmployeeTasksCount(int iEmpolyee, DashBoaedTaskType taskType,DateTime dtDashBoardDate)
        {

            iEmpolyee = MvcApplication.userData.userId;
            UnitOfWork _unitOfWork =
               new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

           int iTasksCount  =0;
            switch (taskType)
            {
                case DashBoaedTaskType.NewAssigned:
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee
                        && t.StatusID == (int)TaskStatus.New && !t.IsDeleted).Count();
                    break;
                case DashBoaedTaskType.Delayed: // Delayed Tasks — batch load entities, evaluate isDelayed in memory
                    var countEntities = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && !t.IsDeleted && t.StatusID != (int)TaskStatus.Rejected).ToList();
                    iTasksCount = countEntities.Count(t => t.isDelayed);
                    break;
                case DashBoaedTaskType.FinishToday: //EndDate =today
                                                    //DateTime? PlusOneDay = DateTime.Now.AddDays(1);
                                                    //   DateTime? MinusOneDay = DateTime.Now.AddDays(-1);
                    DateTime today = DateTime.Today; 
                    DateTime tomorrow = today.AddDays(1);
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee &&  t.EndDate >= today && t.EndDate < tomorrow && t.StatusID == (int)TaskStatus.Inprogress && !t.IsDeleted).Count();   
                    
                    break;
                case DashBoaedTaskType.Susspended: // SuspendedStatud
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee
                        && t.StatusID == (int)TaskStatus.Pending || (t.TaskTLogs.FirstOrDefault(o => o.EmpID == iEmpolyee) != null && t.TaskTLogs.FirstOrDefault(o => o.EmpID == iEmpolyee).EmpID != t.TaskTLogs.OrderByDescending(o => o.TaskTLogID).FirstOrDefault().EmpID) && !t.IsDeleted).Count();                        
                   
                  
                        break;
                case DashBoaedTaskType.Doing:
                    DateTime dt = DateTime.Now.Date;
                    iTasksCount = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee
                     && t.StatusID == (int)TaskStatus.Inprogress && !t.IsDeleted).Count();                   
                   break;
                // && (t.EndDate <dt || t.EndDate>dt)


            }

            return iTasksCount;
        }

        /// <summary>
        /// Returns all 5 tab counts in one method with one UnitOfWork.
        /// Same filters as GetEmployeeTasksCount() switch cases.
        /// </summary>
        public static Dictionary<DashBoaedTaskType, int> GetAllEmployeeCounts()
        {
            int iEmpolyee = MvcApplication.userData.userId;
            UnitOfWork _unitOfWork =
               new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            var counts = new Dictionary<DashBoaedTaskType, int>();

            counts[DashBoaedTaskType.Doing] = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee
                && t.StatusID == (int)TaskStatus.Inprogress && !t.IsDeleted).Count();

            counts[DashBoaedTaskType.NewAssigned] = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee
                && t.StatusID == (int)TaskStatus.New && !t.IsDeleted).Count();

            // Delayed: batch load entities, evaluate isDelayed in memory
            var delayCountEntities = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && !t.IsDeleted && t.StatusID != (int)TaskStatus.Rejected).ToList();
            counts[DashBoaedTaskType.Delayed] = delayCountEntities.Count(t => t.isDelayed);

            counts[DashBoaedTaskType.Susspended] = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee
                && t.StatusID == (int)TaskStatus.Pending || (t.TaskTLogs.FirstOrDefault(o => o.EmpID == iEmpolyee) != null && t.TaskTLogs.FirstOrDefault(o => o.EmpID == iEmpolyee).EmpID != t.TaskTLogs.OrderByDescending(o => o.TaskTLogID).FirstOrDefault().EmpID) && !t.IsDeleted).Count();

            counts[DashBoaedTaskType.FinishToday] = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee && t.EndDate >= today && t.EndDate < tomorrow && t.StatusID == (int)TaskStatus.Inprogress && !t.IsDeleted).Count();

            return counts;
        }

        public static string TaskDelayTime(DashBoardVM task)
        {
            UnitOfWork _unitOfWork =
                 new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            return _unitOfWork.TaskRepository.GetByID(task.taskID).delayTime.Replace('-', ' '); ;

        }

        public double CalculateSpendTime(int? Id)
        {
            double x = 0;
            var task = _unitOfWork.TaskRepository.GetByID(Id);
            DashBoardVM obj = new DashBoardVM();

            if (task != null)
            {
                obj.SpendTimeList = task.TaskTLogs;
                
                foreach (var item in obj.SpendTimeList.Where(a => a.CreatedDate.Date == DateTime.Now.Date.Date))
                {
                    x += Convert.ToDouble(item.TimeCount);
                }
            }

            return x;
        }


        public static string GetTaskDalyTime(DashBoardVM task)
        {
            UnitOfWork _unitOfWork =
                 new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            return _unitOfWork.TaskRepository.GetByID(task.taskID).delayTime.Replace('-', ' '); ;

        }

    }



}