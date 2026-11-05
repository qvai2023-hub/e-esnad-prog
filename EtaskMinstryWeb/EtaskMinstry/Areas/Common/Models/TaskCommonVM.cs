using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using EtaskMinstry;
using EtaskMinstry.App_Code;
using EtaskMinstry.Models.EmployeeTask;
using TaskManagementModel;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Models.Common
{
    public class TaskCommonVM
    {
        #region"Properties"

        public int CurrentStatus { get; set; }

        [ScaffoldColumn(false)]
        public int TaskID { get; set; }

        [LocalizedDisplayName("Project", NameResourceType = typeof (Resources.CompanyTask))]
        public String Project { get; set; }

        [LocalizedDisplayName("Task", NameResourceType = typeof (Resources.CompanyTask))]
        public String Task { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.EmployeeTask))]
        public String Status { get; set; }

        public String DeleteClass { get; set; }

        public int StatusID { get; set; }

        public int PrioirtyID { get; set; }

        public Boolean IsArchived { get; set; }

        public String Prioirty { get; set; }

        public String EmpName { get; set; }

        public int? EmpID { get; set; }

        public DateTime? FinishDate { get; set; }

        public String ArabicFinishDate { get; set; }

        public Decimal? WorkedHours { get; set; }

        public string SpendtimebyDay { get; set; }
        public decimal? ActualTime { get; set; }
        public String RowColor { get; set; }

        public Boolean IsDelayed { get; set; }
        public Boolean IsDeleted { get; set; }

        public ICollection<TaskTLog> TaskTLog { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public String HijriStartDate { get; set; }

        public String HijriEndDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }

        public double Progressbar { get; set; }

        public string Delay { get; set; }

        public Boolean IsEditable { get; set; }
        #endregion

        #region"Manage"

        private UnitOfWork _unitOfWork;

        public TaskCommonVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

