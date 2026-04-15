using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Models.Login
{
    public class Security
    {
        private UnitOfWork _unitOfWork;
        public Security()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public bool? ForgetPawword(string Email)
        {
            try
            {
                var settings = _unitOfWork.Settings.Get().FirstOrDefault();
                if (settings == null)
                    return false;

                var emp = _unitOfWork.UserAccount.Get(i => (i.UserTypeID == (int)LoggedUserType.Employee && i.Employee.Email == Email) || (i.UserTypeID == (int)LoggedUserType.Company && i.Company.Email == Email)).FirstOrDefault();

                if (emp != null)
                {
                    var password = QvLib.Security.DataProtection.Decrypt(emp.Password);

                    if (string.IsNullOrEmpty(password))
                        return null;
                    var body = QvLib.QVMail.LoadMailTemplate("~/MailTemplate/ChangePassword.html").Replace("{Password}", password)
                        .Replace("{Host}", HttpContext.Current.Request.Url.Host);
                    var ret = QvLib.QVMail.SendMail(settings.ServerName, settings.UserName, settings.Password, settings.PortNo,
                         settings.SSL, "استعادة كلمة المرور", Email, body, settings.FromEmail);
                    return ret;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ForgetPassword Error: " + ex.Message);
                return false;
            }
        }
    }
}