using EtaskMinstry.AppCode;
using EtaskMinstry.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;
using QvLib.QVUtil;
using Microsoft.Reporting.WebForms;
using System.Globalization;
using System.Security.Cryptography;

namespace EtaskMinstry.Controllers
{
    public class AttendanceController : Controller
    {
        private AttendanceReportService attendanceReportService;
        private SharedService sharedService;
        public AttendanceController()
        {
           attendanceReportService = new AttendanceReportService();
            sharedService = new SharedService();

        }
        public ActionResult AttendanceReport(int? companyId)
        {
            ViewBag.companyId = companyId;
            ViewBag.Providers = sharedService.GetProviders();
            if (companyId==null)
            {
                // ViewBag.companies = attendanceReportService.GetCompanies();
                ViewBag.companies = Enumerable.Empty<SelectListItem>();
                ViewBag.employees=Enumerable.Empty<SelectListItem>();
            }
            else
            {
                ViewBag.employees = attendanceReportService.GetEmployeesByCompanyId((int)companyId);
            }
            int currentYear = DateTime.Now.Year;
            int yearsBack =5;
            ViewBag.years= Enumerable.Range(currentYear - yearsBack + 1, yearsBack)
                         .OrderByDescending(y => y)
                         .Select(y => new SelectListItem
                         {
                             Value = y.ToString(),
                             Text = y.ToString()
                         });
            return View(companyId);
        }

        [HttpPost]
        public ActionResult GetEmployees(int companyId) {
           var lst= attendanceReportService.GetEmployeesByCompanyId(companyId);
           return Json(lst, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult ViewReport(int? CompanyId, int? EmployeeId, string FromDate, string ToDate)
        //{

        //    var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, FromDate, ToDate);
        //    //if (EmployeeId != null)
        //    //{
        //    //    data[0].filteredEmployee = data[0].employeeName;
        //    //}
        //    ReportAgent.ReportDataSources.Clear();
        //    ReportAgent.AddReportDataSources(new ReportDataSource("DS_attendance", data));
        //    return Redirect("/Reports/Attendance");
        //}
        //public ActionResult ViewReport(int? CompanyId, int? EmployeeId
        //    , string FromYear ,string fromMonth, string toYear, string toMonth)
        //{

        //    string FromDate = new DateTime(int.Parse(FromYear), int.Parse(fromMonth), 1).ToString("dd/MM/yyyy");
        //    int days= DateTime.DaysInMonth(int.Parse(toYear), int.Parse(toMonth));
        //    string ToDate = new DateTime(int.Parse(toYear), int.Parse(toMonth) ,days).ToString("dd/MM/yyyy");
        //    var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, FromDate, ToDate);
        //    string reportMonthName = "";
        //    if (toMonth== fromMonth)
        //    {
        //        reportMonthName = new DateTime(int.Parse(FromYear), int.Parse(fromMonth), 1)
        //                        .ToString("MMMM yyyy", new CultureInfo("ar-EG"));
        //    }
        //    else
        //    {
        //        reportMonthName=" من " + new DateTime(int.Parse(FromYear), int.Parse(fromMonth), 1)
        //                        .ToString("MMMM yyyy", new CultureInfo("ar-EG")) + " الى " + new DateTime(int.Parse(toYear), int.Parse(toMonth), 1)
        //                        .ToString("MMMM yyyy", new CultureInfo("ar-EG"));
        //    }
        //        //if (EmployeeId != null)
        //        //{
        //        //    data[0].filteredEmployee = data[0].employeeName;
        //        //}
        //        ReportAgent.ReportDataSources.Clear();
        //    //ReportAgent.l.ReportParameters("ReportMonth", reportMonthName);
        //    //repv.LocalReport.SetParameters(new ReportParameter("ReportDateTime", currentDateTime));
        //    //ReportAgent.AddReportParameter("ReportMonth", "reportMonthName");

        //    ReportAgent.ReportParameters.Add(new ReportParameter("ReportMonth", reportMonthName));



        //    ReportAgent.AddReportDataSources(new ReportDataSource("DS_attendance", data));
        //    return Redirect("/Reports/Attendance");
        //}

        public ActionResult ViewReport(int? CompanyId, int? EmployeeId, string FromDate, string ToDate)
        {

            var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, FromDate, ToDate);
            //if (EmployeeId != null)
            //{
            //    data[0].filteredEmployee = data[0].employeeName;
            //}
            ReportAgent.ReportDataSources.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_attendance", data));
            return Redirect("/Reports/Attendance");
        }
        [HttpPost]
        public ActionResult GetCompanies(string provider)
        {
            var lst = sharedService.GetCompaniesByproviderId(provider);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetMonths(int Year)
        {
            int currentYear = DateTime.Now.Year;
            int maxMonth = (Year == currentYear) ? DateTime.Now.Month : 12;

            var lst= Enumerable.Range(1, maxMonth)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = CultureInfo.GetCultureInfo("ar-EG")
                                       .DateTimeFormat.GetMonthName(m)   // يناير…ديسمبر
                });
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

    }
}
