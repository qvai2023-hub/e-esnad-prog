using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EtaskMinstry.AppCode;
using EtaskMinstry.Models;
using TaskManagementModel;

namespace EtaskMinstry.AppCode
{
    public static class ServiceManger 
    {


        /// <summary>
        /// Get CompanyEmplyees 
        /// it suppose to Call Service from EDWAM to Get All Emplyee for Copmany
        /// for Now Just Add Static Data as a Demo
        /// </summary>
        /// <returns></returns>
        public static List<EmployeeProfile> GetCompanyEmployee(int companyID)
        {
            List<EmployeeProfile> emps = new List<EmployeeProfile>();
            //Add Etask Employee 
            UnitOfWork _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            var etaskEmps = _unitOfWork.Employee.Get(e => e.CompanyID == companyID && e.IsActive.HasValue? e.IsActive.Value==true:false ); 
            foreach (var emp in etaskEmps)
            {
                emps.Add(new EmployeeProfile() { id = emp.EmpID, name = emp.Name });
            }

            return emps; 
        }

        /// <summary>
        /// Get Employees ISDeleted=false  
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        public static List<EmployeeProfile> GetCompanyEmployeeNotDeleted(int companyID)
        {
            List<EmployeeProfile> emps = new List<EmployeeProfile>();
            //Add Etask Employee 
            UnitOfWork _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            var etaskEmps = _unitOfWork.Employee.Get(e => e.CompanyID == companyID && e.IsDeleted == false && e.IsActive.HasValue ? e.IsActive.Value == true : false);
            foreach (var emp in etaskEmps)
            {
                emps.Add(new EmployeeProfile() { id = emp.EmpID, name = emp.Name });
            }

            return emps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static String GetEmplyeeName(int ID)
        {
            string Name = string.Empty;
            UnitOfWork _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            var emp = _unitOfWork.Employee.Get(e => e.EmpID == ID && e.IsActive.HasValue ? e.IsActive.Value == true : false).FirstOrDefault();
            if (emp != null)
                Name = emp.Name;
            return Name;
        }

        public static List<Emps> GetAllEmployees(int companyId)
        {  
            UnitOfWork   _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            List<Emps> lstallEmployees = _unitOfWork.Employees.Get().Where(e=>e.company_Id == companyId).Select(a => new Emps
                {
                    employee_Id = a.employee_Id,
                    emp_Name = a.Employee_Name,
                    company_Id = companyId,
                    IsActive = true,
                    IsAssigned = false,
                    IsETask = false
                }).AsEnumerable().ToList();

            return lstallEmployees;
        }


        public class Emps
        {
            public string emp_Name { get; set; }
            public int employee_Id { get; set; }
            public int company_Id { get; set; }
            public bool IsActive { get; set; }
            public bool IsAssigned { get; set; }
            public bool IsETask { get; set; }
        }
    }
}