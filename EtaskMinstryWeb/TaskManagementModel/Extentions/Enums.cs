using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagementModel.Extentions
{
    /// <summary>
    /// Definf Possible Task Status 
    /// </summary>
    public enum TaskStatus
    {
        All = 0,
        New = 1,//جديدة"New" ->when Company Create Task or Reassign Task to New Employee or when employee accept task but start date greater than the time of now
        Inprogress = 2, //جارى العمل"Inprogress"-> When Employee Accept Task and Its StartTIme start  it turen to InProgress automtrically 
        Pending = 3,//معلقة"Pending"->When a Company PendTask mean FREEZE
        ReOpen = 4,//اعاده فتح"Reopen"->when a company reOpen a task after employee finish it to the Same employee
        Done = 5,// انتهت"Done"->when an Employee Finish Task
        Approved = 6, //معتمدة"Approved"->when a Company Evaluate its Employees Finished Tasks and Aprrove Task  
        NotAproved = 7, //غيرمعتمدة"NotAproved"->when a Company Evaluate its Employees Finished Tasks and Reject Task  
        Accepted = 8, //مقبولة"Accepted" -> when an Employee Accept a new or Reopen Tasks
        Rejected = 9, //مرفوضة"Rejected" -> when an Employee Reject New or Reopen Tasks
        Delay = 10
    }
    public enum TimeUnitenum
    {
        Month = 1,
        Week = 2,
        Day = 3,
        Hour = 4,
        Minute = 5
    }


}
