using EtaskMinstry.AppCode;
using Microsoft.Reporting.WebForms;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EtaskMinstry.Areas.Company.Report
{
    public partial class TasksReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string reportPath = Server.MapPath("~/ReportsRDLC/") + Convert.ToString(Request.QueryString["reportName"]) + ".rdlc";
                if (File.Exists(reportPath))
                {
                    var umalqura = new System.Globalization.UmAlQuraCalendar();
                    string currentDateTime="";
                    if (MvcApplication.IsGregDate != true)
                    {
                        currentDateTime = string.Format("{0}-{1}-{2} {3:00}:{4:00}",
                                                        umalqura.GetYear(DateTime.Now),
                                                        umalqura.GetMonth(DateTime.Now),
                                                        umalqura.GetDayOfMonth(DateTime.Now),
                                                        umalqura.GetHour(DateTime.Now),
                                                        umalqura.GetMinute(DateTime.Now)
                                                    );
                    }
                    else
                    {
                        currentDateTime = DateTime.Now.ToGregArabicDateTime();
                    }
                    // report viewer
                    repv.Reset();
                    repv.LocalReport.ReportPath = reportPath;
                    repv.LocalReport.EnableHyperlinks = true;
                    // Enable external images
                    repv.LocalReport.EnableExternalImages = true;

                    string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority +Request.ApplicationPath.TrimEnd('/') + "/";
                    string logoPath = baseUrl+ "/Content/Site/img/reportLogo.png";
                    repv.LocalReport.SetParameters(new ReportParameter("ReportDateTime", currentDateTime));
                    // Set LogoUrl only if the report defines it
                    var reportParams = repv.LocalReport.GetParameters();
                    if (reportParams.Any(p => p.Name == "LogoUrl"))
                    {
                        repv.LocalReport.SetParameters(new ReportParameter("LogoUrl", logoPath));
                    }

                    // parameters
                    if (ReportAgent.ReportParameters != null) foreach (var prm in ReportAgent.ReportParameters) repv.LocalReport.SetParameters(prm);
                    // datasources

                    repv.LocalReport.DataSources.Clear();

                    if (ReportAgent.ReportDataSources != null)
                    {
                        foreach (var rds in ReportAgent.ReportDataSources) repv.LocalReport.DataSources.Add(rds);
                    }
                    repv.LocalReport.SubreportProcessing += ReportAgent.SubreportProcEventHandler;
                    repv.LocalReport.Refresh();
                }
                else if (!reportPath.Contains(".gif"))
                {
                    Response.Write("Check report path.");
                }

            }
            else
            {

            }
        }
    }
}