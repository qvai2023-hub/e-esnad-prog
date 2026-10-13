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
    public class EmployeesReportController : Controller
    {
        private EmployeesReportService employeesReportService;
        private SharedService sharedService;
        public EmployeesReportController()
        {
            employeesReportService = new EmployeesReportService();
            sharedService = new SharedService();

        }
        public ActionResult Index()
        {
            ViewBag.Providers = sharedService.GetProviders();
            ViewBag.companies = Enumerable.Empty<SelectListItem>();
            //ViewBag.companies = employeesReportService.GetCompanies();
            return View();
        }

        public ActionResult ViewReport(int CompanyId, string FromDate, string ToDate)
        {
            // #83: optional period filter. Picker posts dd/MM/yyyy (Gregorian) or Hijri dd/mm/yyyy.
            DateTime? from = ParsePickerDate(FromDate);
            DateTime? to = ParsePickerDate(ToDate);

            var data = employeesReportService.GetData(CompanyId, from, to);

            ReportAgent.ReportDataSources.Clear();
            ReportAgent.ReportParameters.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_EmpReport", data));
            ReportAgent.ReportParameters.Add(new ReportParameter("StartDate", from.HasValue ? (MvcApplication.IsGregDate ? from.Value.ToString("yyyy/MM/dd", new System.Globalization.CultureInfo("en-US")) : FromDate) : ""));
            ReportAgent.ReportParameters.Add(new ReportParameter("EndDate", to.HasValue ? (MvcApplication.IsGregDate ? to.Value.ToString("yyyy/MM/dd", new System.Globalization.CultureInfo("en-US")) : ToDate) : ""));
            return Redirect("/AppReports/Report3");
        }

        private static DateTime? ParsePickerDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (MvcApplication.IsGregDate)
            {
                DateTime d;
                return DateTime.TryParseExact(value, "dd/MM/yyyy", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out d) ? d : (DateTime?)null;
            }
            try { return QvLib.QVUtil.Date.hijritodate(value).Date; }
            catch { return null; }
        }
        [HttpPost]
        public ActionResult GetCompanies(string provider)
        {
            var lst = sharedService.GetCompaniesByproviderId(provider);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

    }
}
