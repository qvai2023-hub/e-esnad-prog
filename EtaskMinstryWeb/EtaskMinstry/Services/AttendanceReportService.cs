using EtaskMinstry.Models.Project;
using QvLib.QVUtil;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;
using System.Globalization;

namespace EtaskMinstry.Services
{
    public class AttendanceReportService
    {

        private UnitOfWork _unitOfWork;

        public AttendanceReportService()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }
        //get all companies.
        public SelectList GetCompanies()
        {
          var companies= _unitOfWork.Company.Get(a => a.IsDeleted == false).ToList();
            return new SelectList(companies, "CompanyID", "Name");
         }

        public List<SelectListItem> GetEmployeesByCompanyId(int companyId)
        {
            return _unitOfWork.Employee.Get(x=>x.CompanyID== companyId).ToList().Select(n =>
                           new SelectListItem
                           {
                               Value = n.EmpID.ToString(),
                               Text = n.Name
                           }).ToList();
        }

        //public List<sp_Attendance_Result> GetAttendence(int? CompanyId, int? EmployeeId, string FromDate, string ToDate)
        //{
        //    CultureInfo enCul = new CultureInfo("en-US");

        //    DateTime? from = FromDate == String.Empty? new Nullable<DateTime>() : DateTime.ParseExact(FromDate, "dd/MM/yyyy", enCul);
        //    DateTime? to = ToDate == String.Empty ? new Nullable<DateTime>() : DateTime.ParseExact(ToDate, "dd/MM/yyyy", enCul);
        //    List<SqlParameter> parameters = new List<SqlParameter>();
        //    SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId };
        //    SqlParameter param2 = new SqlParameter("@EmployeeId", SqlDbType.Int) { Value = EmployeeId };

        //    SqlParameter param3 = new SqlParameter("@fromDate", SqlDbType.DateTime) { Value = from };
        //    SqlParameter param4 = new SqlParameter("@todate", SqlDbType.DateTime) { Value = to };


        //    parameters.Add(param1);
        //    parameters.Add(param2);
        //    parameters.Add(param3);
        //    parameters.Add(param4);
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
        public List<sp_Attendance_Result> GetAttendence(int? CompanyId, int? EmployeeId, string FromDate, string ToDate)
        {
            CultureInfo enCul = new CultureInfo("en-US");

            DateTime? from = FromDate == String.Empty ? new Nullable<DateTime>() : DateTime.ParseExact(FromDate, "dd/MM/yyyy", enCul);
            DateTime? to = ToDate == String.Empty ? new Nullable<DateTime>() : DateTime.ParseExact(ToDate, "dd/MM/yyyy", enCul);
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId };
            SqlParameter param2 = new SqlParameter("@EmployeeId", SqlDbType.Int) { Value = EmployeeId };

            SqlParameter param3 = new SqlParameter("@fromDate", SqlDbType.DateTime) { Value = from };
            SqlParameter param4 = new SqlParameter("@todate", SqlDbType.DateTime) { Value = to };


            parameters.Add(param1);
            parameters.Add(param2);
            parameters.Add(param3);
            parameters.Add(param4);
            foreach (var item in parameters)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                    item.SqlValue = DBNull.Value;
                }
            }
            var data = _unitOfWork.Sp_AttendanceResult.CallStoredProcedure("sp_Attendance", parameters.ToArray());
            return data;
        }

    }
}