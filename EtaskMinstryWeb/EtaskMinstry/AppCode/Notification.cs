using EtaskMinstry;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using TaskManagementModel;
using EtaskMinstry.AppCode;
using System.Collections;
using EtaskMinstry;
using System.Threading;
using System.Web.Script.Serialization;
using EtaskMinstry.Models;

/// <summary>
/// user types simulate table usertype this same ids
/// </summary>
//public enum UserType
//{
//    Admin = 1,
//    Employee = 2,
//    Company = 3
//}

//notificationtype simulate table notificationtype
public enum NotificationType
{
    AddComment = 1,
    NewTask,
    InprogressTask,
    AcceptTask,
    RejectTask,
    EndTask,
    Approved,
    NotApproved,
    ChangePriority
}

/// <summary>
/// a class used to serilization of data would sent to the client
/// </summary>
public class NotificationClientObject
{
    public string ID { get; set; }
    public string Type { get; set; }
    public string Time { get; set; }
    public string Value { get; set; }
    public string Link { get; set; }
}


/// <summary>
///  a class pattren retrive arrtibutes we need it to show notifications
/// </summary>
public class NotificationPattren
{
    public int ID { get; set; }
    public int CollectionID { get; set; }
    public string Link { get; set; }
    public string Value { get; set; }
    public DateTime SendDate { get; set; }
    public bool IsSeen { get; set; }
    public string Type { get; set; }


}

/// <summary>
/// the base notification hub class
/// </summary>
public class NotificationHub : Hub
{
  
    /// <summary>
    /// static property of unit of work
    /// </summary>
    protected static UnitOfWork _unitOfWork { get { return new UnitOfWork(MvcApplication.ConnectionString); } }


