using EtaskMinstry.Areas.Admin.Models;
using EtaskMinstry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;

namespace EtaskMinstry.Services
{
    public class EmployeesReportService
    {
        private UnitOfWork _unitOfWork;

        public EmployeesReportService()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }
        //get all companies.
        public SelectList GetCompanies()
        {
            var companies = _unitOfWork.Company.Get(a => a.IsDeleted == false).ToList();
            return new SelectList(companies, "CompanyID", "Name");
        }

        /// <summary>
        /// Employees of a company with their check-in count. #83: count check-ins inside
        /// [from, to] (both inclusive days); with no dates, keep the original current-month count.
        /// </summary>
        public List<EmployeesReportModel> GetData(int CompanyId, DateTime? from = null, DateTime? to = null)
        {
            List<EmployeesReportModel> data = new List<EmployeesReportModel>();
            DateTime rangeStart, rangeEnd;
            if (!from.HasValue && !to.HasValue)
            {
                rangeStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                rangeEnd = rangeStart.AddMonths(1);
            }
            else
            {
                // open ends use bounds that fit SQL datetime (DateTime.MinValue would overflow)
                rangeStart = from.HasValue ? from.Value.Date : new DateTime(1900, 1, 1);
                rangeEnd = to.HasValue ? to.Value.Date.AddDays(1) : new DateTime(9999, 12, 31);
            }
            data = _unitOfWork.Employee.Get(filter: a => a.IsDeleted == false && a.CompanyID == CompanyId, includeProperties: "Attendances").Select(a => new EmployeesReportModel()
            {
                EmpId=a.EmpID,
                employeeName = a.Name,
                JobTitle=a.JobTitle,
                CheckinCount=a.Attendances.Where(x => x.CheckIn >= rangeStart && x.CheckIn < rangeEnd).Count()
            }).ToList();
            return data;
        }
    }
}