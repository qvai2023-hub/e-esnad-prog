using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Dtos.Notifications;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Api.Services;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API device-token endpoints. URL prefix: /api/v1/device-tokens
    ///   POST   /api/v1/device-tokens          — register / refresh
    ///   DELETE /api/v1/device-tokens/{token}  — unregister this device
    ///
    /// Tokens are stored per-user in MobileDeviceToken. One user may have
    /// multiple active tokens (phone + tablet). Re-registering the same
    /// token updates LastSeenDate (and reactivates if disabled).
    /// </summary>
    [JwtAuthorize]
    public class DeviceTokensController : ApiController
    {
        private readonly DeviceTokenService _service = new DeviceTokenService();

        [HttpPost]
        public HttpResponseMessage Register(DeviceTokenRequest request)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            if (request == null || string.IsNullOrWhiteSpace(request.Token))
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("بيانات الطلب غير صحيحة", "INVALID_REQUEST"));

            _service.Register(userData.userId, request.Token, request.Platform, request.Model);

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(null, "تم تسجيل الجهاز"));
        }

        [HttpDelete]
        public HttpResponseMessage Unregister(string token)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            if (string.IsNullOrWhiteSpace(token))
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("بيانات الطلب غير صحيحة", "INVALID_REQUEST"));

            _service.Unregister(userData.userId, token);

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(null, "تم إلغاء تسجيل الجهاز"));
        }
    }
}
