using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace TaskManagementModel
{
     
  
    public partial class Task
    {
       
        //custom prop
        public bool isDelayed
        {
            get
            { //StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Pending

                var noha = TaskID;
                bool isDelayed = false;

                if (StatusID != (int)TaskManagementModel.Extentions.TaskStatus.New
                    && StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Rejected
                    && EmpID != null && !IsArchived)
                {
                    var pendinghours = 0.0d;

                    var tasklogs = TaskTLogs.ToList();
                    //select tasks are Done
                    var taskLog =
                        TaskTLogs.FirstOrDefault(t => t.StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Done);
                    //count task logs
                    int CountTLogs = tasklogs.Count;
                    DateTime? deliverDate = null;

                    if (taskLog != null)
                        deliverDate = taskLog.CreatedDate;

                    //if task delivered 
                    if (deliverDate != null)
                    {
                        return isDelayed = EndDate.HasValue ? EndDate.Value.Date < deliverDate.Value.Date : true;
                    }

                    for (int i = 0; i < CountTLogs; i++)
                    {
                        // if task accepted
                        if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Accepted)
                        {

                            //return isDelayed = EndDate.HasValue ? EndDate.Value.Date >= DateTime.Now.Date : true;

                            if ((EndDate.HasValue ? EndDate.Value.Date : DateTime.Now.Date) >= DateTime.Now.Date)
                            {
                                return isDelayed = false;
                            }
                            else
                            {
                                return isDelayed = true;
                            }


                        } // if task accepted

                            //if task not approved 
                        else if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.NotAproved)
                        {
                            if (deliverDate != null)
                            {
                                return isDelayed = EndDate.HasValue ? EndDate.Value.Date >= deliverDate.Value.Date : true;
                            }
                        }
                        //if task approved 
                        else if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Approved)
                        {
                            if (deliverDate != null)
                            {
                                return isDelayed = EndDate.HasValue ? EndDate.Value.Date >= deliverDate.Value.Date : true;
                            }
                        }
                        //if task not Inprogress 
                        else if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Inprogress)
                        {
                            if ((EndDate.HasValue ? EndDate.Value.Date : DateTime.Now.Date) >= DateTime.Now.Date)
                            {
                                return isDelayed = false;
                            }
                            else
                            {
                                return isDelayed = true;
                            }
                        }
                        //if task pending task
                        else if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Pending)
                        {
                            if ((EndDate.HasValue ? EndDate.Value.Date : DateTime.Now.Date) >= DateTime.Now.Date)
                            {
                                return isDelayed = false;
                            }
                            else
                            {
                                return isDelayed = true;
                            }
                        }


                    }//for loop


                } //if not archived
                else
                {
                    //return false;
                    isDelayed = false;
                }

                return isDelayed;
            }
            set { }

        }

        public string delayTime
        {
            get
            {//StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Pending &&
                if (!IsArchived && isDelayed)
                {
                    var pendinghours = 0.0d;
                    var tasklogs = TaskTLogs.ToList();
                    var taskLog = TaskTLogs.FirstOrDefault(t => t.StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Done);
                    DateTime? deliverDate = null;

                    if (taskLog != null)
                        deliverDate = taskLog.CreatedDate;

                    for (int i = 0; i < tasklogs.Count; i++)
                    {   //if Pending
                        if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Pending)
                        {
                            if (i == tasklogs.Count - 1) //if the last status was pending then calculate the time from now
                            {
                                pendinghours += (DateTime.Now - tasklogs[i].CreatedDate).TotalHours;
                            }
                            else //else calculate the time between the 2 statuses
                            {
                                pendinghours += (tasklogs[i + 1].CreatedDate - tasklogs[i].CreatedDate).TotalHours;
                            }
                        }
                    }

                    if (deliverDate != null)
                    {
                        //return EndDate == null ? "0" : (EndDate.Value.AddHours(pendinghours) - deliverDate.Value).TotalDays >= 1
                        // ? String.Format("{0:0.00}", (EndDate.Value.AddHours(pendinghours) - deliverDate.Value).TotalDays) + " يوم"
                        // : String.Format("{0:0.00}", (EndDate.Value.AddHours(pendinghours) - deliverDate.Value).TotalDays) + " ساعة";
                        double math = 0;
                        string strReturn = "";

                        if (EndDate != null)
                        {
                            string del = "";
                            if ((deliverDate.Value - EndDate.Value).TotalHours > 24)
                            {
                                math = Math.Round(((deliverDate.Value - EndDate.Value).TotalHours / 24), 2);
                                del = math.ToString() + " يوم";
                                if (math != 0)
                                {
                                    strReturn = del.Split('.')[0] + " يوم";
                                }
                                else
                                {
                                    strReturn = del;
                                }
                            }
                            else
                            {
                                math = Math.Round(((deliverDate.Value - EndDate.Value).TotalHours), 2);

                                del = math.ToString() + " ساعة";
                                if (math != 0)
                                {
                                    strReturn = del.Split('.')[0] + " ساعة";
                                }
                                else
                                {
                                    strReturn = del;
                                }
                            }


                            return strReturn;

                        }
                        else
                            return ("0").ToString();
                    }// if delivery date
                    else
                    {
                        if (EndDate != null)
                        {
                            string del = "";
                            if ((DateTime.Now - EndDate.Value).TotalHours > 24)
                                  del = ((DateTime.Now - EndDate.Value).Days).ToString() + " يوم";
                              //  del = (Math.Round(((DateTime.Now - EndDate.Value).TotalHours / 24)) - 1).ToString() + " يوم";
                            else
                                del = Math.Round(((DateTime.Now - EndDate.Value).TotalHours), 2).ToString() + " ساعة";
                            return del;
                        }
                        else
                            return ("0").ToString();

                        //    double doubleVal = 0.0;
                        //    if (ExpectedTime.HasValue)
                        //    {
                        //         doubleVal = System.Convert.ToDouble(ExpectedTime.Value);
                        //    }
                        //    var endExpectedTime = StartDate.Value.AddMinutes(doubleVal);
                        //    var endDate = (EndDate.Value.AddHours(pendinghours) - DateTime.Now).TotalDays;
                        //    var retValue = "";
                        //    if (EndDate == null)
                        //    {
                        //        return "0";
                        //    }
                        //    else if (endDate >= 1)
                        //    {
                        //        retValue =
                        //            String.Format("{0:0.00}",
                        //                          (EndDate.Value.AddHours(pendinghours) - DateTime.Now).TotalDays) + " يوم";
                        //    }
                        //    else
                        //    {
                        //        if (EndDate == StartDate)
                        //        {


                        //            if (endExpectedTime < DateTime.Now)
                        //            {
                        //                EndDate = endExpectedTime;
                        //                retValue = String.Format("{0:0.00}",
                        //                                         (EndDate.Value.AddMinutes(pendinghours) - DateTime.Now)
                        //                                             .TotalDays) + " دقائق";
                        //            }
                        //        }
                        //        else
                        //        {

                        //            retValue = String.Format("{0:0.00}",
                        //                                     (EndDate.Value.AddHours(pendinghours) - DateTime.Now)
                        //                                         .TotalDays) + " ساعة";
                        //        }
                        //        return retValue;
                        //    }
                        //    //return EndDate == null
                        //    //           ? "0"
                        //    //           : (EndDate.Value.AddHours(pendinghours) - DateTime.Now).TotalDays >= 1
                        //    //                  ? String.Format("{0:0.00}",
                        //    //                                  (EndDate.Value.AddHours(pendinghours) - DateTime.Now)
                        //    //                                      .TotalDays) + " يوم"
                        //    //                  : String.Format("{0:0.00}",
                        //    //                                  (EndDate.Value.AddHours(pendinghours) - DateTime.Now)
                        //    //                                      .TotalDays) + " ساعة";
                        //    return retValue;
                    }
                }
                else
                    return "0";
            }
            set { }

        }

        public double DelayPercentage
        {
            get
            {

                if (EndDate.HasValue && StartDate.HasValue)
                {
                    TimeSpan Left = (EndDate.Value.Date - DateTime.Now.Date);
                    TimeSpan Total = (EndDate.Value.Date - StartDate.Value.Date);
                    if (EndDate.Value.Date >= DateTime.Now.Date && Total >= Left)
                    {
                        double result = Convert.ToDouble(Total.Days + 1) - Convert.ToDouble(Left.Days);
                        return 100 * (result / Convert.ToDouble(Total.Days + 1));

                    }
                    else if (StartDate > DateTime.Now)
                    {
                        return 0;
                    }
                    else
                    {
                        return 100;
                    }
                }
                else
                {
                    return 0;
                }




            }
            set { }
        }


        /// <summary>
        /// To get sum time log spent in specific task per day
        /// </summary>
        public Dictionary<string, TaskTimeDetails> GetTaskTimeLog
        {
            get
            {
                var taskID = TaskID;
                //get all employees that work in this task
                var allEmp = TaskTLogs.GroupBy(e => e.EmpID);
                // get all Created Date of this task
                var allDates = TaskTLogs.GroupBy(d => d.CreatedDate.Date);

                // every employee with his time log for this task
                var empsTaskTimeLog = new List<TaskTimeDetails>();
                foreach (var itemDate in allDates)
                {
                    //group by employee of this date
                    var lstEmp = itemDate.GroupBy(e => e.EmpID);
                    //loop on emplyees to get last date with Time Count that this employee added
                    foreach (var itemEmp in lstEmp)
                    {
                        //get last date with time count added by this employee
                        var lastDate = itemEmp.LastOrDefault(t => t.TimeCount != null);
                        if (lastDate != null)
                        {
                            //add this object (lastDate) to empsTaskTimeLog type of(TaskTimeDetails)
                            empsTaskTimeLog.Add(new TaskTimeDetails()
                            {
                                LogTime = ((decimal)lastDate.TimeCount).ToString(),
                                isToday = lastDate.CreatedDate.Date == DateTime.Today.Date,
                                LogDate = lastDate.CreatedDate.Date.ToString(),
                                empId = lastDate.EmpID.Value
                            });
                        }
                    }
                }

                Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();

                var groupByLogDate = empsTaskTimeLog.GroupBy(s => s.LogDate).ToList();

                foreach (var itemLogDate in groupByLogDate)
                {
                    decimal totalTimeCountSameDay = 0;
                    //get simillar dates to count logtime of them
                    var lstSimillarDates = empsTaskTimeLog.FindAll(a => a.LogDate == itemLogDate.Key);

                    //count logtime for the same logdate
                    foreach (var date in lstSimillarDates)
                    {
                        totalTimeCountSameDay += decimal.Parse(date.LogTime);
                    }
                    if (!uniqueTimeLog.ContainsKey(itemLogDate.Key))
                        uniqueTimeLog.Add(itemLogDate.Key, new TaskTimeDetails()
                        {
                            LogTime = totalTimeCountSameDay == 0 ? itemLogDate.ToList()[0].LogTime : totalTimeCountSameDay.ToString(),
                            isToday = itemLogDate.ToList()[0].isToday,
                            LogDate = (int.Parse(System.Configuration.ConfigurationManager.AppSettings["ISGreg"].ToString()) == 1) ? itemLogDate.Key.ToGregArabicDate() : itemLogDate.Key.ToHijriArabicDate()
                        });
                }
                return uniqueTimeLog;

            }
            set { }

        }

    }
    public class TaskTimeDetails
    {
        public string LogDate;
        public string LogTime;
        public bool isToday;
        public int empId;
    }

    public static class ExtentionDateFormat
    { 
        public static string ToHijriArabicDate(this string sdt)
        {
            sdt = sdt.Split(' ')[0];
            CultureInfo higri_format = new CultureInfo("ar-SA");
            //DateTime dt = DateTime.ParseExact(sdt, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            try
            {
                DateTime dt = Convert.ToDateTime(sdt);
                //  dt.ToUniversalTime();
                higri_format.DateTimeFormat.Calendar = new UmAlQuraCalendar();
                return dt.ToString("ddd d MMM   yyyy", higri_format);
            }
            catch
            {
                return "";
            }
        }

        public static string ToGregArabicDate(this string stdate)
        {
            stdate = stdate.Split(' ')[0];
            CultureInfo higri_format = new CultureInfo("ar-SA");
            //DateTime dt = DateTime.ParseExact(sdt, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            try
            {
                DateTime dt = Convert.ToDateTime(stdate);
                higri_format.DateTimeFormat.Calendar = new GregorianCalendar();
                return dt.ToString("ddd d MMM   yyyy", higri_format);
            }
            catch
            {
                return "";
            }
        }

    }

}

   





