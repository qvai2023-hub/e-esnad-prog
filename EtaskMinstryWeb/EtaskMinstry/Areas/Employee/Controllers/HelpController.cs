using EtaskMinstry.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Employee.Controllers
{
    public class HelpController : BaseEmployeeController
    {
        //
        // GET: /Employee/Help/

        public ActionResult Index()
        {
            return View();
        }

    }
}
