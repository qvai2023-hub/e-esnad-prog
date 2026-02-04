using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EtaskMinstry
{
    public enum Actions
    {
        Index = 1,
        Create = 2,
        Edit = 3,
        Delete = 4,
        Login = 5
    }

    /// <summary>
    /// Hold Actions Name 
    /// </summary>
    public enum ActionType
    {
        View = 1,
        Insert = 2,
        Update = 3,
        Delete = 4,
        Login = 5,
        Admin = 6,
        MoveWorkFlow = 7,
        Report = 8,
        LogOut = 9,
        //Use In View Total Log to get Union
        TaskAnyModification = 10,
    }

    public enum ArabicActionscustomType
    {
        مشاهدة = 1,
        اضافة = 2,
        تعديل = 3,
        حذف = 4,
        دخول = 5,
        خروج = 9,
        تغيير_المهام = 10,
     
    }


    public enum UserRole
    {
        Administrator = 1,
        User = 2
    }

    public enum TaskPriority
    {
        Noraml = 1,
        Average = 2,
        High = 3,
        VeryHigh = 4
    }

    public enum TimeUnit
    {
        Month = 1,
        Week = 2,
        Day = 3,
        Hour = 4,
        Minute = 5
    }

    public enum TimeUnitArabic
    {
        شهر = 1,
        اسبوع = 2,
        يوم = 3,
        ساعة = 4,
        دقيقة = 5
    }
   
    public enum TimeUnitValue
    {
        Month = 0,     //Not exist now 
        Week = 4,     //Not exist now 
        Day = 5, 
        Hour = 24,
        Minute = 60  //Not exist now 
    }

    /// <summary>
    /// Definf Possible Task Status 
    /// </summary>
    public enum TaskStatus
    {
        All = 0,
        New = 1,//جديدة"New" ->when Company Create Task or Reassign Task to New Employee or when employee accept task but start date greater than the time of now
        Inprogress = 2, //جارى العمل"Inprogress"-> When Employee Accept Task and Its StartTIme start  it turen to InProgress automtrically 
        Pending = 3,//معلقة"Pending"->When a Company PendTask mean FREEZE
        //ReOpen = 4,//اعاده فتح"Reopen"->when a company reOpen a task after employee finish it to the Same employee
        Done = 5,// انتهت"Done"->when an Employee Finish Task
        Approved = 6, //معتمدة"Approved"->when a Company Evaluate its Employees Finished Tasks and Aprrove Task  
        NotAproved = 7, //غيرمعتمدة"NotAproved"->when a Company Evaluate its Employees Finished Tasks and Reject Task  
        Accepted = 8, //مقبولة"Accepted" -> when an Employee Accept a new or Reopen Tasks
        Rejected = 9, //مرفوضة"Rejected" -> when an Employee Reject New or Reopen Tasks
        Delay = 10
    }

    /// <summary>
    /// Define Possible Comment Status 
    /// </summary>
    public enum TaskCommentStatus
    {
        Shown = 1,	//when comment is shown it is the default status
        Hidden = 2,   //hidden when the copmany show to hide comment
        Reported = 3, //when a user report a comment
        Deleted = 4,  //when an admin delete a reported Comment
    }

    public enum TaskActions
    {
        startdate,
        enddate,
        expectedtime,
        empid,
        isarchived,
        priorityid,
        statusid
    }

    public enum MessageType
    {
        Save = 0,
        ErrorSave,
        ErrorDelete,
        Error,
        Success,
        Cancel,
        ErrorCancel,
        Delete,
        DeleteSuccess,
        NameDuplicate,
        NotFound,
        NotActivated
    }

    public enum userStatus
    {
        newStatus = 1,
        active = 2,
        notActive = 3,
        stoped = 4
    }

    public enum RecurrenceType
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearyly = 4
    }

}
    public enum LoggedUserType
    {
        Admin = 1,
        Employee = 2,
        Company = 3
    }

public enum Provider
{
    Esnad = 0,
    Telesak = 1,
}


