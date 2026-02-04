using System.Globalization;
using EtaskMinstry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.AppCode
{
    public class LogTask
    {
        /// <summary>
        /// Log action
        /// </summary>
        /// <param name="entityToUpdate"></param>
        public static void Log(TaskManagementModel.Task Currentobj, TaskManagementModel.Task beforeUpdate,
                               bool IsDeleted = false)
        {
            if (IsDeleted)
                LogDelete(Currentobj);
            else if (beforeUpdate == null)
                LogInsert(Currentobj);
            else
                LogUpdate(Currentobj, beforeUpdate);
        }

        /// <summary>
        /// log Updated object
        /// </summary>
        /// <param name="Currentobj"></param>
        /// <param name="beforeUpdate"></param>
        private static void LogUpdate(TaskManagementModel.Task Currentobj, TaskManagementModel.Task beforeUpdate)
        {
            //the list to save json
            List<string> JsonList = new List<string>();

            if (Currentobj.EmpID != beforeUpdate.EmpID)
                JsonList.Add("{'columnName':'EmpID','oldVal':'" + beforeUpdate.EmpID + "','newVal':'" + Currentobj.EmpID +
                             "'}");

            if (Currentobj.EndDate != beforeUpdate.EndDate)
                JsonList.Add("{'columnName':'EndDate','oldVal':'" + beforeUpdate.EndDate + "','newVal':'" +
                             Currentobj.EndDate + "'}");

            if (Currentobj.StartDate != beforeUpdate.StartDate)
                JsonList.Add("{'columnName':'StartDate','oldVal':'" + beforeUpdate.StartDate + "','newVal':'" +
                             Currentobj.StartDate + "'}");

            if (Currentobj.Summary != beforeUpdate.Summary)
                JsonList.Add("{'columnName':'Summary','oldVal':'" + beforeUpdate.Summary + "','newVal':'" +
                             Currentobj.Summary + "'}");
            if (Currentobj.Description != beforeUpdate.Description)
                JsonList.Add("{'columnName':'Description','oldVal':'" + beforeUpdate.Description + "','newVal':'" +
                             Currentobj.Description + "'}");

            if (Currentobj.ExpectedTime != beforeUpdate.ExpectedTime)

                if (Currentobj.ExpectedTime != beforeUpdate.ExpectedTime)
                    JsonList.Add("{'columnName':'ExpectedTime','oldVal':'" + (beforeUpdate.ExpectedTime.HasValue ? beforeUpdate.ExpectedTime : 0 )+ " " +
                               (beforeUpdate.TimeUnitID.HasValue ? ((TimeUnitArabic)beforeUpdate.TimeUnitID.Value).ToString() : "") + "','newVal':'" + (Currentobj.ExpectedTime.HasValue ? Currentobj.ExpectedTime : 0) + " " +
                                 (Currentobj.TimeUnitID.HasValue ? ((TimeUnitArabic)Currentobj.TimeUnitID.Value).ToString() : "") + "'}");

            if (Currentobj.PriorityID != beforeUpdate.PriorityID)
                JsonList.Add("{'columnName':'PriorityID','oldVal':'" +
                             new EtaskMinstry.Models.Priority.PriorityDisplay().Get(beforeUpdate.PriorityID).Name +
                             "','newVal':'" +
                             new EtaskMinstry.Models.Priority.PriorityDisplay().Get(Currentobj.PriorityID).Name + "'}");

            if (Currentobj.StatusID != beforeUpdate.StatusID)
            {
                JsonList.Add("{'columnName':'StatusID','oldVal':'" +
                             new EtaskMinstry.Models.Status.StatusDisplay().Get(beforeUpdate.StatusID).Name +
                             "','newVal':'" +
                             new EtaskMinstry.Models.Status.StatusDisplay().Get(Currentobj.StatusID).Name + "'}");
            }

            if (Currentobj.IsArchived != beforeUpdate.IsArchived)
            {
                JsonList.Add("{'columnName':'IsArchived','oldVal':'','newVal':''}");
            }
            if (Currentobj.ProjectID != beforeUpdate.ProjectID)
            {

                
                JsonList.Add(
                    (beforeUpdate.ProjectID != null && Currentobj.ProjectID != null) ?
                    "{'columnName':'ProjectID','oldVal':'" +
              new EtaskMinstry.Models.Project.ProjectDisplay().GetById(beforeUpdate.ProjectID.Value).Name + "','newVal':'" +
                 new EtaskMinstry.Models.Project.ProjectDisplay().GetById(Currentobj.ProjectID.Value).Name + "'}" :

                 (beforeUpdate.ProjectID != null) ? "{'columnName':'ProjectID','oldVal':'" +
              new EtaskMinstry.Models.Project.ProjectDisplay().GetById(beforeUpdate.ProjectID.Value).Name + "','newVal':''}" :

               (Currentobj.ProjectID != null) ? "{'columnName':'ProjectID','oldVal':'','newVal':'" +
                 new EtaskMinstry.Models.Project.ProjectDisplay().GetById(Currentobj.ProjectID.Value).Name + "'}" :
                 "{'columnName':'ProjectID','oldVal':'','newVal':''}");
            }
            //if (Currentobj.TaskComment.Count > beforeUpdate.TaskComment.Count)
            //{
            //    IEnumerable<TaskComment> comments = Currentobj.TaskComment.Except<TaskComment>(beforeUpdate.TaskComment);
            //    foreach (var item in comments)
            //    {
            //        JsonList.Add("{'columnName':'TaskCommentID','oldVal':'','newVal':'" + item.Comment + "'}");
            //    }
            //}


            //if (Currentobj.Attachment.Count > beforeUpdate.Attachment.Count)
            //{
            //    IEnumerable<Attachment> attachments = Currentobj.Attachment.Except<Attachment>(beforeUpdate.Attachment);
            //    foreach (var item in attachments)
            //    {
            //        JsonList.Add("{'columnName':'AttachmentID','oldVal':'','newVal':'" + item.FileName + "'}");
            //    }
            //}

            //if there're fields had changed insert new log for updated items.
            if (JsonList.Count() > 0)
            {
                try
                {
                    string logVal = "[" + String.Join(",", JsonList) + "]";
                    Log(Currentobj.TaskID, logVal, MvcApplication.userData.isCompany,Currentobj.PriorityID,Currentobj.StatusID);
                }
                catch (Exception)
                {
                }
            }
        }


        /// <summary>
        /// Log Inserted Object.
        /// </summary>
        /// <param name="Currentobj"></param>
        private static void LogInsert(TaskManagementModel.Task Currentobj)
        {
            UnitOfWork _unitOfWork =
                new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            //the list to save json
            List<string> JsonList = new List<string>();
            try
            {
                if (Currentobj.StartDate != null && Currentobj.EndDate != null)
                {

                    JsonList = new List<string>()
                    {

                        "{'columnName':'IsNewTask','oldVal':'','newVal':'" + true + "'}",
                        "{'columnName':'EmpID','oldVal':'','newVal':'" + Currentobj.EmpID + "'}",
                       (Currentobj.ProjectID != null)? 
                       "{'columnName':'ProjectID','oldVal':'','newVal':'" +  new EtaskMinstry.Models.Project.ProjectDisplay().GetById(Currentobj.ProjectID.Value).Name + "'}":
                       "{'columnName':'ProjectID','oldVal':'','newVal':''}",

                        "{'columnName':'StartDate','oldVal':'','newVal':'" +
                        Extentions.ToGregDatediff(Currentobj.StartDate.Value) + "'}",
                        "{'columnName':'EndDate','oldVal':'','newVal':'" +
                        Extentions.ToGregDatediff(Currentobj.EndDate.Value) + "'}",
                        "{'columnName':'ExpectedTime','oldVal':'','newVal':'" + Currentobj.ExpectedTime + "'}",
                        "{'columnName':'Description','oldVal':'','newVal':'" + Currentobj.Description + "'}",
                        "{'columnName':'Summary','oldVal':'','newVal':'" + Currentobj.Summary + "'}",
                        "{'columnName':'PriorityID','oldVal':'','newVal':'" +
                        _unitOfWork.PriorityRepository.GetByID(Currentobj.PriorityID).Name + "'}",
                        "{'columnName':'StatusID','oldVal':'','newVal':'" +
                        _unitOfWork.StatusRepository.GetByID(Currentobj.StatusID).Name + "'}"
                    };
                }
                else if(Currentobj.StartDate != null)
                {
                        JsonList = new List<string>()
                        {
                            "{'columnName':'IsNewTask','oldVal':'','newVal':'" + true + "'}",
                            "{'columnName':'EmpID','oldVal':'','newVal':'" + Currentobj.EmpID + "'}",
                               (Currentobj.ProjectID != null)? 
                       "{'columnName':'ProjectID','oldVal':'','newVal':'" +  new EtaskMinstry.Models.Project.ProjectDisplay().GetById(Currentobj.ProjectID.Value).Name + "'}":
                       "{'columnName':'ProjectID','oldVal':'','newVal':''}",
                            "{'columnName':'StartDate','oldVal':'','newVal':'" +
                            Extentions.ToGregDatediff(Currentobj.StartDate.Value) + "'}",
                            "{'columnName':'EndDate','oldVal':'','newVal':''}",
                            "{'columnName':'ExpectedTime','oldVal':'','newVal':'" + Currentobj.ExpectedTime + "'}",
                            "{'columnName':'Description','oldVal':'','newVal':'" + Currentobj.Description + "'}",
                            "{'columnName':'Summary','oldVal':'','newVal':'" + Currentobj.Summary + "'}",
                            "{'columnName':'PriorityID','oldVal':'','newVal':'" +
                            _unitOfWork.PriorityRepository.GetByID(Currentobj.PriorityID).Name + "'}",
                            "{'columnName':'StatusID','oldVal':'','newVal':'" +
                            _unitOfWork.StatusRepository.GetByID(Currentobj.StatusID).Name + "'}"
                        };
                    }
 else if (Currentobj.EndDate != null)
 {
     JsonList = new List<string>()
                    {

                        "{'columnName':'IsNewTask','oldVal':'','newVal':'" + true + "'}",
                        "{'columnName':'EmpID','oldVal':'','newVal':'" + Currentobj.EmpID + "'}",
                          (Currentobj.ProjectID != null)? 
                       "{'columnName':'ProjectID','oldVal':'','newVal':'" +  new EtaskMinstry.Models.Project.ProjectDisplay().GetById(Currentobj.ProjectID.Value).Name + "'}":
                       "{'columnName':'ProjectID','oldVal':'','newVal':''}",
                        "{'columnName':'StartDate','oldVal':'','newVal':''}",
                        "{'columnName':'EndDate','oldVal':'','newVal':'" +
                        Extentions.ToGregDatediff(Currentobj.EndDate.Value) + "'}",
                        "{'columnName':'ExpectedTime','oldVal':'','newVal':'" + Currentobj.ExpectedTime + "'}",
                        "{'columnName':'Description','oldVal':'','newVal':'" + Currentobj.Description + "'}",
                        "{'columnName':'Summary','oldVal':'','newVal':'" + Currentobj.Summary + "'}",
                        "{'columnName':'PriorityID','oldVal':'','newVal':'" +
                        _unitOfWork.PriorityRepository.GetByID(Currentobj.PriorityID).Name + "'}",
                        "{'columnName':'StatusID','oldVal':'','newVal':'" +
                        _unitOfWork.StatusRepository.GetByID(Currentobj.StatusID).Name + "'}"
                    };
 }
 else
 {
     JsonList = new List<string>()
     {
         "{'columnName':'IsNewTask','oldVal':'','newVal':'" + true + "'}",
         "{'columnName':'EmpID','oldVal':'','newVal':'" + Currentobj.EmpID + "'}",
           (Currentobj.ProjectID != null)? 
                       "{'columnName':'ProjectID','oldVal':'','newVal':'" +  new EtaskMinstry.Models.Project.ProjectDisplay().GetById(Currentobj.ProjectID.Value).Name + "'}":
                       "{'columnName':'ProjectID','oldVal':'','newVal':''}",
         "{'columnName':'StartDate','oldVal':'','newVal':''}",
         "{'columnName':'EndDate','oldVal':'','newVal':''}",
         "{'columnName':'ExpectedTime','oldVal':'','newVal':'" + Currentobj.ExpectedTime + "'}",
         "{'columnName':'Description','oldVal':'','newVal':'" + Currentobj.Description + "'}",
         "{'columnName':'Summary','oldVal':'','newVal':'" + Currentobj.Summary + "'}",
         "{'columnName':'PriorityID','oldVal':'','newVal':'" +
         _unitOfWork.PriorityRepository.GetByID(Currentobj.PriorityID).Name + "'}",
         "{'columnName':'StatusID','oldVal':'','newVal':'" +
         _unitOfWork.StatusRepository.GetByID(Currentobj.StatusID).Name + "'}"
     };

 }

                //"{'columnName':'StatusID','oldVal':'','newVal':'" + Currentobj.Status.Name + "'}"
                //"{'columnName':'PriorityID','oldVal':'','newVal':'" + Currentobj.Priority.Name + "'}"
            }
            catch (Exception)
            {
            }

            //if there're fields had changed insert new log for updated items.
            try
            {
                string logVal = "[" + String.Join(",", JsonList) + "]";
                Log(Currentobj.TaskID, logVal, MvcApplication.userData.isCompany, Currentobj.PriorityID,Currentobj.StatusID);
            }
            catch (Exception)
            {
            }
        }

     
        /// <summary>
        /// Log Inserted Object.
        /// </summary>
        /// <param name="Currentobj"></param>
        private static void LogDelete(Task Currentobj)
        {
            //UnitOfWork _unitOfWork =
            //    new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            //the list to save json
            List<string> JsonList = new List<string>();
            try
            {
                JsonList = new List<string>()
                    {
                         "{'columnName':'IsDeletedTask','oldVal':'','newVal':'" + true + "'}"
                    };

                //"{'columnName':'StatusID','oldVal':'','newVal':'" + Currentobj.Status.Name + "'}"
                //"{'columnName':'PriorityID','oldVal':'','newVal':'" + Currentobj.Priority.Name + "'}"
            }
            catch (Exception)
            {
            }

            //if there're fields had changed insert new log for updated items.
            try
            {
                string logVal = "[" + String.Join(",", JsonList) + "]";
                Log(Currentobj.TaskID, logVal, MvcApplication.userData.isCompany, Currentobj.PriorityID,Currentobj.StatusID);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// insert log Recored.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="value"></param>
        /// <param name="isForCompany"></param>

        private static void Log(int taskId, string value, bool isForCompany, int PriorityId, int StatusId)
        {
            UnitOfWork _unitOfWork =
                new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            TaskManagementModel.TaskLog logObj = new TaskManagementModel.TaskLog
                {
                    LogDate = DateTime.Now,
                    TaskID = taskId,
                    Value = value,
                    IsFromCompany = isForCompany,
                    CurrentPriority=PriorityId,
                    CurrentStatus= StatusId
                };
            _unitOfWork.TaskLogRepository.Insert(logObj);
            _unitOfWork.Save();
          //  _unitOfWork.Dispose();
        }

        /// <summary>
        /// log Comment object
        /// </summary>
        /// <param name="Currentobj"></param>
        /// <param name="beforeUpdate"></param>
        public static void LogAddComment(int TaskID, TaskManagementModel.TaskComment objComment)
        {
            LogTaskSingleValue(TaskID, objComment.Comment, "TaskCommentID");
        }

        /// <summary>
        /// log Attachment object
        /// </summary>
        /// <param name="Currentobj"></param>
        /// <param name="beforeUpdate"></param>
        public static void LogAddAttachment(int TaskID, TaskManagementModel.Attachment objAttachment)
        {

            LogTaskSingleValue(TaskID, objAttachment.FileName, "AttachmentID");
        }

        /// <summary>
        /// Log Task Add Value for Comment and Attachment 
        /// </summary>
        /// <param name="TaskID"></param>
        /// <param name="value"></param>
        /// <param name="strColName"></param>
        private static void LogTaskSingleValue(int TaskID, string value, string strColName)
        {
            string logVal = "[{'columnName':'" + strColName + "','oldVal':'','newVal':'" + value + "'}]";
            UnitOfWork _unitOfWork =
                new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var obj = _unitOfWork.TaskRepository.GetByID(TaskID);
            Log(TaskID, logVal, MvcApplication.userData.isCompany,obj.PriorityID,obj.StatusID);
        }

    }
}