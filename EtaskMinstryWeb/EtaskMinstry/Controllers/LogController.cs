using EtaskMinstry.Models.Status;
using EtaskMinstry.Models.TaskLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using EtaskMinstry.AppCode;
using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry.Models.Task;

namespace EtaskMinstry.Controllers
{
    public class LogController : BaseController
    {
        LogVM log;
        
        public LogController()
        {
            log = new LogVM();
          
        }
        [EncryptedActionParameter]
        public ActionResult Index(int taskId=0)
        {
            
            //ViewBag.taskName = taskId != 0 ? log.GetTakName(taskId) : "";
            ViewBag.task = taskId;

           
            if (taskId != 0)
            {
                ViewBag.taskName = new TaskAddEdit().gettask(taskId).Title;
                ViewBag.statusId = new TaskAddEdit().gettask(taskId).StatusID;
                ViewBag.priorityId = new TaskAddEdit().gettask(taskId).PriorityID;

                int? statusId = new TaskAddEdit().gettask(taskId).StatusID;
                int? priortyId = new TaskAddEdit().gettask(taskId).PriorityID;
                ViewBag.status = new SelectList(new StatusDisplay().Get().ToList(), "ID", "Name", statusId);
                ViewBag.priority = new SelectList(log.GetPriorities(), "PriorityID", "Name", priortyId);
            }
            else
            {
                ViewBag.status = new SelectList(new StatusDisplay().Get().ToList(), "ID", "Name");
                ViewBag.priority = new SelectList(log.GetPriorities(), "PriorityID", "Name");
            }
            return View();
        }

        public ActionResult GetLogData(string fromDate, string toDate, int skip = 0, string taskName = "",
                                       int statusId = 0, int priorityId = 0, int taskId = 0)
        {
            int total = 0;
            IEnumerable<LogVM> ret = log.GetLogData(fromDate, toDate, skip, 10, taskName, out total, statusId, priorityId, taskId);
            ViewBag.total = total;
            ViewBag.TaskUrl = EtaskMinstry.MvcApplication.userData.isCompany ? "/Company/Company/TaskDetails/" : "/Employee/Tasks/TaskDetails/";
            return PartialView("PartialLogDate", ret);
        }

        #region General Log Company
        public ActionResult LogGeneral(int CompanyId = 0)
        {
            Generallog.LogView();
            if (MvcApplication.userData != null)
            {
                ViewBag.pages = new SelectList(log.GetControllers(), "ControllerID", "ControllerNameArabic");
                ViewBag.actions = new SelectList(log.GetActionsValue(), "Value", "Text");
                ViewBag.employees = new SelectList(log.GetEmployees(), "EmpID", "Name");
                CompanyId = MvcApplication.userData.userId;
                ViewBag.companyId = CompanyId;
                ViewBag.companyname = log.EmpOrCom_Name()["CompanyName"];
            }
            return View();
        }

        public ActionResult GetLogGeneralData(string fromDate, string toDate, string controllerId = "", int employeeId = 0, ActionType actionId = 0, int skip = 0)
        {
            int total = 0;
            List<Dictionary<string, object>> ret = log.GetLogGeneralDataByCompanyID(MvcApplication.userData.userId, employeeId, actionId, controllerId, fromDate, toDate, skip, 10, out total);
            ViewBag.total = total;
            ViewBag.Remain = total - skip;
            return PartialView("PartialLogGeneralDate", ret);
        }
        #endregion
    }
}
