using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Employee.Controllers
{
    public class TestFileUploadController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(string x)
        {
            string filename = new EtaskMinstry.AppCode.UploadFile().Uploadfile(Request,"/Upload/Task/");
            return View();
        }
    }
}
