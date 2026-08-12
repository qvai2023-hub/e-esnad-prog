using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using EtaskMinstry;
using EtaskMinstry.App_Code;
using EtaskMinstry.Models;
using EtaskMinstry.Models.EmployeeTask;
using TaskManagementModel;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Models.Company
{
    public class CompanyTaskVM
    {
        #region"Properties"

        public int CurrentStatus { get; set; }

        [ScaffoldColumn(false)]
        public int TaskID { get; set; }

        [LocalizedDisplayName("Project", NameResourceType = typeof (Resources.Company.CompanyTask))]
        public String Project { get; set; }

        [LocalizedDisplayName("Task", NameResourceType = typeof (Resources.Company.CompanyTask))]
        public String Task { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof (Resources.EmoloyeeTask.EmployeeTask))]
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
        public String RowColor { get; set; }

        public Boolean IsDelayed { get; set; }
        public Boolean IsDeleted { get; set; }

        public ICollection<TaskTLog> TaskTLog { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public String HijriStartDate { get; set; }

        public String HijriEndDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public double Progressbar { get; set; }

        public string Delay { get; set; }

        #endregion

        #region"Manage"

        private UnitOfWork _unitOfWork;

        public CompanyTaskVM()
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
        // Bug #36 — backwards-compat wrapper.
        // Existing callers (e.g. TaskController.FillDropDownLists ViewBag.Tasks dropdown) keep working.
        // Option B safety cap of 500 rows is applied via SelectPaged so a Company with thousands of tasks
        // can no longer freeze the page (was 80s+ before this fix).
        public List<CompanyTaskVM> Select(String strTitle, String FromStartDate, String ToStartDate, String FromEndDate,
                                          String ToEndDate, int iStatus, int? iEmpolyee, Boolean? bIsArchived,
                                          Boolean? bIsNotAssigned, int iProjectID, int PriorityID)
        {
            return SelectPaged(strTitle, FromStartDate, ToStartDate, FromEndDate, ToEndDate, iStatus,
                               iEmpolyee, bIsArchived, bIsNotAssigned, iProjectID, PriorityID,
                               page: 1, pageSize: 500).Items;
        }

        /// <summary>
        /// Bug #36 — server-side paginated version of Select.
        /// Used by /Company/Company/Index and the AJAX GetTasks endpoints to load only one page (10 rows)
        /// from the database instead of every task. Returns total count for the pager UI.
        /// Delay tab is paginated in C# after the isDelayed post-filter (capped at 500 rows for safety).
        /// </summary>
        public PagedResult<CompanyTaskVM> SelectPaged(String strTitle, String FromStartDate, String ToStartDate, String FromEndDate,
                                          String ToEndDate, int iStatus, int? iEmpolyee, Boolean? bIsArchived,
                                          Boolean? bIsNotAssigned, int iProjectID, int PriorityID,
                                          int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Get Company Employees .
            List<EmployeeProfile> CompanyEmployee =EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployee (MvcApplication.userData.userId);

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

            // Bug #36 — build the FILTER as an IQueryable<Task> (NOT projected, NOT materialized).
            // Counting and paginating at the entity level is much more EF-friendly than
            // counting a complex projection with navigation properties / .Value calls.
            // We project to CompanyTaskVM in-memory AFTER paging, on at most pageSize rows.
            var filteredQuery = _unitOfWork.TaskRepository.Get(includeProperties: "Project,Status,Priority,TaskTLogs", filter: t => (iEmpolyee == 0 || t.EmpID == iEmpolyee)
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
                                                        t.PriorityID == PriorityID));

            // Helper for in-memory projection (runs on at most pageSize entities — fast)
            Func<TaskManagementModel.Task, CompanyTaskVM> projectToVM = t => new CompanyTaskVM()
            {
                TaskID = t.TaskID,
                Task = t.Title,
                Project = (t.ProjectID == null) ? "لا يوجد مشروع" : (t.Project != null ? t.Project.Name : ""),
                Status = t.Status != null ? t.Status.Name : "",
                DeleteClass = (t.StatusID == (int)TaskStatus.New ? "Delete-icon deleteAction" : "DisDelete-icon"),
                StatusID = t.StatusID,
                IsArchived = t.IsArchived,
                PrioirtyID = t.PriorityID,
                Prioirty = t.Priority != null ? t.Priority.Name : "",
                RowColor = t.Priority != null ? t.Priority.Color : "",
                TaskTLog = t.TaskTLogs,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                EmpID = t.EmpID,
                WorkedHours = t.TaskTLogs != null
                    ? t.TaskTLogs.Where(i => i.EmpID == t.EmpID && i.TaskID == t.TaskID && i.StatusID == (int)TaskStatus.Inprogress).Sum(i => i.TimeCount)
                    : 0,
                FinishDate = t.DeliverDate
            };

            int totalCount;
            List<CompanyTaskVM> objTasks;

            ////To Change From Greg. Date To Hijri Date .
            //// Arabic Finish Date .
            //objTasks.ForEach(
            //    i =>
            //    i.ArabicFinishDate = (i.FinishDate.HasValue) ? (MvcApplication.IsGregDate) ? i.FinishDate.Value.ToGregArabicDate() : i.FinishDate.Value.ToHijriArabicDate() : String.Empty);

            //// Arabic Start Date .                     
            //objTasks.ForEach(
            //    i => i.HijriStartDate = (i.StartDate.HasValue) ? (MvcApplication.IsGregDate) ? i.StartDate.Value.ToGregArabicDate() : i.StartDate.Value.ToHijriArabicDate() : String.Empty);

            //// Arabic End Date .                     
            //objTasks.ForEach(
            //    i =>
            //    i.HijriEndDate = (i.EndDate.HasValue) ? (MvcApplication.IsGregDate) ? i.EndDate.Value.ToGregArabicDate() : i.EndDate.Value.ToHijriArabicDate() : String.Empty);

            ////// To Set IsDelayed .
            ////objTasks.ForEach(i => i.IsDelayed = isTaskDelayed(i));

            //// To Set Employee Name .
            //objTasks.ForEach(t => t.EmpName = t.EmpID == null
            //                                       ? "غير مسنده" : (CompanyEmployee.Count(i => i.id == t.EmpID) > 0 ? CompanyEmployee.FirstOrDefault(i => i.id == t.EmpID).name : "-"));

            //// To Set Progressbar for (New && InProgress) 
            //objTasks.ForEach(t => t.Progressbar = ProgressbarPercentage(t));


            //objTasks.ForEach(t => t.Delay = Delaytime(t));

            //objTasks.ForEach(t => t.SpendtimebyDay = TaskManger.SpendTimeByDay(t.WorkedHours));

            if (iStatus != (int)TaskStatus.Delay)
            {
                // Step 1: SELECT COUNT(*) on entities — simple SQL, no projection translation needed.
                totalCount = filteredQuery.Count();

                // Step 2: SELECT TOP pageSize entities at SQL level (OFFSET/FETCH).
                var pagedEntities = filteredQuery.OrderByDescending(t => t.TaskID)
                                                 .Skip((page - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .ToList();

                // Step 3: project to VM in memory (only pageSize rows — fast, no EF gymnastics).
                objTasks = pagedEntities.Select(projectToVM).ToList();

                objTasks.ForEach(t =>
                {
                    t.ArabicFinishDate = (t.FinishDate.HasValue) ? (MvcApplication.IsGregDate) ? t.FinishDate.Value.ToGregArabicDate() : t.FinishDate.Value.ToHijriArabicDate() : String.Empty;
                    t.HijriStartDate = (t.StartDate.HasValue) ? t.StartDate.Value.ToGregArabicDate() : String.Empty;
                    t.HijriEndDate = (t.EndDate.HasValue) ? t.EndDate.Value.ToGregArabicDate() : String.Empty;
                    t.EmpName = t.EmpID == null
                                    ? "غير مسنده" : (CompanyEmployee.Count(i => i.id == t.EmpID) > 0 ? CompanyEmployee.FirstOrDefault(i => i.id == t.EmpID).name : "-");
                });
            }
            else
            {
                // Delay tab: isDelayed is computed in C# (not a DB column), so we must materialize first.
                // Cap at 500 entities for safety so we never freeze the page on companies with thousands of accepted tasks.
                var entities = filteredQuery.OrderByDescending(t => t.TaskID).Take(500).ToList();
                var allTasks = entities.Select(projectToVM).ToList();

                allTasks.ForEach(t =>
                {
                    t.ArabicFinishDate = (t.FinishDate.HasValue) ? (MvcApplication.IsGregDate) ? t.FinishDate.Value.ToGregArabicDate() : t.FinishDate.Value.ToHijriArabicDate() : String.Empty;
                    t.HijriStartDate = (t.StartDate.HasValue) ?  t.StartDate.Value.ToGregArabicDate()  : String.Empty;
                    t.HijriEndDate = (t.EndDate.HasValue)  ? t.EndDate.Value.ToGregArabicDate()  : String.Empty;
                    t.IsDelayed = isTaskDelayed(t);
                    t.EmpName = t.EmpID == null
                                    ? "غير مسنده" : (CompanyEmployee.Count(i => i.id == t.EmpID) > 0 ? CompanyEmployee.FirstOrDefault(i => i.id == t.EmpID).name : "-");
                    t.SpendtimebyDay = TaskManger.SpendTimeByDay(t.WorkedHours);
                    t.Delay = Delaytime(t);
                });

                var delayedOnly = allTasks.Where(i => i.IsDelayed).ToList();
                totalCount = delayedOnly.Count;
                objTasks = delayedOnly.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return new PagedResult<CompanyTaskVM>(objTasks, totalCount, page, pageSize);
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

        public bool isTaskDelayed(CompanyTaskVM task)
        {
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).isDelayed;
        }
        public string Delaytime(CompanyTaskVM task)
        {
            return _unitOfWork.TaskRepository.GetByID(task.TaskID).delayTime;
        }

        /// <summary>
        /// To Calculate progressbar for New and InProgress Tasks 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public double ProgressbarPercentage(CompanyTaskVM task)
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
                if (objTask.CompanyID != MvcApplication.userData.CompanyId)
                    return false;

                AppCode.LogTask.Log(objTask, null, true);
                objTask.IsDeleted = true;
                _unitOfWork.TaskRepository.Update(objTask);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }


        #endregion
    }
}
