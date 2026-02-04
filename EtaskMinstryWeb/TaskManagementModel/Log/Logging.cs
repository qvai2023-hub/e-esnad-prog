using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Linq.Mapping;
using System.Data.Mapping;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using TaskManagementModel;
using System.Configuration;
using EtaskMinstry;
using TaskManagementModel.Log;

namespace TaskManagementModel
{
    public class Logging<TEntity> where TEntity : class
    {
        #region Logging Core functions.
        /// <summary>
        /// log login action.
        /// </summary>
        public void LogLogin(int userId)
        {
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            Log(userId, "Login",
                    DateTime.Now, controllerName, ip);
        }

        /// <summary>
        /// Log View Action.
        /// </summary>
        public void LogView(int userId)
        {
            string controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            Log(userId, "Index", DateTime.Now, controllerName, controllerName);
        }

        /// <summary>
        /// log Deleted Item.
        /// </summary>
        /// <param name="entityToDelete"></param>
        public void LogDelete(TEntity entityToDelete, int userId)
        {
            Type DeletedTypetype = entityToDelete.GetType();
            if (IsLogPage(DeletedTypetype))
            {
                string controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                PropertyInfo[] DeletedProperties = DeletedTypetype.GetProperties();
                try
                {
                    string logValue = DeletedProperties.FirstOrDefault(p => p.Name.Contains("Name") || p.Name.Contains("Title") || p.Name.Contains("subject")).GetValue(entityToDelete).ToString();
                    Log(userId, "Delete", DateTime.Now, controllerName, logValue);
                }
                catch { }
            }
        }

        /// <summary>
        /// Log Inserted Item
        /// </summary>
        /// <param name="entityToInsert"></param>
        public void LogInsert(TEntity entityToInsert)
        {
            Type type = entityToInsert.GetType();
            if (IsLogPage(type))
            {
                PropertyInfo[] Properties = type.GetProperties();
                string controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();

                #region get task properties.
                int TaskId;
                int? statuseID;
                bool isCompany;
                GetTaskProperty(entityToInsert, Properties, out TaskId, out statuseID, out isCompany);
                #endregion

                //the list to save json
                List<string> JsonList = new List<string>();

                //get the primarry key of object to avoid adding it in log by zero value
                var set = ((IObjectContextAdapter)new EtaskMinstryEntities(ConfigurationManager.ConnectionStrings["EtaskMinstryEntities"].ConnectionString)).ObjectContext.CreateObjectSet<TEntity>();
                var entitySet = set.EntitySet;
                //Get Entity Set Key assuming that each entity has a key ( a singleKey not worked for Composite Key)
                var keyName = entitySet.ElementType.KeyMembers.Select(k => k.Name).FirstOrDefault();
                int objId = (int)type.GetProperty(keyName).GetValue(entityToInsert);
                //get Display name of each property in the current object from its local resource.
                Type ResourceType = Type.GetType("LocalResources." + type.Name + "");
                PropertyInfo[] DisplayProperties = null;
                if (ResourceType != null)
                {
                    DisplayProperties = ResourceType.GetProperties();
                }
                foreach (PropertyInfo property in Properties)
                {
                    //get the current property value
                    object value = property.GetValue(entityToInsert) ?? String.Empty;

                    //continue looping without action if the value is empty or navigation prop.
                    if (value.Equals(String.Empty) || property.PropertyType.IsGenericType || property.Name == keyName)
                        continue;

                    string displayName = "";
                    try
                    {
                        if (DisplayProperties != null)
                            displayName = DisplayProperties.FirstOrDefault(p => p.Name == property.Name).GetValue(null).ToString();
                    }
                    catch(Exception)
                    { }

                    //if is datetime convert to arabic text
                    value = ValueConversion(value);

                    //get look up value
                    value = GetlookUpValue(type, property, value);

                    try
                    {
                        //add the row updated to json list
                        JsonList.Add("{'columnName':'" + displayName + "','old_value':'','new_value':'" + value + "'}");
                    }
                    catch (Exception) { }
                }
                //if there're fields had changed insert new log for updated items.
                if (JsonList.Count() > 0)
                {
                    try
                    {
                        string logVal = "{'ID':'" + objId + "','itemText':'" + Properties.FirstOrDefault(p => p.Name.Contains("Name") || p.Name.Contains("Title")) + "','updates':[" + String.Join(",", JsonList) + "]}";
                        //Log(userId, "Create", DateTime.Now, controllerName, logVal);
                        //Log(TaskId, DateTime.Now, logVal, statuseID, isCompany);
                    }
                    catch (Exception) { }
                }
            }
        }

