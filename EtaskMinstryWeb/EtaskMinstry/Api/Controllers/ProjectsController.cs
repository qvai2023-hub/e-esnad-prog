using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Api.Mapping;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API project picker. URL: GET /api/v1/projects
    /// Returns non-deleted projects in the caller's company.
    /// Available to both Employee and Company roles (Q-section: "both roles").
    /// </summary>
    [JwtAuthorize]
    public class ProjectsController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage List()
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            int companyId = userData.isCompany ? userData.userId : (userData.CompanyId ?? 0);

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var projects = uow.ProjectRepository.Get(
                filter: p => !p.IsDeleted && p.CompanyID == companyId,
                orderBy: q => q.OrderBy(p => p.Name)
            ).ToList();

            var dtos = projects.Select(ProjectMapper.ToListItem).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(dtos));
        }
    }
}
