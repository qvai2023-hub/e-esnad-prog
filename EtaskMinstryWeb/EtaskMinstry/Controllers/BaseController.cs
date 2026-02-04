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
           
            // Special Delete For Tasks And Projects .
            if (CurrenClass.ToLower() == "company")
                return new CompanyTaskVM().Delete(id);

            else if (CurrenClass.ToLower() == "project")
                return new ProjectDisplay().Delete(id);
            if (CurrenClass.ToLower() == "employee")
                return new CompanyEmployeeVM().Delete(id);
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
