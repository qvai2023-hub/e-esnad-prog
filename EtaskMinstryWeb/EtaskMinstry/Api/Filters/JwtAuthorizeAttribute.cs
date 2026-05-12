using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Services;
using EtaskMinstry.Models;

namespace EtaskMinstry.Api.Filters
{
    /// <summary>
    /// JWT Bearer authentication for the Mobile API.
    /// On success: sets HttpContext.Current.User and populates
    /// MvcApplication.userData from the JWT claims so any downstream call
    /// to AppCode that reads MvcApplication.userData (e.g. NotificationHub.Send)
    /// continues to find a valid actor identity.
    ///
    /// On failure: short-circuits the request with 401 + ApiResponse envelope.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JwtAuthorizeAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            if (auth == null || !string.Equals(auth.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(auth.Parameter))
            {
                Reject(actionContext, "غير مصرح");
                return;
            }

            var claims = new JwtValidator().Validate(auth.Parameter);
            if (claims == null)
            {
                Reject(actionContext, "غير مصرح");
                return;
            }

            // Mirror the session shape used by the web app so existing AppCode
            // (NotificationHub.Send, UserAccountVM, etc.) keeps working.
            var userData = new UserData
            {
                userId = claims.UserId,
                userName = claims.Name,
                UserTypeId = claims.UserTypeId,
                isCompany = claims.UserTypeId == 3, // LoggedUserType.Company
                CompanyId = claims.CompanyId,
                status = 1,         // userStatus.active
                companyStatus = 1,  // userStatus.active
                isAuthorized = true
            };
            MvcApplication.userData = userData;

            base.OnActionExecuting(actionContext);
        }

        private static void Reject(HttpActionContext actionContext, string message)
        {
            var body = ApiResponse.Fail(message);
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized, body);
        }
    }
}
