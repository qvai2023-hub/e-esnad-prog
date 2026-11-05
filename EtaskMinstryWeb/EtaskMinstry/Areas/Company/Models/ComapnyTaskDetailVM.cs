using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using Resources;
using TaskManagementModel;

namespace EtaskMinstry.Models.Company
{
    public class ComapnyTaskDetailVM
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

        public Nullable<Decimal> ExpectedTime { get; set; }
        public List<TaskTimeDetails> TimeDetails { get; set; }
        public int? TimeUnitID { get; set; }
        public string rejectedDate { get; set; }
        public double Progressbar { get; set; }
        #endregion

        private UnitOfWork _unitOfWork;

        public ComapnyTaskDetailVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public ComapnyTaskDetailVM Select(object iID, bool IsAttach = false)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iID);
            if (objTask == null)
                return null;

            else
            {

                ComapnyTaskDetailVM objDetails = new ComapnyTaskDetailVM();
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
               // objDetails.TimeDetails=objTask.GetTaskTimeLog
              

                //var allEmp = objTask.TaskTLogs.GroupBy(e => e.EmpID);
                //var allDates = objTask.TaskTLogs.GroupBy(d => d.CreatedDate.Date);
                //var emp = new List<TaskTimeDetails>();
                //foreach (var item in allDates)
                //{
                //    var lst = item.GroupBy(e => e.EmpID);
                //    foreach (var itemlst in lst)
                //    {
                //        var lastDate = itemlst.LastOrDefault(t => t.TimeCount != null);
                //        if (lastDate != null)
                //        {
                //            emp.Add(new TaskTimeDetails()
                //            {
                //                LogTime = ((int)lastDate.TimeCount).ToString(),
                //                isToday = lastDate.CreatedDate.Date == DateTime.Today.Date,
                //                LogDate = lastDate.CreatedDate.Date.ToString(),
                //                empId = lastDate.EmpID.Value
                //            });
                //        }
                //    }

                //}

                //var hash = new List<TaskTimeDetails>();
                //foreach (var item in emp)
                //{

                //    hash.Add(new TaskTimeDetails()
                //    {
                //        LogTime = item.LogTime,
                //        isToday = item.isToday,
                //        LogDate = item.LogDate,
                //        empId = item.empId
                //    });
                //}
                //// int totalTimeCountSameDay = 0;
                //Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();

                //for (int i = 0; i < hash.Count(); i++)
                //{
                //    decimal totalTimeCountSameDay = 0;
                //    if (hash.Count() > 1 && i != hash.Count() - 1)
                //    {
                //        if (hash[i].LogDate.Contains(hash[i + 1].LogDate))
                //        {
                //            totalTimeCountSameDay = decimal.Parse(hash[i].LogTime) + decimal.Parse(hash[i + 1].LogTime);
                //        }
                //        else
                //        {
                //            totalTimeCountSameDay = decimal.Parse(hash[i].LogTime);

                //        }
                //    }
                //    if (!uniqueTimeLog.ContainsKey(hash[i].LogDate.ToString()))
                //        uniqueTimeLog.Add(hash[i].LogDate, new TaskTimeDetails()
                //        {
                //            LogTime = totalTimeCountSameDay == 0 ? hash[i].LogTime : totalTimeCountSameDay.ToString(),
                //            isToday = hash[i].isToday,
                //            LogDate = (MvcApplication.IsGregDate) ? hash[i].LogDate.ToGregArabicDate() : hash[i].LogDate.ToHijriArabicDate()
                //        });

                //}

                //Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();
                //foreach (var tlog in timelog)
                //{
                //    if (uniqueTimeLog.ContainsKey(tlog.CreatedDate.Date.ToString()))
                //        uniqueTimeLog[tlog.CreatedDate.Date.ToString()] = new TaskTimeDetails() { LogTime = totalTimeCountSameDay == 0 ? ((int)tlog.TimeCount).ToString() : totalTimeCountSameDay.ToString(), isToday = tlog.CreatedDate.Date == DateTime.Today.Date, LogDate = (MvcApplication.IsGregDate) ? tlog.CreatedDate.ToGregArabicDate() : tlog.CreatedDate.ToHijriArabicDate() };
                //    else
                //        uniqueTimeLog.Add(tlog.CreatedDate.Date.ToString(), new TaskTimeDetails() { LogTime = totalTimeCountSameDay == 0 ? ((int)tlog.TimeCount).ToString() : totalTimeCountSameDay.ToString(), isToday = tlog.CreatedDate.Date == DateTime.Today.Date, LogDate = (MvcApplication.IsGregDate) ? tlog.CreatedDate.ToGregArabicDate() : tlog.CreatedDate.ToHijriArabicDate() });
                    
                //}
               // objDetails.TimeDetails.AddRange(uniqueTimeLog.Values);
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
