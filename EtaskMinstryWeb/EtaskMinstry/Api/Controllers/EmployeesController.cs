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
    /// Mobile API employee picker. URL: GET /api/v1/employees
    /// Returns active, non-deleted employees in the caller's company.
    /// Company role only — Employee callers get 403 EMPLOYEE_ONLY_REVERSE.
    /// </summary>
    [JwtAuthorize]
    public class EmployeesController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage List()
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (!userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للشركات فقط", "COMPANY_ONLY"));

            int companyId = userData.userId;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var employees = uow.Employee.Get(
                filter: e => e.CompanyID == companyId
                             && e.IsActive == true
                             && e.IsDeleted != true,
                orderBy: q => q.OrderBy(e => e.Name)
            ).ToList();

            var dtos = employees.Select(EmployeeMapper.ToListItem).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(dtos));
        }
    }
}
