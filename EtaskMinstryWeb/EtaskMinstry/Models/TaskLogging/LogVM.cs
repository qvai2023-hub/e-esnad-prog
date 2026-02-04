using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;
using EtaskMinstry.App_Code;
using System.ComponentModel.DataAnnotations;
using EtaskMinstry.AppCode;
using EtaskMinstry;
using System.Web.Mvc;


namespace EtaskMinstry.Models.TaskLogging
{
    public class LogVM
    {
       public int? taskID { get; set; }
        public string title { get; set; }
        public int? statuseID { get; set; }
        public string statuse { get; set; }
        public DateTime logdate { get; set; }
        public bool isCompany { get; set; }
        public string userName { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public List<JsonObject> jsonObj { set; get; }

        public bool isNewTask { get; set; }
        public bool isDeletedTask { get; set; }
        public string timeUnit { get; set; }
        public bool StatusIsShown { get; set; }
        public bool PriorityIsShown { get; set; }
        public bool ShowOther { get; set; }
        [ScaffoldColumn(false)]
        private UnitOfWork _unitOfWork;

        public LogVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            jsonObj = new List<JsonObject>();
        }


        /// <summary>
        /// Get log data according to search criteria.
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>  
        /// <param name="skip"></param>
        /// <param name="taskId"></param>
        /// <param name="getByStatuse"></param>
        /// <returns></returns>
        public IEnumerable<LogVM> GetLogData(string fromDate, string toDate, int skip, int take,string taskName , out int total
            , int statuseID, int priorityId, int taskId)
        {

            if (priorityId == 0 && statuseID == 0)
            {
                StatusIsShown = true;
                PriorityIsShown = true;
                ShowOther = true;
            }
            else if (priorityId != 0 && statuseID != 0)
            {
                ShowOther = true;
                StatusIsShown = statuseID != 0;
                PriorityIsShown = (priorityId != 0);
            }
            else if (taskName != "")
            {
                ShowOther = true;
                StatusIsShown = true;
                PriorityIsShown = (priorityId != 0);
            }
            else
            {
                ShowOther = false;
                StatusIsShown = true;
                PriorityIsShown = (priorityId != 0);
            }

            List<LogVM> log = new List<LogVM>();
            var currentusetrId = MvcApplication.userData.userId;
            //get all employee task intervals dateTime form table TaskTLog
            var allTaskData = _unitOfWork.TaskStatuseLog.Get(t => t.TaskID == taskId && t.EmpID == currentusetrId).ToList();

            //get all log of this task
            var taskLogData = _unitOfWork.TaskLogRepository.Get(t => (taskName == "" || t.Task.Title.ToLower().Contains(taskName.Trim().ToLower()))
                && ((statuseID == 0 || t.CurrentStatus == statuseID) && (priorityId == 0 || t.CurrentPriority == priorityId) && (taskId == 0 || t.TaskID == taskId))

                && (MvcApplication.userData.isCompany ? t.Task.CompanyID == currentusetrId : (t.Task.EmpID == currentusetrId || t.Task.TaskTLogs.Count(c => c.EmpID == currentusetrId) > 0)));

            //to get the log of this task of current Employee only 
            // with condition taskLogDate >= empCreateDate
            //.......... EXAMPLE ............
            //  (( LogDate > = '2014-12-18 15:41:40.0' AND
            // LogDate < = '2014-12-18 15:42:53.9') OR 
            // ( LogDate > = '2014-12-18 15:42:58.')
            // OR (Value like '%IsNewTask%' AND Value like '%''columnName'':''EmpID'',''oldVal'':'''',''newVal'':''405''%'))
            var result = new List<TaskLog>();

            if (MvcApplication.userData.isCompany == false)
            {

                for (int i = 0; i < allTaskData.Count(); i++)
                {

                    //To ignor millisecond 
                    DateTime t1 = allTaskData[i].CreatedDate.AddSeconds(-1);

                    //to evaluate Object
                    var objAllTaskData = allTaskData[i];

                    // get end of interval by get next employee (not currentEmployee) with status new task = 1
                    var endDate =
                        _unitOfWork.TaskStatuseLog.Get()
                                   .FirstOrDefault(
                                       e =>
                                       e.TaskTLogID > objAllTaskData.TaskTLogID && e.TaskID == objAllTaskData.TaskID &&
                                       e.EmpID != currentusetrId && e.StatusID == 1);

                    //get all TaskTLog
                    var allResult = result.Select(r => r.TaskLogID).ToList();
                    if (endDate != null)
                    {
                        DateTime t2 = endDate.CreatedDate;



                        //get the TaskTLog of current employee without duplicate in list of result by start and end of intervals
                        //to get unique data without duplicate --> !(allResult.Contains(t.TaskLogID))
                        result.AddRange(
                            taskLogData.Where(
                                t => (t.LogDate >= t1)
                                     && (t.LogDate <= t2) && !(allResult.Contains(t.TaskLogID))));
                    }
                    else
                    {
                        // if TaskTLog the start and end of it are the same
                        result.AddRange(
                            taskLogData.Where(
                                t => (t.LogDate >= t1) && !(allResult.Contains(t.TaskLogID))));
                    }



                }

                if (result.Count > 0)
                {
                    //if this task is newTask .. appear for this employee that is assgin for him
                    foreach (var taskLog in taskLogData)
                    {
                        if (taskLog.Value.Contains("IsNewTask") &&
                            taskLog.Value.Contains("'columnName':'EmpID','oldVal':'','newVal':'" + currentusetrId + "'") &&
                            (taskLog.Value.Contains("Summary") || taskLog.Value.Contains("Description") ||
                             taskLog.Value.Contains("ExpectedTime")))
                        {

                            var sameData = taskLogData.Single(t => t.TaskLogID == taskLog.TaskLogID);
                            if (sameData == null)
                            {
                                result.Add(taskLog);
                            }
                            //break;
                        }

                    }

                    taskLogData = result.AsQueryable();


                    //leaved to test reassigned scenario.......................................................................................................
                    //(t.Task.TaskTLog.FirstOrDefault() == null || t.Task.TaskTLog.All(o=>o.EmpID== currentusetrId)) ?
                    //t.Task.EmpID == currentusetrId : (t.LogDate >= t.Task.TaskTLog.FirstOrDefault(o => o.EmpID == currentusetrId).CreatedDate) 

                    //&& (t.LogDate<=t.Task.TaskTLog.Where(o => o.EmpID == currentusetrId).OrderByDescending(o=>o.TaskTLogID).FirstOrDefault().CreatedDate));

                    //taskLogData = taskLogData.Where(t => t.Task.TaskTLog.Where(o => o.EmpID == currentusetrId));

                    if (!String.IsNullOrEmpty(fromDate))
                    {

                        DateTime sDate = MvcApplication.IsGregDate ? fromDate.ToGregExactformate() : fromDate.ToGregExact();
                        taskLogData = taskLogData.Where(l => l.LogDate >= sDate);
                    }

                    if (!String.IsNullOrEmpty(toDate))
                    {
                        DateTime eDate = MvcApplication.IsGregDate ? toDate.ToGregExactformate().AddHours(23).AddMinutes(59) : toDate.ToGregExact().AddHours(23).AddMinutes(59);
                        taskLogData = taskLogData.Where(logdate => logdate.LogDate <= eDate);
                    }



                    taskLogData.OrderByDescending(l => l.TaskLogID)
                               .Skip(skip).Take(take).ToList().ForEach(delegate(TaskManagementModel.TaskLog item)
                               {
                                   log.Add(new LogVM()
                                   {
                                       taskID = item.TaskID,
                                       title = item.Task.Title,
                                       logdate = item.LogDate,
                                       isCompany = item.IsFromCompany,
                                       userName =
                                           item.IsFromCompany == true
                                               ? "المدير المسؤول"
                                               : "الموظف ::" +
                                                 ServiceManger.GetEmplyeeName(item.Task.EmpID.Value),
                                       jsonObj = EtaskMinstry.AppCode.Generallog.ParseJson(item.Value),
                                       isNewTask =
                                           EtaskMinstry.AppCode.Generallog.ParseJson(item.Value)
                                                       .Any(o => o.columnName.ToLower() == "isnewtask"),
                                       isDeletedTask =
                                           EtaskMinstry.AppCode.Generallog.ParseJson(item.Value)
                                                       .Any(o => o.columnName.ToLower() == "isdeletedtask"),
                                       timeUnit = item.Task.TimeUnitID.HasValue ?item.Task.TimeUnit.Name: "",
                                       PriorityIsShown = PriorityIsShown,
                                       StatusIsShown = StatusIsShown,
                                       ShowOther = ShowOther

                                   });
                               });

                }
            }
            else
            {

                if (!String.IsNullOrEmpty(fromDate))
                {

                    DateTime sDate = MvcApplication.IsGregDate ? fromDate.ToGregExactformate() : fromDate.ToGregExact();
                    taskLogData = taskLogData.Where(l => l.LogDate >= sDate);
                }

                if (!String.IsNullOrEmpty(toDate))
                {
                    DateTime eDate = MvcApplication.IsGregDate ? toDate.ToGregExactformate().AddHours(23).AddMinutes(59) : toDate.ToGregExact().AddHours(23).AddMinutes(59);
                    taskLogData = taskLogData.Where(logdate => logdate.LogDate <= eDate);
                }


                taskLogData.OrderByDescending(l => l.TaskLogID)
                           .Skip(skip).Take(take).ToList()
                .ForEach(delegate(TaskManagementModel.TaskLog item)
                {
                    log.Add(new LogVM()
                    {
                        taskID = item.TaskID,
                        title = item.Task.Title,
                        logdate = item.LogDate,
                        isCompany = item.IsFromCompany,
                        userName =
                            item.IsFromCompany == true
                                ? "المدير المسؤول"
                                : "الموظف ::" +
                                  ServiceManger.GetEmplyeeName(item.Task.EmpID.Value),
                        jsonObj = EtaskMinstry.AppCode.Generallog.ParseJson(item.Value),
                        isNewTask =
                            EtaskMinstry.AppCode.Generallog.ParseJson(item.Value)
                                        .Any(o => o.columnName.ToLower() == "isnewtask"),
                        isDeletedTask =
                            EtaskMinstry.AppCode.Generallog.ParseJson(item.Value)
                                        .Any(o => o.columnName.ToLower() == "isdeletedtask"),
                        timeUnit = item.Task.TimeUnitID.HasValue ? item.Task.TimeUnit.Name : " ",
                        PriorityIsShown = PriorityIsShown,
                        StatusIsShown = StatusIsShown,
                        ShowOther = ShowOther

                    });
                });
            }
            total = taskLogData.Count();
            return log;
        }

