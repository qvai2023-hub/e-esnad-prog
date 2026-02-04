using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace EtaskMinstry.Models
{
    public class CustomTxtbox
    {
        [Required(ErrorMessage = "*")]
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
           ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Search { get; set; }
    }

}

