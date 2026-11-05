using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using Resources;
using TaskManagementModel;

namespace EtaskMinstry.Models.Common
{
    public class TaskDetailCommonVM
    {

        #region F E I L D  S  &  P R O P E R T I E S

        public int TaskID { get; set; }
        public string Title { get; set; }
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
          ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Description { get; set; }
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
          ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Summary { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string FinishDate { get; set; }
        public string CreatedDate { get; set; }
        public string EstimatedTime { get; set; }
        public string ActualTime { get; set; }

        public int? EmpID { get; set; }
        public string EmployeeName { get; set; }

        public int CompanyID { get; set; }
        public bool IsArchived { get; set; }

        public int PriorityID { get; set; }
        public string priorityName { get; set; }
        public bool IsAttach { get; set; }
        public bool IsDelayed { get; set; }
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public string Project { get; set; }
        public int ProjectID { get; set; }

        [RegularExpression(ValidationResource.ExtpectedTime, ErrorMessageResourceName = "ExpectedTime",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "الوقت المقدر")]
        public Nullable<Decimal> ExpectedTime { get; set; }
        public List<TaskTimeDetails> TimeDetails { get; set; }
        public int? TimeUnitID { get; set; }
        public string rejectedDate { get; set; }
        public double Progressbar { get; set; }
        #endregion

        private UnitOfWork _unitOfWork;

        public TaskDetailCommonVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public TaskDetailCommonVM Select(object iID, bool IsAttach = false)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iID);
            if (objTask == null)
                return null;

            else
            {

                TaskDetailCommonVM objDetails = new TaskDetailCommonVM();
                objDetails.IsAttach = IsAttach;
                objDetails.CompanyID = objTask.CompanyID;
                objDetails.ProjectID = (objTask.ProjectID.HasValue) ? objTask.ProjectID.Value : 0;
                objDetails.Project = objTask.Project == null ? "-" : objTask.Project.Name;
                objDetails.PriorityID = objTask.PriorityID;
                objDetails.priorityName = objTask.Priority.Name;
                objDetails.CreatedDate = (MvcApplication.IsGregDate) ? objTask.CreatedDate.ToGregDatediff() : objTask.CreatedDate.ToHijriDate();
                objDetails.Description = objTask.Description;
                objDetails.EmpID = objTask.EmpID.HasValue ? objTask.EmpID.Value : 0;
                objDetails.EmployeeName = objTask.EmpID.HasValue ? EtaskMinstry.AppCode.ServiceManger.GetEmplyeeName(objTask.EmpID.Value) : "_";
                objDetails.EndDate = objTask.EndDate.HasValue ? (MvcApplication.IsGregDate) ? objTask.EndDate.Value.ToGregDatediff() : objTask.EndDate.Value.ToHijriDate() : "-";
                objDetails.ExpectedTime = objTask.ExpectedTime.HasValue ? objTask.ExpectedTime : null;
                objDetails.TimeUnitID = objTask.TimeUnitID.HasValue ? objTask.TimeUnitID : null;
                objDetails.EstimatedTime = objTask.ExpectedTime + " " + (objTask.TimeUnit == null ? "" : objTask.TimeUnit.Name);
                objDetails.ActualTime = objTask.ActualTime.HasValue ? TaskManger.SpendTimeByDay(objTask.ActualTime) : "0";
                objDetails.IsArchived = objTask.IsArchived;
                objDetails.StartDate = objTask.StartDate.HasValue ? (MvcApplication.IsGregDate) ? objTask.StartDate.Value.ToGregDatediff() : objTask.StartDate.Value.ToHijriDate() : "-";
                objDetails.StatusID = objTask.StatusID;
                objDetails.FinishDate = objTask.DeliverDate.HasValue ? (MvcApplication.IsGregDate) ? objTask.DeliverDate.Value.ToGregDatediff() : objTask.DeliverDate.Value.ToHijriDate() : "_";
                objDetails.StatusName = objTask.Status.Name;
                objDetails.Summary = objTask.Summary;
                objDetails.Title = objTask.Title;
                objDetails.TaskID = objTask.TaskID;
                objDetails.IsDelayed = objTask.isDelayed;
                objDetails.Progressbar = ProgressbarPercentage(objTask);
                var timelog = objTask.TaskTLogs.Where(t => t.TimeCount != null && t.TimeCount != 0);
                objDetails.TimeDetails = new List<TaskTimeDetails>();             
                objDetails.TimeDetails.AddRange(objTask.GetTaskTimeLog.Values);
                return objDetails;
            }
        }

        /// <summary>
        /// Save Task .
        /// </summary>
        /// <returns> Boolean </returns>
        public bool Save()
        {
            bool result = false;

            var objTask = _unitOfWork.TaskRepository.GetByID(TaskID);
            if (objTask != null)
            {

                objTask.EndDate = DateTime.Parse(EndDate);
                objTask.ExpectedTime = ExpectedTime;
                objTask.IsArchived = IsArchived;
                objTask.PriorityID = PriorityID;
                objTask.ProjectID = ProjectID;
                objTask.StartDate = DateTime.Parse(StartDate);
                objTask.StatusID = StatusID;
                objTask.TimeUnitID = TimeUnitID;
                _unitOfWork.TaskRepository.Update(objTask);
                _unitOfWork.Save();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// To Calculate progressbar for New and InProgress Tasks 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public double ProgressbarPercentage(TaskManagementModel.Task task)
        {
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).DelayPercentage;
        }




    }






   
}
