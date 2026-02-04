using EtaskMinstry.Controllers;
using EtaskMinstry.Models.EmployeeTask;
using EtaskMinstry.Models.Priority;
using EtaskMinstry.Models.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using EtaskMinstry.Models.Employee;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Configuration;
using EtaskMinstry.CustomAttrbutes;

namespace EtaskMinstry.Areas.Employee.Controllers
{
   // [RequireHttps]
    public class TasksController : BaseEmployeeController
    {
        EmployeeTaskVM employeeTaskVM;
        EmployeeTaskListVM listTasks;
        StatusDisplay status;
        


        public TasksController()
        {
            employeeTaskVM = new EmployeeTaskVM();
            listTasks = new EmployeeTaskListVM();
            status = new StatusDisplay();
        }
        //[CryptoValueProvider]
        public ActionResult Index(int s = 0)
        {
            EtaskMinstry.AppCode.TaskManger.UpdateTaskStatus();
            var tasks = new EmployeeTaskListVM();
            tasks.FillTasks(Request.QueryString["s"] == null ? s : int.Parse(Request.QueryString["s"].ToString()), Request.QueryString["t"] == null ? "" : Request.QueryString["t"].ToString(), 0, "", "","","");
            ViewData["SearchResult"] = tasks;
            ViewBag.priority = new SelectList(new PriorityDisplay().Get().ToList(), "ID", "Name");
            ViewBag.count = tasks.TaskList.Count();
            ViewBag.NewCount = tasks.NewCount.ToString();
            ViewBag.EndedCount = tasks.EndedCount.ToString();
            var status = new StatusDisplay();
            ViewBag.status = status.GetByEmployee(MvcApplication.userData.userId);
            Generallog.LogView();
            return View();
        }



        /// <summary>
        /// To get All tasks according to search criteria.
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="pageNo"></param>
        /// <param name="statuse"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GetTasks(int status = 0, string taskTitle = "", int priorityId = 0, string fDate = "", string tDate = "", string FromEndDate = "", string ToEndDate = "")
        {
            listTasks.FillTasks(status, taskTitle, priorityId, fDate, tDate,FromEndDate,ToEndDate);
            return PartialView("PartialEmpTask", listTasks);
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
        public ActionResult GetTasks(int? page, int status = 0, string taskTitle = "", int priorityId = 0, string fDate = "", string tDate = "", string FromEndDate = "", string ToEndDate = "")
        {
            listTasks.FillTasks(status, taskTitle, priorityId, fDate, tDate, FromEndDate, ToEndDate);
            return PartialView("PartialEmpTask", listTasks);
        }

        //public ActionResult GetTasks(int status = 0)
        //{
        //    return RedirectToAction("Index", new { s = status });
        //}


        /// <summary>
        /// deliver task eather accept or rafuse it.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="isAccept"></param>
        /// <returns></returns>
        public bool DeliverTask(int taskId, bool isAccept)
        {
            if (isAccept)
                return EtaskMinstry.AppCode.TaskManger.EmpAcceptTask(taskId);
            else
                return EtaskMinstry.AppCode.TaskManger.EmpRejectTask(taskId);
            // return listTasks.DeliverTask(taskId, isAccept);
        }

        /// <summary>
        /// finalize task.
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool EndTask(int taskId)
        {
            return listTasks.EndTask(taskId);
        }


        #region T A S K  D E T A I L S  V I E W
        /// <summary>
        /// Get Task Detail view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [EncryptedActionParameter]
        public ActionResult TaskDetails(string id, int isAttach = 0)
        {
            ViewBag.TimeUnit = new SelectList(new EtaskMinstry.Models.TimeUnit.TimeUnitDisplay().Get(), "ID", "Name");
            ViewBag.Emplyees = new SelectList(EtaskMinstry.AppCode.ServiceManger.GetCompanyEmployeeNotDeleted(MvcApplication.userData.userId), "id", "name");
            if (!TaskManger.IsValidTask(id))
                return RedirectToAction("Index");
            return View(new EmployeeTaskDetailVM().Select(int.Parse(id), isAttach == 1));
        }

        /// <summary>
        /// Get Task Extension 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [EncryptedActionParameter]
        public ActionResult TaskExtensions(string id)
        {

            return PartialView(new EtaskMinstry.Models.Employee.TaskExtensionsVM().Select(int.Parse(id)));
        }

        #endregion


        #region T A S K  A C T I O N S


        public string UpdateTaskDalyTime(int taskID, decimal Duration)
        {

            return EtaskMinstry.AppCode.TaskManger.EmpUpdateDalyTaskTime(taskID, Duration);
        }

        /// <summary>
        /// Update Task Time
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="Duration"></param>
        /// <returns></returns>
        public string UpdateTaskTime(int taskID, decimal Duration)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpUpdateTaskTime(taskID, Duration);
        }

        /// <summary>
        /// Start Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public bool StartTask(int taskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmployeeStartTask(taskID);


        }

        /// <summary>
        /// Employee Accept Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public bool AcceptTask(int taskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpAcceptTask(taskID);

        }

        /// <summary>
        /// Employee Reject Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <param name="iEmployeeID"></param>
        /// <returns></returns>
        public bool RejectTask(int taskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpRejectTask(taskID);
        }

        /// <summary>
        /// Employee Finish Task
        /// </summary>
        /// <param name="taskID"></param>
        /// <returns></returns>
        public bool EmpFinishTask(int taskID)
        {
            return EtaskMinstry.AppCode.TaskManger.EmpFinishTask(taskID);
        }

        #endregion

        #region T A S K  C O M M E N T

        [HttpPost]
        public int AddComment(int taskID, string strComment, int iEmployeeID)
        {
            return EtaskMinstry.AppCode.TaskManger.AddComment(taskID, strComment, iEmployeeID);
        }

        public bool HideComment(int iCommentID)
        {
            return EtaskMinstry.AppCode.TaskManger.HideComment(iCommentID);
        }

        public bool ShowComment(int iCommentID)
        {
            return EtaskMinstry.AppCode.TaskManger.ShowComment(iCommentID);
        }

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
        [EncryptedActionParameter]
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
                    return RedirectToAction("TaskDetails", new { id = Extentions.Encrypt(Taskid.ToString()), isAttach = 1 });
                }
                else
                {
                    return RedirectToAction("TaskDetails", new { id = Extentions.Encrypt(Taskid.ToString()), isAttach = 1 });
                }
            //}
            //else
            //{
            //    return RedirectToAction("TaskDetails", new { id = Extentions.Encrypt(Taskid.ToString()), isAttach = 1 });
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
    }
}
