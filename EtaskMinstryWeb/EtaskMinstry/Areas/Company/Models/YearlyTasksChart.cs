using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using EtaskMinstry.Models.Company;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Company.Models
{
    public class YearlyTasksChart
    {
        public String CreatedTasks { get; set; }

        public String RefusedTasks { get; set; }

        public String ApprovedTasks { get; set; }

        public String DoneTasks { get; set; }

        public int TotalTasks { get; set; }

        public SelectList Years { get; set; }

        private UnitOfWork _unitOfWork;

        public YearlyTasksChart()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            List<int> lstYears = new List<int>();

            for (int i = DateTime.Now.GetHijriYear(); i > DateTime.Now.GetHijriYear() - 10; i--)
                lstYears.Add(i);

            Years = new SelectList(lstYears);
        }

        /// <summary>
        /// To Get Count each Task type By Year .
        /// </summary>
        /// <param name="iYear"></param>
        /// <param name="iEmpID"></param>
        /// <param name="iMonth">Hijri month (1-12), 0 = all months</param>
        public void GetTasks(int iYear, int iEmpID, int iMonth = 0)
        {
            if (iYear == 0)
                iYear = DateTime.Now.GetHijriYear();

            DateTime dtStartYear = ("1/1/" + iYear).ToGregExact();

            DateTime dtEndYear = dtStartYear.AddYears(1);

            // Get All Tasks By Company .
            var lstTasks = _unitOfWork.TaskRepository.Get(i => i.CompanyID == MvcApplication.userData.userId
                                                               &&
                                                               !i.IsDeleted
                                                               &&
                                                               i.CreatedDate >= dtStartYear
                                                               &&
                                                               i.CreatedDate <= dtEndYear
                                                                && (iEmpID == 0 || i.EmpID == iEmpID)).ToList();
            // Get All TaskTLogs By Company .
            var lstTaskTLogs = _unitOfWork.TaskStatuseLog.Get(i => i.Task.CompanyID == MvcApplication.userData.userId
                                                                   &&
                                                                   !i.Task.IsDeleted
                                                                   &&
                                                                   i.CreatedDate >= dtStartYear
                                                                   &&
                                                                   i.CreatedDate <= dtEndYear
                                                                    && (iEmpID == 0 || i.EmpID == iEmpID)
                                                                   ).ToList();
            // Loop For 12 Months .
            for (int i = 1; i <= 12; i++)
            {
                DateTime dtStartMonth = ("1/" + i + "/" + iYear).ToGregExact();

                // If a specific month is selected, only count for that month; others get 0
                if (iMonth > 0 && i != iMonth)
                {
                    CreatedTasks += "0,";
                    ApprovedTasks += "0,";
                    RefusedTasks += "0,";
                    DoneTasks += "0,";
                    continue;
                }

                CreatedTasks += lstTasks.Count(
                    t => t.CreatedDate >= dtStartMonth && t.CreatedDate <= dtStartMonth.AddMonths(1)) + ",";

                //Approved tasks by create date of change status to be approved
                ApprovedTasks += lstTaskTLogs.Count(
                    t => t.CreatedDate >= dtStartMonth && t.CreatedDate <= dtStartMonth.AddMonths(1) &&
                         t.StatusID == (int)TaskStatus.Approved) + ",";

                RefusedTasks += lstTaskTLogs.Count(
                    t => t.CreatedDate >= dtStartMonth && t.CreatedDate <= dtStartMonth.AddMonths(1) &&
                         t.StatusID == (int) TaskStatus.NotAproved) + ",";

                DoneTasks += lstTaskTLogs.Count(
                    t => t.CreatedDate >= dtStartMonth && t.CreatedDate <= dtStartMonth.AddMonths(1) &&
                         t.StatusID == (int) TaskStatus.Done && !t.Task.IsDeleted) + ",";
            }
        }


        /// <summary>
        /// To Get Count all Tasks By Year .
        /// </summary>
        /// <param name="iYear"></param>
        public void GetAllTasksByYear(int iYear)
        {
            if (iYear == 0)
                iYear = DateTime.Now.GetHijriYear();

            DateTime dtStartYear = ("1/1/" + iYear).ToGregExact();

            DateTime dtEndYear = dtStartYear.AddYears(1);

            // Get All Tasks By Company .
            var lstTasks = _unitOfWork.TaskRepository.Get(i => i.CompanyID == MvcApplication.userData.userId
                                                               &&
                                                               !i.IsDeleted
                                                               &&
                                                               i.CreatedDate >= dtStartYear
                                                               &&
                                                               i.CreatedDate <= dtEndYear).ToList();
            // Get All TaskTLogs By Company .
            var lstTaskTLogs = _unitOfWork.TaskStatuseLog.Get(i => i.Task.CompanyID == MvcApplication.userData.userId
                                                                   &&
                                                                   !i.Task.IsDeleted
                                                                   &&
                                                                   i.CreatedDate >= dtStartYear
                                                                   &&
                                                                   i.CreatedDate <= dtEndYear).ToList();
            // Loop For 12 Months .
            for (int i = 1; i <= 12; i++)
            {
                DateTime dtStartMonth = ("1/" + i + "/" + iYear).ToGregExact();

                TotalTasks += lstTasks.Count(
                   t => t.CreatedDate >= dtStartMonth && t.CreatedDate <= dtStartMonth.AddMonths(1));


            }
        }
    }
}