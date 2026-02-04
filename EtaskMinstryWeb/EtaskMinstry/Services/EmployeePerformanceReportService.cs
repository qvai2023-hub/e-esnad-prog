using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using TaskManagementModel;
using System.Web.Mvc;

namespace EtaskMinstry.Services
{
    public class EmployeePerformanceReportService
    {
        private UnitOfWork _unitOfWork;

        public EmployeePerformanceReportService()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }
        public List<sp_EmployeePerformanceReport_Result> GetData(int CompanyId,string FromDate, string ToDate)
        {
            CultureInfo enCul = new CultureInfo("en-US");

            DateTime? from = FromDate == String.Empty ? new Nullable<DateTime>() : DateTime.ParseExact(FromDate, "dd/MM/yyyy", enCul);
            DateTime? to = ToDate == String.Empty ? new Nullable<DateTime>() : DateTime.ParseExact(ToDate, "dd/MM/yyyy", enCul);
            List<SqlParameter> parameters = new List<SqlParameter>();
            SqlParameter param1 = new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId };
            SqlParameter param2 = new SqlParameter("@PeriodStart", SqlDbType.DateTime) { Value = from };
            SqlParameter param3 = new SqlParameter("@PeriodEnd", SqlDbType.DateTime) { Value = to };
            parameters.Add(param1);
            parameters.Add(param2);
            parameters.Add(param3);
            foreach (var item in parameters)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                    item.SqlValue = DBNull.Value;
                }
            }
            var data = _unitOfWork.sp_EmployeePerformanceReportResult.CallStoredProcedure("sp_EmployeePerformanceReport", parameters.ToArray()).ToList();
            return data;
        }
        public SelectList GetCompanies()
        {
            var companies = _unitOfWork.Company.Get(a => a.IsDeleted == false).ToList();
            return new SelectList(companies, "CompanyID", "Name");
        }

    }
}