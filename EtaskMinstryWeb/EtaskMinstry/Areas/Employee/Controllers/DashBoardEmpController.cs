using EtaskMinstry.AppCode;
using EtaskMinstry.Areas.Employee.Models;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Employee;
using EtaskMinstry.Models.EmployeeTask;
using EtaskMinstry.Models.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Employee.Controllers
{
    public class DashBoardEmpController : BaseEmployeeController
    {
        //
        // GET: /Employee/DashBoard/
        EmployeeTaskListVM listTasks;
        StatusDisplay status;

        public DashBoardEmpController()
        {
            listTasks = new EmployeeTaskListVM();
            status = new StatusDisplay();
        }

        public ActionResult Index(DashBoaedTaskType? taskType, int? iEmployee)
        {
            ViewBag.domain = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            if (MvcApplication.userData != null)
            {
                // Throttle UpdateTaskStatus — run only once every 5 minutes per employee
                string sessionKey = "LastTaskUpdate_Emp_" + MvcApplication.userData.userId;
                DateTime? lastRun = Session[sessionKey] as DateTime?;
                if (lastRun == null || (DateTime.Now - lastRun.Value).TotalMinutes >= 5)
                {
                    EtaskMinstry.AppCode.TaskManger.UpdateTaskStatus();
                    Session[sessionKey] = DateTime.Now;
                }

                iEmployee = MvcApplication.userData.userId;
                DashBoaedTaskType Type = taskType.HasValue ? taskType.Value : DashBoaedTaskType.Doing;
                var s = new DashBoardVM().Select(iEmployee.Value, Type);
                ViewData["EmployeeTask"] = s;
                ViewBag.EmpCounts = DashBoardVM.GetAllEmployeeCounts();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Security", new { area = "" });
            }
        }


        [HttpPost]
        public ActionResult GetTasks(DashBoaedTaskType? taskType)
        {
            if (taskType == null)
                taskType = (DashBoaedTaskType?)DashBoaedTaskType.Doing ;
            return PartialView("~/Areas/Employee/Views/DashBoardEmp/PartialDBEmpTask.cshtml", new DashBoardVM().Select(MvcApplication.userData.userId, taskType));
        }

        public ActionResult GetTasks(DashBoaedTaskType? taskType, int? page)
        {
            if (taskType == null)
                taskType = (DashBoaedTaskType?)DashBoaedTaskType.Doing;
            return RedirectToAction("Index", new { taskType = taskType, page = page });
        }

        public bool Accept(int iTaskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpAcceptTask(iTaskID);

        }
        public bool Reject(int iTaskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpRejectTask(iTaskID);

        }

        public bool Finish(int iTaskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpFinishTask(iTaskID);

        }


        public string UpdateTaskTime(int iTaskID, decimal Duration)
        {
            //if (new DashBoardVM().CalculateSpendTime(iTaskID) + Convert.ToDouble(Duration) <= 24)
                return EtaskMinstry.AppCode.TaskManger.EmpUpdateTaskTime(iTaskID, Duration);

            //else
            //    return "ErrorMax";

        }
    }
}
