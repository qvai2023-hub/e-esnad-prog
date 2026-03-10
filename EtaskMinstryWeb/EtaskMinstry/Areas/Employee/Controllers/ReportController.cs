using EtaskMinstry.Areas.Employee.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Project;
using TaskManagementModel;
using EtaskMinstry.AppCode;
using Microsoft.Reporting.WebForms;
using System.Data.SqlClient;
using System.Data;

namespace EtaskMinstry.Areas.Employee.Controllers
{
    public class ReportController : BaseEmployeeController
    {
        public UnitOfWork _unitOfWork;
        public ActionResult Index()
        {
            return View();
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

            SqlParameter param = new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = 0 };
            SqlParameter param1 = new SqlParameter("@ProjectIds", SqlDbType.NVarChar) { Value = model.withoutProject ? "-2" : model.projects == null ? "" : string.Join(",", model.projects) };
            SqlParameter param2 = new SqlParameter("@EmpId", SqlDbType.NVarChar) { Value = MvcApplication.userData.userId.ToString() };
            SqlParameter param3 = new SqlParameter("@notInEmpList", SqlDbType.Bit) { Value = 0 };
            SqlParameter param4 = new SqlParameter("@notInProjectList", SqlDbType.Bit) { Value = model.withoutProjectChoises };

            SqlParameter param5 = new SqlParameter("@StatusId", SqlDbType.NVarChar) { Value = model.Status == null ? "" : model.Status.Contains(-1) ? "-1" : string.Join(",", model.Status) };
            SqlParameter param6 = new SqlParameter("@PriorityId", SqlDbType.NVarChar) { Value = model.priorities == null ? "" : string.Join(",", model.priorities) };
            SqlParameter param7;
            SqlParameter param8;
            SqlParameter param10;
            SqlParameter param11;
            if (EtaskMinstry.MvcApplication.IsGregDate)
            {
                 param7 = new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.fromDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.fromDate) };
                 param8 = new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.toDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.toDate) };
                 param10 = new SqlParameter("@FromEndDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.fromendDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.fromendDate) };
                 param11 = new SqlParameter("@ToEndDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.toendDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.ConvertDate(model.toendDate) };
            }
            else
            {
                 param7 = new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.fromDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.fromDate).Date };
                 param8 = new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.toDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.toDate).Date };
                 param10 = new SqlParameter("@FromEndDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.fromendDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.fromendDate).Date };
                 param11 = new SqlParameter("@ToEndDate", SqlDbType.DateTime) { Value = string.IsNullOrEmpty(model.toendDate) ? new Nullable<DateTime>() : QvLib.QVUtil.Date.hijritodate(model.toendDate).Date };
            }

            SqlParameter param9 = new SqlParameter("@taskName", SqlDbType.NVarChar) { Value = model.taskName == null ? "" : model.taskName };

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


            foreach (var item in parameters)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                    item.SqlValue = DBNull.Value;
                }
            }
            var tasks = _unitOfWork.SP_CompanyTasksResults.CallStoredProcedure("sp_CompanyTasks", parameters.ToArray());
            ReportAgent.ReportDataSources.Clear();
            ReportAgent.ReportParameters.Clear();
            //use serialize session
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_CompanyTasks", tasks));

            return Redirect("/Reports/CompanyTasks");
        }

        //public ActionResult ShowTaskReport(ReportPreperationVM model)
        //{
        //    //EtaskMinstryEntities db = new EtaskMinstryEntities();

        //   // var tasks = db.sp_ProjectTasks(0, 0, 0).ToList();
        //    //ReportAgent.ReportDataSources.Clear();
        //    //ReportAgent.ReportDataSources.Add(new ReportDataSource("DS_ProjectTasks", tasks));
        //    return Redirect("/Reports/ProjectTasks");
        //}
        /// <summary>
        /// prepare project report
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// load report of project tasks
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <returns></returns>
        public ActionResult LoadProjectReport(int? ProjectID)
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.BigInt) { Value = MvcApplication.userData.CompanyId.ToString() };
            SqlParameter param2 = new SqlParameter("@ProjectId", SqlDbType.NVarChar) { Value = ProjectID.HasValue ? ProjectID.Value.ToString() : "0" };
            SqlParameter param3 = new SqlParameter("@EmployeeId", SqlDbType.NVarChar) { Value = MvcApplication.userData.userId.ToString() };
            parameters.Add(param1);
            parameters.Add(param2);
            parameters.Add(param3);
            var tasks = _unitOfWork.SP_ProjecetTasksResults.CallStoredProcedure("sp_ProjectTasks", parameters.ToArray());
            ReportAgent.ReportDataSources.Clear();
            ReportAgent.ReportParameters.Clear();
            //use serialize session
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_ProjectTasks", tasks));
            return Redirect("/Reports/ProjectTasks");
        }
    }
}
