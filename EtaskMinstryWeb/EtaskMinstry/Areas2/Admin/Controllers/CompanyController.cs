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
            var companies = new CompanyVM().Select(string.Empty, string.Empty, string.Empty);
            ViewBag.count = companies.Count();
            return View(companies);
        }
        public ActionResult IndexOld()
        {
            var companies = new CompanyVM().Select(string.Empty, string.Empty, string.Empty);
            ViewBag.count = companies.Count();
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

        [HttpPost, ActionName("Index")]
        public ActionResult Search(string Name, string Email, string CommercialRegister)
        {
            return PartialView("~/Areas/Admin/Views/Company/_Index.cshtml", new CompanyVM().Select(Name, Email, CommercialRegister));
     
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

        public JsonResult CheckDublicateName(string Name)
        {
            return new CompanyVM().CheckDublicateName(Name)
                ? Json(true, JsonRequestBehavior.AllowGet)
                : Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckDublicateEmail(string Email)
        {
            return new CompanyVM().CheckDublicateEmail(Email)
                ? Json(true, JsonRequestBehavior.AllowGet)
                : Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckDublicateCommercialRegister(string CommercialRegister)
        {
            return new CompanyVM().CheckDublicateCommercialRegister(CommercialRegister)
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
