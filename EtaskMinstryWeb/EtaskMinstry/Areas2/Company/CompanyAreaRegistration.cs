using System.Web.Mvc;

namespace EtaskMinstry.Areas.Company
{
    public class CompanyAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Company";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Company_default",
                "Company/{controller}/{action}/{id}",
                new { controller = "Company", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
