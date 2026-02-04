using EtaskMinstry.Areas.Company.Models;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Admin.Models
{
    public class CompanyVM
    {

        public int Id { get; set; }

        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "الاسم")]
        [Remote("CheckDublicateName", "Company", ErrorMessage = "هذا {0} مستخدم من قبل")]
        public string Name { get; set; }

        [Display(Name = "العنوان")]
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Address { get; set; }

        [RegularExpression(ValidationResource.revMail, ErrorMessageResourceName = "ValidRegularEmail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "البريد الالكتروني")]
        [Remote("CheckDublicateEmail", "Company", ErrorMessage = "هذا {0} مستخدم من قبل")]
        public string Email { get; set; }


        [Display(Name = "رقم الهاتف")]
        [RegularExpression(ValidationResource.revPhone, ErrorMessageResourceName = "ValidPhoneNo",
             ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Phone { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "السجل التجاري")]
        [Remote("CheckDublicateCommercialRegister", "Company", ErrorMessage = "هذا {0} مستخدم من قبل")]
        public string CommercialRegister { get; set; }

        [Display(Name = "الشعار")]
        public string Logo { get; set; }

        public Setting settingObj { get; set; }

        private UnitOfWork _unitOfWork;

        public CompanyVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        //get all companies.
        public List<TaskManagementModel.Company> GetCompanies()
        {
            return _unitOfWork.Company.Get().ToList();
        }

        //get all controllers.
        public List<TaskManagementModel.ControllerName> GetControllers()
        {
            return _unitOfWork.ControllerNameRepository.Get().Where(a => a.ControllerID != 12 && a.ControllerID != 15 && a.ControllerID != 16 && a.ControllerID != 17).ToList();
        }


        public List<CompanyVM> Select(string Name, string Email, string CommercialRegister)
        {
            List<CompanyVM> companies = new List<CompanyVM>();

            companies = _unitOfWork.Company.Get(a => a.IsDeleted == false && a.Name.Contains(Name) && a.Email.Contains(Email)
                && a.CommercialRegister.Contains(CommercialRegister)).Select(a => new CompanyVM()
            {
                Id = a.CompanyID,
                Name = a.Name,
                Address = a.Address,
                Email = a.Email,
                Phone = a.Phone,
                CommercialRegister = a.CommercialRegister,
                IsActive = (bool)a.IsActive,
                Logo = a.Logo
            }).ToList();

            return companies;
        }

        public CompanyVM Details(int Id)
        {
            var company = _unitOfWork.Company.GetByID(Id);
            if (company != null)
            {
                return new CompanyVM()
                {
                    Id = company.CompanyID,
                    Name = company.Name,
                    Address = company.Address,
                    Email = company.Email,
                    Phone = company.Phone,
                    CommercialRegister = company.CommercialRegister,
                    IsActive = (bool)company.IsActive,
                    Logo = company.Logo
                };
            }
            else
            {
                return null;
            }
        }

        public void SetIsActive(int? Id)
        {
            var company = _unitOfWork.Company.GetByID(Id);
            settingObj = _unitOfWork.Settings.GetByID(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
            string Message = "";
            if (company.IsActive == false)
            {
                company.IsActive = true;
                Message = "لقد تم تغيير الحالة الخاصة بك لفعال";
            }
            else
            {
                company.IsActive = false;
                Message = "لقد تم تغيير الحالة الخاصة بك لغير فعال";
            }
            _unitOfWork.Company.Update(company);
            _unitOfWork.Save();

            QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "eTask - Your Status Changed", company.Email, Message, settingObj.FromEmail);

        }

        public Boolean Save()
        {
            settingObj = _unitOfWork.Settings.Get().FirstOrDefault();//(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
            Boolean bReuslt = false;
            if (Id == 0)
            {
                //Insert Company
                TaskManagementModel.Company company = new TaskManagementModel.Company()
                {
                    Name = Name,
                    Address = Address,
                    Phone = Phone,
                    Email = Email,
                    CommercialRegister = CommercialRegister,
                    IsActive = IsActive,
                    IsDeleted = false,
                    Logo = Logo

                };
                _unitOfWork.Company.Insert(company);
                _unitOfWork.Save();

                //Insert User
                TaskManagementModel.UserAccount user = new TaskManagementModel.UserAccount()
                {
                    UserName = company.Email,
                    Password = QvLib.Security.DataProtection.Encrypt(company.Email.Split('@')[0] + company.CompanyID),
                    CompanyID = company.CompanyID,
                    IsCompany = true,
                    UserTypeID = (int)LoggedUserType.Company
                };
                _unitOfWork.UserAccount.Insert(user);

                //Send Email for the Company with Created Password
                string DycPass = QvLib.Security.DataProtection.Decrypt(user.Password);
                string LoadTemp = (QvLib.QVMail.LoadMailTemplate("/MailTemplate/email.html"));
                string host = HttpContext.Current.Request.Url.Host;
                LoadTemp = LoadTemp.Replace("{CompanyName}", company.Name).Replace("{UserName}", company.Email).Replace("{Password}", DycPass).Replace("{Host}", host);
                QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "مرحبا بك فى برنامج ادارة المهام", company.Email, LoadTemp, settingObj.FromEmail);
                _unitOfWork.Save();
                bReuslt = company.CompanyID > 0 ? true : false;
            }
            else
            {
                //Update Company
                var company = _unitOfWork.Company.GetByID(Id);

                if (company != null)
                {
                    company.Name = Name;
                    company.Address = Address;
                    company.Phone = Phone;
                    //company.Email = Email;
                    company.CommercialRegister = CommercialRegister;
                    company.IsActive = IsActive;
                    company.Logo = Logo;
                    _unitOfWork.Company.Update(company);
                    _unitOfWork.Save();

                    //Send Email 
                    QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "اي-تاسك | رسالة ادارية", company.Email, "لقد تم تعديل في بياناتك من قبل الأدمن", settingObj.FromEmail);

                }

                bReuslt = true;
            }
            return bReuslt;
        }

        public Boolean Delete(int Id)
        {
            Boolean bRturn = false;
            var company = _unitOfWork.Company.GetByID(Id);
            if (company != null)
            {
                company.IsDeleted = true;
                _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                bRturn = true;
            }
            return bRturn;
        }

        public bool CheckDublicateName(string Name)
        {
            return _unitOfWork.Company.Get(a => a.Name == Name).FirstOrDefault() == null ? true : false;
        }

        public bool CheckDublicateEmail(string Email)
        {
            //   return _unitOfWork.Company.Get(a => a.Email == Email).FirstOrDefault() == null ? true : false;
            return new CompanyEmployeeVM().CheckForUniqueEmail(0, Email);
        }


        public bool CheckDublicateCommercialRegister(string CommercialRegister)
        {
            return _unitOfWork.Company.Get(a => a.CommercialRegister == CommercialRegister).FirstOrDefault() == null ? true : false;
        }
    }
}