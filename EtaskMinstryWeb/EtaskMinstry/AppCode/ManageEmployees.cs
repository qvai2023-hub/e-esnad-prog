using EtaskMinstry.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EtaskMinstry.AppCode
{
    public class ManageEmployees
    {
        TaskManagementModel.UnitOfWork _unitOfWork;

        public ManageEmployees() 
        {
            _unitOfWork = new TaskManagementModel.UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        /// <summary>
        /// get all employee in specific company.
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public List<UserData> GetEmpsInCompany(int CompanyId) 
        {
            return _unitOfWork.Employee.Get(e => e.CompanyID== CompanyId && e.IsDeleted == false).Select(e => new UserData
            {
                userId = e.EmpID,
                userName = e.Name
            }).ToList();
        }

        /// <summary>
        /// get Company Of passed employee
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        public UserData GetCompanyOfEmployee(int empId)
        {
            return _unitOfWork.Employee.Get(e => e.EmpID == empId).Select(e => new UserData
              {
                  userId = e.EmpID,
                  CompanyId=e.CompanyID
              }).FirstOrDefault();
        }
    }
}