        //get all priorities.
        public List<TaskManagementModel.Priority> GetPriorities()
        {
            return _unitOfWork.PriorityRepository.Get().ToList();
        }

        //get all controllers.
        public List<TaskManagementModel.ControllerName> GetControllers()
        {
            return _unitOfWork.ControllerNameRepository.Get().Where(a => a.ControllerID != 10 && a.ControllerID != 6 && a.ControllerID != 11 && a.ControllerID != 14 && a.ControllerID != 7 && a.ControllerID != 13 && a.ControllerID != 12 && a.ControllerID != 15 && a.ControllerID != 16 && a.ControllerID != 17).ToList();
        }

        public List<TaskManagementModel.Employee> GetEmployees()
        {
            List<TaskManagementModel.Employee> lstemp = new List<TaskManagementModel.Employee>();
            try
            {
                lstemp = _unitOfWork.Employee.Get().Where(e => e.CompanyID == (MvcApplication.userData.userId) && e.IsDeleted == false).ToList();
            }
            catch 
            {  }
            return lstemp;          
        }

        public class ActionModel
        {
            public ActionModel()
            {
                ActionsList = new List<SelectListItem>();
            }
            public int ActionId { get; set; }
            public IEnumerable<SelectListItem> ActionsList { get; set; }
        }

