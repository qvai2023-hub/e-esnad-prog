using EtaskMinstry.Areas.Admin.Models;
using EtaskMinstry.Controllers;
using EtaskMinstry.CustomAttrbutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Admin.Controllers
{
    public class CompanyController : BaseAdminController
    {
       
        public ActionResult Index()
        {
            var companies = new CompanyVM().Select(string.Empty, string.Empty, string.Empty,null);
            return View(companies);
        }

        [EncryptedActionParameter]
        public ActionResult Details(int Id)
        {
            var company = new CompanyVM().Details(Id);
            if (company != null)
                return View(company);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Search(string Name, string Email, string CommercialRegister, string IsActive)
        {
            List<CompanyVM> data = new List<CompanyVM>();
            if (IsActive == "1")
            {
                data = new CompanyVM().Select(Name, Email, CommercialRegister, true);
            }
            else if (IsActive == "2")
            {
                data = new CompanyVM().Select(Name, Email, CommercialRegister, false);
            }
            else 
            {
                data = new CompanyVM().Select(Name, Email, CommercialRegister, null);
            }
            return PartialView("~/Areas/Admin/Views/Company/_Index.cshtml", data);
        }

        [EncryptedActionParameter]
        public ActionResult AddEdit(int? Id)
        {
            if (Id.HasValue && Id != 0)
            {
                var company = new CompanyVM().Details(Id.Value);
                if (company != null)
                    return View(company);
                else
                    return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddEdit(CompanyVM company)
        {
            company.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public override Boolean Delete(int Id, string CurrenClass)
        {
            return new CompanyVM().Delete(Id);
        }


        [HttpPost]
        public void SetIsActive(int? Id)
        {
            new CompanyVM().SetIsActive(Id);
        }

        public JsonResult CheckDublicateName(string Name ,int Id=0)
        {
            return new CompanyVM().CheckDublicateName(Name,Id)
                ? Json(true, JsonRequestBehavior.AllowGet)
                : Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckDublicateEmail(string Email, int Id=0)
        {
            return new CompanyVM().CheckDublicateEmail(Email,Id)
                ? Json(true, JsonRequestBehavior.AllowGet)
                : Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckDublicateCommercialRegister(string CommercialRegister,int Id=0)
        {
            return new CompanyVM().CheckDublicateCommercialRegister(CommercialRegister ,Id)
                ? Json(true, JsonRequestBehavior.AllowGet)
                : Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadFile()
        {
            string sFileName = "4394a169-f3f3-45d3-9342-43e74c410a6b.jpg";
            return File("/Upload/Task/" + sFileName, "application/octet-stream", sFileName);
        }
    }
}
