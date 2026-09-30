using EtaskMinstry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.Models.Company;
using EtaskMinstry.Models.Project;
using TaskManagementModel;
using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry.Areas.Company.Models;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Controllers
{
    [CustomAuthorize]
  // [QVRequireHttps] 
    //[RequireHttps]
    public class BaseController : Controller
    {
        [HttpPost]
        public virtual Boolean Delete(int id, string CurrenClass)
        {
            CompanyEmployeeVM obj = new CompanyEmployeeVM();

            // Every area controller inherits this action, and nothing checked that the record
            // belongs to the caller: any logged-in user could delete any company's tasks,
            // projects or employees. A company may only delete its own records; Admin may
            // delete employees and use the generic branch.
            var user = MvcApplication.userData;
            bool isAdmin = user != null && user.UserTypeId == (int)LoggedUserType.Admin;
            bool isCompany = user != null && user.isCompany;
            var ownerCheck = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            // Special Delete For Tasks And Projects .
            // "dashboard": the Company dashboard's delete icon posts from the DashBoard
            // controller. It is a task delete, but it used to fall into the generic branch
            // below, which treated the task id as an employee id.
            if (CurrenClass.ToLower() == "company" || CurrenClass.ToLower() == "dashboard")
            {
                var task = ownerCheck.TaskRepository.GetByID(id);
                if (!isCompany || task == null || task.CompanyID != user.userId)
                    return false;
                return new CompanyTaskVM().Delete(id);
            }

            else if (CurrenClass.ToLower() == "project")
            {
                var project = ownerCheck.ProjectRepository.GetByID(id);
                if (!isCompany || project == null || project.CompanyID != user.userId)
                    return false;
                return new ProjectDisplay().Delete(id);
            }
            if (CurrenClass.ToLower() == "employee")
            {
                var employee = ownerCheck.Employee.GetByID(id);
                if (employee == null || !(isAdmin || (isCompany && employee.CompanyID == user.userId)))
                    return false;
                return new CompanyEmployeeVM().Delete(id);
            }
            else if (!isAdmin)
            {
                return false;
            }
            else
            {
                string Email = obj.GetEmail(id);
                //bool DeleteSuccess= GenericDelete.Delete(id, CurrenClass, System.Configuration.ConfigurationManager
                //                                              .ConnectionStrings["ETaskEntities"].ToString());
                bool DeleteSuccess = false;
                bool DeleteSuccess2 = false;
                var userAccountID = obj.GetEmpIDUserAccount(id);
                DeleteSuccess = GenericDelete.Delete(userAccountID, "UserAccount",
                                                            System.Configuration.ConfigurationManager
                                                                  .ConnectionStrings["ETaskEntities"].ToString());
              DeleteSuccess2=  obj.UpdateEmp(id);
                    //DeleteSuccess2 = GenericDelete.Delete(id, CurrenClass,
                    //                                       System.Configuration.ConfigurationManager
                    //                                             .ConnectionStrings["ETaskEntities"].ToString());
               
                //var empID = obj.GetEmpID(id);
                //if (empID != null)
                //{

                //    var userAccountID = obj.GetEmpIDUserAccount(empID);
                //    DeleteUserAccount = GenericDelete.Delete(userAccountID, "UserAccount",
                //                                               System.Configuration.ConfigurationManager
                //                                                     .ConnectionStrings["ETaskEntities"].ToString());
                //}

                //Send Email to Emplotyee in  case if employee   
                if (CurrenClass == "Employee" && DeleteSuccess)
                {
                    
                    var settingObj = obj.GetSettings();
                    
                 QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "الحذف من برنامج ادارة المهام", Email, "لقد تم حذفك من برنامج ادارة المهام", settingObj.FromEmail);
                     return DeleteSuccess;
                }else
                {
                   return DeleteSuccess;
                }
               
            }


            
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            ViewBag.domain = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            // Generallog.LogView();
            return base.BeginExecuteCore(callback, state);
        }
        
       
    }

   

}
