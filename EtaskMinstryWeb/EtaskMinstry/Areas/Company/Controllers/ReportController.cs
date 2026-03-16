using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using EtaskMinstry.Models.Company;
using EtaskMinstry.Models.Project;
using TaskManagementModel;
using EtaskMinstry.Areas.Company.Models;
using EtaskMinstry.Controllers;
using System.Data.SqlClient;
using System.Data;
using EtaskMinstry.AppCode;
using Microsoft.Reporting.WebForms;
using EtaskMinstry.Services;
using QvLib.QVUtil;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class ReportController : BaseCompanyController
    {
        public UnitOfWork _unitOfWork;
        //
        // GET: /Company/Report/

        public ActionResult Index()
        {
            Generallog.LogView();
            return View();
        }


        public ActionResult LoadProjectReport(int? ProjectID)
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = MvcApplication.userData.CompanyId.ToString() };
            SqlParameter param2 = new SqlParameter("@ProjectId", SqlDbType.NVarChar) { Value = ProjectID.HasValue ? ProjectID.Value.ToString() : "0" };
            SqlParameter param3 = new SqlParameter("@EmployeeId", SqlDbType.NVarChar) { Value = "0" };
            parameters.Add(param1);
            parameters.Add(param2);
            parameters.Add(param3);
            var tasks = _unitOfWork.SP_ProjecetTasksResults.CallStoredProcedure("sp_ProjectTasks", parameters.ToArray());
            ReportAgent.ReportDataSources.Clear();
            //use serialize session
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_ProjectTasks", tasks));
            return Redirect("/Reports/ProjectTasks");
        }

        public ActionResult ProjectReport()
        {
            if (MvcApplication.userData.isCompany)
            {
                ViewBag.Project = new SelectList(new ProjectDisplay().Get(), "ID", "Name");
 
            }
            else //Employee
            {
                ViewBag.Project = new SelectList(new ProjectDisplay().GetEmployeeProject(MvcApplication.userData.userId), "ID", "Name");
            }
            return View("ProjectReport");

        }

        public ActionResult TaskChart(String ProjectID, Boolean? hdnWithoutProject,String EmpID, String ddlHijriDateYear)
        {
            ViewBag.Project = new SelectList(new ProjectDisplay().Get(), "ID", "Name");
            ViewBag.Employees = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "ID", "Name");
            ViewBag.Years = new YearlyTasksChart().Years;
            int iProjectID = ProjectID.IntParse();
            int empID = EmpID.IntParse();
            int yearID = ddlHijriDateYear.IntParse();
            DateTime dtStartYear = DateTime.Now;
            DateTime dtEndYear = DateTime.Now;
            if (yearID != 0)
            {
                dtStartYear = ("1/1/" + yearID).ToGregExact();

                 dtEndYear = dtStartYear.AddYears(1);
            }
            List<CompanyTaskVM> companyTasksList = new UnitOfWork(
                                         System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"]
                                             .ToString())
                                         .TaskRepository.Get(i => i.CompanyID == MvcApplication.userData.CompanyId && i.IsDeleted == false
                                                                  && (hdnWithoutProject.Value
                                                                      ? !i.ProjectID.HasValue
                                                                      : (iProjectID == 0 || i.ProjectID == iProjectID))
                                                                  && (empID == 0 || i.EmpID == empID)
                                                      &&( yearID == 0 ||  (i.CreatedDate >= dtStartYear && i.CreatedDate <= dtEndYear))
                                                    
                                         ).Select(t => new CompanyTaskVM { StatusID = t.StatusID , TaskID = t.TaskID }).ToList();

            // To Set IsDelayed .
            companyTasksList.ForEach(i => i.IsDelayed = new UnitOfWork(
                                         System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"]
                                             .ToString()).TaskRepository.GetByID(i.TaskID).isDelayed);

            return View("TaskChart", companyTasksList);
        }


        public ActionResult TasksByMonthsChart(String ddlHijriDateYear,String EmpID)
        {
            ViewBag.Employees = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "ID", "Name");
            int empID = EmpID.IntParse();
           
            var obj = new YearlyTasksChart();

            obj.GetTasks(ddlHijriDateYear.IntParse(),empID);

            return View("TasksByMonthsChart", obj);
        }


        /// <summary>
        /// open search page to prepare for required task report
        /// </summary>
        /// <returns></returns>
        public ActionResult TaskReportPreperation()
        {
            ReportPreperationVM reportPreperationVM = new ReportPreperationVM();
            return View(reportPreperationVM);
        }

        public ActionResult ShowTaskReportDetails(string empId)
        {
            List<int> empIds = new List<int>();
            empIds.Add(Convert.ToInt32(empId));
            return this.ShowTaskReport(new ReportPreperationVM { employees = empIds });
        }

        /// <summary>
        /// show report according to filtar criteria.
        /// "-2" in project  represent that you need all tasks not related to any project
        /// "-2" in employees represent that you need all tasks not assigned to any employee. 
        /// "-1" in statuse represent to delay tasks.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ShowTaskReport(ReportPreperationVM model)
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter param = new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = MvcApplication.userData.userId };
            SqlParameter param1 = new SqlParameter("@ProjectIds", SqlDbType.NVarChar) { Value = model.withoutProject ? "-2" : model.projects == null ? "" : string.Join(",", model.projects) };
            //Selected emps not passed correctely 
            SqlParameter param2 = new SqlParameter("@EmpId", SqlDbType.NVarChar) { Value = model.withoutEmps ? "-2" : model.employees == null ? "" : string.Join(",", model.employees) };
            SqlParameter param3 = new SqlParameter("@notInEmpList", SqlDbType.Bit) { Value = model.withotEmpsChoises };
            SqlParameter param4 = new SqlParameter("@notInProjectList", SqlDbType.Bit) { Value = model.withoutProjectChoises };

            SqlParameter param5 = new SqlParameter("@StatusId", SqlDbType.NVarChar) { Value = model.Status == null ? "" : model.Status.Contains(-1) ? "-1" : string.Join(",", model.Status) };
            SqlParameter param6 = new SqlParameter("@PriorityId", SqlDbType.NVarChar) { Value = model.priorities == null ? "" : string.Join(",", model.priorities) };
            SqlParameter param7;
            SqlParameter param8;
            SqlParameter param10;
            SqlParameter param11;
            if (EtaskMinstry.MvcApplication.IsGregDate)
            {
                 param7 = new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = model.fromDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.fromDate) };
                 param8 = new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = model.toDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.toDate) };
                 param10 = new SqlParameter("@FromEndDate", SqlDbType.DateTime) { Value = model.fromendDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.fromendDate) };
                 param11 = new SqlParameter("@ToEndDate", SqlDbType.DateTime) { Value = model.toendDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.toendDate) };
            }
            else
            {
                 param7 = new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = model.fromDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.fromDate).Date };
                 param8 = new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = model.toDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.toDate).Date };
                 param10 = new SqlParameter("@FromEndDate", SqlDbType.DateTime) { Value = model.fromendDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.fromendDate).Date };
                 param11 = new SqlParameter("@ToEndDate", SqlDbType.DateTime) { Value = model.toendDate == null ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.toendDate).Date };
            }

            
            SqlParameter param9 = new SqlParameter("@taskName", SqlDbType.NVarChar) { Value = model.taskName == null ? "" : model.taskName };
            SqlParameter param12 = new SqlParameter("@CalendarType", SqlDbType.Int) { Value = model.calendarType };

            parameters.Add(param);
            parameters.Add(param1);
            parameters.Add(param2);
            parameters.Add(param3);
            parameters.Add(param4);
            parameters.Add(param5);
            parameters.Add(param6);
            parameters.Add(param7);
            parameters.Add(param8);
            parameters.Add(param9);
            parameters.Add(param10);
            parameters.Add(param11);
            parameters.Add(param12);

            foreach (var item in parameters)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                    item.SqlValue = DBNull.Value;
                }
            }
            var tasks = _unitOfWork.SP_CompanyTasksResults.CallStoredProcedure("sp_CompanyTasks", parameters.ToArray());

            // Report header parameters
            string companyName = MvcApplication.userData.companyName ?? "";
            string startDateDisplay = "";
            string endDateDisplay = "";
            string reportPeriod = "";

            if (!string.IsNullOrEmpty(model.fromDate) && !string.IsNullOrEmpty(model.toDate))
            {
                startDateDisplay = model.fromDate;
                endDateDisplay = model.toDate;

                DateTime fromDt;
                DateTime toDt;
                if (EtaskMinstry.MvcApplication.IsGregDate)
                {
                    fromDt = QvLib.QVUtil.Date.ConvertDate(model.fromDate).Value;
                    toDt = QvLib.QVUtil.Date.ConvertDate(model.toDate).Value;
                }
                else
                {
                    fromDt = QvLib.QVUtil.Date.hijritodate(model.fromDate).Date;
                    toDt = QvLib.QVUtil.Date.hijritodate(model.toDate).Date;
                }

                if (model.calendarType == 1)
                {
                    CultureInfo arCulture = new CultureInfo("ar-SA");
                    arCulture.DateTimeFormat.Calendar = new GregorianCalendar();
                    startDateDisplay = fromDt.ToString("yyyy/MM/dd");
                    endDateDisplay = toDt.ToString("yyyy/MM/dd");
                    reportPeriod = fromDt.ToString("MMMM yyyy", arCulture);
                }
                else
                {
                    CultureInfo hijriCulture = new CultureInfo("ar-SA");
                    hijriCulture.DateTimeFormat.Calendar = new System.Globalization.UmAlQuraCalendar();
                    startDateDisplay = fromDt.ToString("yyyy/MM/dd", hijriCulture);
                    endDateDisplay = toDt.ToString("yyyy/MM/dd", hijriCulture);
                    reportPeriod = fromDt.ToString("MMMM yyyy", hijriCulture);
                }
            }

            ReportAgent.ReportDataSources.Clear();
            ReportAgent.ReportParameters.Clear();
            ReportAgent.ReportParameters.Add(new ReportParameter("CompanyName", companyName));
            ReportAgent.ReportParameters.Add(new ReportParameter("StartDate", startDateDisplay));
            ReportAgent.ReportParameters.Add(new ReportParameter("EndDate", endDateDisplay));
            ReportAgent.ReportParameters.Add(new ReportParameter("ReportPeriod", reportPeriod));
            ReportAgent.ReportParameters.Add(new ReportParameter("CalendarType", model.calendarType.ToString()));
            //use serialize session
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_CompanyTasks", tasks));
            return Redirect("/Reports/CompanyTasks");
        }


        public ActionResult EmployeeReport()
        {
            ViewBag.Employees = new SelectList(new ManageEmployees().GetEmpsInCompany(MvcApplication.userData.userId), "userId", "userName");
            return View();
        }

        public ActionResult LoadEmployeeReport(ReportEmployeeTasks model)
        {
            CultureInfo enCul = new CultureInfo("en-US");
            DateTime? from = string.IsNullOrEmpty(model.FromDate) ? new Nullable<DateTime>() : DateTime.ParseExact(model.FromDate, "dd/MM/yyyy", enCul);
            DateTime? to =string.IsNullOrEmpty(model.ToDate) ? new Nullable<DateTime>() : DateTime.ParseExact(model.ToDate, "dd/MM/yyyy", enCul);
            DateTime? FromEndDate =   string.IsNullOrEmpty(model.FromEndDate) ? new Nullable<DateTime>() : DateTime.ParseExact(model.FromEndDate, "dd/MM/yyyy", enCul);
            DateTime? ToEndDate =   string.IsNullOrEmpty(model.ToEndDate) ? new Nullable<DateTime>() : DateTime.ParseExact(model.ToEndDate, "dd/MM/yyyy", enCul);
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = MvcApplication.userData.CompanyId };
            SqlParameter param2 = new SqlParameter("@EmpIds", SqlDbType.NVarChar) { Value = model.employees == null ? "" : string.Join(",", model.employees) };
            SqlParameter param3 = new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = from };
            SqlParameter param4 = new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = to };
            SqlParameter param5 = new SqlParameter("@FromEndDate", SqlDbType.DateTime) { Value = FromEndDate };
            SqlParameter param6 = new SqlParameter("@ToEndDate", SqlDbType.DateTime) { Value = ToEndDate };

            parameters.Add(param1);
            parameters.Add(param2);
            parameters.Add(param3);
            parameters.Add(param4);
            parameters.Add(param5);
            parameters.Add(param6);

            foreach (var item in parameters)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                    item.SqlValue = DBNull.Value;
                }
            }
            var tasks = _unitOfWork.SP_TotalEmployeeTasks_Result.CallStoredProcedure("sp_TotalEmployeeTasks", parameters.ToArray());
            ReportAgent.ReportDataSources.Clear();
            ReportAgent.ReportParameters.Clear();
            //use serialize session
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_TotalEmployeeTasks", tasks));
          
           // object[] listParams = new object[11];
          //  var companyTasks = _unitOfWork.SP_CompanyTasksResults.CallStoredProcedure("sp_CompanyTasks", ["0",'','',"0","0",'','',null,null,'',null,null]);
          
            // saving subreport in tempdata
           // TempData["MaintenanceOperationsRequests"] = new ReportDataSource("ds_maintenancepropertyrequest", companyTasks);
         
            return Redirect("/Reports/TotalEmployeeTasks");
        }

        public ActionResult BriefTasksReport(int CompanyId)
        {
            TasksService _TasksService = new TasksService();
            var data = _TasksService.GetData(CompanyId);

            ReportAgent.ReportDataSources.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DataSet1", data));
            return Redirect("/Reports/BriefTasksReport");
        }
    }
}