using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Company.Models
{
    public class CompanyProfileVM
    {
          public int Id { get; set; }
          public int CompID { get; set; }
        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required]
        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [RegularExpression(ValidationResource.revMail, ErrorMessageResourceName = "ValidRegularEmail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
       
        public string Birthdate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string CompanyID { get; set; }
        [Display(Name = "رقم الجوال")]
        [RegularExpression(ValidationResource.revMobile, ErrorMessageResourceName = "ValidateMobile",
             ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Phone { get; set; }
        [Display(Name = "العنوان")]
         public string Address { get; set; }
         private UnitOfWork _unitOfWork;

         public CompanyProfileVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }



         public CompanyProfileVM CompanyDetails(int CompanyID)
         {
             var objComp = _unitOfWork.Company.Get().Where(a => a.CompanyID == CompanyID).FirstOrDefault();

             return new CompanyProfileVM()
             {
                 CompID = objComp.CompanyID,
                 Id = objComp.CompanyID,
                 Name = objComp.Name,
                 Email = objComp.Email,
                 IsActive = (bool)objComp.IsActive,
                 IsDeleted = (bool)objComp.IsDeleted,
                 Phone = objComp.Phone,
                 Address = objComp.Address
             };

         }


         public bool ChangeEmail(string Email, int CompanyID)
         {
             bool bReturn = false;

             var objComp = _unitOfWork.Company.Get().Where(a => a.CompanyID == CompanyID).FirstOrDefault();
             var objUser = _unitOfWork.UserAccount.Get().Where(a => a.CompanyID == CompanyID && a.IsCompany==true).FirstOrDefault();

             if (objComp != null && objUser != null)
             {
                 objComp.Email = Email;
                 objUser.UserName = Email;

                 _unitOfWork.Company.Update(objComp);
                 _unitOfWork.UserAccount.Update(objUser);
                 _unitOfWork.Save();
                 bReturn = true;
             }
             return bReturn;

         }


         public bool ChangeAddress(string Address, int CompanyID)
         {
             bool bReturn = false;

             var objComp = _unitOfWork.Company.Get().Where(a => a.CompanyID == CompanyID).FirstOrDefault();


             if (objComp != null)
             {
                 objComp.Address = Address;

                 _unitOfWork.Company.Update(objComp);

                 _unitOfWork.Save();
                 bReturn = true;
             }
             return bReturn;

         }
    }
}