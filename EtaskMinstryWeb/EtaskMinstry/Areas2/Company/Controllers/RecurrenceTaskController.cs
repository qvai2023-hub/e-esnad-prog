using EtaskMinstry.Areas.Company.Models;
using EtaskMinstry.CustomAttrbutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class RecurrenceTaskController : Controller
    {
        //
        // GET: /Company/RecurrenceTask/


        public ActionResult Index()
        {
            return View();
        }

        RecurrenceTaskVM obj = new RecurrenceTaskVM();
        [EncryptedActionParameter]
        public ActionResult AddEdit(int? Id)
        {
           
           var projects = obj.GetAllProjects().ToList();
           ViewData["Projects"] = new SelectList(projects, "ProjectID", "Name");
           var recurrenceTypes = obj.GetAllRecurrenceType().ToList();
           ViewData["RecurrenceType"] = new SelectList(recurrenceTypes, "RecurrenceTypeID", "Title");
           obj.Employees= obj.GetAllEmployees();
          
           return View(obj);
        }

        [HttpPost]
        public ActionResult AddEdit(RecurrenceTaskVM obj)
        {
            if (!obj.Id.HasValue)
                obj.Id = 0;
            obj.Save();
            return RedirectToAction("AddEdit");
        }
        


    }
}
