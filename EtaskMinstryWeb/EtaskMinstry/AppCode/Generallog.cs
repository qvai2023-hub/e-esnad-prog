using EtaskMinstry.Models.TaskLogging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using TaskManagementModel;

namespace EtaskMinstry.AppCode
{
   
    #region Some helper Classes.
    /// <summary>
    /// this class is use in serialization  json of  updated items
    /// </summary>
    public class LogEditItem
    {
        public string columnName { get; set; }
        public string text { get; set; }
        public string old_value { get; set; }
        public string new_value { get; set; }
    }

    public class generalLogEditItem
    {
        public string columnName { get; set; }
        public string oldVal { get; set; }
        public string newVal { get; set; }

    }

    public class ManyToManyItem
    {
        public Actions Type { get; set; }
        public string text { get; set; }
        public string value { get; set; }
        public string old_value { get; set; }
    }

    public class LogEdit
    {
        public string ID { get; set; }
        public string itemText { get; set; }
        public List<LogEditItem> updates { get; set; }
        public List<ManyToManyItem> ManyToManyItems { get; set; }
    }

    public class generalLogEdit
    {
        public string ID { get; set; }
        public string itemText { get; set; }
        public List<generalLogEditItem> updates { get; set; }
        public List<ManyToManyItem> ManyToManyItems { get; set; }
    }

    public class JsonObject
    {
        public string columnName { get; set; }
        public string oldVal { get; set; }
        public string newVal { get; set; }
    }

    #endregion

    /// <summary>
    /// class to log system actions
    /// call log function after each action want to log it
    /// pass old and new object in case to update or pass only the new object in case to insert.
    /// EX.Update --> Generallog.Log(Identity, Currentobj, beforeUpdate,IsfromCompany, Title);
    /// Insert --> Generallog.Log(Identity, Currentobj, null,IsfromCompany, Title);
    /// </summary>
    public class Generallog
    {       
        /// <summary>
        /// Base Log Function in it get current controllername which call this Log
        /// </summary>
        /// <param name="Identity"></param>
        /// <param name="Currentobj"></param>
        /// <param name="beforeUpdate"></param>
        /// <param name="IsfromCompany"></param>
        /// <param name="Title"></param>
        /// <param name="EmpID"></param>
        /// <param name="CompanyID"></param>
        public static void Log(int Identity, object Currentobj, object beforeUpdate, bool IsfromCompany, string Title, int EmpID, int CompanyID)
        {
            try
            {
                string controllername = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                int ConID = 0;
                UnitOfWork _unitOfWork = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
                ControllerName con = _unitOfWork.ControllerNameRepository.Get(c => c.ControllerNameEnglish == controllername).FirstOrDefault();
                ConID = con != null ? con.ControllerID : 0;
                if (beforeUpdate == null)
                    LogInsert(Identity, Title, Currentobj, IsfromCompany, EmpID, CompanyID, ConID);
                else
                    LogUpdate(Identity, Title, Currentobj, beforeUpdate, IsfromCompany, EmpID, CompanyID, ConID);
            }
            catch
            {   }          
        }

