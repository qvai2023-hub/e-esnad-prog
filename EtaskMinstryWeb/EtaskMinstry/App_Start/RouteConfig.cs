using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EtaskMinstry
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Reports
            routes.MapRoute(
                name: "Reports",
                url: "Reports/{ReportName}",
                defaults: new { controller = "Reports", action = "LoadReport" }
            );

            routes.MapRoute(
                 name: "Default",
                url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Security", action = "login", id = UrlParameter.Optional },
                 namespaces: new[] { "EtaskMinstry.Areas.Company.Controllers", "EtaskMinstry.Areas.Employee.Controllers", "EtaskMinstry.Areas.Admin.Controllers", "EtaskMinstry.Controllers" }
             );


        }
    }
}