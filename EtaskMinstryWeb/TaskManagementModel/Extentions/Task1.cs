using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagementModel.Extentions;
namespace TaskManagementModel
{
    public partial class Task
    {
        //custom prop


        //public bool isDelayed
        //{
        //    get
        //    { //StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Pending

        //        var noha = TaskID;
        //        bool isDelayed = false;

        //        if (StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Approved &&
        //            StatusID != (int)TaskManagementModel.Extentions.TaskStatus.NotAproved && !IsArchived)
        //        {
        //            var pendinghours = 0.0d;

        //            var tasklogs = TaskTLogs.ToList();
        //            var taskLog =
        //                TaskTLogs.FirstOrDefault(t => t.StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Done);
        //            int CountTLogs = tasklogs.Count;
        //            DateTime? deliverDate = null;

        //            if (taskLog != null)
        //                deliverDate = taskLog.CreatedDate;
        //            //if num of Tasklogs == 1 
        //            //if (CountTLogs == 1)
        //            //{
        //            //    CountTLogs = CountTLogs;
        //            //}
        //            //else
        //            //{
        //            //    CountTLogs = CountTLogs - 1;
        //            //}
        //            for (int i = 0; i < CountTLogs; i++)
        //            {
        //                //if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Pending)
        //                //{

        //                //    //pendinghours += (tasklogs[i + 1].CreatedDate - tasklogs[i].CreatedDate).TotalHours;
        //                //}
        //                //else 
        //                if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Accepted)
        //                {

        //                    if (ExpectedTime != null)
        //                    {
        //                        double doubleVal = System.Convert.ToDouble(ExpectedTime.Value);
        //                        var endExpectedTime = new DateTime();
        //                        var createDate = new DateTime();
        //                        if (ExpectedTime != 0)
        //                        {
        //                            // if i accept task before its start Date
        //                            if (tasklogs[i].Task.StartDate.HasValue)
        //                            {
        //                                if (tasklogs[i].Task.StartDate.Value.Date >= DateTime.Now.Date)
        //                                {
        //                                    createDate = tasklogs[i].Task.StartDate.Value;
        //                                }
        //                                else if (tasklogs[i].Task.EndDate.Value.Date <= tasklogs[i].CreatedDate)
        //                                {
        //                                    createDate = tasklogs[i].Task.StartDate.Value;
        //                                }
        //                                else
        //                                {
        //                                    createDate = tasklogs[i].CreatedDate;

        //                                }
        //                            }
        //                            switch (TimeUnitID)
        //                            {
        //                                // Convert iTotalDays To Months .
        //                                case (int)TimeUnitenum.Month:
        //                                    endExpectedTime = createDate.AddMonths((int)doubleVal);
        //                                    //endExpectedTime = tasklogs[i].CreatedDate.AddMonths((int)doubleVal);

        //                                    break;

        //                                // Convert iTotalDays To Weeks .
        //                                case (int)TimeUnitenum.Week:
        //                                    endExpectedTime = createDate.AddDays(doubleVal * (double)7);
        //                                    //endExpectedTime = tasklogs[i].CreatedDate.AddDays(doubleVal * (double)7);

        //                                    break;

        //                                case (int)TimeUnitenum.Day:
        //                                    if (doubleVal == 1.0)
        //                                    {
        //                                        endExpectedTime = createDate.AddHours(8);
        //                                    }
        //                                    else
        //                                    {
        //                                        endExpectedTime = createDate.AddDays(doubleVal);
        //                                    }
        //                                    //endExpectedTime = tasklogs[i].CreatedDate.AddDays(doubleVal);

        //                                    break;

        //                                // Convert iTotalDays To Hours .
        //                                case (int)TimeUnitenum.Hour:
        //                                    endExpectedTime = createDate.AddHours(doubleVal);
        //                                    //endExpectedTime = tasklogs[i].CreatedDate.AddHours(doubleVal);

        //                                    break;


        //                                case (int)TimeUnitenum.Minute:
        //                                    endExpectedTime = createDate.AddMinutes(doubleVal);
        //                                    //endExpectedTime = tasklogs[i].CreatedDate.AddMinutes(doubleVal);

        //                                    break;
        //                            }


        //                        }
        //                        else
        //                        {
        //                            endExpectedTime = EndDate.HasValue ? EndDate.Value : DateTime.Now;
        //                        }

        //                        // pendinghours += ( tasklogs[i].CreatedDate - endExpectedTime).TotalMinutes;

        //                        if (tasklogs[tasklogs.Count - 1].StatusID ==
        //                            (int)TaskManagementModel.Extentions.TaskStatus.Pending
        //                            && tasklogs[tasklogs.Count - 1].CreatedDate <= endExpectedTime)
        //                        {
        //                            // return false;
        //                            return isDelayed = false;

