using EtaskMinstry.Areas.Employee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Employee.Controllers
{
    public class EmployeeController : Controller
    {
        //
        // GET: /Employee/Employee/
        EmployeeVM obj = new EmployeeVM();
        public ActionResult Index()
        {

            int EmpId = MvcApplication.userData.userId;
           
            var emp=obj.EmployeeDetails(EmpId);
           
            return View(emp);
        }

        [HttpPost]
        public JsonResult ChangeEmail(string Email)
        {
            int EmpId = MvcApplication.userData.userId;
            return Json(obj.ChangeEmail(Email, EmpId));
       
        }




        [HttpPost]
        public JsonResult ChangeAddress(string Address)
        {
            int EmpId = MvcApplication.userData.userId;
            return Json(obj.ChangeAddress(Address, EmpId));

        }

        [HttpPost]
        public JsonResult ChangeMobile(string Mobile)
        {
            int EmpId = MvcApplication.userData.userId;
            return Json(obj.ChangeMobile(Mobile, EmpId));

        }
    }

}
