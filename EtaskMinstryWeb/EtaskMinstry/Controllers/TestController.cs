using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EtaskMinstry.AppCode;
using TaskManagementModel;
namespace EtaskMinstry.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DoTaskAction(int iTaskId, string strAction)
        {
            TaskWorkflow taskWorkFlow = new TaskWorkflow();
            StatusChangeResult statusInfo = taskWorkFlow.ChangeTaskStatus(iTaskId, (TaskWorkFlowActions)Enum.Parse(typeof(TaskWorkFlowActions), strAction));
            return Json(statusInfo);
        }

        public ActionResult getTables()
        {
            return View();
        }
    }
}
