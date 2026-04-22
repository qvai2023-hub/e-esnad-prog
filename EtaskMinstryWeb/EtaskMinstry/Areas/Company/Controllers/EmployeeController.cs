using EtaskMinstry.Areas.Company.Models;
using EtaskMinstry.Controllers;
using EtaskMinstry.CustomAttrbutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using TaskManagementModel;


namespace EtaskMinstry.Areas.Company.Controllers
{
    public class EmployeeController : BaseCompanyController
    {
        //
        // GET: /Company/Employee/

        public ActionResult Index()
        {
            int CompanyId = MvcApplication.userData.userId;
            var employees = new CompanyEmployeeVM().Select(CompanyId, string.Empty, null, string.Empty, null, string.Empty);
            Generallog.LogView();
            return View(employees);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(string Name, bool? bGender, string JobTitle, bool? IsActive, string Email)
        {

            int CompanyId = MvcApplication.userData.userId;
            return PartialView("~/Areas/Company/Views/Employee/PartialEmployee.cshtml", new CompanyEmployeeVM().Select(CompanyId, Name, bGender, JobTitle, IsActive, Email));
        }
        [EncryptedActionParameter]
        public ActionResult AddEdit(int? EmpID)
        {
            if (EmpID.HasValue && EmpID != 0)
            {
                ViewBag.IsValiRecap = true;
                var emp = new CompanyEmployeeVM().EmployeeDetails(EmpID.Value);
                return View(emp);
            }
            else
            {

                return View();
            }
        }

        [HttpPost]
        [EncryptedActionParameter]
        public ActionResult AddEdit(CompanyEmployeeVM obj)
        {
            bool bReturn = false;
            if (obj == null)
                obj.EmpID = 0;
            if (Extentions.ValidateReCaptcha())
            {
                bReturn = obj.Save();
                if (!bReturn)
                {
                    ViewBag.Message = null;
                }
                else
                {
                    ViewBag.Message = "Save";
                }
                ViewBag.IsValiRecap = true;
                return View();
            }
            else
            {
                ViewBag.ShowMail = true;
                ViewBag.IsValiRecap = false;
                return View(obj);
            }
        }
        /// <summary>
        /// Check DuplicateName
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public ActionResult CheckDuplicate(string Name, int EmpID = 0)
        {
            int? companyID = companyID = MvcApplication.userData.CompanyId;
            var result = new CompanyEmployeeVM().CheckForUniqueName(companyID, EmpID, Name);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check Duplicate Email
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public ActionResult CheckDuplicateEmail(string Email, int EmpID = 0)
        {
            int? companyID = MvcApplication.userData.CompanyId;

            var result = new CompanyEmployeeVM().CheckForUniqueEmail(companyID, Email, EmpID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check Duplicate Mobile
        /// </summary>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        public ActionResult CheckDuplicateMobile(string Mobile, int EmpID = 0)
        {
            int? companyID = MvcApplication.userData.CompanyId;
            var result = new CompanyEmployeeVM().CheckForUniqueMobile(companyID, EmpID, Mobile);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Check Duplicate NationalID
        /// </summary>
        /// <param name="NationalID"></param>
        /// <returns></returns>
        public ActionResult CheckDuplicateNationID(string NationalID, int EmpID = 0)
        {
            int? companyID = MvcApplication.userData.CompanyId;
            var result = new CompanyEmployeeVM().CheckForUniqueNationalID(companyID, EmpID, NationalID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckForUniquSequenceNumberLaborOfficeID(int SequenceNumber = 0, int LaborOfficeID = 0, int EmpID = 0)
        {
            var result = new CompanyEmployeeVM().CheckForUniquSequenceNumberLaborOfficeID(EmpID, SequenceNumber, LaborOfficeID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public void SetIsActive(int? EmpID)
        {
            new CompanyEmployeeVM().SetIsActive(EmpID);
        }

        [HttpPost]
        public ActionResult ValidateDateLessThanToday(string Birthdate)
        {
            // validate your date here and return True if validated
            var result = true;
            if (MvcApplication.IsGregDate)
            {
                // if (EtaskMinstry.Extentions.ToGregExactformate(Birthdate) > DateTime.Now)
                if (EtaskMinstry.Extentions.GregDates(Birthdate) > DateTime.Now)
                {
                    result = false;
                }
            }
            else
            {
                DateTime Date = (EtaskMinstry.Extentions.HijriToGregDates(Birthdate)).GetValueOrDefault();
        
             //   DateTime Date = DateTime.ParseExact(Birthdate, "ddMMyyy", null);
              //  DateTime Date = DateTime.ParseExact(Birthdate, "dd/MM/yyyy", null);
                if (Date > DateTime.Now)
                {
                    result = false;
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check  Nationl ID if its for deleted empin this company
        /// </summary>
        /// <param name="NationalID"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckDeletedEmpNationlID(string NationalID)
        {
            int? companyID = MvcApplication.userData.CompanyId;
            var result = new CompanyEmployeeVM().CheckForDeletedEmpNationlID(companyID, NationalID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult RehireDeletedEmp(int EmpId)
        {
            var result = new CompanyEmployeeVM().UpdateRehireDeletedEmployee(EmpId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// add the uploaded image on the server and get its path 
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <returns></returns>
        [HttpPost]
        public string AddImage(HttpPostedFileBase uploaImage)
        {

            string strPath = "/Upload/Employee/";
            string filename = new EtaskMinstry.AppCode.UploadFile().Uploadfile(Request, strPath);
            if (filename != "FAILED")
            {
                return strPath + filename;
            }
            else
            {
                return "UploadError";
            }

        }


        [EncryptedActionParameter]
        public ActionResult Details(int? EmpID)
        {
            if (EmpID.HasValue && EmpID != 0)
            {
                ViewBag.IsValiRecap = true;
                var emp = new CompanyEmployeeVM().EmployeeDetails(EmpID.Value);
                return View(emp);
            }
            else
            {

                return View();
            }
        }


    }
}
