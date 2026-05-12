using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using EtaskMinstry.Api.Dtos.Common;

namespace EtaskMinstry.Api.Filters
{
    /// <summary>
    /// Wraps every successful Mobile API response in the standard ApiResponse
    /// envelope so controllers can return raw DTOs (or PagedResponse) and stay
    /// thin. Already-wrapped responses (ApiResponse, PagedResponse) pass through
    /// untouched. Non-2xx responses are not re-wrapped — those go through
    /// ApiExceptionFilter or are set explicitly by the controller.
    /// </summary>
    public class ApiResponseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response == null) return;
            if (!context.Response.IsSuccessStatusCode) return;

            ObjectContent objectContent = context.Response.Content as ObjectContent;
            if (objectContent == null) return;

            object value = objectContent.Value;
            if (value is ApiResponse || value is PagedResponse) return;

            var wrapped = ApiResponse.Ok(value);
            context.Response = context.Request.CreateResponse(HttpStatusCode.OK, wrapped);
        }
    }
}
