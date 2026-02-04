using EtaskMinstry.Models.Project;
using EtaskMinstry.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class ReportsController : Controller
    {
        public ReportsController()
        {
            //var serverName = QvLib.QVUtil.AppSetting.GetAppSetting("reportServerName");
            //var userName = QvLib.QVUtil.AppSetting.GetAppSetting("reportUserName");
            //var password = QvLib.QVUtil.AppSetting.GetAppSetting("reportPassword");
            //NetworkCredential nwc = new NetworkCredential(userName, password, serverName);
            //WebClient client = new WebClient();
            //client.Credentials = nwc;
        }

        //public ActionResult LoadReport(string reportPath)
        //{
        //    string Pagesrc = "/Report/Report.aspx?";
        //    var repParametars = (Dictionary<string, string>)TempData["ReportParametas"];
        //    if (repParametars != null)
        //    {
        //        foreach (var item in repParametars)
        //        {
        //            Pagesrc += item.Key + "=" + item.Value + "&";
        //        }
        //    }
        //    Pagesrc += "ServerUrl=" + QvLib.QVUtil.AppSetting.GetAppSetting("ServerUrl");
        //    Pagesrc += "&RepPath=" + reportPath;
        //    Pagesrc += "&ServerName=" + QvLib.QVUtil.AppSetting.GetAppSetting("ServerName");

        //    ViewBag.Src = Pagesrc;

        //    return View();
        //}

        public ActionResult LoadReport(string reportName)
        {
            // report page parameters: reportName
            string reportPageUrl = "/Report/Report.aspx?reportName=" + reportName;
            ViewBag.Src = reportPageUrl;
            // return view
            return View("/Views/Reports/LoadReport.cshtml");
        }
  


    }

   
}
