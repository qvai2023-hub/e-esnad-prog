using EtaskMinstry.App_Code;
using EtaskMinstry.AppCode;
using EtaskMinstry.Models.Company;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Company
{
    public class CompanyDashboardVM
    {
        private readonly UnitOfWork _unitOfWork;

        public CompanyDashboardVM(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        #region"Properties"


        [ScaffoldColumn(false)]
        public int TaskID { get; set; }


        [LocalizedDisplayName("Task", NameResourceType = typeof(Resources.Company.CompanyTask))]
        public String Task { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof(Resources.EmoloyeeTask.EmployeeTask))]
        public String Status { get; set; }


        public int StatusID { get; set; }

        public int PrioirtyID { get; set; }

        public String Prioirty { get; set; }

        public String EmpName { get; set; }

        public int? EmpID { get; set; }

        public DateTime? FinishDate { get; set; }



        public String ArabicStartDate { get; set; }


        public Boolean IsDelayed { get; set; }
        public string delayTime { get; set; }


        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public String HijriStartDate { get; set; }

        public String HijriEndDate { get; set; }
        public String HijriFinishDate { get; set; }
        public string HijriRejectedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public DashBoaedTaskType DBStatus { get; set; }
        public DateTime? SuspendedDate { get; set; }
        public string HijriSuspendedDate { get; set; }
        public double delayPercentage { get; set; }
        #endregion

     
        public async Task<List<CompanyDashboardVM>> SelectCompanyTasksAsync(DashBoaedTaskType taskType, int page = 1, int pageSize = 20)
        {
            var tasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId)
                .AsNoTracking() // Performance boost
                .OrderByDescending(t => t.TaskID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return tasks.Select(t => new CompanyDashboardVM(_unitOfWork)
            {
                TaskID = t.TaskID,
                Task = t.Title,
                Status = t.Status.Name,
                StatusID = t.StatusID,
                PrioirtyID = t.PriorityID,
                Prioirty = t.Priority.Name,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                EmpID = t.EmpID,
                FinishDate = t.DeliverDate
            }).ToList();
        }

    }

}