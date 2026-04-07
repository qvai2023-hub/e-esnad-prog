using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Company;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Web.UI;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class DashBoardController : BaseCompanyController
    {
        //
        // GET: /Company/DashBoard/

        public ActionResult Index(DashBoaedTaskType? taskType)
        {

            if (MvcApplication.userData != null)
            {
                EtaskMinstry.AppCode.TaskManger.UpdateTaskStatus();
                ViewBag.Employee = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "id", "name");
                DashBoaedTaskType Type = taskType.HasValue ? taskType.Value : DashBoaedTaskType.Empfinish;
                int count = 0;
                int page = 1;
                int pageSize = 10;
                ViewData["CompanyTask"] = new DaskBoardCompanyTaskVM().SelectCompanyTasks(Type, out count, page, pageSize);
                ViewBag.pageSize = pageSize;
                ViewBag.count = count;
                ViewBag.page = page;
                ViewBag.taskType = (int)Type;
                ViewBag.pagesNumber = Math.Ceiling((decimal)ViewBag.count / (decimal)ViewBag.pageSize);
                ViewBag.Counts = DaskBoardCompanyTaskVM.GetAllCounts();
                Generallog.LogView();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Security", new { area = "" });
            }
        }


        [HttpPost]
        public ActionResult GetTasks(DashBoaedTaskType taskType)
        {
            return PartialView("~/Areas/Company/Views/DashBoard/PartialDBCompTask.cshtml", new DaskBoardCompanyTaskVM().SelectCompanyTasks(taskType));
        }

     
        public ActionResult GetTasks(DashBoaedTaskType taskType, int? page)
        {
            return RedirectToAction("Index", new { taskType = taskType, page = page });
        }
        [HttpGet]
        public ActionResult GetTasksbypage(DashBoaedTaskType taskType,int page)
        {
            int count = 0;
            int pageSize = 10;
            var tasks= new DaskBoardCompanyTaskVM().SelectCompanyTasks(taskType, out count, page, pageSize);
            ViewBag.pageSize = pageSize;
            ViewBag.count = count;
            ViewBag.page = page;
            ViewBag.taskType = (int)taskType;
            ViewBag.pagesNumber = Math.Ceiling((decimal)ViewBag.count / (decimal)ViewBag.pageSize);
            return PartialView("~/Areas/Company/Views/DashBoard/PartialDBCompTask.cshtml", tasks);
        }

    }
}
