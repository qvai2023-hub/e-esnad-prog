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
        [Remote("CheckDublicateName", "Company", AdditionalFields = "Id", ErrorMessage = "هذا {0} مستخدم من قبل")]
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
        [Remote("CheckDublicateEmail", "Company", AdditionalFields = "Id", ErrorMessage = "هذا {0} مستخدم من قبل")]
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
        [Remote("CheckDublicateCommercialRegister", "Company", AdditionalFields = "Id", ErrorMessage = "هذا {0} مستخدم من قبل")]
        public string CommercialRegister { get; set; }

        [Display(Name = "الشعار")]
        public string Logo { get; set; }

        public Setting settingObj { get; set; }


        [Display(Name = "رقم المكتب")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Range(1, 38, ErrorMessage = "يجب ان يكون رقم صحيح من 1 الى 38")]
        [RegularExpression(@"^[0-9]+[0-9]*$", ErrorMessage = "يجب ان يكون رقم صحيح من 1 الى 38")]
        // [Remote("CheckForUniquSequenceNumberLaborOfficeID", "Employee", AdditionalFields = "Id,SequenceNumber", ErrorMessage = "الدمج بين رقم المكتب و رقم التسلسل موجود من قبل")]
        public int LaborOfficeID { get; set; }

        [Display(Name = "رقم التسلسل")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Range(1, int.MaxValue, ErrorMessage = "يجب ان يكون رقم صحيح أكبر من 0 و أصغر من 2,147,483,648")]
        [RegularExpression(@"^[0-9]+[0-9]*$", ErrorMessage = "يجب ان يكون رقم صحيح أكبر من 0 و أصغر من 2,147,483,648")]
        // [Remote("CheckForUniquSequenceNumberLaborOfficeID", "Employee", AdditionalFields = "Id,LaborOfficeID", ErrorMessage = "الدمج بين رقم المكتب و رقم التسلسل موجود من قبل")]
        public int SequenceNumber { get; set; }

        [Display(Name = "عرض تقرير الحضور والانصراف")]
        public bool HasAttendanceReport { get; set; }
        [Display(Name = "عرض التقرير المختصر للمهام")]
        public bool BriefTaskReport { get; set; }
        public string Password { get; set; }

        public int DonetasksCount { get; set; }
        public int tasksCount { get; set; }

        [Display(Name = "تلى ساك")]
        public bool IsTelesak { get; set; }

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

        public List<TaskManagementModel.Company> GetActiveCompanies()
        {
            return _unitOfWork.Company.Get(x => x.IsDeleted == false).ToList();
        }
        //get all controllers.
        public List<TaskManagementModel.ControllerName> GetControllers()
        {
            return _unitOfWork.ControllerNameRepository.Get().Where(a => a.ControllerID != 12 && a.ControllerID != 15 && a.ControllerID != 16 && a.ControllerID != 17).ToList();
        }


        public List<CompanyVM> Select(string Name, string Email, string CommercialRegister, bool? IsActive)
        {
            var currentMonth = DateTime.Now.Month;
            List<CompanyVM> companies = new List<CompanyVM>();
            companies = _unitOfWork.Company.Get(a => a.IsDeleted == false && a.Name.Contains(Name) && a.Email.Contains(Email)
                && a.CommercialRegister.Contains(CommercialRegister) && (IsActive == null || a.IsActive == IsActive)).ToList().Select(a => new CompanyVM()
                {
                    Id = a.CompanyID,
                    Name = a.Name,
                    Address = a.Address,
                    Email = a.Email,
                    Phone = a.Phone,
                    CommercialRegister = a.CommercialRegister,
                    IsActive = (bool)a.IsActive,
                    Logo = a.Logo,
                    LaborOfficeID = LaborOfficeID,
                    SequenceNumber = SequenceNumber,
                    Password = QvLib.Security.DataProtection.Decrypt(a.UserAccounts.FirstOrDefault().Password),
                    DonetasksCount =a.Tasks.Where(x => x.IsDeleted == false && x.CreatedDate.Month == currentMonth && (x.StatusID == (int)TaskStatus.Done ||x.StatusID==(int)TaskStatus.Approved)).Count(),
                    tasksCount = a.Tasks.Where(x => x.IsDeleted == false && x.CreatedDate.Month == currentMonth).Count(),
                }).OrderByDescending(x=>x.Id).ToList();

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
                    Logo = company.Logo,
                    LaborOfficeID = company.LaborOfficeID != null ? (int)company.LaborOfficeID : 0,
                    SequenceNumber = company.SequenceNumber != null ? (int)company.SequenceNumber : 0,
                    HasAttendanceReport = company.HasAttendanceReport == true ? true : false,
                    BriefTaskReport = company.BriefTaskReport == true ? true : false,
                    IsTelesak = company.UserAccounts.FirstOrDefault(x=>x.IsCompany==true).IsTelesak == true ? true : false,
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

            QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "esnad - Your Status Changed", company.Email, Message, settingObj.FromEmail);

        }

        public Boolean Save()
        {
            settingObj = _unitOfWork.Settings.Get().FirstOrDefault();//(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
            Boolean bReuslt = false;
            if (Id == 0)//add
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
                    Logo = Logo,
                    LaborOfficeID = LaborOfficeID,
                    SequenceNumber = SequenceNumber,
                    HasAttendanceReport = HasAttendanceReport,
                    BriefTaskReport = BriefTaskReport
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
                    UserTypeID = (int)LoggedUserType.Company,
                    IsTelesak= IsTelesak
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
                    company.LaborOfficeID = LaborOfficeID;
                    company.SequenceNumber = SequenceNumber;
                    company.HasAttendanceReport = HasAttendanceReport;
                    company.BriefTaskReport = BriefTaskReport;

                    ///////////////////

                    var lst=_unitOfWork.UserAccount.Get(x => x.CompanyID == company.CompanyID).ToList();
                    for (int i= 0 ;i< lst.Count();i++)
                    {
                        lst[i].IsTelesak = IsTelesak;
                        _unitOfWork.UserAccount.Update(lst[i]);
                    }
                    ////////////////////////////////////
                    _unitOfWork.Company.Update(company);
                    _unitOfWork.Save();

                    //Send Email 
                    QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "اسناد | رسالة ادارية", company.Email, "لقد تم تعديل في بياناتك من قبل الأدمن", settingObj.FromEmail);

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

        public bool CheckDublicateName(string Name, int Id)
        {
            if (Id != 0)
                return _unitOfWork.Company.Get(a => a.Name == Name && a.CompanyID != Id && a.IsDeleted == false).FirstOrDefault() == null ? true : false;
            else
                return _unitOfWork.Company.Get(a => a.Name == Name && a.IsDeleted == false).FirstOrDefault() == null ? true : false;
        }

        public bool CheckDublicateEmail(string Email, int Id)
        {
            // return _unitOfWork.Company.Get(a => a.Email == Email && a.CompanyID != Id && a.IsDeleted == false).FirstOrDefault() == null ? true : false;
            return new CompanyEmployeeVM().CheckForUniqueEmail(Id, Email);
        }


        public bool CheckDublicateCommercialRegister(string CommercialRegister, int Id)
        {
            if (Id != 0)

                return _unitOfWork.Company.Get(a => a.CommercialRegister == CommercialRegister && a.CompanyID != Id && a.IsDeleted == false).FirstOrDefault() == null ? true : false;
            else
                return _unitOfWork.Company.Get(a => a.CommercialRegister == CommercialRegister && a.IsDeleted == false).FirstOrDefault() == null ? true : false;

        }
        public int GetCompanyIdByName(string name)
        {
            var company= _unitOfWork.Company.Get(a => a.Name.Contains(name)).FirstOrDefault();
            if(company!=null)
            {
                return company.CompanyID;

            }
            return 0;
        }
        }
    }