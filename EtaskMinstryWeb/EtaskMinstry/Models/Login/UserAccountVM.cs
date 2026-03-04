using EtaskMinstry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Net.PeerToPeer;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.SessionState;
using TaskManagementModel;
using System.Data.Entity;
using System.Globalization;

namespace EtaskMinstry.Models.Login
{
    public class UserAccountVM
    {
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        public string ConfirmNewPassword { get; set; }

        public string Email { get; set; }

        public Setting settingObj { get; set; }
        private UnitOfWork _unitOfWork;

        public UserAccountVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }


        public bool ChangePassowrd(string Old, string New)
        {
            int userId = MvcApplication.userData.userId;
            UserAccount objUser;
            bool breturn = false;
            if (MvcApplication.userData.isCompany)
            {
                objUser = _unitOfWork.UserAccount.Get().Where(a => a.CompanyID == userId && a.UserTypeID == (int)LoggedUserType.Company).FirstOrDefault();
                Email = objUser.Company.Email;
            }
            else
            {
                objUser = _unitOfWork.UserAccount.Get().Where(a => a.EmpID == userId && a.UserTypeID == MvcApplication.userData.UserTypeId).FirstOrDefault();
                Email = objUser.Employee.Email;
            }
            if (objUser != null)
            {
                OldPassword = objUser.Password;
                if (OldPassword == QvLib.Security.DataProtection.Encrypt(Old))
                {
                    objUser.Password = QvLib.Security.DataProtection.Encrypt(New);
                    _unitOfWork.UserAccount.Update(objUser);

                    _unitOfWork.Save();
                    //success
                    breturn = true;
                    //send mail to inform that password has been changed
                    settingObj = _unitOfWork.Settings.GetByID(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
                    string DycPass = QvLib.Security.DataProtection.Decrypt(objUser.Password);
                    string LoadTemp = (QvLib.QVMail.LoadMailTemplate("/MailTemplate/ChangePassword.html"));
                    string host = HttpContext.Current.Request.Url.Host;


                    LoadTemp = LoadTemp.Replace("{Password}", DycPass)
                                       .Replace("{Host}", host);
                    if (settingObj != null)
                    {
                        QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password,
                                              settingObj.PortNo, settingObj.SSL, "برنامج إدارة المهام", Email, LoadTemp,
                                              settingObj.FromEmail);
                    }
                }
                else
                {
                    //fail
                    breturn = false;
                }
            }

            return breturn;
        }

        public bool LoginETask(string UserName, string Password)
        {


            string password = QvLib.Security.DataProtection.Encrypt(Password);
            var user =
                _unitOfWork.UserAccount.Get()
                           .FirstOrDefault(
                               item =>
                               item.UserName == UserName && item.Password == password &&
                               item.UserTypeID != (int)LoggedUserType.Admin);
            TaskManagementModel.Employee activeEmp = new TaskManagementModel.Employee();
            TaskManagementModel.Company activeCompany = new TaskManagementModel.Company();
            bool bReturn = false;

            if (user != null)
            {
                // Check if ApplicationName is configured and user belongs to this application
                bool? appIsTelesak = QvLib.Security.ApplicationSettings.IsApplicationTelesak();
                if (appIsTelesak == null)
                {
                    // ApplicationName not configured - reject login
                    return false;
                }

                bool userIsTelesak = user.IsTelesak ?? false;
                if (appIsTelesak.Value != userIsTelesak)
                {
                    // User does not belong to this application
                    return false;
                }

                //employee
                if (user.EmpID != null)
                {
                    activeEmp = _unitOfWork.Employee.Get().FirstOrDefault(u => u.EmpID == user.EmpID && u.IsActive == true && u.IsDeleted==false);
                    if (activeEmp != null)
                    {
                        Checkin((int)user.EmpID);
                        //activeCompany = _unitOfWork.Company.Get().FirstOrDefault(u => u.CompanyID == user.CompanyID && u.IsActive == true);
                        return checkLoginETask(user, bReturn);
                    }

                }
                else //company
                {
                    activeCompany = _unitOfWork.Company.Get().FirstOrDefault(u => u.CompanyID == user.CompanyID && u.IsActive == true && u.IsDeleted == false);
                    if (activeCompany != null)
                    {
                        return checkLoginETask(user, bReturn);
                    }
                }
            }
            return bReturn;

        }

        private static bool checkLoginETask(UserAccount user, bool bReturn)
        {
            if (user == null)
            {
                return bReturn;
            }
            else
            {
                EtaskMinstry.Models.UserData userData = new EtaskMinstry.Models.UserData();

                userData.userName = user.UserName;
                userData.isCompany = (bool)user.IsCompany;
                userData.CompanyId = user.CompanyID.Value;
                userData.Email = user.UserName;
                userData.status = (int)userStatus.active;
                userData.companyStatus = (int)userStatus.active;
                userData.UserTypeId = user.UserTypeID.Value;
                if (user.IsCompany == false)
                    userData.userId = user.EmpID.Value;

                else
                {
                    userData.userId = user.CompanyID.Value;
                    userData.companyName = user.Company.Name;
                    userData.HasAttendanceReport = user.Company.HasAttendanceReport == true? true:false;
                    userData.BriefTaskReport = user.Company.BriefTaskReport == true ? true : false;
                }
                //userData.isAuthorized = true;
                MvcApplication.userData = userData;
                ///  If  MvcApplication.userData is null  :means that the session has been hijacked
                if (MvcApplication.userData == null)
                    bReturn = false;
                else
                    bReturn = true;
                return bReturn;
            }
        }

        public bool LoginAdmin(string UserName, string Password)
        {
            string password = QvLib.Security.DataProtection.Encrypt(Password);
            var user = _unitOfWork.UserAccount.Get().FirstOrDefault(item => item.UserName == UserName && item.Password == password && item.UserTypeID == (int)LoggedUserType.Admin);
            bool bReturn = false;

            if (user == null)
                return bReturn;
            else
            {
                EtaskMinstry.Models.UserData userData = new EtaskMinstry.Models.UserData();

                userData.userName = user.UserName;
                userData.isCompany = (bool)user.IsCompany;
                userData.CompanyId = user.CompanyID;
                userData.Email = user.UserName;
                userData.userId = user.ID;
                userData.status = (int)userStatus.active;
                userData.companyStatus = (int)userStatus.active;
                userData.UserTypeId = user.UserTypeID.Value;

                MvcApplication.userData = userData;
                bReturn = true;
                return bReturn;
            }


        }

        public void Checkin(int userId)
         {
            TaskManagementModel.Attendance obj = new TaskManagementModel.Attendance();
            obj.EmpId = userId;
           string date= DateTime.Now.ToString("dd/MM/yyyy HH:mm:s");
            obj.CheckIn= DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:s", CultureInfo.InvariantCulture);
            _unitOfWork.AttendanceRepository.Insert(obj);
            _unitOfWork.Save();
        }
        public void Checkout(int userId)
        {
            var currentDate = DateTime.Now.Date;
            var lst = _unitOfWork.AttendanceRepository.Get(item => item.EmpId == userId && item.CheckIn.Value.Year == currentDate.Year
                       && item.CheckIn.Value.Month == currentDate.Month
                       && item.CheckIn.Value.Day == currentDate.Day).ToList();
            var obj = lst.LastOrDefault();
            string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm:s");
            obj.CheckOut = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:s", CultureInfo.InvariantCulture);
            _unitOfWork.AttendanceRepository.Update(obj);
            _unitOfWork.Save();
        }
    }
    }