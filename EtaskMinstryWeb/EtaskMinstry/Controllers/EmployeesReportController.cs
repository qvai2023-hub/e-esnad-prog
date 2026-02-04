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

        public ActionResult ViewReport(int CompanyId)
        {

            var data = employeesReportService.GetData(CompanyId);

            ReportAgent.ReportDataSources.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_EmpReport", data));
            return Redirect("/Reports/Report3");
        }
        [HttpPost]
        public ActionResult GetCompanies(string provider)
        {
            var lst = sharedService.GetCompaniesByproviderId(provider);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

    }
}
