using EtaskMinstry.Areas.Company.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Company/Profile/

        CompanyProfileVM obj = new CompanyProfileVM();
        public ActionResult Index()
        {
            int compId = MvcApplication.userData.userId;

            var com = obj.CompanyDetails(compId);
            Generallog.LogView();
            return View(com);
        }


        [HttpPost]
        public JsonResult ChangeEmail(string Email)
        {
            int compId = MvcApplication.userData.userId;
            return Json(obj.ChangeEmail(Email, compId));

        }


        [HttpPost]
        public JsonResult ChangeAddress(string Address)
        {
            int compId = MvcApplication.userData.userId;
            return Json(obj.ChangeAddress(Address, compId));

        }
    }
}
