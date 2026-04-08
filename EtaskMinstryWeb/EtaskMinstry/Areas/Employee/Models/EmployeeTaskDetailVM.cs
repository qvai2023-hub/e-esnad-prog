using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using TaskManagementModel;

namespace EtaskMinstry.Models.Employee
{
    public class EmployeeTaskDetailVM
    {

        #region F E I L D  S  &  P R O P E R T I E S

        public int TaskID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public DateTime? TaskStartDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CreatedDate { get; set; }
        public string EstimatedTime { get; set; }
        public string ActualTime { get; set; }

        public int? EmpID { get; set; }
        public string EmployeeName { get; set; }

        public int CompanyID { get; set; }
        public bool IsArchived { get; set; }

        public int PriorityID { get; set; }
        public string priorityName { get; set; }
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public string Project { get; set; }
        public bool IsAttach { get; set; }
        public bool IsDelayed { get; set; }
        public Nullable<Decimal> ExpectedTime { get; set; }
        public List<TaskTimeDetails> TimeDetails { get; set; }
        public string TasktodayTime { get; set; }
        public int? TimeUnitID { get; set; }
        public string DilverTime { get; set; }
        public List<TaskTLog> TaskLog { get; set; }

        public int? LastEmpWriteTimeCount { get; set; }
        #endregion

        private UnitOfWork _unitOfWork;

        public EmployeeTaskDetailVM()
        {
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        public EmployeeTaskDetailVM Select(object iID, bool IsAttach = false)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iID);
            if (objTask == null)
                return null;

            else
            {

                EmployeeTaskDetailVM objDetails = new EmployeeTaskDetailVM();
                objDetails.IsAttach = IsAttach;
                objDetails.CompanyID = objTask.CompanyID;
                objDetails.Project = objTask.Project == null ? "-" : objTask.Project.Name;
                objDetails.PriorityID = objTask.PriorityID;
                objDetails.priorityName = objTask.Priority.Name;
                objDetails.CreatedDate = (MvcApplication.IsGregDate) ? objTask.CreatedDate.ToGregDate() : objTask.CreatedDate.ToHijriDate();
                objDetails.Description = objTask.Description;
                objDetails.EmpID = objTask.EmpID.HasValue ? objTask.EmpID.Value : 0;
                objDetails.EmployeeName = objTask.EmpID.HasValue ? ServiceManger.GetEmplyeeName(objTask.EmpID.Value) : "_";
                objDetails.EndDate = objTask.EndDate.HasValue ? (MvcApplication.IsGregDate) ? objTask.EndDate.Value.ToGregDate() : objTask.EndDate.Value.ToHijriDate() : "-";
                objDetails.ExpectedTime = objTask.ExpectedTime.HasValue ? objTask.ExpectedTime : null;
                objDetails.TimeUnitID = TimeUnitID.HasValue ? TimeUnitID : null;
                objDetails.EstimatedTime = (objTask.ExpectedTime.HasValue && objTask.TimeUnitID.HasValue) ? objTask.ExpectedTime.Value + " " + objTask.TimeUnit.Name : "";
                //  objDetails.ActualTime = objTask.ActualTime.HasValue ? SpendTimeByEmployee(MvcApplication.userData.userId,objTask.TaskID) : "0";
                objDetails.ActualTime = objTask.ActualTime.HasValue ? TaskManger.SpendTimeByDay(objTask.ActualTime.Value) : "0";
                objDetails.IsArchived = objTask.IsArchived;
                objDetails.IsDelayed = objTask.isDelayed;
                objDetails.LastEmpWriteTimeCount = objTask.TaskTLogs.FirstOrDefault(t => t.CreatedDate.Date == DateTime.Now.Date && t.TimeCount != null) == null ? 0 : objTask.TaskTLogs.OrderByDescending(t => t.TaskTLogID).FirstOrDefault(t => t.CreatedDate.Date == DateTime.Now.Date && t.TimeCount != null).EmpID;
                objDetails.StartDate = objTask.StartDate.HasValue ? (MvcApplication.IsGregDate) ? objTask.StartDate.Value.ToGregDate() : objTask.StartDate.Value.ToHijriDate() : "-";
                objDetails.TaskStartDate = objTask.StartDate.HasValue ? objTask.StartDate.Value : new Nullable<DateTime>();
                //here for employee  check if current employee for task or reassigned for another emp
                if (objTask.EmpID == EtaskMinstry.MvcApplication.userData.userId)
                {
                    objDetails.StatusID = objTask.StatusID;
                    objDetails.StatusName = objTask.Status.Name;
                }
                else
                {
                    // get last Employee log 
                    var log = objTask.TaskTLogs.LastOrDefault(l => l.EmpID == EtaskMinstry.MvcApplication.userData.userId);
                    objDetails.StatusID = log.StatusID;
                    objDetails.StatusName = log.Status.Name;
                }
                objDetails.Summary = objTask.Summary;
                objDetails.Title = objTask.Title;
                objDetails.TaskID = objTask.TaskID;
                var timelog = objTask.TaskTLogs.Where(t => t.TimeCount != null && t.TimeCount != 0); // && t.EmpID == EtaskMinstry.MvcApplication.userData.userId

            
                objDetails.TimeDetails = new List<TaskTimeDetails>();               
                objDetails.TimeDetails.AddRange(objTask.GetTaskTimeLog.Values);

            
                objDetails.DilverTime = objTask.DeliverDate.HasValue ? (MvcApplication.IsGregDate) ? "تاريخ التسليم :" + objTask.DeliverDate.Value.ToGregDate() + " " : "تاريخ التسليم :" + objTask.DeliverDate.Value.ToHijriDate() + "::" : "";
                objDetails.TasktodayTime = objDetails.TimeDetails.FirstOrDefault(t => t.isToday) == null ? "0" : objDetails.TimeDetails.FirstOrDefault(t => t.isToday).LogTime.Split(' ')[0];
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

                objTask.EndDate = DateTime.Parse(EndDate.HijriToGregDate());
                objTask.ExpectedTime = ExpectedTime;
                objTask.IsArchived = IsArchived;
                objTask.PriorityID = PriorityID;
                objTask.StartDate = DateTime.Parse(StartDate.HijriToGregDate());
                objTask.StatusID = StatusID;
                objTask.TimeUnitID = TimeUnitID;
                _unitOfWork.TaskRepository.Update(objTask);
                _unitOfWork.Save();
                result = true;
            }
            return result;
        }
       

