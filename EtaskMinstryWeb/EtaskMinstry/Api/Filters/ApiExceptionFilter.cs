using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using EtaskMinstry.Api.Dtos.Common;

namespace EtaskMinstry.Api.Filters
{
    /// <summary>
    /// Catches unhandled exceptions from Mobile API actions and returns an
    /// ApiResponse with HTTP 500. By default the response is sanitized
    /// (Arabic message only). To diagnose a 500 in dev, set
    /// `&lt;add key="ApiDetailedErrors" value="true" /&gt;` in Web.config —
    /// the response will then include the exception type, message, and inner
    /// exception message.
    ///
    /// NEVER enable ApiDetailedErrors in production.
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            // Always log to Debug output so server-side telemetry has it.
            System.Diagnostics.Debug.WriteLine(
                "[ApiException] {0}: {1}\n{2}",
                context.Exception != null ? context.Exception.GetType().FullName : "(null)",
                context.Exception != null ? context.Exception.Message : "(null)",
                context.Exception != null ? context.Exception.StackTrace : string.Empty);

            string detailed = ConfigurationManager.AppSettings["ApiDetailedErrors"];
            bool showDetail = string.Equals(detailed, "true", StringComparison.OrdinalIgnoreCase);

            ApiResponse body;
            if (showDetail && context.Exception != null)
            {
                string detail = context.Exception.GetType().Name + ": " + context.Exception.Message;
                if (context.Exception.InnerException != null)
                {
                    detail += " ← " + context.Exception.InnerException.GetType().Name
                            + ": " + context.Exception.InnerException.Message;
                }
                body = ApiResponse.Fail(detail, "SERVER_ERROR");
            }
            else
            {
                body = ApiResponse.Fail("حدث خطأ غير متوقع، يرجى المحاولة لاحقاً", "SERVER_ERROR");
            }

            context.Response = context.Request.CreateResponse(
                HttpStatusCode.InternalServerError, body);
        }
    }
}