/// <summary>
/// Search Tasks .
/// </summary>
/// <param name="strTitle"></param>
/// <param name="FromStartDate"></param>
/// <param name="ToStartDate"></param>
/// <param name="FromEndDate"></param>
/// <param name="ToEndDate"></param>
/// <param name="iStatus"></param>
/// <param name="iEmpolyee"></param>
/// <param name="bIsArchived"></param>
/// <param name="bIsNotAssigned"></param>
/// <param name="iProjectID"></param>
/// <param name="PriorityID"></param>
/// <returns></returns>
        public List<TaskCommonVM> Select(String strTitle, String FromStartDate, String ToStartDate, String FromEndDate,
                                          String ToEndDate, int iStatus, int? iEmpolyee, Boolean? bIsArchived,
                                          Boolean? bIsNotAssigned, int iProjectID, int PriorityID)
        {
            // Get Company Employees .
            List<EmployeeProfile> CompanyEmployee =EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployee (MvcApplication.userData.CompanyId.Value);
           
    DateTime dtFromStartDate = new DateTime();
            DateTime dtToStartDate = new DateTime();

            if (!String.IsNullOrEmpty(FromStartDate))
                dtFromStartDate =(MvcApplication.IsGregDate ?  FromStartDate.ToGregExactformate() : FromStartDate.ToGregExact()) ;
               

            if (!String.IsNullOrEmpty(ToStartDate))
                dtToStartDate =(MvcApplication.IsGregDate ?  ToStartDate.ToGregExactformate() : ToStartDate.ToGregExact());
             

            DateTime dtFromEndDate = new DateTime();
            DateTime dtToEndDate = new DateTime();

            if (!String.IsNullOrEmpty(FromEndDate))
                dtFromEndDate =(MvcApplication.IsGregDate ? FromEndDate.ToGregExactformate(): FromEndDate.ToGregExact());
             
            if (!String.IsNullOrEmpty(ToEndDate))
              dtToEndDate =(MvcApplication.IsGregDate ?  ToEndDate.ToGregExactformate() : ToEndDate.ToGregExact());              
             

            strTitle = strTitle.Trim().ToLower();

            List<TaskCommonVM> objTasks =
                _unitOfWork.TaskRepository.Get(t => (iEmpolyee == 0 || t.EmpID == iEmpolyee)
                                                    &&
                                                    (!bIsNotAssigned.HasValue || t.EmpID == null)
                                                    &&
                                                    (!bIsArchived.HasValue || t.IsArchived)
                                                    &&
                                                    (iStatus == 0 || iStatus == (int)TaskStatus.Delay 
                                                    ||
                                                     (iStatus == (int)TaskStatus.Accepted
                                                          ? t.StatusID == iStatus
                                                            //&& t.StartDate > DateTime.Now
                                                          : t.StatusID == iStatus)
                                                    )
                                                    && t.Title.ToLower().Contains(strTitle)
                                                    && t.CompanyID == MvcApplication.userData.CompanyId
                                                    && !t.IsDeleted

                                                    &&
                                                    (String.IsNullOrEmpty(FromStartDate) ||
                                                     t.StartDate.Value >= dtFromStartDate)

                                                    && (String.IsNullOrEmpty(ToStartDate) ||
                                                        t.StartDate.Value <= dtToStartDate)

                                                    && (String.IsNullOrEmpty(FromEndDate) ||
                                                        t.EndDate.Value >= dtFromEndDate)

                                                    && (String.IsNullOrEmpty(ToEndDate) ||
                                                        t.EndDate.Value <= dtToEndDate)

                                                    && (iProjectID == 0 || t.ProjectID == iProjectID)

                                                    && (PriorityID == 0 ||
                                                        t.PriorityID == PriorityID)).Select(t => new TaskCommonVM()
                                                            {
                                                                TaskID = t.TaskID,
                                                                Task =  t.Title,
                                                                Project = (t.ProjectID== null)? "لا يوجد مشروع" :t.Project.Name,
                                                                Status = t.Status.Name,
                                                                DeleteClass =
                                                                    (t.StatusID == (int) TaskStatus.New
                                                                         // New Tasks Only Can Delete .
                                                                         ? "Delete-icon deleteAction"
                                                                         : "DisDelete-icon"),
                                                                StatusID = t.StatusID,
                                                                IsArchived = t.IsArchived,
                                                                PrioirtyID = t.PriorityID,
                                                                Prioirty = t.Priority.Name,
                                                                RowColor = t.Priority.Color,
                                                                TaskTLog = t.TaskTLogs,
                                                                StartDate = t.StartDate.Value,
                                                                EndDate = t.EndDate.Value,
                                                                EmpID = t.EmpID.Value,
                                                                CreatedBy = t.Createdby.Value,
                                                                ActualTime = t.ActualTime.Value,
                                                                // Sum Worked Hours .
                                                                WorkedHours = t.TaskTLogs.Where(
                                                                    i =>
                                                                    i.EmpID == t.EmpID
                                                                    &&
                                                                    i.TaskID == t.TaskID
                                                                    &&
                                                                    i.StatusID ==
                                                                    (int) TaskStatus.Inprogress)
                                                                               .Sum(i => i.TimeCount),

                                                                // Get Finish Date .
                                                                FinishDate = t.DeliverDate,
                                                             
                                                                /*t.TaskTLog.FirstOrDefault(
                                                                    i =>
                                                                    i.EmpID == t.EmpID
                                                                    &&
                                                                    i.TaskID == t.TaskID
                                                                    &&
                                                                    i.StatusID ==
                                                                    (int) TaskStatus.Done).CreatedDate*/
                                                            }).OrderByDescending(i => i.TaskID).Take(500).ToList(); // Bug #36 — Option B safety cap

            //To Change From Greg. Date To Hijri Date .
            // Arabic Finish Date .
            objTasks.ForEach(
                i =>
                i.ArabicFinishDate = (i.FinishDate.HasValue) ? (MvcApplication.IsGregDate) ? i.FinishDate.Value.ToGregArabicDate() : i.FinishDate.Value.ToHijriArabicDate() : String.Empty);

            // Arabic Start Date .                     
            objTasks.ForEach(
                i => i.HijriStartDate = (i.StartDate.HasValue) ? (MvcApplication.IsGregDate) ? i.StartDate.Value.ToGregArabicDate() : i.StartDate.Value.ToHijriArabicDate() : String.Empty);

            // Arabic End Date .                     
            objTasks.ForEach(
                i =>
                i.HijriEndDate = (i.EndDate.HasValue) ?(MvcApplication.IsGregDate)? i.EndDate.Value.ToGregArabicDate():i.EndDate.Value.ToHijriArabicDate() : String.Empty);

            // Batch load task entities for isDelayed, DelayPercentage, delayTime (avoids N+1 per task)
            var taskIds = objTasks.Select(t => t.TaskID).ToList();
            var taskEntities = _unitOfWork.TaskRepository.Get(t => taskIds.Contains(t.TaskID)).ToList();
            var delayedIds = new HashSet<int>(taskEntities.Where(t => t.isDelayed).Select(t => t.TaskID));
            var delayDict = taskEntities.ToDictionary(t => t.TaskID, t => new { t.delayTime, t.DelayPercentage });

            objTasks.ForEach(t => {
                t.IsDelayed = delayedIds.Contains(t.TaskID);
                t.Progressbar = delayDict.ContainsKey(t.TaskID) ? delayDict[t.TaskID].DelayPercentage : 0;
                t.Delay = delayDict.ContainsKey(t.TaskID) ? delayDict[t.TaskID].delayTime : null;
            });

            // To Set Employee Name .
            objTasks.ForEach(t => t.EmpName = t.EmpID ==null
                                                   ?  "غير مسنده":(CompanyEmployee.Count(i => i.id == t.EmpID) > 0 ? CompanyEmployee.FirstOrDefault(i => i.id == t.EmpID).name: "-"));

           // objTasks.ForEach(t => t.SpendtimebyDay = TaskManger.SpendTimeByDay(t.WorkedHours));
            objTasks.ForEach(t => t.SpendtimebyDay = TaskManger.SpendTimeByDay(t.ActualTime));

            objTasks.ForEach(t => t.IsEditable = TaskManger.CanEditTask(t.TaskID)); // TODO: batch candidate — CanEditTask source unknown
            // To Get Only Delayed Task .
            if (iStatus == (int)TaskStatus.Delay)
                objTasks = objTasks.Where(i => i.IsDelayed).ToList();

            return objTasks;
        }

        //public string SpendTimeByDay(decimal? ActualTime)
        //{
        //    if (ActualTime != null)
        //    {
        //        string del = "";
        //        string strReturn = "";
        //        if (ActualTime.Value > 24)
        //        {
        //            var Day = Math.Round((ActualTime.Value / 24), 2);
        //            del = Day.ToString() + " يوم";
        //            if (Day != 0)
        //            {
        //                strReturn = del.Split('.')[0] + " يوم";
        //            }
        //            else
        //            {
        //                strReturn = del;
        //            }
        //            var hours = (ActualTime.Value % 24);
        //            strReturn += hours + " ساعة";
        //        }
        //        else
        //        {
        //            var math = Math.Round(ActualTime.Value, 2);
        //            del = math.ToString() + " ساعة";
        //            if (math != 0)
        //            {
        //                strReturn = del.Split('.')[0] + " ساعة";
        //            }
        //            else
        //            {
        //                strReturn = del;
        //            }
        //        }
        //        return strReturn;
        //    }
        //    else
        //        return "0";
        //}

        public bool isTaskDelayed(TaskCommonVM task)
        {
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).isDelayed;
        }
        public string Delaytime(TaskCommonVM task)
        {
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).delayTime;
        }

        /// <summary>
        /// To Calculate progressbar for New and InProgress Tasks 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public double ProgressbarPercentage(TaskCommonVM task)
        {
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).DelayPercentage;
        }
        /// <summary>
        /// Delete Task .
        /// </summary>
        /// <param name="iTaskID"> Task ID </param>
        /// <returns> Boolean </returns>
        public Boolean Delete(int iTaskID)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);

            // Check If Task Is Exists Or Is Deleted .
            if (objTask != null)
            {
                if (objTask.IsDeleted)
                    return false;
                else
                {
                    // Check If Task Is New And Not Assigned To Employee .
                    if (objTask.StatusID == (int) TaskStatus.New)
                    {
                        // Check If Task Is Not Assigned To Employee .
                        if (objTask.EmpID.HasValue)
                        {
                            // Get All Attachments of this Task .
                            var objAttachments = objTask.Attachments.ToList();

                            // Get All Comments of this Task .
                            var objComments = objTask.TaskComments.ToList();

                            // Get All Logs of this Task .
                            var objlogs = objTask.TaskLogs.ToList();

                            // Delete All Attachments of this Task .
                            foreach (var objAttachment in objAttachments)
                            {
                                _unitOfWork.AttachmentRepository.Delete(objAttachment.AttachmentID);

                                // To Delete From Server .
                                if (System.IO.File.Exists(
                                    HttpContext.Current.Server.MapPath("/Upload/Task/" + objAttachment.FileName)))

                                    System.IO.File.Delete(
                                        HttpContext.Current.Server.MapPath("/Upload/Task/" + objAttachment.FileName));
                            }

                            // Delete All Comments of this Task .
                            foreach (var objComment in objComments)
                                _unitOfWork.TaskCommentRepository.Delete(objComment.TaskCommentID);

                            // Delete All Logs of this Task .
                            foreach (var objLog in objlogs)
                               // _unitOfWork.TaskLogRepository.Delete(objLog.TaskLogID);

                            AppCode.LogTask.Log(objTask, null, true);

                            objTask.IsDeleted = true;
                            _unitOfWork.TaskRepository.Update(objTask);
                          //  _unitOfWork.TaskRepository.Delete(objTask.TaskID);

                            _unitOfWork.Save();

                            return true;
                        }
                        else // Check If Task Is Assigned To Employee .
                        {
                            AppCode.LogTask.Log(objTask, null, true);
                            objTask.IsDeleted = true;
                            _unitOfWork.Save();
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        #endregion
    }
}
