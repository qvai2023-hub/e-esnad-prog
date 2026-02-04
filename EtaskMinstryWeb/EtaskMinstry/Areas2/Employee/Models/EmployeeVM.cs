using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Employee.Models
{
    public class EmployeeVM
    {

         public int Id { get; set; }
        public int EmpID { get; set; }
        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required]
        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "المسمى الوظيفي")]
        public string JobTitle { get; set; }

        [Display(Name = "النوع")]
        public bool Gender { get; set; }

        [RegularExpression(ValidationResource.revMail, ErrorMessageResourceName = "ValidRegularEmail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime HireDate { get; set; }
        [Display(Name = "تاريخ الميلاد")]
        public string Birthdate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string CompanyID { get; set; }
        [Display(Name = "رقم الجوال")]
        [RegularExpression(ValidationResource.revMobile, ErrorMessageResourceName = "ValidateMobile",
             ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
         public string Mobile { get; set; }
        [Display(Name = "العنوان")]
         public string Address { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        public string ConfirmNewPassword { get; set; }

        [Display(Name = "الرقم القومي")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(@"^\d{10,10}$", ErrorMessage = "برجاء ادخال 10 ارقام فقط")]
        [StringLength(10, ErrorMessage = "عدد الحروف تجاوز العدد المسموح")]
        public Int64 NationalID { get; set; }

        [Display(Name = "رقم المكتب")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Range(1, 38, ErrorMessage = "يجب ان يكون رقم صحيح من 1 الى 38")]
        [RegularExpression(@"^[1-9]+[0-9]*$", ErrorMessage = "يجب ان يكون رقم صحيح من 1 الى 38")]
        public int LaborOfficeID { get; set; }

        [Display(Name = "رقم التسلسل")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Range(1, int.MaxValue, ErrorMessage = "يجب ان يكون رقم صحيح أكبر من 0 و أصغر من 2,147,483,648")]
        [RegularExpression(@"^[1-9]+[0-9]*$", ErrorMessage = "يجب ان يكون رقم صحيح أكبر من 0 و أصغر من 2,147,483,648")]
        public int SequenceNumber { get; set; }
         private UnitOfWork _unitOfWork;

         public EmployeeVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

         public EmployeeVM EmployeeDetails(int EmployeeId)
         {
             var objEmp = _unitOfWork.Employee.Get().Where(a=> a.EmpID == EmployeeId).FirstOrDefault();
             
                 return new EmployeeVM()
                 {
                     EmpID = objEmp.EmpID,
                     Id = objEmp.EmpID,
                     Name = objEmp.Name,
                     JobTitle = objEmp.JobTitle,
                     Gender = (bool)objEmp.Gender,
                     Email = objEmp.Email,
                     IsActive = (bool)objEmp.IsActive,
                     IsDeleted = (bool)objEmp.IsDeleted,
                     Mobile = objEmp.Mobile,
                     Birthdate = objEmp.Birthdate != null? EtaskMinstry.Extentions.ToHijriDate((DateTime)objEmp.Birthdate):"",
                     Address = objEmp.Address
                 };
             
         }


         public bool ChangeEmail(string Email, int EmployeeId)
         {
             bool bReturn = false;

             var objEmp = _unitOfWork.Employee.Get().Where(a => a.EmpID == EmployeeId).FirstOrDefault();
             var objUser= _unitOfWork.UserAccount.Get().Where(a => a.EmpID == EmployeeId).FirstOrDefault();

             if (objEmp != null && objUser != null)
             {
                 objEmp.Email = Email;
                 objUser.UserName = Email;

                 _unitOfWork.Employee.Update(objEmp);
                 _unitOfWork.UserAccount.Update(objUser);
                 _unitOfWork.Save();
                 bReturn = true;
             }
             return bReturn;

         }


         public bool ChangeAddress(string Address, int EmployeeId)
         {
             bool bReturn = false;

             var objEmp = _unitOfWork.Employee.Get().Where(a => a.EmpID == EmployeeId).FirstOrDefault();
            

             if (objEmp != null)
             {
                 objEmp.Address = Address;
                
                 _unitOfWork.Employee.Update(objEmp);
                
                 _unitOfWork.Save();
                 bReturn = true;
             }
             return bReturn;

         }


         public bool ChangeMobile(string Mobile, int EmployeeId)
         {
             bool bReturn = false;

             var objEmp = _unitOfWork.Employee.Get().Where(a => a.EmpID == EmployeeId).FirstOrDefault();


             if (objEmp != null)
             {
                 objEmp.Mobile = Mobile;

                 _unitOfWork.Employee.Update(objEmp);

                 _unitOfWork.Save();
                 bReturn = true;
             }
             return bReturn;

         }


         public bool ChangePassowrd(string Old, string New)
         {
             int EmpId = MvcApplication.userData.userId;

             bool breturn = false;
             var objUser = _unitOfWork.UserAccount.Get().Where(a => a.EmpID == EmpId).FirstOrDefault();
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
                 }
                 else
                 {
                     //fail
                     breturn = false;
                 }
             }

             return breturn;
         }
    }
}