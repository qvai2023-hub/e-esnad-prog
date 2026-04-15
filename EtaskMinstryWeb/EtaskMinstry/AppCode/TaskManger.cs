using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EtaskMinstry;
using EtaskMinstry.App_Code;
using EtaskMinstry.Areas.Employee.Models.EmployeeTask;
using EtaskMinstry.Models.Attachment;
using EtaskMinstry.Models.EmployeeTask;
using EtaskMinstry.Models.Holiday;
using EtaskMinstry.Models.WeekEnd;
using Microsoft.AspNet.SignalR.Hubs;
using TaskManagementModel;

namespace EtaskMinstry.AppCode
{
    public class TaskManger
    {
        /// <summary>
        /// constructor region
        /// </summary>
        private UnitOfWork _unitOfWork;
        public TaskManger()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        #region  U P D A T E  T A S K  S T A T U S  O N L O A D

        public static void UpdateTaskStatus()
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            //     UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.AppSettings["ETaskEntities"].ToString());         
            if (MvcApplication.userData.isCompany)
            {
                //var Tasks =   _unitOfWork.TaskRepository.Get(t=>t.CompanyID == MvcApplication.userData.userId && (t.StartDate.HasValue ?t.StartDate.Value.Date <= DateTime.Now.Date : false && t.StatusID== (int)TaskStatus.Accepted ) && !t.IsArchived);
                var Tasks = _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId && t.StatusID == (int)TaskStatus.Accepted && !t.IsArchived);
                foreach (var item in Tasks)
                {
                    var date = item.StartDate;
                    if (item.StartDate.HasValue)
                    {

                        if (item.StartDate.Value.Date <= DateTime.Now.Date)
                        {
                            item.StatusID = (int)TaskStatus.Inprogress;
                        }

                    }
                }

