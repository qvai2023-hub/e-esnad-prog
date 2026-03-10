using EtaskMinstry.AppCode;
using EtaskMinstry.Models.Priority;
using EtaskMinstry.Models.Project;
using EtaskMinstry.Models.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using System.ComponentModel.DataAnnotations;
using Resources;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Employee.Models
{
    public class ReportPreperationVM
    {
        [RegularExpression(ValidationResource.RevString50, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string taskName { get; set; }
        public List<int> projects { get; set; }
        public SelectList ddlProjects { get; set; }
        public bool withoutProjectChoises { get; set; }
        public bool withoutProject { get; set; }
        public List<int> Status { get; set; }
        public List<int> priorities { get; set; }

        public string fromDate { get; set; }
        public string toDate { get; set; }
        public string fromendDate { get; set; }
        public string toendDate { get; set; }
        public string calendarType { get; set; }
        public List<StatusDisplay> chkStatus { get; set; }
        public List<PriorityDisplay> chkPriorities { get; set; }
       // public UnitOfWork _unitOfWork;

        public ReportPreperationVM()
        {
            int CompanyId =(int)new ManageEmployees().GetCompanyOfEmployee(MvcApplication.userData.userId).CompanyId;
            ddlProjects = new SelectList(new ProjectDisplay().GetProjectsByCompany(CompanyId), "ID", "Name");
            chkPriorities = new PriorityDisplay().Get();
            chkStatus = new StatusDisplay().Get();
            
        }
    }
}
