using EtaskMinstry.Controllers;
using EtaskMinstry.CustomAttrbutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BaseAdminController : BaseController
    {
    }
}
