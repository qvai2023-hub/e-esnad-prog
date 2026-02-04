using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Resources;
using TaskManagementModel;

namespace EtaskMinstry.Models
{
    public class ContactUs
    {
        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegularFullName50",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "الاسم")]
        public string Name { get; set; }
       
        
        [RegularExpression(ValidationResource.revMail, ErrorMessageResourceName = "ValidRegularEmail",
             ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "البريد الالكتروني")]
        public string Email { get; set; }

        [Display(Name = "رقم الهاتف")]
        [RegularExpression(ValidationResource.MobNumbersG, ErrorMessageResourceName = "ValidPhoneNo",
             ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Mobile { get; set; }

        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Message { get; set; }


        public string code { get; set; }

        public string MessageTypeId { get; set; }


        private UnitOfWork _unitOfWork;

        public ContactUs()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public void Save(ContactUs obj)
        {
            TaskManagementModel.ContactU contact = new TaskManagementModel.ContactU()
            {
                Name = obj.Name,
                Mobile = obj.Mobile,
                Email = obj.Email,
                Message = obj.Message

            };
            _unitOfWork.ContactURepository.Insert(contact);
            _unitOfWork.Save();
        }

        public List<ContactUs> GetAll()
        {
            var obj = _unitOfWork.ContactURepository.Get().Select(a => new ContactUs
            {
                Name = a.Name,
                Email = a.Email,
                Message = a.Message,
                Mobile = a.Mobile
            }).ToList();
            return obj;
        }
    }
}