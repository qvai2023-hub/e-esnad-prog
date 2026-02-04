using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Project;
using EtaskMinstry.Models.Task;
using EtaskMinstry.CustomAttrbutes;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class ProjectController : BaseCompanyController
    {
        //
        // GET: /Project/

        public ActionResult Index()
        {
            ViewData["SearchResult"] = new ProjectDisplay().Get().AsEnumerable();
            Generallog.LogView();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetProjects(String strName = "")
        {
            return PartialView("PartialAllProject", new ProjectDisplay().Get(strName));
        }

        [EncryptedActionParameter]
        public ActionResult GetProject(int iID)
        {
            return PartialView("PartialOneProject", new ProjectAddEdit().Get(iID));
        }

        public ActionResult NewProject()
        {
            return PartialView("PartialOneProject");
        }

        [HttpPost]
        public Boolean SaveProject(ProjectAddEdit objProject)
        {
            //NotificationHub.Send(Users.Employee(405), NotificationType.AddComment,"تم إضافة مشروع جديد " + objProject.Name, "");

            return objProject.Save();
        }

        [HttpPost]
        public Boolean ArchiveTask(ProjectAddEdit objProject)
        {
            return objProject.Save();
        }

        /// <summary>
        /// To Get Tasks By Project ID .
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTasksByProject(int ProjectID)
        {
            return Json(new TaskAddEdit().GetTasksByProject(ProjectID), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Contoller to check country Code valdiation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult checkDublicated(string Name, int ID = 0)
        {
            bool isDublicated = new ProjectAddEdit().CheckDublication(ID, Name);
            return Json(!isDublicated, JsonRequestBehavior.AllowGet);
        }

    }
}
