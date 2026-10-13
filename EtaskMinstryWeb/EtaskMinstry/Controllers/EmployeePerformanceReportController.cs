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
    // Admin-only report (Admin menu). Was reachable anonymously with any CompanyId.
    [EtaskMinstry.CustomAttrbutes.AdminAuthorize]
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
            ReportAgent.ReportParameters.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_EmployeePerformance", data));
            // Bug #82: show the selected period in the report title (exports included)
            ReportAgent.ReportParameters.Add(new ReportParameter("StartDate", ToDisplayDate(FromDate)));
            ReportAgent.ReportParameters.Add(new ReportParameter("EndDate", ToDisplayDate(ToDate)));
            return Redirect("/AppReports/EmployeePerformance");
        }

        // Picker posts dd/MM/yyyy; show yyyy/MM/dd like the other reports. Hijri input is shown as entered.
        private static string ToDisplayDate(string value)
        {
            DateTime d;
            if (MvcApplication.IsGregDate && DateTime.TryParseExact(value, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out d))
                return d.ToString("yyyy/MM/dd", new System.Globalization.CultureInfo("en-US"));
            return value ?? "";
        }

        [HttpPost]
        public ActionResult GetCompanies(string provider)
        {
            var lst = sharedService.GetCompaniesByproviderId(provider);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }


    }
}
