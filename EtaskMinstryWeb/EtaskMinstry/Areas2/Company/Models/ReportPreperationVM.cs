using EtaskMinstry.AppCode;
using EtaskMinstry.Models.Priority;
using EtaskMinstry.Models.Project;
using EtaskMinstry.Models.Status;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Company.Models
{
    [Serializable]
    public class ReportPreperationVM
    {
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string taskName { get; set; }
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
           ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public List<int> projects { get; set; }
        public SelectList ddlProjects { get; set; }
        public bool withoutProjectChoises { get; set; }
        public bool withoutProject { get; set; }
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
           ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public List<int> employees { get; set; }
        public SelectList ddlEmployees { get; set; }
        public bool withotEmpsChoises { get; set; }
        public bool withoutEmps { get; set; }
        public List<int> Status { get; set; }
        public List<int> priorities { get; set; }
        //public DateTime? fromDate { get; set; }
        //public DateTime? toDate { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public string fromendDate { get; set; }
        public string toendDate { get; set; }
        public string calendarType { get; set; }
        public List<StatusDisplay> chkStatus { get; set; }
        public List<PriorityDisplay> chkPriorities { get; set; }


        public ReportPreperationVM()
        {
            ddlProjects = new SelectList(new ProjectDisplay().Get(),"ID","Name");
            ddlEmployees = new SelectList(new ManageEmployees().GetEmpsInCompany(MvcApplication.userData.userId), "userId", "userName");
            chkPriorities = new PriorityDisplay().Get();
            chkStatus = new StatusDisplay().Get();
        }
    }
}