        private static void GetTaskProperty(TEntity entityToInsert, PropertyInfo[] Properties, out int TaskId, out int? statuseID, out bool isCompany)
        {

            int.TryParse(Properties.FirstOrDefault(p => p.Name.Contains("TaskID")).GetValue(entityToInsert).ToString(), out TaskId);

            statuseID = null;
            try
            {
                statuseID = (int)Properties.FirstOrDefault(p => p.Name.Contains("StatusID")).GetValue(entityToInsert);
            }
            catch { }

            isCompany = false;
            try
            {
                isCompany = (bool)HttpContext.Current.Session["isCompany"];
            }
            catch (Exception) { }
            ///////////////////////////////////////////////////////////
        }

        /// <summary>
        /// Log Updated Item
        /// </summary>
        /// <param name="entityToUpdate"></param>
        public void LogUpdate(TEntity entityToUpdate)
        {

            string controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            Type type = entityToUpdate.GetType();
            if (IsLogPage(type))
            {
                PropertyInfo[] Properties = type.GetProperties();

                #region get task properties.
                int TaskId;
                int? statuseID;
                bool isCompany;
                GetTaskProperty(entityToUpdate, Properties, out TaskId, out statuseID, out isCompany);
                #endregion

                //IEnumerable<CustomAttributeData> data = type.Attributes;
                //To Get Entity Key Name 
                //First Define Entiy Set 
                var set = ((IObjectContextAdapter)new EtaskMinstryEntities(ConfigurationManager.ConnectionStrings["EtaskMinstryEntities"].ConnectionString)).ObjectContext.CreateObjectSet<TEntity>();
                var entitySet = set.EntitySet;
                //Get Entity Set Key assuming that each entity has a key ( a singleKey not worked for Composite Key)
                var keyName = entitySet.ElementType.KeyMembers.Select(k => k.Name).FirstOrDefault();
                //Get Enity Object ID 
                int objId = (int)type.GetProperty(keyName).GetValue(entityToUpdate);
                using (var _context = new EtaskMinstryEntities(ConfigurationManager.ConnectionStrings["EtaskMinstryEntities"].ConnectionString))
                {
                    DbSet<TEntity> _dbSet = _context.Set<TEntity>();
                    object beforeUpdate = _dbSet.Find(objId);// get object before update.

                    //the list to save json
                    List<string> JsonList = new List<string>();
                    //fetch the properties of the table

                    //get Display name of each property in the current object from its local resource.
                    Type ResourceType = Type.GetType("Resources." + type.Name.Split('_')[0] + "");
                    PropertyInfo[] DisplayProperties = null;
                    if (ResourceType != null)
                    {
                        DisplayProperties = ResourceType.GetProperties();
                    }

                    foreach (PropertyInfo property in Properties)
                    {
                        //get the current property value
                        object OldValue = property.GetValue(beforeUpdate) ?? String.Empty;
                        object CurrentValue = property.GetValue(entityToUpdate) ?? String.Empty;

                        //contnue looping without any action
                        //if the current and old value are the same, or any one of theme is empty
                        //or the current prop is navigation prop
                        if (OldValue.Equals(String.Empty) || CurrentValue.Equals(String.Empty) || CurrentValue.Equals(OldValue) || CurrentValue == null
                            || property.PropertyType.IsGenericType || CurrentValue.GetType().Namespace == "System.Data.Entity.DynamicProxies")
                            continue;

                        string displayName = "";
                        try
                        {
                            if (DisplayProperties != null)
                                displayName = DisplayProperties.FirstOrDefault(p => p.Name == property.Name).GetValue(null).ToString();
                            displayName = DataDefination.Task_Title;
                        }
                        catch { }

                        //value conversion.
                        CurrentValue = ValueConversion(CurrentValue);
                        OldValue = ValueConversion(OldValue);

                        //get look up.
                        OldValue = GetlookUpValue(type, property, OldValue);
                        CurrentValue = GetlookUpValue(type, property, CurrentValue);

                        try
                        {
                            //add the row updated to json list
                            JsonList.Add("{'columnName':'" + displayName + "','old_value':'" + OldValue + "','new_value':'" + CurrentValue + "'}");
                        }
                        catch (Exception) { }
                    }
                    //if there're fields had changed insert new log for updated items.
                    if (JsonList.Count() > 0)
                    {
                        try
                        {
                            string logVal = "{'ID':'" + objId + "','itemText':'" + Properties.FirstOrDefault(p => p.Name.Contains("Name") || p.Name.Contains("Title"))
                                .GetValue(beforeUpdate) + "','updates':[" + String.Join(",", JsonList) + "]}";

                            //Log(userId, "Edit", DateTime.Now, controllerName, logVal);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        #endregion

        #region Helper functions.
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
        /// Insert new log record
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="ActionName"></param>
        /// <param name="ActionTime"></param>
        /// <param name="PageName"></param>
        /// <param name="logValue"></param>
        public void Log(int UserID, string ActionName, DateTime ActionTime, string PageName, string logValue)
        {
            //using (var db = new EtaskMinstryEntities())
            //{
            //    int actionID = db.Action.Where(a => a.Name == ActionName).Select(a => a.ActionID).FirstOrDefault();

            //    int ControllerID = db.PageControllers.Where(c => c.Name == PageName).Select(c => c.ControllerID).FirstOrDefault();

            //    if (ControllerID != 0)
            //    {
            //        var log = new TaskLog
            //        {
            //            UserID = UserID,
            //            ActionID = actionID,
            //            ActionTime = ActionTime,
            //            PageID = ControllerID,
            //            Value = logValue
            //        };
            //        db.Log.Add(log);
            //        db.SaveChanges();
            //    }
            //}
        }

       

        /// <summary>
        /// Convert inserted object and its value
        /// to json form.
        /// </summary>
        /// <param name="JSON"></param>
        /// <param name="strSectionPath"></param>
        /// <returns></returns>
        public Dictionary<string[], List<string>> GetInsertItems(string JSON, string strSectionPath)
        {
            strSectionPath = strSectionPath.Replace("Default.aspx", "").ToLower();
            List<string> updates = new List<string>();
            LogEdit data = new LogEdit();
            //data = new JavaScriptSerializer().Deserialize<LogEdit>(JSON);
            try
            {
                data = new JavaScriptSerializer().Deserialize<LogEdit>(JSON);
            }
            catch (Exception) { }

            if (data.updates != null)
            {
                foreach (LogEditItem item in data.updates)
                {
                    if (item.columnName.ToLower().Contains("path"))
                    {
                        if (item.new_value.Trim() != "")
                        {

                            if (isImage(item.new_value))
                                item.new_value = "<a type=\"pic\"  href=\"/DataImages/" + strSectionPath + "Thump/" + item.new_value + "\">عرض الصورة</a>";
                            else
                                item.new_value = "<a target=\"_blank\" type=\"file\" href=\"/DataImages/" + strSectionPath + item.new_value + "\">تحميل الملف</a>";
                        }
                        else
                        {
                            item.new_value = "لا يوجد ملف";
                        }
                    }
                    else if (item.columnName.ToLower().Contains("password")) // no need to show password in log
                    {
                        item.new_value = "*****"; //QvLib.Security.DataProtection.Decrypt(item.new_value);
                    }
                    updates.Add("تم تعيين  <span class='def_color'>" + item.columnName + "</span> إلى" + " \"" + item.new_value + "\"");
                }
            }

            if (data.ManyToManyItems != null)
            {
                foreach (ManyToManyItem item in data.ManyToManyItems)
                {
                    if (item.Type == Actions.Create)
                        updates.Add("تم إضافة <span class='def_color'>" + item.value + "</span> فى <span class='def_color'>" + item.text + "</span>");
                    else if (item.Type == Actions.Delete)
                        updates.Add("تم حذف <span class='def_color'>" + item.value + "</span> من <span class='def_color'>" + item.text + "</span>");


                }
            }
            Dictionary<string[], List<string>> Return = new Dictionary<string[], List<string>>();
            Return.Add(new string[] { data.ID, data.itemText }, updates);
            return Return;
        }

        /// <summary>
        /// Convert updated object and its oldvalues & current values
        /// to json form.
        /// </summary>
        /// <param name="JSON"></param>
        /// <param name="strSectionPath"></param>
        /// <returns></returns>
        public Dictionary<string[], List<string>> GetEditItems(string JSON, string strSectionPath)
        {
            strSectionPath = strSectionPath.Replace("Default.aspx", "").ToLower();
            List<string> updates = new List<string>();
            LogEdit data = new LogEdit();
            //data = new JavaScriptSerializer().Deserialize<LogEdit>(JSON);
            try
            {
                data = new JavaScriptSerializer().Deserialize<LogEdit>(JSON);
            }
            catch (Exception) { }

            if (data.updates != null)
            {
                foreach (LogEditItem item in data.updates)
                {
                    if (item.columnName.ToLower().Contains("path"))
                    {
                        if (item.old_value.Trim() != "")
                        {
                            if (isImage(item.old_value))
                                item.old_value = "<a type=\"pic\"  href=\"/DataImages/" + strSectionPath + "Thump/" + item.old_value + "\">عرض الصورة</a>";
                            else
                                item.old_value = "<a target=\"_blank\" type=\"file\" href=\"/DataImages/" + strSectionPath + item.old_value + "\">تحميل الملف</a>";
                        }
                        else
                        {
                            item.old_value = "لا يوجد ملف";
                        }
                        if (item.new_value.Trim() != "")
                        {

                            if (isImage(item.new_value))
                                item.new_value = "<a type=\"pic\"  href=\"/DataImages/" + strSectionPath + "Thump/" + item.new_value + "\">عرض الصورة</a>";
                            else
                                item.new_value = "<a target=\"_blank\" type=\"file\" href=\"/DataImages/" + strSectionPath + item.new_value + "\">تحميل الملف</a>";
                        }
                        else
                        {
                            item.new_value = "لا يوجد ملف";
                        }
                    }
                    else if (item.columnName.ToLower().Contains("password"))
                    {
                        item.old_value = "*****";//QvLib.Security.DataProtection.Decrypt(item.old_value);
                        item.new_value = "*****";//QvLib.Security.DataProtection.Decrypt(item.new_value);
                    }
                    updates.Add("تم تعيين  <span class='def_color'>" + item.columnName + "</span> من" + " \"" + item.old_value + "\"" + " إلى" + " \"" + item.new_value + "\"");
                }
            }

            if (data.ManyToManyItems != null)
            {
                foreach (ManyToManyItem item in data.ManyToManyItems)
                {
                    if (item.Type == Actions.Create)
                        updates.Add("تم إضافة <span class='def_color'>" + item.value + "</span> فى <span class='def_color'>" + item.text + "</span>");
                    else if (item.Type == Actions.Delete)
                        updates.Add("تم حذف <span class='def_color'>" + item.value + "</span> من <span class='def_color'>" + item.text + "</span>");
                    else if (item.Type == Actions.Edit)
                        updates.Add("تم تعيين  <span class='def_color'>" + item.text + "</span> من" + " \"" + item.old_value + "\"" + " إلى" + " \"" + item.value + "\"");

                }
            }
            Dictionary<string[], List<string>> Return = new Dictionary<string[], List<string>>();
            Return.Add(new string[] { data.ID, data.itemText }, updates);
            return Return;
        }

        /// <summary>
        /// check if the current property reperesent an image
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool isImage(string filename)
        {
            string ext = filename.Substring(filename.LastIndexOf(".") + 1);
            string[] image_extentions = new string[] { "png", "gif", "jpg", "jpeg" };
            if (image_extentions.Contains(ext))
                return true;
            else
                return false;
        }

        /// <summary>
        /// execute sql statement.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sql"></param>
        static void ExecuteSql(ObjectContext c, string sql)
        {
            var entityConnection = (System.Data.EntityClient.EntityConnection)c.Connection;
            DbConnection conn = entityConnection.StoreConnection;
            ConnectionState initialState = conn.State;
            try
            {
                if (initialState != ConnectionState.Open)
                    conn.Open();  // open connection if not already open
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (initialState != ConnectionState.Open)
                    conn.Close(); // only close connection if not initially open
            }
        }

        /// <summary>
        /// if the passed value
        /// refer to look up table
        /// get the orginal value instead of forign key.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetlookUpValue(Type type, PropertyInfo property, object value)
        {
            //fetch the properties of the table
            //get table references prefernces
            string tableName = type.Name.Split('_')[0];
            IList<TableRef_Result> references = new EtaskMinstryEntities().TableRef(tableName).ToList();
            //detect if it's a lookup value from another table
            TableRef_Result feild = references.Where(i => i.FK_COLUMN_NAME == property.Name).FirstOrDefault();
            if (feild != null) // if table have forien keys
            {
                using (var db = new EtaskMinstryEntities())
                {
                    db.Database.Connection.Open();
                    using (var cmd = db.Database.Connection.CreateCommand()) //open DbCommand
                    {
                        if (!String.IsNullOrEmpty(value.ToString()))
                        {
                            //sql statement to select the row of refrenced table which equal the old value
                            cmd.CommandText = "SELECT * FROM [" + feild.REFERENCED_TABLE_NAME + "] WHERE " + feild.REFERENCED_COLUMN_NAME + " = " + value;
                            //execute the sql statement and save it in datareader
                            DbDataReader reader = cmd.ExecuteReader();
                            reader.Read(); //read first row
                            //get the second field whitch usually be the Name field and save it as the value of the old_value
                            value = reader[1];
                            reader.Close();
                        }
                    }
                }
            }
           return value;
        }


        public bool IsLogPage(Type obj)
        {
            //try
            //{
            //    string currentObject = obj.Name.Split('_')[0];
            //    return new EtaskMinstryEntities().PageControllers.FirstOrDefault(p => p.Name.ToLower().
            //        Contains(currentObject.ToLower())).IsLog;
            //}
            //catch
            //{
                return false;
           // }

        }

        #endregion
    }

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

    #endregion
}

