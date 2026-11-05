using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry;
using Resources;
using TaskManagementModel;
using EtaskMinstry.AppCode;

namespace EtaskMinstry.Models.TaskCommon
{
    public class TaskAddEditCommon
    {
        #region "Properties"

        // Task ID .
        public int TaskID { get; set; }

        // Task Title .
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Remote("checkDublicated", "TaskCommon", AdditionalFields = "TaskID",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages),
            ErrorMessageResourceName = "DublicatedItem")]
        [Display(Name = "عنوان المهمة")]
        public String Title { get; set; }

        // Task Description .
        [RegularExpression(ValidationResource.RevStringmax, ErrorMessageResourceName = "ValidRegulaDetail",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Display(Name = "الوصف")]
        public String Description { get; set; }

        // Task Summary .
        [RegularExpression(ValidationResource.RevString350, ErrorMessageResourceName = "ValidRegularSummary350",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Display(Name = "الملخص")]
        public String Summary { get; set; }

        // Task Start Date .
        [Display(Name = "تاريخ البداية")]
        public String StartDate { get; set; }

        // Task End Date .
        [Display(Name = "تاريخ النهاية")]
        public String EndDate { get; set; }

        // Task Create Date .
        public DateTime CreatedDate { get; set; }

        public int? Createdby { get; set; }

        // Expected Time .
        
        [RegularExpression(ValidationResource.ExtpectedTime, ErrorMessageResourceName = "ExpectedTime",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Display(Name = "الوقت المقدر")]
        public Decimal? ExpectedTime { get; set; }

        // Time Unit For Expected Time .       
        public int? TimeUnitID { get; set; }

        // Actual Time .
        public Decimal ActualTime { get; set; }

        // Employee ID.
        [Display(Name = "إسناد المهمة لـ")]
        public int? EmpID { get; set; }

        // Company ID.
        public int CompanyID { get; set; }

        // Is Archived .
        [Display(Name = "نقل إلي الأرشيف")]
        public Boolean IsArchived { get; set; }

        // Task Priority .
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Display(Name = "الأهمية")]
        public int PriorityID { get; set; }

        public String Priority { get; set; }

        // Task Status .
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof (ValidationMessage.ValidationMessages))]
        [Display(Name = "الحالة")]
        public int StatusID { get; set; }

        public String Status { get; set; }

        // Task Project .
        [Display(Name = "المشروع")]
        public int? ProjectID { get; set; }


        public TaskManagementModel.Project Project { get; set; }

        // Task Attachments .
        public List<TaskManagementModel.Attachment> Attachment { get; set; }

        // Files Description .
        public String FilesDescription { get; set; }

        // Files Description .
        public String RemovedFiles { get; set; }

        // Task Saved or Not TO Show PopUp .
        public Boolean? Saved { get; set; }
        // TO Show PopUp Recaptchanotcheck .
        public Boolean? RecaptchCheck { get; set; }
        #endregion

        #region "Task Manage"

        private UnitOfWork _unitOfWork;

        public TaskAddEditCommon()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        /// <summary>
        /// Save Task .
        /// </summary>
        /// <returns> Boolean </returns>
        public int Save()
        {

            int result = 0;

            // Insert New Task .
            if (TaskID == 0)
            {
                var objTask = new TaskManagementModel.Task()
                    {
                        ActualTime = ActualTime,
                        CompanyID = MvcApplication.userData.CompanyId.Value,
                        CreatedDate = DateTime.Now,
                        Description = Description,
                        EmpID = EmpID,
                        ExpectedTime = ExpectedTime.HasValue ? ExpectedTime : 0,
                        IsArchived = IsArchived,
                        PriorityID = PriorityID,
                        Project =  Project,
                        ProjectID = ProjectID,
                        StatusID = (int) TaskStatus.New,
                        Summary = Summary,
                        Createdby=!MvcApplication.userData.isCompany ? MvcApplication.userData.userId : (int?)null,
                        TimeUnitID = TimeUnitID.HasValue ? TimeUnitID : null,                       
                        Title = Title
                    };
          
                if (!String.IsNullOrEmpty(EndDate))
                        objTask.EndDate = (MvcApplication.IsGregDate ? EndDate.ToGregExactformate() : EndDate.ToGregExact());

                if (!String.IsNullOrEmpty(StartDate))
                    objTask.StartDate =(MvcApplication.IsGregDate ? StartDate.ToGregExactformate() : StartDate.ToGregExact()) ;

                //if (EndDate != null && StartDate != null && EndDate == StartDate && !ExpectedTime.HasValue)
                //{
                //    //objTask.ExpectedTime = 24;
                //    //objTask.TimeUnitID = 4;
                //}
                _unitOfWork.TaskRepository.Insert(objTask);
                _unitOfWork.Save();

                if (objTask.EmpID.HasValue)
                    NotificationHub.Send(Users.Employee(objTask.EmpID.Value), NotificationType.NewTask,
                                         "تم اسناد المهمة ' " + objTask.Title +
                                         " ' لك ", "/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                //Common
                //if(!MvcApplication.userData.isCompany)
                //{
                //    NotificationHub.Send(Users.Company(MvcApplication.userData.CompanyId.Value), NotificationType.NewTask,
                //                            "تم اسناد المهمة ' " + objTask.Title + " ' للموظف " + ServiceManger.GetEmplyeeName((int)objTask.EmpID), "/Employee/Tasks/TaskDetails/" + objTask.TaskID);
                //}

                if (objTask.TaskID > 0)
                {
                    result = objTask.TaskID;
                    Saved = true;
                }
                else
                {
                    Saved = false;
                }
                //log
                AppCode.LogTask.Log(objTask, null);
                _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                {
                    TaskID = objTask.TaskID,
                    CreatedDate = DateTime.Now,
                    EmpID = objTask.EmpID,
                    StatusID = (int)TaskStatus.New
                });
                _unitOfWork.Save();
            }
            else // Update Task 
            {
                var objTask = _unitOfWork.TaskRepository.GetByID(TaskID);

                if (objTask != null)
                {
                    var beforeUpdateObj = objTask.Clone<TaskManagementModel.Task>();
                    objTask.ActualTime = ActualTime;
                    objTask.Description = Description;
                    objTask.Summary = Summary;
                    objTask.EmpID = EmpID;

                    if (String.IsNullOrEmpty(EndDate))
                        objTask.EndDate = null;
                    else
                        objTask.EndDate =(MvcApplication.IsGregDate ? EndDate.ToGregExactformate() :EndDate.ToGregExact()) ;

                    objTask.ExpectedTime = ExpectedTime;
                    objTask.IsArchived = IsArchived;
                    objTask.PriorityID = PriorityID;
                    objTask.ProjectID = ProjectID;

                    if (String.IsNullOrEmpty(StartDate))
                        objTask.StartDate = null;
                    else
                        objTask.StartDate =(MvcApplication.IsGregDate? StartDate.ToGregExactformate():StartDate.ToGregExact());

                    objTask.Summary = Summary;
                    objTask.TimeUnitID = TimeUnitID.Value;
                    objTask.Title = Title;

                    _unitOfWork.TaskRepository.Update(objTask);
                    _unitOfWork.Save();
                    result = objTask.TaskID;
                    Saved = true;

                    //log.
                    AppCode.LogTask.Log(objTask, beforeUpdateObj);
                  
                }
                else
                {
                    Saved = false;
                }
            }
            return result;
        }

        public TaskAddEditCommon Select(object iID)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(iID);

            if (objTask == null)
                return null;

            return new TaskAddEditCommon()
                {
                    CompanyID = MvcApplication.userData.userId,
                    ProjectID = objTask.ProjectID,
                    PriorityID = objTask.PriorityID,
                    CreatedDate = objTask.CreatedDate,
                    Description = objTask.Description,
                    EmpID = objTask.EmpID.Value,
                    EndDate = objTask.EndDate.HasValue ? MvcApplication.IsGregDate ? objTask.EndDate.Value.ToGregDate() : objTask.EndDate.Value.ToHijriDate() : "",
                    ExpectedTime = objTask.ExpectedTime.HasValue ? objTask.ExpectedTime : null,
                    IsArchived = objTask.IsArchived,
                    StartDate = objTask.StartDate.HasValue ? MvcApplication.IsGregDate ? objTask.StartDate.Value.ToGregDate() : objTask.StartDate.Value.ToHijriDate() : "",
                    StatusID = objTask.StatusID,
                    TimeUnitID = objTask.TimeUnitID.Value,
                    Summary = objTask.Summary,
                    Title = objTask.Title,
                    TaskID = objTask.TaskID,
                    Attachment = objTask.Attachments.ToList()
                };
        }

       /// <summary>
        /// Check Task Name 
       /// </summary>
       /// <param name="TaskID"></param>
       /// <param name="Title"></param>
       /// <returns></returns>
        public bool CheckDublication(int? TaskID, string Title)
        {
            return
                _unitOfWork.TaskRepository.Get(
                    i =>
                    i.Title.ToLower() == Title.ToLower().Trim()
                    &&
                    i.TaskID != TaskID
                    &&
                    i.CompanyID == MvcApplication.userData.CompanyId.Value
                    &&
                    !i.IsDeleted).Any();
        }

        /// <summary>
        /// To Get Tasks By Project ID .
        /// </summary>
        /// <param name="ProjectID"></param>
        /// <returns></returns>
        public List<TaskAddEditCommon> GetTasksByProject(int ProjectID)
        {
            return _unitOfWork.TaskRepository.Get(filter: i => i.ProjectID == ProjectID && !i.IsDeleted, includeProperties: "Priority,Status")
                              .Select(
                                  i => new TaskAddEditCommon()
                                      {
                                          TaskID = i.TaskID,
                                          Title = i.Title,
                                          Priority = i.Priority.Name,
                                          PriorityID = i.PriorityID,
                                          Status = i.Status.Name
                                      }).ToList();
        }

        public TaskManagementModel.Task gettask(int ID)
        {
            var objTask = _unitOfWork.TaskRepository.GetByID(ID);
            return objTask;
        }
       
        #endregion
    }
}