using System.Web.Mvc;
using EtaskMinstry;
using EtaskMinstry.App_Code;
using EtaskMinstry.AppCode;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Mail;
using System.Web;
using TaskManagementModel;
using System.Transactions;

namespace EtaskMinstry.Areas.Admin.Models
{
    public class CompanyEmployeeVM
    {

        public int Id { get; set; }
        public int EmpID { get; set; }
        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegularFullName50",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]

        [Remote("CheckDuplicate", "Employee", AdditionalFields = "EmpID,CompanyID", ErrorMessage = "الاسم موجود بالفعل")]
        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegularFullName50",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "المسمى الوظيفي")]
        public string JobTitle { get; set; }

        [Display(Name = "النوع")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public bool Gender { get; set; }

        [RegularExpression(ValidationResource.revMail, ErrorMessageResourceName = "ValidRegularEmail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "البريد الالكتروني")]
        [Remote("CheckDuplicateEmail", "Employee", AdditionalFields = "CompanyID", ErrorMessage = "البريد الالكتروني مستخدم من قبل")]
        public string Email { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime HireDate { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [Remote("ValidateDateLessThanToday", "Employee", ErrorMessage = "تاريخ الميلاد يجب ان يكون قبل اليوم.", HttpMethod = "POST")]
        public string Birthdate { get; set; }

        public string BirthdateNum { get; set; }

        public DateTime LastModifiedDate { get; set; }

         [Display(Name = "الشركة")]
         [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]

        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        [Display(Name = "رقم الجوال")]
        [RegularExpression(ValidationResource.revMobile, ErrorMessageResourceName = "ValidateMobile",
             ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Remote("CheckDuplicateMobile", "Employee", AdditionalFields = "EmpID", ErrorMessage = "رقم الجوال مستخدم من قبل")]
        public string Mobile { get; set; }

        [Display(Name = "العنوان")]
        [RegularExpression(ValidationResource.RevString200, ErrorMessageResourceName = "ValidRegularSummary200",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Address { get; set; }

        [Display(Name = "رقم الهوية الوطنية/الاقامة")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(@"^\d{10,10}$", ErrorMessage = "برجاء ادخال 10 ارقام فقط")]
        [Remote("CheckDuplicateNationID", "Employee", AdditionalFields = "EmpID", ErrorMessage = "الرقم القومى موجود من قبل")]
        public string NationalID { get; set; }

        [Display(Name = "رقم المكتب")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Range(1, 38, ErrorMessage = "يجب ان يكون رقم صحيح من 1 الى 38")]
        [RegularExpression(@"^[0-9]+[0-9]*$", ErrorMessage = "يجب ان يكون رقم صحيح من 1 الى 38")]
        [Remote("CheckForUniquSequenceNumberLaborOfficeID", "Employee", AdditionalFields = "EmpID,SequenceNumber", ErrorMessage = "الدمج بين رقم المكتب و رقم التسلسل موجود من قبل")]
        public int LaborOfficeID { get; set; }

        [Display(Name = "رقم التسلسل")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Range(1, int.MaxValue, ErrorMessage = "يجب ان يكون رقم صحيح أكبر من 0 و أصغر من 2,147,483,648")]
        [RegularExpression(@"^[0-9]+[0-9]*$", ErrorMessage = "يجب ان يكون رقم صحيح أكبر من 0 و أصغر من 2,147,483,648")]
        //[Remote("CheckForUniquSequenceNumberLaborOfficeID", "Employee", AdditionalFields = "EmpID,LaborOfficeID", ErrorMessage = "الدمج بين رقم المكتب و رقم التسلسل موجود من قبل")]
        public int SequenceNumber { get; set; }

        [Display(Name = "الصورة الشخصية")]
        public string ImageName { get; set; }


        [Display(Name = "صورة الهوية الوطنية/الاقامة")]
        public string NationaIDImage { get; set; }

        public Setting settingObj { get; set; }
        public string HF_Name { get; set; }

        private UnitOfWork _unitOfWork;

        public CompanyEmployeeVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }


        /// <summary>
        /// check duplication of Name
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool CheckForUniqueName(int? companyID, int? empID, string Name)
        {

            var result = _unitOfWork.Employee.Get().Count(e => e.CompanyID == companyID && e.EmpID != empID && e.Name.Contains(Name)) > 0 ? false : true;
            return result;
        }

        /// <summary>
        /// check duplication of Email
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
        public bool CheckForUniqueEmail(int? companyID, string Email)
        {
            //email cannot be repeated in the same comany
            //email cannot repeated if it's existed in other comany and this record is not deleted
            //var Empresult = _unitOfWork.Employee.Get().Count(e => (companyID == 0 && e.Email.Contains(Email) && e.IsDeleted == true) || (e.CompanyID == companyID && e.Email.Contains(Email)) || (e.CompanyID != companyID && e.Email.Contains(Email))) > 0 ? false : true;
            var Empresult = _unitOfWork.Employee.Get().Count(e => ((companyID == 0 && e.Email.Contains(Email)) || (e.CompanyID == companyID && e.Email.Contains(Email)) || (e.CompanyID != companyID && e.Email.Contains(Email))) && e.IsDeleted == false) > 0 ? false : true;

            //email cannot repeated in  any company 
            var CompResult = _unitOfWork.Company.Get().Count(e => (e.Email.Contains(Email)) && e.IsDeleted == false) > 0 ? false : true;

            return Empresult && CompResult;
        }

        /// <summary>
        /// check duplication of mobile number
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        public bool CheckForUniqueMobile(int? companyID, int? EmpID, string Mobile)
        {

            var result = _unitOfWork.Employee.Get().Count(e => e.CompanyID == companyID && e.EmpID != EmpID && e.Mobile.Contains(Mobile) || (e.CompanyID != companyID && e.Mobile.Contains(Mobile) && e.IsDeleted == false)) > 0 ? false : true;
            return result;
        }
        public bool CheckForUniqueNationalID(int? companyID, int EmpID, string NationalID)
        {
            var result = _unitOfWork.Employee.Get().Count(e => e.EmpID != EmpID && e.NationalID.Contains(NationalID) || (e.CompanyID != companyID && e.NationalID.Contains(NationalID) && e.IsDeleted == false)) > 0 ? false : true;
            return result;
        }
        /// <summary>
        /// it means that this national id for a deleted employee in the same company
        /// and not working now in another company
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="EmpID"></param>
        /// <param name="NationalID"></param>
        /// <returns></returns>
        public int CheckForDeletedEmpNationlID(int? companyID, string NationalID)
        {
            int deletedEmpId = 0;
            //employee that has been deleted from this company before and he is not working now in another company now
            var deletedEmpResult = _unitOfWork.Employee.Get().Where(e => e.CompanyID == companyID && e.NationalID.Contains(NationalID) && e.IsDeleted == true).FirstOrDefault();
            if (deletedEmpResult != null)
            {
                deletedEmpId = deletedEmpResult.EmpID;
                //check if the employee national id to rehire is working currently in another company
                var currentWorking = _unitOfWork.Employee.Get().Where(e => e.CompanyID != companyID && e.NationalID.Contains(NationalID) && e.IsDeleted == false).FirstOrDefault();
                if (currentWorking != null)
                    deletedEmpId = 0;
            }

            return deletedEmpId;
        }

        /// <summary>
        ///  check duplication of combination between SequenceNumber and LaborOfficeID
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <param name="SequenceNumber"></param>
        /// <param name="LaborOfficeID"></param>
        /// <returns></returns>
        public bool CheckForUniquSequenceNumberLaborOfficeID(int? empID, int SequenceNumber, int LaborOfficeID)
        {
   
            var Empresult = _unitOfWork.Employee.Get().Count(e => (e.EmpID != empID && SequenceNumber > 0 && LaborOfficeID > 0 && e.SequenceNumber == SequenceNumber && e.LaborOfficeID == LaborOfficeID)) > 0 ? false : true;

            return Empresult;
        }

        public Setting GetSettings()
        {
            return _unitOfWork.Settings.GetByID(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
        }

        public string GetEmail(int Id)
        {
            var email = _unitOfWork.Employee.GetByID(Id);
            return email.Email;
        }

        public int GetEmpID(int Id)
        {
            var empID = _unitOfWork.Employee.GetByID(Id);
            return empID.EmpID;
        }

        public int GetEmpIDUserAccount(int Id)
        {
            var objEmp = _unitOfWork.UserAccount.Get().FirstOrDefault(e => e.EmpID == Id);
            int empID = 0;
            if (objEmp != null)
            {
                empID = objEmp.ID;
            }
            return empID;

        }
        public bool UpdateEmp(int Id)
        {
            var objEmp = _unitOfWork.Employee.Get().FirstOrDefault(e => e.EmpID == Id);
            bool bReturn = false;
            if (objEmp != null)
            {
                objEmp.IsDeleted = true;
                _unitOfWork.Save();
                bReturn = true;
            }
            return bReturn;

        }
        public bool UpdateRehireDeletedEmployee(int empId)
        {
            settingObj = GetSettings();
            var objEmp = _unitOfWork.Employee.Get().FirstOrDefault(e => e.EmpID == empId);
            bool bReturn = false;
            if (objEmp != null)
            {
                //update isDeleted=false
                objEmp.IsDeleted = false;
                //Add user account record to enable rehired employee to  login
                TaskManagementModel.UserAccount user = new TaskManagementModel.UserAccount()
                {
                    UserName = objEmp.Email,
                    CompanyID = objEmp.CompanyID,
                    EmpID = objEmp.EmpID,
                    IsCompany = false,
                    Password = QvLib.Security.DataProtection.Encrypt(objEmp.Email.Split('@')[0] + objEmp.EmpID + "d"),
                    UserTypeID = (int)LoggedUserType.Employee
                };
                _unitOfWork.UserAccount.Insert(user);
                _unitOfWork.Save();
                //send mail for rehire
                string DycPass = QvLib.Security.DataProtection.Decrypt(user.Password);
                string LoadTemp = (QvLib.QVMail.LoadMailTemplate("/MailTemplate/email.html"));
                string host = HttpContext.Current.Request.Url.Host;
                LoadTemp = LoadTemp.Replace("{CompanyName}", string.IsNullOrEmpty(MvcApplication.userData.companyName) ? MvcApplication.userData.userName.Split('@')[0] : MvcApplication.userData.companyName)
                                  .Replace("{UserName}", objEmp.Email)
                                   .Replace("{Password}", DycPass)
                                   .Replace("{Host}", host);
                QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "مرحبا بك في برنامج إدارة العمل عن بعد", objEmp.Email, LoadTemp, settingObj.FromEmail);
                bReturn = true;


            }
            return bReturn;

        }
        public List<CompanyEmployeeVM> Select(int CompanyId, string Name, bool? bGender, string JobTitle, bool? bIsActive, string Email)
        {
            List<CompanyEmployeeVM> objEmp = new List<CompanyEmployeeVM>();

            objEmp = _unitOfWork.Employee.Get(e => (CompanyId == 0 || e.CompanyID == CompanyId) && e.Name.Contains(Name) && e.JobTitle.Contains(JobTitle) && e.Email.Contains(Email)
                && (bGender == null || e.Gender == bGender) && (bIsActive == null || e.IsActive == bIsActive) && (e.IsDeleted == false)).Select(e => new CompanyEmployeeVM()
                {
                    Id = e.EmpID,
                    EmpID = e.EmpID,
                    Name = e.Name,
                    CompanyName = e.Company.Name,
                    JobTitle = e.JobTitle != null ? e.JobTitle : "لا يوجد",
                    Gender = (bool)e.Gender,
                    Email = e.Email,
                    IsActive = (bool)e.IsActive,
                    IsDeleted = (bool)e.IsDeleted
                }).ToList();

            return objEmp;
        }


        public CompanyEmployeeVM EmployeeDetails(int EmployeeId)
        {
            var objEmp = _unitOfWork.Employee.GetByID(EmployeeId);

            CompanyEmployeeVM empObj = new CompanyEmployeeVM();
            empObj.EmpID = objEmp.EmpID;
            empObj.Id = objEmp.EmpID;
            empObj.Name = objEmp.Name;
            empObj.CompanyID = objEmp.CompanyID.Value;
            empObj.CompanyName = objEmp.Company.Name;
            empObj.JobTitle = objEmp.JobTitle;
            empObj.Gender = (bool)objEmp.Gender;
            empObj.Email = objEmp.Email;
            empObj.IsActive = (bool)objEmp.IsActive;
            empObj.IsDeleted = (bool)objEmp.IsDeleted;
            empObj.Mobile = objEmp.Mobile;
           // empObj.Birthdate = (objEmp.Birthdate.HasValue) ? (MvcApplication.IsGregDate) ? objEmp.Birthdate.Value.ToGregDatediff() : objEmp.Birthdate.Value.ToHijriArabicDate() : String.Empty;

            empObj.Birthdate = (objEmp.Birthdate.HasValue)
               ? (MvcApplication.IsGregDate)
                   ? objEmp.Birthdate.Value.ToGregDate3() :
                   objEmp.Birthdate.Value.ToHijriDate() : String.Empty;
           
            empObj.Address = objEmp.Address;
            empObj.LaborOfficeID = (int)objEmp.LaborOfficeID;
            empObj.SequenceNumber = (int)objEmp.SequenceNumber;
            empObj.NationalID = objEmp.NationalID;
            empObj.ImageName = objEmp.ImageName;
            empObj.NationaIDImage = objEmp.NationaIDImage;
            return empObj;
        }

        public void SetIsActive(int? EmployeeId)
        {

            var objEmp = _unitOfWork.Employee.GetByID(EmployeeId);
            var objbefore = objEmp.Clone();
            settingObj = _unitOfWork.Settings.GetByID(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
            string Message = "";
            if (objEmp.IsActive == false)
            {
                objEmp.IsActive = true;
                Message = "لقد تم تغيير الحالة الخاصة بك ل'فعال'  ";
            }
            else
            {
                objEmp.IsActive = false;
                Message = "لقد تم تغيير الحالة الخاصة بك ل'غير فعال'  ";
            }
            _unitOfWork.Employee.Update(objEmp);
            _unitOfWork.Save();

            QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "مرحبا بك فى برنامج ادارة المهام", objEmp.Email, Message, settingObj.FromEmail);
            //log.

            Generallog.Log(EmployeeId.Value, objEmp, objbefore, true, objEmp.Name, 0, (int)objEmp.CompanyID);
            //Generallog.Log(EmployeeId.Value, objEmp, objbefore, true, Name, 0, MvcApplication.userData.userId);
        }

        public Boolean Save()
        {
            settingObj = _unitOfWork.Settings.Get().FirstOrDefault();//.GetByID(int.Parse(ConfigurationManager.AppSettings["ContactUsToEmail"].ToString()));
            Boolean bReuslt = false;
            string LoadTemp = string.Empty;
            using (TransactionScope tsTransScope = new TransactionScope())//(TransactionScopeOption.Suppress))
            {

                if (Id == 0)
                {

                    TaskManagementModel.Employee obj = new TaskManagementModel.Employee()
                    {
                        CompanyID = CompanyID,
                        Name = Name,
                        EmpID = EmpID,
                        Mobile = Mobile,
                        Birthdate =
                            Birthdate != null
                                ? (MvcApplication.IsGregDate) ? EtaskMinstry.Extentions.GregDates(Birthdate) : EtaskMinstry.Extentions.HijriToGregDates(Birthdate.Split(' ')[0])
                                : null,
                        Email = Email,
                        IsActive = IsActive,
                        IsDeleted = false,
                        HireDate = DateTime.Now,
                        JobTitle = JobTitle != null ? JobTitle : "لا يوجد",
                        Gender = Gender,
                        Address = Address,
                        LaborOfficeID = LaborOfficeID,
                        SequenceNumber = SequenceNumber,
                        NationalID = NationalID,
                        ImageName = ImageName,
                        NationaIDImage = NationaIDImage

                    };
                    _unitOfWork.Employee.Insert(obj);
                    _unitOfWork.Save();

                    obj.EmpID = obj.EmpID;
                    _unitOfWork.Employee.Update(obj);
                    TaskManagementModel.UserAccount user = new TaskManagementModel.UserAccount()
                    {
                        UserName = obj.Email,
                        CompanyID = obj.CompanyID,
                        EmpID = obj.EmpID,
                        IsCompany = false,
                        Password = QvLib.Security.DataProtection.Encrypt(obj.Email.Split('@')[0] + obj.EmpID),
                        UserTypeID = (int)LoggedUserType.Employee
                    };
                    _unitOfWork.UserAccount.Insert(user);
                    string DycPass = QvLib.Security.DataProtection.Decrypt(user.Password);
                    LoadTemp = (QvLib.QVMail.LoadMailTemplate("/MailTemplate/email.html"));
                   // string host = HttpContext.Current.Request.Url.Host;
                    string host = HttpContext.Current.Request.Url.Authority;
                    var com = new CompanyVM().Details(CompanyID);

                    LoadTemp = LoadTemp.Replace("{CompanyName}", (com != null)? com.Name : "")
                                      .Replace("{UserName}", obj.Email)
                                       .Replace("{Password}", DycPass)
                                       .Replace("{Host}", host);

                    var send = QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password,
                                              settingObj.PortNo, settingObj.SSL, "مرحبا بك في برنامج إدارة العمل عن بعد", obj.Email, LoadTemp,
                                              settingObj.FromEmail);

                    _unitOfWork.Save();

                    bReuslt = obj.EmpID > 0 ? true : false;
                    //log.
                    Generallog.Log(Id, obj, null, true, Name,obj.EmpID, (int)obj.CompanyID);
                   // Generallog.Log(Id, obj, null, true, Name, MvcApplication.userData.userId, (int)obj.CompanyID);

                }
                else
                {
                    var obj = _unitOfWork.Employee.GetByID(Id);
                    var objbefore = obj.Clone();
                    if (obj != null)
                    {
                        obj.Gender = Gender;
                        obj.Name = Name;
                        obj.CompanyID = CompanyID;
                        obj.LastModifiedDate = DateTime.Now;
                        obj.Mobile = Mobile;
                        obj.JobTitle = JobTitle;
                        obj.IsActive = IsActive;
                        obj.Birthdate = Birthdate != null
                            ? (MvcApplication.IsGregDate) ? EtaskMinstry.Extentions.GregDates(Birthdate) : EtaskMinstry.Extentions.HijriToGregDates(Birthdate.Split(' ')[0]) : null;
                        obj.Address = Address;
                        obj.LaborOfficeID = LaborOfficeID;
                        obj.SequenceNumber = SequenceNumber;
                        obj.NationalID = NationalID;
                        obj.ImageName = ImageName;
                        obj.NationaIDImage = NationaIDImage;
                        _unitOfWork.Employee.Update(obj);

                        _unitOfWork.Save();
                        //  NotificationHub.Send(Users.Employee(obj.EmpID), NotificationType.NewTask, "لقد تم تعديل في بياناتك من قبل الشركة ", @"/Employee/employee/" );

                        string companyName = "";
                        if (MvcApplication.userData.isCompany)
                        {
                            companyName = MvcApplication.userData.companyName;
                        }

                        LoadTemp = (QvLib.QVMail.LoadMailTemplate("/MailTemplate/Notification.html"));
                        string host = HttpContext.Current.Request.Url.Host;


                        LoadTemp = LoadTemp.Replace("{Notification}", "لقد تم تعديل في بياناتك من قبل شركة")
                            .Replace("{Company}", MvcApplication.userData.userName)
                                           .Replace("{Host}", host);

                 var updatesend=       QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password,
                                              settingObj.PortNo, settingObj.SSL, "اي-تاسك | رسالة ادارية", obj.Email, LoadTemp,
                                              settingObj.FromEmail);


                        //  QvLib.QVMail.SendMail(settingObj.ServerName, settingObj.UserName, settingObj.Password, settingObj.PortNo, settingObj.SSL, "", obj.Email, companyName + "لقد تم تعديل في بياناتك من قبل الشركة", settingObj.FromEmail);

                    }

                    bReuslt = true;
                    //log.
                    Generallog.Log(Id, obj, objbefore, true, Name, obj.EmpID, (int)obj.CompanyID);
                   // Generallog.Log(Id, obj, objbefore, true, Name, MvcApplication.userData.userId, (int)obj.CompanyID);
                }
                //complete transaction
                tsTransScope.Complete();
            }
            return bReuslt;
        }

        public bool SendMail(string subject, string to, string body, string from)
        {
            bool success = true;
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            try
            {
                MailAddress fromAddress = new MailAddress(from.Trim().ToString());
                message.From = fromAddress;
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                smtpClient.Host = settingObj.ServerName.ToString();
                smtpClient.Port = settingObj.PortNo;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(settingObj.UserName.Trim().ToString(), settingObj.Password.Trim().ToString());
                smtpClient.EnableSsl = settingObj.SSL;
                smtpClient.Send(message);

            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }


    }
}