        //                        }
        //                        else
        //                        {

        //                            if (endExpectedTime.Date < DateTime.Now.Date)
        //                            {
        //                                //if task delivered 
        //                                if (deliverDate != null)
        //                                {
        //                                    // return EndDate == null? false: EndDate.Value.AddHours(pendinghours) < deliverDate;
        //                                    //   return
        //                                    return isDelayed = EndDate == null ? false : endExpectedTime < deliverDate;
        //                                }

        //                                else
        //                                {
        //                                    //return EndDate == null? false: EndDate.Value.AddHours(pendinghours).Date < DateTime.Now.Date;
        //                                    //return EndDate == null ? false : endExpectedTime.Date < DateTime.Now.Date;
        //                                    return isDelayed = EndDate == null ? false : endExpectedTime < DateTime.Now;
        //                                }
        //                                // EndDate = EndDate.Value.AddMinutes(pendinghours).Date;
        //                            }
        //                            else if (endExpectedTime.Date == DateTime.Now.Date)
        //                            {
        //                                //if task delivered 
        //                                if (deliverDate != null)
        //                                {
        //                                    // return EndDate == null? false: EndDate.Value.AddHours(pendinghours) < deliverDate;
        //                                    //return EndDate == null ? false : deliverDate > endExpectedTime;
        //                                    return isDelayed = EndDate == null ? false : deliverDate > endExpectedTime;
        //                                }

        //                                else
        //                                {
        //                                    //return EndDate == null? false: EndDate.Value.AddHours(pendinghours).Date < DateTime.Now.Date;
        //                                    //  return EndDate == null ? false : endExpectedTime.Date < DateTime.Now.Date;
        //                                    return isDelayed = EndDate == null ? false : endExpectedTime.Date < DateTime.Now.Date;

        //                                }
        //                            }
        //                            else if (endExpectedTime.Date > DateTime.Now.Date)
        //                            {
        //                                return isDelayed = false;
        //                            }
        //                            else
        //                                return isDelayed = true;
        //                        }
        //                    }
        //                } // if task accepted


        //                else if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.New
        //                    && (EndDate.HasValue ? EndDate.Value : DateTime.Now.Date) >= DateTime.Now.Date)
        //                {
        //                    isDelayed = false;
        //                    // return false;
        //                }
        //                else if (tasklogs[i].StatusID == (int)TaskManagementModel.Extentions.TaskStatus.Pending
        //               && (EndDate.HasValue ? EndDate.Value : DateTime.Now.Date) >= DateTime.Now.Date)
        //                {
        //                    isDelayed = false;
        //                    // return false;
        //                }
        //                else
        //                {
        //                    isDelayed = true;
        //                }

        //            }//for loop

        //            //if (deliverDate != null)
        //            //    return EndDate == null ? false : EndDate.Value.AddHours(pendinghours) < deliverDate;

        //            //else
        //            //    return EndDate == null ? false : EndDate.Value.AddHours(pendinghours).Date < DateTime.Now.Date;
        //            //return true;
        //            // isDelayed = true;
        //        }
        //        else
        //        {

        //            //return false;
        //            isDelayed = false;
        //        }

        //        return isDelayed;
        //    }
        //    set { }

        //}

        public string delayTime2
        {
            get
            {//StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Pending &&
                if ( StatusID != (int)TaskManagementModel.Extentions.TaskStatus.Approved && !IsArchived && isDelayed)
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
                                pendinghours += (DateTime.Now- tasklogs[i].CreatedDate).TotalHours;
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
                            if ((deliverDate.Value.AddHours(pendinghours) - EndDate.Value).TotalHours > 24)
                            {
                                math =   Math.Round(
                                        ((deliverDate.Value - EndDate.Value.AddHours(pendinghours)).TotalHours/24), 2);
                                del =  math.ToString() + " يوم";
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
                                math = Math.Round(((deliverDate.Value - EndDate.Value.AddHours(pendinghours)).TotalHours),2);
                                
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
                    }
                    else
                    {
                        if (EndDate != null)
                        {
                            string del = "";
                            if ((DateTime.Now.AddHours(pendinghours) - EndDate.Value).TotalHours > 24)
                                del = Math.Round(((DateTime.Now - EndDate.Value.AddHours(pendinghours)).TotalHours / 24),2).ToString() + " يوم";
                            else
                                del = Math.Round(((DateTime.Now - EndDate.Value.AddHours(pendinghours)).TotalHours), 2).ToString() + " ساعة";
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

        public double DelayPercentage2
        {
            get {

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
        
    }
}
