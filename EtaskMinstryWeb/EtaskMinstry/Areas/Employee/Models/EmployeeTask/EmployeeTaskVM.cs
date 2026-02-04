using EtaskMinstry;
using EtaskMinstry.App_Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Models.EmployeeTask
{
    public class EmployeeTaskVM
    {
        [ScaffoldColumn(false)]
        public int taskID { get; set; }

        [ScaffoldColumn(false)]
        public int priorityId { get; set; }

        [ScaffoldColumn(false)]
        public int statusId { get; set; }

        [LocalizedDisplayName("taskName",NameResourceType=typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string taskName { get; set; }

        [LocalizedDisplayName("projectName", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string projectName { get; set; }

        [LocalizedDisplayName("priority", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string priority { get; set; }

        [LocalizedDisplayName("priority", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string rowColor { get; set; }

        [LocalizedDisplayName("state", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string state { get; set; }

        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string startDate { get; set; }

        [LocalizedDisplayName("StartDate", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string US_startDate { get; set; }

        [LocalizedDisplayName("EndDate", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string endDate { get; set; }

        [LocalizedDisplayName("isAccepted", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public bool isAccepted { get; set; }

        [LocalizedDisplayName("actualTime", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public decimal? actualTime { get; set; }

        [LocalizedDisplayName("DelayTime", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public string delaytime { get; set; }

        public string EndTaskClass{ get; set; }
        public string EndTaskSource { get; set; }

         private UnitOfWork _unitOfWork;

         public EmployeeTaskVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public List<EmployeeTaskVM> Select(int iEmpolyee)
        {

            List<EmployeeTaskVM> objTasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == iEmpolyee).Select(t => new EmployeeTaskVM()
            {
                taskID = t.TaskID,
                taskName = t.Title,
                projectName = t.Project.Name,
                state = t.Status.Name,
                startDate = t.StartDate.Value.ToString(),
                endDate = t.EndDate.Value.ToString()
            }).ToList();

                

            return objTasks;
        }

    }
}