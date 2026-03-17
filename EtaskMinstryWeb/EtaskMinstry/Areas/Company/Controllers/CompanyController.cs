using EtaskMinstry.Controllers;
using EtaskMinstry.Models;
using EtaskMinstry.Models.Company;
using EtaskMinstry.Models.Priority;
using EtaskMinstry.Models.Project;
using EtaskMinstry.Models.Status;
using EtaskMinstry.Models.TimeUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;
using EtaskMinstry.AppCode;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Configuration;
using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry.Models.Task;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class CompanyController : BaseCompanyController
    {
        //
        // GET: /Company/
        //[ValidateInput(false)]
        //[EncryptedActionParameter]
        public ActionResult Index(int s = 0, bool? isNotAssign = null, bool? isArchive = null)
        {
             EtaskMinstry.AppCode.TaskManger.UpdateTaskStatus();
             ViewData["CompanyTask"] = new CompanyTaskVM().Select(Request.QueryString["t"] == null ? "" : Request.QueryString["t"].ToString(), "", "", "", "", Request.QueryString["s"] == null ? s : int.Parse(Request.QueryString["s"].ToString()), 0, isArchive, isNotAssign, 0, 0);

            ViewBag.statuse = new SelectList(new StatusDisplay().Get().ToList(), "ID", "Name");
            ViewBag.Employee = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "id", "name");
            ViewBag.Project = new SelectList(new ProjectDisplay().Get(), "ID", "Name");
            Generallog.LogView();
            return View();
        }

        /// <summary>
        /// Task Details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EncryptedActionParameter]
        public ActionResult TaskDetails(string id, int isAttach = 0)
        {
            ViewBag.TimeUnit = new SelectList(new EtaskMinstry.Models.TimeUnit.TimeUnitDisplay().Get(), "ID", "Name");
            ViewBag.Emplyees = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "id", "name");
            int? projId= new TaskAddEdit().gettask(int.Parse(id)).ProjectID;
            if(projId != null)
            ViewBag.Project = new SelectList(new ProjectDisplay().Get(), "ID", "Name", projId);
            //     ViewBag.Company = new SelectList(new CompanyVM().GetCompanies(), "CompanyID", "Name",emp.CompanyID);
            else
            ViewBag.Project = new SelectList(new ProjectDisplay().Get(), "ID", "Name");
            //ViewBag.Emplyees = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetAllEmployees(MvcApplication.userData.userId), "employee_Id", "emp_Name");
            if (!TaskManger.IsValidTask(id))
                return RedirectToAction("Index");
            else
                return View(new ComapnyTaskDetailVM().Select(int.Parse(id), isAttach == 1));
        }

        /// <summary>
        /// Task Extension
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EncryptedActionParameter]
        public ActionResult TaskExtensions(string id)
        {
            return PartialView(new TaskExtensionsVM().Select(int.Parse(id)));
        }

        #region T A S K  A C T I O N S

        /// <summary>
        /// Edit Task Data
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="ProrityID"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="Duration"></param>
        /// <param name="timeUnitID"></param>
        /// <returns></returns>
        public string EditTask(int taskID, int ProrityID, string StartDate, string EndDate, decimal Duration,
                               int? timeUnitID, int? projectID,string desc=null)
        {
            int? timeunit = timeUnitID.HasValue ? timeUnitID : null;
            if (validateTaskDuration(StartDate, EndDate, Duration, timeunit))
                return
                    EtaskMinstry.AppCode.TaskManger.EditTask(taskID, ProrityID, StartDate, EndDate, Duration,
                                                               timeunit, projectID, desc) == true
                        ? "Sucess"
                        : "Fail";
            else
                return "DurationError";
        }

        /// <summary>
        /// Validate Task Duration
        /// </summary>
        /// <param name="dtStartDate"></param>
        /// <param name="dtEndDate"></param>
        /// <param name="iTimeCount"></param>
        /// <param name="iTimeUnit"></param>
        /// <returns></returns>
        public bool validateTaskDuration(String dtStartDate, String dtEndDate, decimal iTimeCount, int? iTimeUnit)
        {
            return EtaskMinstry.AppCode.TaskManger.validateTaskDuration(dtStartDate, dtEndDate, iTimeCount, iTimeUnit);
        }

        /// <summary>
        /// Reassign Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="iEmployeeID"></param>
        /// <returns></returns>
        public bool ReAssignTask(int taskID, int iEmployeeID)
        {
            return EtaskMinstry.AppCode.TaskManger.AssignTask(taskID, iEmployeeID);
        }

        /// <summary>
        /// Pend Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public bool PendTask(int taskID)
        {
            return EtaskMinstry.AppCode.TaskManger.PendTask(taskID);
           //return new TaskWorkflow().ChangeTaskStatus(taskID, TaskWorkFlowActions.Pause).IsChanged;
        }

        /// <summary>
        /// Unpend Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public string UnPendTask(int taskID)
        {
            return EtaskMinstry.AppCode.TaskManger.UnPendTask(taskID);
            //return new TaskWorkflow().ChangeTaskStatus(taskID, TaskWorkFlowActions.Reopen).NewStatus.ToString();
        }


        /// <summary>
        /// CompanyAcceptTask
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public bool CompanyAcceptTask(int iTaskID)
        {
            return EtaskMinstry.AppCode.TaskManger.CompanyAcceptTask(iTaskID);
        }
        /// <summary>
        /// Company Evaluate and Reject Task 
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        public bool CompanyRejectTask(int iTaskID)
        {
            return EtaskMinstry.AppCode.TaskManger.CompanyRejectTask(iTaskID);
        }


        #endregion

        #region T A S K  C O M M E N T

        /// <summary>
        /// Add Comment 
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="strComment"></param>
        /// <param name="iEmployeeID"></param>
        /// <returns></returns>
        public ActionResult AddComment(int taskID, string strComment, int iEmployeeID)
        {
            //test notifications
            //new Notification().NotifyWholeSystem(new NotificationMessage()
            //{
            //    Message = "New Comment added "+strComment.SubString(100),
            //    Type = NotificationType.AddComment
            //});

            int commentID = EtaskMinstry.AppCode.TaskManger.AddComment(taskID, strComment, iEmployeeID);
            bool isCompany = EtaskMinstry.AppCode.TaskManger.GetUserCommmet(commentID);
            var d = new { id = commentID, isCompany = isCompany };
            return Json(d, JsonRequestBehavior.AllowGet);
            //return EtaskMinstry.AppCode.TaskManger.AddComment(taskID, strComment, iEmployeeID);
        }

        /// <summary>
        /// Hide Task Comment
        /// </summary>
        /// <param name="iCommentID"></param>
        /// <returns></returns>
        public bool HideComment(int iCommentID)
        {
            return EtaskMinstry.AppCode.TaskManger.HideComment(iCommentID);
        }

        /// <summary>
        /// Show Task Comment
        /// </summary>
        /// <param name="iCommentID"></param>
        /// <returns></returns>
        public bool ShowComment(int iCommentID)
        {
            return EtaskMinstry.AppCode.TaskManger.ShowComment(iCommentID);
        }

        /// <summary>
        /// Report Task Comment
        /// </summary>
        /// <param name="iCommentID"></param>
        /// <returns></returns>
        public bool ReportComment(int iCommentID)
        {
            return EtaskMinstry.AppCode.TaskManger.ReportComment(iCommentID);
        }

        #endregion

        #region T A S K    A T T A C H M E N T

        //[HttpPost]
        //public bool AddAttachment(HttpPostedFileBase uploadFile ,int iTaskID, string strfileName, string strDescription)
        //{
        //    string filename = new EtaskMinstry.AppCode.UploadFile().Uploadfile(Request);
        //    return EtaskMinstry.AppCode.TaskManger.AttachTaskFile(iTaskID, filename, strDescription);
        //}



        [HttpPost]
        public ActionResult AddAttachment(HttpPostedFileBase uploadFile)
        {
            int Taskid = int.Parse(Request.Form["taskID"]);
            //if (Extentions.ValidateReCaptcha())
            //{
                string filename = new EtaskMinstry.AppCode.UploadFile().Uploadfile(Request,"/Upload/Task/");
                if (filename != "FAILED")
                {
                    var description = Request.Form["txtFileDescription"];
                    EtaskMinstry.AppCode.TaskManger.AttachTaskFile(Taskid, filename, description);
                    return RedirectToAction("TaskDetails", new { id = Taskid, isAttach = 1 });
                }
                else
                {
                    return RedirectToAction("TaskDetails", new { id = Taskid, isAttach = 1 });
                }
            //}
            //else
            //{
            //    return RedirectToAction("TaskDetails", new { id = Taskid, isAttach = 1 });
            //}
        }


        public FileResult DownloadAttachment(string fileName)
        {
            try
            {
                return File(Server.MapPath("/Upload/Task/" + fileName), "application/octet-stream", fileName);
            }
            catch
            {
                return File(Server.MapPath("/Upload/Task/" + fileName), "application/octet-stream", fileName);
            }
        }

        #endregion

        /// <summary>
        /// Search Tasks .
        /// </summary>
        /// <param name="bIsArchived"></param>
        /// <param name="endtDate"></param>
        /// <param name="strTitle"></param>
        /// <param name="iStatus"></param>
        /// <param name="EmpID"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetTasks(Boolean? bIsNotAssigned, Boolean? bIsArchived, String FromStartDate,
                                     String ToStartDate, String FromEndDate, String ToEndDate, String strTitle = "",
                                     TaskStatus iStatus = 0, int EmpID = 0, int PriorityID = 0, int ProjectID = 0,
                                     int[] EmpIDs = null)
        {
            int projID = (ProjectID == 0 ? Request.QueryString["ProjectID"].IntParse() : ProjectID);

            // Multi-employee filter: get all tasks then filter by selected employees
            if (EmpIDs != null && EmpIDs.Length > 0)
            {
                var allTasks = new CompanyTaskVM().Select(strTitle, FromStartDate, ToStartDate,
                                                          FromEndDate, ToEndDate, (int)iStatus,
                                                          0, bIsArchived, bIsNotAssigned, projID, PriorityID);
                var filtered = allTasks.Where(t => t.EmpID.HasValue && EmpIDs.Contains(t.EmpID.Value)).ToList();
                return PartialView("PartialCompTask", filtered);
            }

            return PartialView("PartialCompTask", new CompanyTaskVM().Select(strTitle, FromStartDate, ToStartDate,
                                                                             FromEndDate, ToEndDate, (int)iStatus,
                                                                             EmpID, bIsArchived, bIsNotAssigned,
                                                                             projID, PriorityID));
        }


        /// <summary>
        /// To get All tasks according to search criteria.
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="pageNo"></param>
        /// <param name="statuse"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTasks(int? page, Boolean? bIsNotAssigned, Boolean? bIsArchived, String FromStartDate,
                                    String ToStartDate, String FromEndDate, String ToEndDate, String strTitle = "",
                                    TaskStatus iStatus = 0, int EmpID = 0, int PriorityID = 0, int ProjectID = 0,
                                    int[] EmpIDs = null)
        {
            int projID = (ProjectID == 0 ? Request.QueryString["ProjectID"].IntParse() : ProjectID);

            if (EmpIDs != null && EmpIDs.Length > 0)
            {
                var allTasks = new CompanyTaskVM().Select(strTitle, FromStartDate, ToStartDate,
                                                          FromEndDate, ToEndDate, (int)iStatus,
                                                          0, bIsArchived, bIsNotAssigned, projID, PriorityID);
                var filtered = allTasks.Where(t => t.EmpID.HasValue && EmpIDs.Contains(t.EmpID.Value)).ToList();
                return PartialView("PartialCompTask", filtered);
            }

            return PartialView("PartialCompTask", new CompanyTaskVM().Select(strTitle, FromStartDate, ToStartDate,
                                                                             FromEndDate, ToEndDate, (int)iStatus,
                                                                             EmpID, bIsArchived, bIsNotAssigned,
                                                                             projID, PriorityID));
        }

        //public ActionResult GetTasks(int? page, int iStatus = 0, bool? bIsNotAssigned = null, bool? bIsArchived = null)
        //{
        //    return RedirectToAction("Index", new { s = iStatus, isNotAssign = bIsNotAssigned, isArchive = bIsArchived });
        //}



        /// <summary>
        /// Archive Task .
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        [HttpPost]
        public Boolean Archive(int iTaskID)
        {
            return new TaskManger().Archive(iTaskID);      
        }

        /// <summary>
        /// Accept Task .
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        [HttpPost]
        public Boolean AcceptTask(int iTaskID)
        {
            return new TaskManger().AcceptRefusedTask(iTaskID, TaskStatus.Approved);
        }

        /// <summary>
        /// Refused Task .
        /// </summary>
        /// <param name="iTaskID"></param>
        /// <returns></returns>
        [HttpPost]
        public Boolean RefusedTask(int iTaskID)
        {
            return new TaskManger().AcceptRefusedTask(iTaskID, TaskStatus.NotAproved);
        }

        /// <summary>
        /// Change task priority
        /// </summary>
        /// <param name="TaskID"></param>
        /// <param name="PriorityID"></param>
        /// <returns></returns>
        public Boolean ChangePriority(int TaskID, int PriorityID)
        {
            return new TaskManger().ChangePriority(TaskID, PriorityID);
        }

        /// <summary>
        /// Bulk Delete Tasks (New status only, max 500).
        /// </summary>
        /// <param name="taskIds">Array of Task IDs</param>
        /// <returns>JSON with success/fail counts</returns>
        [HttpPost]
        public ActionResult BulkDelete(int[] taskIds)
        {
            int successCount = 0;
            int failCount = 0;

            if (taskIds == null || taskIds.Length == 0)
                return Json(new { successCount = 0, failCount = 0 });

            if (taskIds.Length > 500)
                return Json(new { successCount = 0, failCount = 0, error = "الحد الأقصى للحذف 500 مهمة" });

            var vm = new CompanyTaskVM();
            foreach (var taskId in taskIds)
            {
                if (vm.Delete(taskId))
                    successCount++;
                else
                    failCount++;
            }

            return Json(new { successCount, failCount });
        }
    }
}
