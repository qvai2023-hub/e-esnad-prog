using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Company;
using System.Runtime.InteropServices;

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
                
                ViewData["CompanyTask"] = new DaskBoardCompanyTaskVM().SelectCompanyTasks(Type);
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

    }
}
