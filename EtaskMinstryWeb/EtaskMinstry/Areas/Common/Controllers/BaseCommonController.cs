using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;

namespace EtaskMinstry.Controllers
{
    [CommonAuthorize]
    public class BaseCommonController : BaseController
    {

    }
}