    /// <summary>
    /// the instance of hub context enable us to send notification from server side
    /// </summary>
    private static IHubContext HubContext { get { return GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); } }


    /// <summary>
    /// get notification
    /// </summary>
    /// <param name="Skip">skip value</param>
    /// <param name="Take">take value</param>
    /// <returns>list of NotificationPattrens</returns>
    public static List<NotificationPattren> Get(int Skip, int Take)
    {
        if (MvcApplication.userData == null) return new List<NotificationPattren>();
        var usertype = MvcApplication.userData.isCompany ? LoggedUserType.Company : LoggedUserType.Employee;
        var unitOfWork = new UnitOfWork(MvcApplication.ConnectionString);
      


        return unitOfWork.NotificationCollections
            .Get(filter: s => s.InstanceID  == MvcApplication.userData.userId && s.UserTypeID == (int)usertype)
            .OrderByDescending(s=>s.ID).Skip(Skip).Take(Take)
            .Select(i => new NotificationPattren()
             {
                  Link = i.Notification.LinkToGo,
                  CollectionID = i.ID,
                  ID = i.Notification.ID,
                  SendDate = i.Notification.CreateDate,
                  IsSeen = i.IsSeen,
                  Value = i.Notification.Value.Substring(0, 200),
                  Type = i.Notification.NotificationType.Name,
              }).ToList();
    }

    /// <summary>
    /// get notification
    /// </summary>
    /// <param name="Skip">skip value</param>
    /// <param name="Take">take value</param>
    /// <returns>list of NotificationPattrens</returns>
    public static List<NotificationPattren> GetAll()
    {
        if (MvcApplication.userData == null) return new List<NotificationPattren>();
        var usertype = MvcApplication.userData.isCompany ? LoggedUserType.Company : LoggedUserType.Employee;
        var unitOfWork = new UnitOfWork(MvcApplication.ConnectionString);



        return unitOfWork.NotificationCollections
            .Get(filter: s => s.InstanceID == MvcApplication.userData.userId && s.UserTypeID == (int)usertype)
            .OrderByDescending(s => s.ID)
            .Select(i => new NotificationPattren()
            {
                Link = i.Notification.LinkToGo,
                CollectionID = i.ID,
                ID = i.Notification.ID,
                SendDate = i.Notification.CreateDate,
                IsSeen = i.IsSeen,
                Value = i.Notification.Value.Substring(0, 200),
                Type = i.Notification.NotificationType.Name,
            }).ToList();
    }
    /// <summary>
    /// to send notification
    /// </summary>
    /// <param name="To">oject of class Users that represents who to send message to him</param>
    /// <param name="type">type of the message</param>
    /// <param name="message">the message text</param>
    /// <param name="linkToGo">link user clicks</param>
    /// <param name="messageID">if it was a notification for message</param>
    public static void Send(Users To, NotificationType type, string message, string linkToGo, int? messageID = null)
    {
        var now = DateTime.Now;

        var unitOfWork = new UnitOfWork(MvcApplication.ConnectionString);

        //create the main motification object
        var notification = new TaskManagementModel.Notification();
        notification.LinkToGo = linkToGo;
        notification.MessageID = messageID;
        notification.NotificationTypeID = (int)type;
        notification.Value = message;
        notification.CreateDate = now;
        unitOfWork.Notifications.Insert(notification);

        //save it to get the id
        unitOfWork.Save();
        //the collection to send
        var collection = new List<TaskManagementModel.NotificationCollection>();
        switch(To.UsersType)
        {
            case UsersType.Company:
                {
                    if (MvcApplication.userData.isCompany && MvcApplication.userData.userId == To.InstanceID && type != NotificationType.InprogressTask)
                        break;

                    var item = new NotificationCollection()
                     {
                         InstanceID = To.InstanceID ,
                         IsSeen = false,
                         SeenDate = null,
                         NotificationID = notification.ID,
                         UserTypeID = (int)LoggedUserType.Company 
                     };
                    collection.Add(item);
                    unitOfWork.NotificationCollections.Insert(item);
                }
                break;
            case UsersType.Employee:
                {
                    if (!MvcApplication.userData.isCompany && MvcApplication.userData.userId == To.InstanceID && type != NotificationType.InprogressTask)
                        break;
                    var item = new NotificationCollection()
                    {
                        InstanceID = To.InstanceID ,
                        IsSeen = false,//default
                        SeenDate = null,//default
                        NotificationID = notification.ID,
                        UserTypeID = (int)LoggedUserType.Employee//user type id
                    };
                    //add to collection
                    collection.Add(item);
                    //add the collection to be inserted
                    unitOfWork.NotificationCollections.Insert(item);
                }
                break;
            case UsersType.AllCompanyEmployees: //send message to company emps
                {
                    //call serverice and bring them
                  EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployee(To.InstanceID).Where(i=>i.id!= MvcApplication.userData.userId).ToList().ForEach(delegate(EmployeeProfile i) 
                    {

                   

                        var item = new NotificationCollection()
                        {
                            InstanceID = i.id , //user id
                            IsSeen = false, //default
                            SeenDate = null, //default
                            NotificationID = notification.ID,
                            UserTypeID = (int)LoggedUserType.Employee //user type id
                        };
                        //add to collection
                        collection.Add(item);
                        //add the collection to be inserted
                        unitOfWork.NotificationCollections.Insert(item);
                        
                    });
                   
                }
                break;
            case UsersType.ProjectEmployees:
                {
                   

                }
                break;
            default:
                break;
        }

        //save collection
        unitOfWork.Save();

        //get current session keys
        var currentSession = unitOfWork.SIGNAL_R_SESSIONs.Get().ToList();
        //get the list specific to send now
        var lstToSend = currentSession.Where(i => collection.Any(s => s.InstanceID  == i.UserID  && s.UserTypeID == i.UserTypeID)).Select(i => i.ConnectionID).ToList();
        //create object to be serialized and send it to client
        var objToSend = new NotificationClientObject() 
        {
            ID = notification.ID.ToString(),
            Time = MvcApplication.IsGregDate ? now.ToGregArabicDateTime() : now.ToHijriArabicDateTime(),
            Type = type.ToString(),
            Value = message.SubString(50),
            Link = string.IsNullOrEmpty(notification.LinkToGo) ? "" : notification.LinkToGo.Substring(0, notification.LinkToGo.LastIndexOf('/'))+ "/"+ Extentions.Encrypt(notification.LinkToGo.Split('/').Last())
        };
        //call SendGeneralNotification function that's exists on the client's browser and pass the object we created.
        HubContext.Clients.Clients(lstToSend).SendGeneralNotification(new JavaScriptSerializer().Serialize(objToSend));
    }


    /// <summary>
    /// on connected ... update connection id
    /// </summary>
    /// <returns></returns>

    public override System.Threading.Tasks.Task OnConnected()
    {
        Clients.Client(Context.ConnectionId).UpdateConnectionID(Context.ConnectionId);
        return base.OnConnected();
    }

    /// <summary>
    /// on OnReconnected ... update connection id
    /// </summary>
    /// <returns></returns>

    public override System.Threading.Tasks.Task OnReconnected()
    {
        Clients.Client(Context.ConnectionId).UpdateConnectionID(Context.ConnectionId);
        return base.OnReconnected();
    }


    /// <summary>
    ///O nDisconnected ... remove session
    /// </summary>
    /// <returns></returns>
    public override System.Threading.Tasks.Task OnDisconnected()
    {
        var _unitOfWork = new UnitOfWork(MvcApplication.ConnectionString);
        var SIGNAL_R_SESSION = _unitOfWork.SIGNAL_R_SESSIONs.Get(i => i.ConnectionID == Context.ConnectionId).FirstOrDefault();
        if (SIGNAL_R_SESSION != null)
        {
            _unitOfWork.SIGNAL_R_SESSIONs.Delete(SIGNAL_R_SESSION);
        }
        return base.OnDisconnected();
    }
}

/// <summary>
/// userstype to be send
/// </summary>
public enum UsersType
{
    AllSystem,
    AllCompanies,
    AllCompanyEmployees,
    Company,
    Employee,
    ProjectEmployees
}
/// <summary>
/// to simplify the use of send method in the notificaiton class
/// </summary>
public class Users
{
    public int InstanceID;
    public UsersType UsersType;

    public static Users AllSystem { get { return new Users() { InstanceID = 0, UsersType = UsersType.AllSystem }; } }
    public static Users AllCompanies { get { return new Users() { InstanceID = 0, UsersType = UsersType.AllCompanies }; } }


    public static Users AllCompanyEmployees(int CompanyID)
    {
        return new Users() { InstanceID = CompanyID, UsersType = UsersType.AllCompanyEmployees };
    }

    public static Users Company(int CompanyID)
    {
        return new Users() { InstanceID = CompanyID, UsersType = UsersType.Company };
    }

    public static Users Employee(int EmployeeID)
    {
        return new Users() { InstanceID = EmployeeID, UsersType = UsersType.Employee };
    }

    public static Users ProjectEmployees(int ProjectID)
    {
        return new Users() { InstanceID = ProjectID, UsersType = UsersType.ProjectEmployees };
    }
}