        /// <summary>
        /// spent time by employee of task
        /// </summary>
        /// <param name="empid"></param>
        /// <param name="taskid"></param>
        /// <returns></returns>
        public string SpendTimeByEmployee(int empid, int taskid)
        {
            decimal totaltime = 0;
            decimal timeValue = 0;
            var objTask = _unitOfWork.TaskRepository.GetByID(taskid);
            if (objTask != null) //check if Task not null
            {

                Dictionary<string, decimal> uniqueTimeLog = new Dictionary<string, decimal>();
                var timelog = objTask.TaskTLogs.Where(t => t.TimeCount != null && t.EmpID == empid);
                foreach (var tlog in timelog)
                {
                    if (uniqueTimeLog.ContainsKey(tlog.CreatedDate.Date.ToString()))
                        uniqueTimeLog[tlog.CreatedDate.Date.ToString()] = (decimal)tlog.TimeCount;
                    else
                        uniqueTimeLog.Add(tlog.CreatedDate.Date.ToString(), (decimal)tlog.TimeCount);
                }
                if (uniqueTimeLog.Count() < 1)
                {
                    totaltime = timeValue;
                }
                else
                {

                    foreach (var itemTime in uniqueTimeLog)
                    {

                        totaltime += itemTime.Value;

                    }

                }

            }
            string day = TaskManger.SpendTimeByDay(totaltime);
            return day;
        }


        public Dictionary<string, TaskTimeDetails> GetTaskTimeLog(EmployeeTaskDetailVM Task)
        {
            string del = "";
            Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();


            return uniqueTimeLog;
        }

    }



    //public class TaskTimeDetails
    //{
    //    public string LogDate;
    //    public string LogTime;
    //    public bool isToday;
    //    public int empId;

    //}

}