       /// <summary>
        /// Log Update to save current obj and before obj and title ,iIdentity ID
       /// </summary>
       /// <param name="iIdentity"></param>
       /// <param name="Title"></param>
       /// <param name="Currentobj"></param>
       /// <param name="beforeUpdate"></param>
       /// <param name="Isfromcompany"></param>
        private static void LogUpdate(int iIdentity, string Title, object Currentobj, object beforeUpdate, bool Isfromcompany, int EmpID, int CompanyID,int ControllerID)
        {
            Type type = Currentobj.GetType();
            PropertyInfo[] Properties = type.GetProperties();

           
            //the list to save json
            List<string> JsonList = new List<string>();
            //fetch the properties of the table

            foreach (PropertyInfo property in Properties)
            {

                string ss = property.Name;
                //get the current property value
                object OldValue = property.GetValue(beforeUpdate) ?? String.Empty;
                object CurrentValue = property.GetValue(Currentobj) ?? String.Empty;

                //contnue looping without any action
                //if the current and old value are the same, or any one of theme is empty
                //or the current prop is navigation prop
                if (property.Name != "LastModifiedDate")
                {
                    if (OldValue.Equals(String.Empty) || CurrentValue.Equals(String.Empty) || CurrentValue.Equals(OldValue) || CurrentValue == null
                        || CurrentValue.GetType().Namespace == "System.Data.Entity.DynamicProxies")
                        continue;
                    // || property.PropertyType.IsGenericType
                    //value conversion.
                    CurrentValue = ValueConversion(CurrentValue);
                    OldValue = ValueConversion(OldValue);


                    try
                    {
                        //add the row updated to json list
                        JsonList.Add("{'columnName':'" + property.Name + "','oldVal':'" + OldValue + "','newVal':'" + CurrentValue + "'}");
                    }
                    catch (Exception) { }
                }
            }
            //if there're fields had changed insert new log for updated items.
            if (JsonList.Count() > 0)
            {
                try
                {
                    New_Log(ActionType.Update,ControllerID ,"{'ID':'" + iIdentity + "','itemText':'" + Title + "','updates':[" + String.Join(",", JsonList) + "]}", Isfromcompany,EmpID,CompanyID);//save old and new values of table updated        
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Log Insert to save currentobj and before = null , title ,iIdentity ID
        /// </summary>
        /// <param name="iIdentity"></param>
        /// <param name="strTitle"></param>
        /// <param name="Currentobj"></param>
        /// <param name="Isfromcompany"></param>
        private static void LogInsert(int iIdentity, string strTitle, object Currentobj,bool Isfromcompany, int EmpID, int CompanyID,int ControllerID)
        {
            Type type = Currentobj.GetType();
            PropertyInfo[] Properties = type.GetProperties();

            //the list to save json
            List<string> JsonList = new List<string>();
            //fetch the properties of the table

            foreach (PropertyInfo property in Properties)
            {
                //get the current property value
                object CurrentValue = property.GetValue(Currentobj) ?? String.Empty;

                //continue looping without any action
                //if the current and old value are the same, or any one of theme is empty
                //or the current prop is navigation prop
                if (CurrentValue.Equals(String.Empty) || CurrentValue == null
                    || property.PropertyType.IsGenericType || CurrentValue.GetType().Namespace == "System.Data.Entity.DynamicProxies")
                    continue;

                //value conversion.
                CurrentValue = ValueConversion(CurrentValue);               

                try
                {
                    //add the row updated to json list               
                    JsonList.Add("{'columnName':'" + property.Name + "','oldVal':'','newVal':'" + CurrentValue + "'}");
                }
                catch (Exception) { }
            }
            //if there're fields had changed insert new log for updated items.
            if (JsonList.Count() > 0)
            {
                try
                {
                    New_Log(ActionType.Insert, ControllerID, "{'ID':'" + iIdentity + "','itemText':'" + strTitle + "','updates':[" + String.Join(",", JsonList) + "]}", Isfromcompany, EmpID, CompanyID);//save old and new values of table updated        
                  
                }
                catch (Exception) { }
            }
        }



        /// <summary>
        /// insert log Recored.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="value"></param>
        /// <param name="isForCompany"></param>
        private static void New_Log(ActionType type, int ControllerID, string JSON, bool Iscompany, int EmpID, int CompanyID)
        {
            UnitOfWork _unitOfWork = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            int? currentEmpID = null;
            if (EmpID != 0)
                currentEmpID = EmpID;
            int? currentCompID = null;
            if (CompanyID != 0)
                currentCompID = CompanyID;
            TaskManagementModel.LogGeneral logObj = new TaskManagementModel.LogGeneral
            {
                ActionTime = DateTime.Now,
                ActionID = (int)type,
                ControllerName=_unitOfWork.ControllerNameRepository.GetByID(ControllerID),
                Value = JSON,
                IsCompany = Iscompany,
                EmployeeID = currentEmpID,
                CompanyID = currentCompID,                
            };
            _unitOfWork.LogGeneralRepository.Insert(logObj);
            _unitOfWork.Save();
            _unitOfWork.Dispose();
        }
        

        /// <summary>
        /// Convert the passed value
        /// </summary>
        /// <param name="CurrentValue"></param>
        /// <returns></returns>
        private static object ValueConversion(object CurrentValue)
        {
            //some conversions to the current value................................

            //if is datetime convert to arabic text
            if (CurrentValue.GetType() == typeof(DateTime))
                CurrentValue = Convert.ToDateTime(CurrentValue);

            //if it bool
            else if (CurrentValue.GetType() == typeof(bool))
            {
                if (Convert.ToString(CurrentValue) == "True")
                    CurrentValue = "نعم";
                else if (Convert.ToString(CurrentValue) == "False")
                    CurrentValue = "لا";
            }
            return CurrentValue;
        }

        /// <summary>
        /// Serialize json Object to Custom Object.
        /// </summary>
        /// <param name="jsonVal"></param>
        /// <returns></returns>
        public static List<JsonObject> ParseJson(string jsonVal)
        {
            List<JsonObject> res = new List<JsonObject>();
            try
            {
                res = new JavaScriptSerializer().Deserialize<List<JsonObject>>(jsonVal);
                //if (E_TaskWide2.MvcApplication.userData.isCompany)
               // {
                  //  List<Edawam_MvcTask.Models.UserData> CompanyEmployee = ServiceCaller.GetEmployees((int)E_TaskWide2.MvcApplication.userData.userId);
                    //foreach (var item in res)
                    //{
                    //    if (item.columnName == "EmpID")
                    //    {
                    //        item.newVal = ServiceManger.GetEmplyeeName(int.Parse( item.newVal));
                    //    }
                    //}

                    foreach (var item in res)
                    {
                        if (item.columnName == "EmpID" && int.Parse(item.newVal) == MvcApplication.userData.userId &&
                            !MvcApplication.userData.isCompany)
                        {
                            item.newVal = ServiceManger.GetEmplyeeName(int.Parse(item.newVal));
                        }
                        else if (item.columnName == "EmpID" && MvcApplication.userData.isCompany)
                        {
                            item.newVal = ServiceManger.GetEmplyeeName(int.Parse(item.newVal));
                        }
                    }
              
               // }
            }
            catch(Exception){}
            return res;
        }

        /// <summary>
        /// Log Delete action
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="CompanyID"></param>
        public static void Delete(string strTitle, int CompanyID)
        {
            try
            {
                string controllername = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                int ConID = 0;
                UnitOfWork _unitOfWork = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
                ControllerName con = _unitOfWork.ControllerNameRepository.Get(c => c.ControllerNameEnglish == controllername).FirstOrDefault();
                ConID = con != null ? con.ControllerID : 0;
                New_Log(ActionType.Delete, ConID, strTitle, true, 0, CompanyID);
            }
            catch
            {  }           
        }


        /// <summary>
        /// Actiontype.Insert If Inserted action , Actiontype.Update If Updated action
        /// </summary>
        /// <param name="JSON"></param>
        /// <param name="InsORUp"></param>
        /// <returns></returns>
        public Dictionary<string[], List<string>> GetItemsInsertedOrUpdated(string JSON, ActionType action)
        {

            List<string> updates = new List<string>();
            generalLogEdit data = new generalLogEdit();
            try
            {
                data = new JavaScriptSerializer().Deserialize<generalLogEdit>(JSON);
            }
            catch (Exception) { }

            if (data.updates != null)
            {

                if (action == ActionType.Insert)
                {
                    foreach (generalLogEditItem item in data.updates)
                    {
                        if (!item.columnName.Contains("ID") && !item.columnName.Contains("IsDeleted"))
                        {
                            
                            string colmnname = HttpContext.GetGlobalResourceObject("ColumnNameLog", item.columnName).ToString();
                            updates.Add("تم تعيين  <span>" + colmnname + "</span> إلى" + " \"" + item.newVal + "\"");
                        }
                    }
                }
                if (action == ActionType.Update)
                {
                    foreach (generalLogEditItem item in data.updates)
                    {
                        if (!item.columnName.Contains("ID") && !item.columnName.Contains("IsDeleted"))
                        {
                            string colmnname = HttpContext.GetGlobalResourceObject("ColumnNameLog", item.columnName).ToString();
                            updates.Add("تم تعيين  <span>" + colmnname + "</span> من" + " \"" + item.oldVal + "\"" + " إلى" + " \"" + item.newVal + "\"");
                        }
                    }
                }
            }
            Dictionary<string[], List<string>> Return = new Dictionary<string[], List<string>>();
            Return.Add(new string[] { data.ID, data.itemText }, updates);
            return Return;
        }
               
        private static bool isImage(string filename)
        {
            string ext = filename.Substring(filename.LastIndexOf(".") + 1);
            string[] image_extentions = new string[] { "png", "gif", "jpg", "jpeg" };
            if (image_extentions.Contains(ext))
                return true;
            else
                return false;
        }
             
        /// <summary>
        /// this function to log Two actions 
        /// Login  if action==1
        /// Logout if action==2
        /// </summary>
        /// <param name="action"></param>
        public static void LoginORLogout(ActionType action)
        {
            if (MvcApplication.userData != null)
            {
                LogVM log = new LogVM();
                string controllername = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                int ConID = 0;
                UnitOfWork _unitOfWork = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
                ControllerName con = _unitOfWork.ControllerNameRepository.Get(c => c.ControllerNameEnglish == controllername).FirstOrDefault();
                ConID = con != null ? con.ControllerID : 0;
                //get client ip address
                string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                //save the log Action and ip address
                int COMId = 0;
                int EMPID = 0;
                if (MvcApplication.userData.UserTypeId != 1)
                {
                    COMId = int.Parse(log.EmpOrCom_Name()["CompanyId"].ToString());
                    EMPID = int.Parse(log.EmpOrCom_Name()["EmployeeId"].ToString());
                }
                if (action == ActionType.Login)
                {
                    New_Log(ActionType.Login, ConID, ip, MvcApplication.userData.isCompany, EMPID, COMId);
                }
                else if (action == ActionType.LogOut)
                {
                    New_Log(ActionType.LogOut, ConID, ip, MvcApplication.userData.isCompany, EMPID, COMId);
                }
            }
        
        }
        /// <summary>
        /// Log View Action.
        /// if Company View save its ID only BUT if Employee View save EmployeeID and his companyID
        /// </summary>
        public static void LogView()
        {
            if (MvcApplication.userData!=null && MvcApplication.userData.UserTypeId != (int)LoggedUserType.Admin)
            {
                UnitOfWork _unitOfWork = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
                HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                string controllername = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                int ConID = 0;
                if (controllername == "Company" || controllername =="Tasks")
                {
                    controllername = "Company/index";
                }
                if (controllername == "Log")
                {
                    controllername = "Log/LogGeneral";
                }
                ControllerName con = _unitOfWork.ControllerNameRepository.Get(c => c.ControllerNameEnglish == controllername).FirstOrDefault();
                ConID = con != null ? con.ControllerID : 0;
                int COMId;
                int EMPID;
                if (MvcApplication.userData != null)
                {
                    if (MvcApplication.userData.isCompany)
                    {
                        COMId = MvcApplication.userData.userId;
                        EMPID = 0;
                    }
                    else
                    {
                        EMPID = MvcApplication.userData.userId;
                        Employee emp = _unitOfWork.Employee.GetByID(EMPID);
                        Company com = _unitOfWork.Company.Get(c => c.CompanyID == emp.CompanyID).FirstOrDefault();
                        COMId = (com != null) ? com.CompanyID : 0;
                    }
                    New_Log(ActionType.View, ConID, "مشاهدة", MvcApplication.userData.isCompany, EMPID, COMId);
                }
            }
        }
    }
}

 
