using EtaskMinstry.CustomAttrbutes;
using EtaskMinstry.Controllers;
using EtaskMinstry.Models.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;

namespace EtaskMinstry.Controllers
{
    [CompanyAuthorize]
    public class BaseCompanyController : BaseController
    {
       
    }

   
}
