using EtaskMinstry.Areas.Admin.Models;
using EtaskMinstry.Controllers;
using EtaskMinstry.CustomAttrbutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TaskManagementModel;


namespace EtaskMinstry.Areas.Admin.Controllers
{
    public class EmployeeController : BaseAdminController
    {
        // GET: /Company/Employee/
        public ActionResult Index()
        {
            //ViewBag.Company = new SelectList(new CompanyVM().GetCompanies(), "CompanyID", "Name");
            var employees = new CompanyEmployeeVM().Search(string.Empty, string.Empty, null, string.Empty, null, string.Empty);
            return View(employees);
        }

        [HttpPost]
        // public ActionResult Search(string Name, string companyName, bool? bGender, string JobTitle, bool? IsActive, string Email)
        public ActionResult Search(string Name, string companyName,  string JobTitle, string Email,string IsActive)
        {
            List<CompanyEmployeeVM> data = new List<CompanyEmployeeVM>(); 
            if (IsActive=="1")
            {
                data = new CompanyEmployeeVM().Search(companyName, Name, null, JobTitle, true, Email);
            }
            else if (IsActive == "2")
            {
                data = new CompanyEmployeeVM().Search(companyName, Name, null, JobTitle, false, Email);
            }
            else
            {
                data = new CompanyEmployeeVM().Search(companyName, Name, null, JobTitle, null, Email);
            }

            return PartialView("~/Areas/Admin/Views/Employee/_Index.cshtml", data);
        }
        [EncryptedActionParameter]
        public ActionResult AddEdit(int? id)
        {

            ViewBag.Company = new SelectList(new CompanyVM().GetActiveCompanies(), "CompanyID", "Name");
            if (id.HasValue && id != 0)
            {
                ViewBag.IsValiRecap = true;
                var emp = new CompanyEmployeeVM().EmployeeDetails(id.Value);
                ViewBag.Company = new SelectList(new CompanyVM().GetActiveCompanies(), "CompanyID", "Name",emp.CompanyID);
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
           
            //if (Extentions.ValidateReCaptcha())
            //{
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
                ViewBag.Company = new SelectList(new CompanyVM().GetActiveCompanies(), "CompanyID", "Name");
            //return View();
            return RedirectToAction("Index");
            //}
            //else
            //{
            //    ViewBag.Company = new SelectList(new CompanyVM().GetActiveCompanies(), "CompanyID", "Name",obj.CompanyID);
            //    ViewBag.ShowMail = true;
            //    ViewBag.IsValiRecap = false;
            //   return View(obj);
            //  //  return RedirectToAction("AddEdit");
            //}
        }
        /// <summary>
        /// Check DuplicateName
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public ActionResult CheckDuplicate(string Name,int? companyID, int Id=0 )
        {
            //int? companyID = companyID = MvcApplication.userData.CompanyId;
            var result = new CompanyEmployeeVM().CheckForUniqueName(companyID, Id, Name);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check Duplicate Email .. الان لا يتكرر على مستوى الدتا بيز
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public ActionResult CheckDuplicateEmail(string Email, int CompanyID)
        {
             
            var result = new CompanyEmployeeVM().CheckForUniqueEmail(CompanyID, Email);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckEmployeeDuplicateEmail(string Email, int CompanyID, int? Id)
        {

            var result = new CompanyEmployeeVM().CheckEmployeeUniqueEmail(CompanyID, Email, Id);
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

        public ActionResult CheckForUniquSequenceNumberLaborOfficeID(int Id = 0, int SequenceNumber = 0, int LaborOfficeID = 0)
        {
            var result = new CompanyEmployeeVM().CheckForUniquSequenceNumberLaborOfficeID(Id, SequenceNumber, LaborOfficeID);
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

        //[HttpPost]
        //public Boolean Delete(int Id)
        //{
        //    return new CompanyEmployeeVM().Delete(Id);
        //}

    }
}
