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

        public List<EmployeesReportModel> GetData(int CompanyId)
        {
            List<EmployeesReportModel> data = new List<EmployeesReportModel>();
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            data = _unitOfWork.Employee.Get(filter: a => a.IsDeleted == false && a.CompanyID == CompanyId, includeProperties: "Attendances").Select(a => new EmployeesReportModel()
            {
                EmpId=a.EmpID,
                employeeName = a.Name,
                JobTitle=a.JobTitle,
                CheckinCount=a.Attendances.Where(x=>x.CheckIn.Value.Month == currentMonth &&x.CheckIn.Value.Year==currentYear).Count()
            }).ToList();
            return data;
        }
    }
}