        public IEnumerable<SelectListItem> GetActionsValue()
        {
            ActionModel model = new ActionModel();
            IEnumerable<ArabicActionscustomType> actionTypes = Enum.GetValues(typeof(ArabicActionscustomType))
                                                       .Cast<ArabicActionscustomType>();
            model.ActionsList = from action in actionTypes
                                select new SelectListItem
                                {
                                    Text = action.ToString(),
                                    Value = ((int)action).ToString()
                                };
            return (model.ActionsList);
        }

        /// <summary>
        /// GetLogGeneralDataByCompanyID
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="Skip"></param>
        /// <param name="take"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetLogGeneralDataByCompanyID(int CompanyId, int employeeId, ActionType action, string controllerId, string fromDate, string toDate, int Skip, int take, out int total)
        {
            var LogGeneralData = _unitOfWork.TotalLogRepository.Get(t => t.CompanyID == CompanyId);
            //try
            //{
                Dictionary<string[], List<string>> jsonGeneral = new Dictionary<string[], List<string>>();


                Generallog logObj = new Generallog();
                if (!String.IsNullOrEmpty(fromDate))
                {

                    DateTime sDate =MvcApplication.IsGregDate ? fromDate.ToGregExactformate(): fromDate.ToGregExact();
                    LogGeneralData = LogGeneralData.Where(l => l.ActionTime >= sDate);
                }

                if (!String.IsNullOrEmpty(toDate))
                {
                    DateTime eDate =MvcApplication.IsGregDate ? fromDate.ToGregExactformate().AddHours(23).AddMinutes(59) : toDate.ToGregExact().AddHours(23).AddMinutes(59);
                    LogGeneralData = LogGeneralData.Where(logdate => logdate.ActionTime <= eDate);
                }

                if (!String.IsNullOrEmpty(controllerId))
                {
                    int conId = int.Parse(controllerId);
                    //if (conId == 11 || conId == 8)
                    //{
                    //    LogGeneralData = LogGeneralData.Where(c => c.ActionID == (int)action);
                    //}
                    //else
                        LogGeneralData = LogGeneralData.Where(con => con.ControllerID == conId);
                }

                if (employeeId != 0)
                {
                    LogGeneralData = LogGeneralData.Where(emp => emp.EmployeeID == employeeId);
                }

                if ((int)action != 0)
                {
                    LogGeneralData = LogGeneralData.Where(c => c.ActionID == (int)action);
                }
                total = LogGeneralData.Count();
                List<LogGeneral> log = new List<LogGeneral>();
                List<Dictionary<string, object>> json = new List<Dictionary<string, object>>();
                LogGeneralData.OrderByDescending(l => l.ActionTime)
                    .Skip(Skip).Take(take).ToList().ForEach(delegate(TaskManagementModel.TotalLog item)
                    {
                        string empName = null;
                        if (item.EmployeeID != null)
                        {
                            empName = _unitOfWork.Employee.Get(e=>e.EmpID==item.EmployeeID).FirstOrDefault().Name;
                        }
                        Dictionary<string, object> itemloggeneral = new Dictionary<string, object>();
                        var company = _unitOfWork.Company.Get(c => c.CompanyID == item.CompanyID).FirstOrDefault();
                        var controller = item.ControllerID != null ? _unitOfWork.ControllerNameRepository.GetByID(item.ControllerID) : null;
                        int taskid = 0;
                        TaskManagementModel.Task Task = new TaskManagementModel.Task();
                        if (MvcApplication.IsGregDate)
                        itemloggeneral.Add("date_hijri", item.ActionTime.ToGregArabicDate());
                        else
                        itemloggeneral.Add("date_hijri", item.ActionTime.ToHijriArabicDate()) ;
                       
                        ActionType actionTtype = (ActionType)Enum.Parse(typeof(ActionType), item.ActionID.ToString());
                        if (actionTtype == ActionType.TaskAnyModification)
                        {
                            taskid = int.Parse(item.Value);
                            Task = _unitOfWork.TaskRepository.Get(c => c.TaskID == taskid).FirstOrDefault();
                        }
                        if (actionTtype == ActionType.Login)
                        {
                            
                            string val = item.Value == "::1" ? "127.0.0.1" : item.Value;
                            object option = (item.EmployeeID == null) ? "تم تسجيل دخول الشركة" + " " + company.Name + " " + " إلى النظام من عنوان الايبى " + val :
                                "تم تسجيل دخول الموظف" + empName + " إلى النظام من عنوان الايبى " + val;
                            itemloggeneral.Add("value", option);
                        }

                        if (actionTtype == ActionType.LogOut)
                        {
                            string val = item.Value == "::1" ? "127.0.0.1" : item.Value;
                            object option = (item.EmployeeID == null) ? " تم تسجيل خروج الشركة" + " "+company.Name+" " + " من النظام من عنوان الايبى " + val :
                                " تم تسجيل خروج الموظف " + " " + empName + " " + " من النظام من عنوان الايبى " + val;
                            itemloggeneral.Add("value", option);

                        }
                        else if (actionTtype == ActionType.Delete)
                        {
                            if(controller != null)
                             itemloggeneral.Add("value", "تم حذف " + item.Value + "  من <span><a target='_blank'  href='/Company/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + "</a></span>") ;
                        }
                        else if (actionTtype == ActionType.Update)
                        {
                            Dictionary<string[], List<string>> items = logObj.GetItemsInsertedOrUpdated(item.Value, ActionType.Update);
                            itemloggeneral.Add("value", "تم القيام بعملية تعديل على " + items.Keys.FirstOrDefault()[1] + " فى <span><a target='_blank' href='/Company/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + "</a></span>");
                            itemloggeneral.Add("updates", String.Join("#", items.Values.FirstOrDefault()));
                        }
                        else if (actionTtype == ActionType.View)
                        {
                            if (controller != null)
                            {
                                //for log controller
                                if (controller.ControllerID == 9)
                                {
                                    object option = (item.EmployeeID == null) ? " تم مشاهدة   <span><a target='_blank' href='/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + " من قبل الشركة " + " " + company.Name + " " + "</a></span>" :
                                      "تم مشاهدة   <span><a target='_blank' href='/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + " من قبل الموظف " + " " + empName + " " + "</a></span>";
                                    itemloggeneral.Add("value", option);
                                }
                                else
                                {
                                    object option = (item.EmployeeID == null) ? " تم مشاهدة   <span><a target='_blank' href='/Company/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + " من قبل الشركة " + " " + company.Name + " " + "</a></span>" :
                                       "تم مشاهدة   <span><a target='_blank' href='/Company/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + " من قبل الموظف " + " " + empName + " " + "</a></span>";
                                    itemloggeneral.Add("value", option);
                                }
                            }
                        }
                        else if (actionTtype == ActionType.Insert)
                        {
                            if (controller != null)
                            {
                                Dictionary<string[], List<string>> inserted = logObj.GetItemsInsertedOrUpdated(item.Value, ActionType.Insert);
                                itemloggeneral.Add("value", " تم القيام بعملية إضافة " + inserted.Keys.FirstOrDefault()[1] + "فى <span><a target='_blank' href='/Company/" + controller.ControllerNameEnglish + "'>" + controller.ControllerNameArabic + "</a></span>");
                                itemloggeneral.Add("updates", String.Join("#", inserted.Values.FirstOrDefault()));
                            }
                       }
                        else if (actionTtype == ActionType.TaskAnyModification)
                        {
                            itemloggeneral.Add(" task ", " تم التغيير فى المهمة " + Task.Title + "<span><a target='_blank'  href='/Log/index?taskId=" + Task.TaskID + "'> لمشاهدة التفاصيل </a></span>");
                        }
                        itemloggeneral.Add("action_id", item.ActionID);
                        json.Add(itemloggeneral);

                    });
                return json;
            //}
            //catch
            //{
            //    List<Dictionary<string, object>> c = new List<Dictionary<string, object>>();
            //    total = 0;
            //    return c;
            //}
        }

        /// <summary>
        /// Function to get Dictionary consists of EmployeeName and his CompanyName when User is an Employee ,
        /// And Get CompanyName when User is a company and set EmpName=Empty
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> EmpOrCom_Name()
        {
            Dictionary<string, object> option = new Dictionary<string, object>();
            try
            {
                int COMId = 0;
                int EMPID = 0;
                if (MvcApplication.userData.isCompany)
                {
                    COMId = MvcApplication.userData.userId;
                    var com = _unitOfWork.Company.Get(c => c.CompanyID == COMId).FirstOrDefault();
                    option.Add("CompanyName", com.Name);
                    option.Add("EmployeeName", string.Empty);
                    option.Add("CompanyId", com.CompanyID);
                    option.Add("EmployeeId", 0);
                }
                else
                {
                    EMPID = MvcApplication.userData.userId;
                    var emp = _unitOfWork.Employee.Get(e => e.EmpID == EMPID).FirstOrDefault();
                    var com = _unitOfWork.Company.Get(c => c.CompanyID == emp.CompanyID).FirstOrDefault();
                    option.Add("CompanyName", com.Name);
                    option.Add("EmployeeName", emp.Name);
                    option.Add("CompanyId", com.CompanyID);
                    option.Add("EmployeeId", emp.EmpID);
                }

            }
            catch
            { }
            return option;


        }
    }
}