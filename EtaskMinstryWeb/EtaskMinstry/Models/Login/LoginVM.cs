using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EtaskMinstry.Models.Login
{
    public class LoginVM
    {
        [Display(Name = "اسم الدخول")]
        [RegularExpression(ValidationResource.revMail, ErrorMessageResourceName = "ValidRegularEmail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Required(ErrorMessage = "حقل الزامي")]
        public string UserName { get; set; }

         
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        [Required(ErrorMessage = "حقل الزامي")]
        public string Password { get; set; }

        [Display(Name = "تذكـــــــــــــــــرنى")]
        public bool RememberMe { get; set; }
    }
}