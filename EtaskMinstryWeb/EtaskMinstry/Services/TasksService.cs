using EtaskMinstry.Models;
using QvLib.QVUtil;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TaskManagementModel;

namespace EtaskMinstry.Services
{
    public class TasksService
    {
        private UnitOfWork _unitOfWork;

        public TasksService()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        //public List<EmployeesReportModel> GetBriefTasksReportData(int CompanyId)
        //{
        //    CultureInfo enCul = new CultureInfo("en-US");
        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId };
        //    parameters.Add(param1);

        //    foreach (var item in parameters)
        //    {
        //        if (item.Value == null)
        //        {
        //            item.Value = DBNull.Value;
        //            item.SqlValue = DBNull.Value;
        //        }
        //    }
        //    var data = _unitOfWork.Sp_AttendanceResult.CallStoredProcedure("sp_Attendance", parameters.ToArray());
        //    return data;
        //}

        public List<BriefTasksReportModel> GetData(int CompanyId)
        {
            List<BriefTasksReportModel> data = new List<BriefTasksReportModel>();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            data = _unitOfWork.TaskRepository.Get(a => a.IsDeleted == false && a.CompanyID == CompanyId && a.EmpID!=null&&
           a.StartDate.Value.Month == currentMonth && a.StartDate.Value.Year == currentYear).Select(a => new BriefTasksReportModel()
           {
               Id=a.Employee.EmpID,
               BriefTaskName = a.BriefTaskName,
               CreatedDate = a.CreatedDate,
               Name = a.Employee.Name,
               JobTitle = a.Employee.JobTitle,
               status = a.Status.Name,
               IsActive=a.Employee.Tasks.Any(t => t.IsDeleted == false && t.StartDate.Value.Month == currentMonth && t.StartDate.Value.Year == currentYear && t.StatusID==(int)TaskStatus.Done) ? "نعم" : "لا"
           }).ToList();
            return data;
        }
    }
}