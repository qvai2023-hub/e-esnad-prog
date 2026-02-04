using EtaskMinstry.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Company.Controllers
{
    public class HelpController : BaseCompanyController
    {
        //
        // GET: /Company/Help/

        public ActionResult Index()
        {
            return View();
        }

    }
}
