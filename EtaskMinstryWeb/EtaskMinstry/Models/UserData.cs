using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EtaskMinstry.Models
{
    [Serializable]
    public class UserData
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public bool isCompany { get; set; }
        public string Email { get; set; }
        public string logo { get; set; }
        public int status { get; set; }
        public int companyStatus { get; set; }
        public string NotificationClientID { get; set; }
        public bool isAuthorized { get; set; }
        public int? CompanyId { get; set; }//in case of employee return his Company id
        public string companyName { get; set; }
        public int UserTypeId { get; set; }
        public bool HasAttendanceReport { get; set; }
        public bool BriefTaskReport { get; set; }

    }
}