                _unitOfWork.Save();
            }
            else
            {
                var Tasks = _unitOfWork.TaskRepository.Get(t => t.EmpID == MvcApplication.userData.userId && t.StatusID == (int)TaskStatus.Accepted && !t.IsArchived);
                foreach (var item in Tasks)
                {
                    var date = item.StartDate;
                    if (item.StartDate.HasValue)
                    {

                        if (item.StartDate.Value.Date <= DateTime.Now.Date)
                        {
                            item.StatusID = (int)TaskStatus.Inprogress;
                        }



                    }
                }

                _unitOfWork.Save();
            }

        }



        #endregion

        #region C O M P A N Y  A C T I O N S


        /// <summary>
        /// Handle PendTask 
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool PendTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.Pending;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.Pending
                });
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Pend  iTaskID  objTask.EmpID
                if ((objTask.EmpID.HasValue) && objTask.StatusID != (int)TaskStatus.Rejected)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, " تم أيقاف المهمة : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم أيقاف المهمة  : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle PendTask 
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static string UnPendTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {

                //    //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();

                int lastStatus = objTask.TaskTLogs.LastOrDefault(l => l.StatusID != (int)TaskStatus.Pending) == null ? (int)TaskStatus.New : objTask.TaskTLogs.LastOrDefault(l => l.StatusID != (int)TaskStatus.Pending).StatusID;

                //Update Task Status 
                objTask.StatusID = lastStatus;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = lastStatus
                });
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  UnPend  iTaskID  objTask.EmpID

                if ((objTask.EmpID.HasValue) && objTask.StatusID != (int)TaskStatus.Rejected)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم اعاده تفعيل المهمة : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم اعاده تفعيل المهمة : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);
                return objTask.Status.Name;

            }

            return " ";
        }

        /// <summary>
        /// Handle AssignTask 
        /// check if Assign or Reassign
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <param name="iEmployeeID"></param>
        /// <returns></returns>
        public static bool AssignTask(int iTaskID, int iEmployeeID)
        {
            //Define Unit ofWork 
            bool isReassign = false;
            //bool return if save succes
            bool success = false;
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);

            if (objTask != null) //check if Task not null
            {
                if (objTask.StatusID== (int)TaskStatus.Inprogress)
                {
                    success= ReassignInprogressTask(iTaskID, iEmployeeID);
                }
                else
                { 
                //Check if task has already Emloyee i.e. Reassign Case or first assignation 
                // IF isReassign==false  "غير مسندة"  , isReassign==true  "متوقفة"
                isReassign = objTask.EmpID == null ? false : objTask.EmpID == iEmployeeID ? false : true;
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();

                //Update Emp ID 
                objTask.EmpID = iEmployeeID;
                //Status will be new @newlyAssigned Employee 
                if (isReassign) objTask.StatusID = (int)TaskStatus.New;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                //If reassign case stop task @current assigned employee
                if (isReassign)
                {
                    _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                    {
                        TaskID = iTaskID,
                        CreatedDate = DateTime.Now,
                        EmpID = beforeUpdateObj.EmpID,
                        StatusID = (int)TaskStatus.Pending,
                    });
                    //TODO: ADD NOTIFICATION  Pend  iTaskID  objTask.EmpID
                    if (beforeUpdateObj.EmpID.HasValue)
                        NotificationHub.Send(Users.Employee(beforeUpdateObj.EmpID.Value), NotificationType.NewTask, "تم ايقاف المهمة: " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                }

                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = iEmployeeID,
                    StatusID = (int)TaskStatus.New,
                });
                _unitOfWork.Save();
                success = true;
                //TODO: ADD NOTIFICATION  Assign  iTaskID  iEmployeeID
                NotificationHub.Send(Users.Employee(iEmployeeID), NotificationType.NewTask, "تم اسناد المهمة : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم اعاده اسناد  المهمة : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

                //return isReassign;
                return success;
            }
            }
            return success;
        }

        public static bool ReassignInprogressTask(int iTaskID, int iEmployeeID)
        {
            bool success = false;
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null)
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.Pending;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.Pending
                });
                _unitOfWork.Save();
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم ايقاف المهمة: " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                if (iEmployeeID != null)
                {
                    // var newTask = objTask.Clone<TaskManagementModel.Task>();
                    var newTask = new Task();

                    newTask.CompanyID = objTask.CompanyID;
                    newTask.Description = objTask.Description;
                    newTask.ExpectedTime = objTask.ExpectedTime.HasValue ? objTask.ExpectedTime : 0;
                    newTask.IsArchived = objTask.IsArchived;
                    newTask.Project = objTask.Project;
                    newTask.ProjectID = objTask.ProjectID;
                    newTask.Summary = objTask.Summary;
                    newTask.TimeUnitID = objTask.TimeUnitID.HasValue ? objTask.TimeUnitID : null;
                    newTask.Title = objTask.Title;
                    newTask.BriefTaskName = objTask.BriefTaskName;
                    newTask.PriorityID = objTask.PriorityID;

                    newTask.StartDate = objTask.StartDate;
                    newTask.EndDate = objTask.EndDate;

                    newTask.CreatedDate = DateTime.Now;
                    newTask.EmpID = iEmployeeID;
                    newTask.StatusID = (int)TaskStatus.New;

                    _unitOfWork.TaskRepository.Insert(newTask);
                    _unitOfWork.Save();
                    //log
                    AppCode.LogTask.Log(newTask, objTask);
                    _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                    {
                        TaskID = newTask.TaskID,
                        CreatedDate = DateTime.Now,
                        EmpID = newTask.EmpID,
                        StatusID = (int)TaskStatus.New
                    });
                    _unitOfWork.Save();

                    success = true;
                    //TODO: ADD NOTIFICATION  Assign  iTaskID  iEmployeeID
                    NotificationHub.Send(Users.Employee(iEmployeeID), NotificationType.NewTask, "تم اسناد المهمة : " + newTask.Title, @"/Employee/Tasks/TaskDetails/" + newTask.TaskID);
                    NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم اعاده اسناد  المهمة : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);
                    //Attachments
                    var uploadedFiles = _unitOfWork.AttachmentRepository.Get().Where(i => i.TaskID == objTask.TaskID).ToList();

                    // Insert Attachments .
                    for (int j = 0; j < uploadedFiles.Count; j++)
                    {
                        var objAttachment = new AttachmentDisplay()
                        {
                            TaskID = newTask.TaskID,
                            FileName = uploadedFiles[j].FileName,
                            Description = uploadedFiles[j].Description,
                            OriginalFileName = uploadedFiles[j].OriginalFileName
                        };

                        new AttachmentDisplay().Insert(objAttachment);

                        // Log Attachment .
                        LogTask.LogAddAttachment(iTaskID, new Attachment()
                        {
                            AttachmentID = objAttachment.ID,
                            Description = objAttachment.Description,
                            FileName = objAttachment.FileName,
                            TaskID = newTask.TaskID
                        });
                    }
                    return success;
                }
            }
            return success;
        }


        /// <summary>
        /// Company Evaluate and Reject Task 
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool CompanyRejectTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.NotAproved;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.NotAproved
                });
                _unitOfWork.Save();

                //TODO: ADD NOTIFICATION  Reject  iTaskID  objTask.EmpID
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم رفض المهمة من قبل الشركه  : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم رفض المهمة من قبل الشركه  : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update Task Info
        /// proirity startTime , ExpectedEndTime  , Expected Time , 
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool EditTask(int Taskid, int ProrityID, string StartDate, string EndDate, decimal duration
            , int? timeUnitID, int? projectID, string desc)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(Taskid);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Info
                objTask.PriorityID = ProrityID;
                if (!string.IsNullOrEmpty(StartDate))
                    objTask.StartDate = DateTime.ParseExact((MvcApplication.IsGregDate ? StartDate : StartDate.HijriToGregDate()), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(EndDate))
                    objTask.EndDate = DateTime.ParseExact((MvcApplication.IsGregDate ? EndDate : EndDate.HijriToGregDate()), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                objTask.ExpectedTime = duration;
                objTask.TimeUnitID = timeUnitID;
                if (desc != null)
                    objTask.Description = desc;
                if (projectID.HasValue && projectID != 0)
                    objTask.ProjectID = _unitOfWork.ProjectRepository.GetByID(projectID).ProjectID;
                else
                    objTask.ProjectID = null;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Update  iTaskID  objTask.EmpID
                if ((objTask.EmpID.HasValue) && objTask.StatusID != (int)TaskStatus.Rejected)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم تعديل المهمة  : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم تعديل المهمة :" + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Validate Task Duration
        /// </summary>
        /// <param name="dtStartDate"></param>
        /// <param name="dtEndDate"></param>
        /// <param name="iTimeCount"></param>
        /// <param name="iTimeUnit"></param>
        /// <returns></returns>
        public static Boolean validateTaskDuration(String dtStartDate, String dtEndDate, decimal iTimeCount, int? iTimeUnit)
        {
            if (dtStartDate != "" && dtEndDate != "" && iTimeUnit != null)
            {
                DateTime StartDate = DateTime.ParseExact((MvcApplication.IsGregDate ? dtStartDate : dtStartDate.HijriToGregDate()), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime EndDate = DateTime.ParseExact((MvcApplication.IsGregDate ? dtEndDate : dtEndDate.HijriToGregDate()), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                decimal iTotalDays = (decimal)(EndDate - StartDate).TotalDays + 1;
                // Get Holidays .
                int Holidays = 0;
                // Get All Company WeekEnds .
                List<WeekEndDisplayVM> lstWeekEnds = new WeekEndDisplayVM().GetByCompany(1);
                // Get All Company HoliDays .
                List<HolidayDisplayVM> lstHoliDays = new HolidayDisplayVM().GetByCompany(1);
                for (int i = 0; i < iTotalDays; i++)
                {
                    DateTime dtToCheckDate = StartDate.AddDays(i);
                    // HoliDays .
                    if (lstHoliDays.Any(d => dtToCheckDate >= d.StartDate && dtToCheckDate <= d.EndDate))
                        Holidays++;
                    else // Week Ends .
                        Holidays += lstWeekEnds.Count(w =>
                                                      dtToCheckDate.DayOfWeek ==
                                                      (DayOfWeek)Enum.Parse(typeof(DayOfWeek), w.WeekEndDay.ToString()));
                }
                iTotalDays = iTotalDays - Holidays;
                switch (iTimeUnit)
                {
                    case (int)TimeUnit.Month:
                        if (iTimeCount > iTotalDays / ((int)TimeUnitValue.Day * (int)TimeUnitValue.Week + 2))
                            return false;
                        break;

                    case (int)TimeUnit.Week:
                        if (iTimeCount > iTotalDays / (int)TimeUnitValue.Day)
                            return false;
                        break;

                    case (int)TimeUnit.Day:
                        if (iTimeCount > iTotalDays)
                            return false;
                        break;

                    case (int)TimeUnit.Hour:
                        if (iTimeCount > iTotalDays * (int)TimeUnitValue.Hour)
                            return false;
                        break;

                    case (int)TimeUnit.Minute:
                        if (iTimeCount > iTotalDays * (int)TimeUnitValue.Hour * (int)TimeUnitValue.Minute)
                            return false;
                        break;
                }
                return true;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// CompanyAcceptTask
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool CompanyAcceptTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.Approved;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.Approved
                });
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Accept  iTaskID  objTask.EmpID
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم قبول المهمة من قبل الشركه :  " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم قبول المهمة من قبل الشركة : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }

            return false;
        }

        /// <summary>
        /// ArcheiveTask
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool ArcheiveTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Set IsArcheived  = true
                //objTask.IsAccepted = true;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);

                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Archeive  iTaskID  objTask.EmpID
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم ارشفة المهمة :" + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم ارشفة المهمة : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }

            return false;
        }

        /// <summary>
        /// HideComment
        /// </summary>
        /// <param name="iCommentID"></param>
        /// <returns></returns>
        public static bool HideComment(int iCommentID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //Get Comment Object
            var objComment = _unitOfWork.TaskCommentRepository.GetByID(iCommentID);
            if (objComment != null) //check if Task not null
            {
                //Update Comment Status 
                objComment.CommentStatusId = (int)TaskCommentStatus.Hidden;
                //Update Task 
                _unitOfWork.TaskCommentRepository.Update(objComment);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// ShowComment
        /// </summary>
        /// <param name="iCommentID"></param>
        /// <returns></returns>
        public static bool ShowComment(int iCommentID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //Get Comment Object
            var objComment = _unitOfWork.TaskCommentRepository.GetByID(iCommentID);
            if (objComment != null) //check if Task not null
            {
                //Update Comment Status 
                objComment.CommentStatusId = (int)TaskCommentStatus.Shown;
                //Update Task 
                _unitOfWork.TaskCommentRepository.Update(objComment);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Move Task To Archive .
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public Boolean Archive(int iTaskID)
        {
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            var obj = _unitOfWork.TaskRepository.GetByID(iTaskID);

            if (obj != null)
            {
                var beforeUpdate = obj.Clone();

                if (obj.StatusID == (int)TaskStatus.Approved)
                {
                    obj.IsArchived = true;
                    _unitOfWork.TaskRepository.Update(obj);
                    _unitOfWork.Save();
                }
                AppCode.LogTask.Log(obj, beforeUpdate);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Accept and Refused Task .
        /// </summary>
        /// <param name="iTaskID"> Task ID </param>
        /// <param name="taskStatus"> Task Status </param>
        /// <returns>Boolean</returns>
        public Boolean AcceptRefusedTask(int iTaskID, TaskStatus taskStatus)
        {
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());

            if (taskStatus == TaskStatus.Approved || taskStatus == TaskStatus.NotAproved)
            {
                var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);

                if (objTask != null)
                {
                    var beforeUpdate = objTask.Clone();
                    objTask.StatusID = (int)taskStatus;
                    _unitOfWork.TaskRepository.Update(objTask);

                    _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                    {
                        TaskID = iTaskID,
                        CreatedDate = DateTime.Now,
                        EmpID = objTask.EmpID,
                        StatusID = (int)taskStatus
                    });

                    // Send Notification To Employee .
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), (taskStatus == TaskStatus.Approved
                                                                                   ? NotificationType.Approved
                                                                                   : NotificationType.NotApproved),
                                         (taskStatus == TaskStatus.Approved
                                              ? objTask.Title + " : تم إعتماد المهمة "
                                              : objTask.Title + " :تم رفض المهمة "), taskStatus == TaskStatus.Approved ? "/Employee/Tasks/TaskDetails/" + iTaskID : "/Employee/Tasks/");
                    _unitOfWork.Save();

                    //log
                    AppCode.LogTask.Log(objTask, beforeUpdate);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Change Task Priority .
        /// </summary>
        /// <param name="iTaskID">Task ID</param>
        /// <param name="iPriorityID"> New Priority ID</param>
        /// <returns></returns>
        public Boolean ChangePriority(int iTaskID, int iPriorityID)
        {
            var obj = _unitOfWork.TaskRepository.GetByID(iTaskID);
            var beforeUpdate = obj.Clone();
            if (obj != null)
            {
                if (obj.EmpID.HasValue)
                {
                    NotificationHub.Send(Users.Employee(obj.EmpID.Value), NotificationType.ChangePriority,
                                         " تم تغيير الأولوية من " +
                                         obj.Priority.Name +
                                         " إلى " +
                                         _unitOfWork.PriorityRepository.GetByID(iPriorityID).Name +
                                         " فى المهمة " +
                                         obj.Title
                                         , "");
                }

                obj.PriorityID = iPriorityID;

                _unitOfWork.TaskRepository.Update(obj);
                _unitOfWork.Save();

                //log
                AppCode.LogTask.Log(obj, beforeUpdate);

                return true;
            }
            return false;
        }

        #endregion

        #region E M P L O Y E E  A C T I O N S

        /// <summary>
        /// Employee Accept Task 
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool EmpAcceptTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.Accepted;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                    {
                        TaskID = iTaskID,
                        CreatedDate = DateTime.Now,
                        EmpID = objTask.EmpID,
                        StatusID = (int)TaskStatus.Accepted
                    });

                if (objTask.StartDate <= DateTime.Now)
                {
                    //Update Task Status to be Inprogree 
                    objTask.StatusID = (int)TaskStatus.Inprogress;
                    //Log TaskRecord
                    EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);

                    _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                        {
                            TaskID = iTaskID,
                            CreatedDate = DateTime.Now,
                            EmpID = objTask.EmpID,
                            StatusID = (int)TaskStatus.Inprogress
                        });
                }

                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Emp Accept  iTaskID  objTask.EmpID 
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask,
                                       " تم قبول المهمة من قبل الموظف : " + objTask.Title + "::" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::",
                                         @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask,
                                   " تم قبول المهمة من قبل الموظف : " + objTask.Title + "::" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::",
                                     @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Employee Reject Task
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool EmpRejectTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.Rejected;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.Rejected
                });
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Emp Reject  iTaskID  objTask.EmpID
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم رفض المهمة من قبل الموظف :" + objTask.Title + "::" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::", @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم رفض المهمة من قبل الموظف :" + objTask.Title + "::" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::", @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Employee FinishTask
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool EmpFinishTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                //Update Task Status 
                objTask.StatusID = (int)TaskStatus.Done;
                objTask.DeliverDate = DateTime.Now;
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.Done
                });
                _unitOfWork.Save();
                //TODO: ADD NOTIFICATION  Emp Finish  iTaskID  objTask.EmpID   
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم انهاء المهمة من قبل الموظف :" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value)  + "::" + objTask.Title + "::", @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, " تم انهاء المهمة من قبل الموظف :" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value)  + "::" + objTask.Title + "::", @"/company/company/TaskDetails/" + objTask.TaskID);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Employee Update Task Time
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <param name="timeValue"></param>
        /// <returns></returns>
        public static string EmpUpdateDalyTaskTime(int iTaskID, decimal timeValue)
        {
            //Define Unit ofWork
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {

                decimal totalDalyTime = 0;

                // Find today's time log for the current employee and update it
                var todayLog = _unitOfWork.TaskStatuseLog.Get(filter: t =>
                    t.TaskID == iTaskID &&
                    t.EmpID == MvcApplication.userData.userId &&
                    System.Data.Entity.DbFunctions.TruncateTime(t.CreatedDate) == System.Data.Entity.DbFunctions.TruncateTime(DateTime.Now)
                ).OrderByDescending(t => t.CreatedDate).FirstOrDefault();

                if (todayLog != null)
                {
                    todayLog.TimeCount = timeValue;
                    _unitOfWork.TaskStatuseLog.Update(todayLog);
                }

                _unitOfWork.Save();

                // Recalculate total ActualTime from all time logs
                Dictionary<string, TaskTimeDetails> uniqueTimeLog = objTask.GetTaskTimeLog;

                if (uniqueTimeLog.Count() < 1)
                {
                    objTask.ActualTime = timeValue;
                }
                else
                {
                    objTask.ActualTime = 0;
                    foreach (var itemTime in uniqueTimeLog.Values)
                    {
                        objTask.ActualTime += Decimal.Parse(itemTime.LogTime);
                    }
                }

                _unitOfWork.TaskRepository.Update(objTask);
                _unitOfWork.Save();

                // Return today's time for the current employee
                uniqueTimeLog = objTask.GetTaskTimeLog;
                foreach (var itemTime in uniqueTimeLog.Values)
                {
                    if (itemTime.isToday)
                    {
                        if (itemTime.empId == MvcApplication.userData.userId)
                        {
                            totalDalyTime = timeValue;
                        }
                        else
                        {
                            totalDalyTime = Decimal.Parse(itemTime.LogTime);
                        }
                    }
                }
                return totalDalyTime.ToString();
            }
            else
                return "Error";
        }



        /// <summary>
        /// Employee Update Task Time
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <param name="timeValue"></param>
        /// <returns></returns>
        public static string EmpUpdateTaskTime(int iTaskID, decimal timeValue)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                if (objTask.StatusID == (int)TaskStatus.New)
                    objTask.StatusID = (int)TaskStatus.Inprogress;
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = objTask.StatusID,
                    TimeCount = timeValue,
                    TimUnitID = (int)TimeUnit.Hour
                });
                _unitOfWork.Save();


                //get all emps that work in this task
                Dictionary<string, TaskTimeDetails> uniqueTimeLog = new Dictionary<string, TaskTimeDetails>();

                uniqueTimeLog = objTask.GetTaskTimeLog;
                if (uniqueTimeLog.Count() < 1)
                {
                    objTask.ActualTime = timeValue;
                }
                else
                {
                    objTask.ActualTime = 0;
                    _unitOfWork.Save();
                    foreach (var itemTime in uniqueTimeLog.Values)
                    {

                        objTask.ActualTime += Decimal.Parse(itemTime.LogTime);

                    }
                    //objTask.ActualTime += timeValue;
                }
                // objTask.ActualTime = uniqueTimeLog.Values.Sum();
                //Log TaskRecord
                EtaskMinstry.AppCode.LogTask.Log(objTask, beforeUpdateObj);
                //Update Task 
                _unitOfWork.TaskRepository.Update(objTask);
                //TODO: ADD NOTIFICATION  Emp Start  iTaskID  objTask.EmpID  
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم تعديل الوقت المستغرق من قبل الموظف :" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::" + objTask.Title + "::", @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, " تم تعديل الوقت المستغرق من قبل الموظف :" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value)  + "::" + objTask.Title + "::", @"/company/company/TaskDetails/" + objTask.TaskID);

                _unitOfWork.Save();

                return SpendTimeByDay(objTask.ActualTime);
            }
            return "Error";
        }

        /// <summary>
        /// Start Task Time
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public static bool EmployeeStartTask(int iTaskID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
            if (objTask != null) //check if Task not null
            {
                //Clone Task Object for Log purpose
                var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();

                objTask.StatusID = (int)TaskStatus.Inprogress;
                //Insert TaskStatusLog Record 
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = iTaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = objTask.StatusID,
                });
                //TODO: ADD NOTIFICATION  Emp Start  iTaskID  objTask.EmpID  
                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, "تم بدء المهمة من قبل الموظف :" + objTask.Title + "::" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::", @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, " تم بدء المهمة من قبل الموظف :" + objTask.Title + "::" + ServiceManger.GetEmplyeeName(objTask.EmpID.Value) + "::", @"/company/company/TaskDetails/" + objTask.TaskID);

                _unitOfWork.Save();
                return true;
            }
            return false;
        }


        #endregion

        #region C O M M A N  A C T I O N S
        /// <summary>
        /// AddComment
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <param name="strComment"></param>
        /// <param name="iEmployeeID"></param>
        /// <returns></returns>
        public static int AddComment(int iTaskID, string strComment, int iEmployeeID)
        {
            try
            {
                //Define Unit ofWork 
                UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
                var objTask = _unitOfWork.TaskRepository.GetByID(iTaskID);
                TaskComment objComment = new TaskComment()
               {
                   TaskID = iTaskID,
                   CreatedDate = DateTime.Now,
                   EmpID = iEmployeeID == 0 ? null : (int?)iEmployeeID,
                   Comment = strComment,
                   CommentStatusId = (int)TaskCommentStatus.Shown,
                   IsFromCompany = MvcApplication.userData.isCompany,

               };

                _unitOfWork.TaskCommentRepository.Insert(objComment);
                EtaskMinstry.AppCode.LogTask.LogAddComment(iTaskID, objComment);
                //TODO: ADD NOTIFICATION  AddComment   iTaskID  isFromCompany Comment   
                if ((objTask.EmpID.HasValue) && objTask.StatusID != (int)TaskStatus.Rejected)
                    NotificationHub.Send(Users.Employee((int)objTask.EmpID), NotificationType.NewTask, "تم التعليق على المهمة : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم التعليق على المهمة : " + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

                _unitOfWork.Save();
                return objComment.TaskCommentID;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// ReportComment
        /// </summary>
        /// <param name="iCommentID"></param>
        /// <returns></returns>
        public static bool ReportComment(int iCommentID)
        {
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //Get Comment Object
            var objComment = _unitOfWork.TaskCommentRepository.GetByID(iCommentID);
            if (objComment != null) //check if Task not null
            {
                //Update Comment Status 
                objComment.CommentStatusId = (int)TaskCommentStatus.Reported;
                //Update Task 
                _unitOfWork.TaskCommentRepository.Update(objComment);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// AttachTaskFile
        /// </summary>
        /// <param name="iTaskId"></param>
        /// <param name="strfileName"></param>
        /// <param name="strDescription"></param>
        /// <returns></returns>
        public static bool AttachTaskFile(int iTaskId, string strfileName, string strDescription, string strOriginalFileName = null)
        {
            //Define Unit ofWork
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            var objTask = _unitOfWork.TaskRepository.GetByID(iTaskId);
            Attachment objAttachment = new TaskManagementModel.Attachment()
            {
                Description = strDescription,
                FileName = strfileName,
                OriginalFileName = strOriginalFileName,
                TaskID = iTaskId
            };
            _unitOfWork.AttachmentRepository.Insert(objAttachment);
            EtaskMinstry.AppCode.LogTask.LogAddAttachment(iTaskId, objAttachment);
            //TODO: ADD NOTIFICATION  AddAttachment   iTaskID  isFromCompany AttachmentfileName         
            if ((objTask.EmpID.HasValue) && objTask.StatusID != (int)TaskStatus.Rejected)
                NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask, " تم اضافه مرفق  للمهمة : " + objTask.Title, @"/Employee/Tasks/TaskDetails/" + objTask.TaskID);
            NotificationHub.Send(Users.Company(objTask.CompanyID), NotificationType.NewTask, "تم اضافه مرفق  للمهمة :" + objTask.Title, @"/company/company/TaskDetails/" + objTask.TaskID);

            _unitOfWork.Save();
            return true;

        }

        public static bool GetUserCommmet(int iCommentID)
        {
            bool iscomp = false;
            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //Get Comment Object
            var objComment = _unitOfWork.TaskCommentRepository.GetByID(iCommentID);
            if (objComment != null) //check if Task not null
            { iscomp = objComment.IsFromCompany; }
            return iscomp;

        }

        #endregion

        #region  C H E C K  T A S K
        public static bool IsValidTask(string id)
        {
            var isValid = false;
            try
            {
                //Define Unit ofWork 
                UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
                //GetTaskObject
                var objTask = _unitOfWork.TaskRepository.GetByID(int.Parse(id));
                //check security
                if (EtaskMinstry.MvcApplication.userData.UserTypeId == (int)LoggedUserType.Employee)
                {
                    var tasklogs = objTask.TaskTLogs.Where(l => l.EmpID == EtaskMinstry.MvcApplication.userData.userId);
                    if ((objTask != null && tasklogs.Count() > 0) || (objTask.EmpID == EtaskMinstry.MvcApplication.userData.userId && objTask.StatusID == (int)TaskStatus.New)) //check if Task not null
                    {
                        isValid = objTask.IsDeleted != true;
                    }
                }
                else
                {
                    if (objTask != null) //check if Task not null
                    {
                        isValid = objTask.IsDeleted != true;
                    }
                }

            }
            catch
            { }
            return isValid;
        }

        public static bool CompanyHasTasks()
        {

            //Define Unit ofWork 
            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            //GetTaskObject
            return _unitOfWork.TaskRepository.Get(t => t.CompanyID == MvcApplication.userData.userId).Count() > 0;

        }
        #endregion

        #region H E L P E R    M E T H O D  S
        /// <summary>
        /// to calculate Actual time by hours and days
        /// </summary>
        /// <param name="ActualTime"></param>
        /// <returns></returns>
        public static string SpendTimeByDay(decimal? ActualTime)
        {
            if (ActualTime != null)
            {
                string del = "";
                string strReturn = "";
                if (ActualTime.Value > 24)
                {
                    var Day = Math.Round((ActualTime.Value / 24), 2);
                    del = Day.ToString();
                    if (Day != 0)
                    {
                        strReturn = del.Split('.')[0] + " يوم ";
                    }
                    else
                    {
                        strReturn = del;
                    }
                    var hours = (ActualTime.Value % 24);
                    strReturn += hours + " ساعة ";
                }
                else
                {
                    var math = Math.Round(ActualTime.Value, 2);
                    del = math.ToString();
                    if (math != 0)
                    {
                        strReturn = del + " ساعة ";
                       // strReturn = del.Split('.')[0] + " ساعة ";
                      
                    }
                    else
                    {
                        strReturn = del;
                    }
                }
                return strReturn;
            }
            else
                return "0";
        }
        #endregion

          public Boolean Delete(int TaskID)
        {
            try
            {
            var obj = _unitOfWork.TaskRepository.Delete(TaskID);

                _unitOfWork.Save();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }

}