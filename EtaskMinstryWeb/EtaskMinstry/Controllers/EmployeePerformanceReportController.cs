using EtaskMinstry.AppCode;
using EtaskMinstry.Services;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Controllers
{
    public class EmployeePerformanceReportController : Controller
    {
        private EmployeePerformanceReportService EmployeePerformanceReportService;
        private SharedService sharedService;
        public EmployeePerformanceReportController()
        {
            EmployeePerformanceReportService = new EmployeePerformanceReportService();
            sharedService = new SharedService();

        }
        public ActionResult Index()
        {
            ViewBag.Providers = sharedService.GetProviders();
                ViewBag.companies = Enumerable.Empty<SelectListItem>();
               // ViewBag.companies = EmployeePerformanceReportService.GetCompanies();
       
            return View();
        }

        [HttpPost]
        public ActionResult Index(int CompanyId, string FromDate, string ToDate)
        {

            var data = EmployeePerformanceReportService.GetData( CompanyId, FromDate, ToDate);
            if (data==null || data.Count()==0)
            {
                ViewBag.message = "لا يوجد بيانات";
                ViewBag.companies = EmployeePerformanceReportService.GetCompanies();

                return View();
            }    
            ReportAgent.ReportDataSources.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_EmployeePerformance", data));
            return Redirect("/Reports/EmployeePerformance");
        }

        [HttpPost]
        public ActionResult GetCompanies(string provider)
        {
            var lst = sharedService.GetCompaniesByproviderId(provider);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }


    }
}
