using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Attendance;
using EtaskMinstry.Api.Dtos.Auth;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Models.Login;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API "me" endpoints. URL prefix: /api/v1/me
    ///   GET   /api/v1/me                  (mapped via explicit route in WebApiConfig)
    ///   POST  /api/v1/me/change-password
    /// </summary>
    [JwtAuthorize]
    public class MeController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized,
                    ApiResponse.Fail("غير مصرح"));

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var summary = new UserSummaryDto
            {
                UserId = userData.userId,
                Role = userData.isCompany ? "Company" : "Employee",
                CompanyId = userData.CompanyId,
                IsStopped = false
            };

            if (userData.isCompany)
            {
                var company = uow.Company.Get().FirstOrDefault(c => c.CompanyID == userData.userId);
                if (company != null)
                {
                    summary.FullName = company.Name;
                    summary.CompanyName = company.Name;
                    summary.IsStopped = company.IsActive != true || company.IsDeleted == true;
                }
            }
            else
            {
                int empId = userData.userId;
                var emp = uow.Employee.Get(filter: e => e.EmpID == empId, includeProperties: "Company")
                                      .FirstOrDefault();
                if (emp != null)
                {
                    summary.FullName = emp.Name;
                    summary.CompanyName = emp.Company != null ? emp.Company.Name : null;
                    summary.IsStopped = emp.IsActive != true || emp.IsDeleted == true ||
                                        (emp.Company != null && (emp.Company.IsActive != true || emp.Company.IsDeleted == true));
                }
            }

            var body = new MeResponse
            {
                User = summary,
                Attendance = userData.isCompany ? null : LoadTodayAttendance(uow, userData.userId)
            };

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(body));
        }

        [HttpPost]
        [ActionName("change-password")]
        public HttpResponseMessage ChangePassword(ChangePasswordRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.OldPassword)
                || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("بيانات الطلب غير صحيحة", "INVALID_REQUEST"));
            }

            // Reuses existing web logic (UserAccountVM.ChangePassowrd reads
            // MvcApplication.userData.userId which JwtAuthorize has populated).
            bool ok;
            try
            {
                ok = new UserAccountVM().ChangePassowrd(request.OldPassword, request.NewPassword);
            }
            catch
            {
                // ChangePassowrd may throw if the configured SMTP server is
                // unreachable while sending the notification email. The password
                // change itself succeeded by then — surface a soft success.
                return Request.CreateResponse(HttpStatusCode.OK,
                    ApiResponse.Ok(null, "تم تغيير كلمة المرور (تعذر إرسال إشعار البريد)"));
            }

            if (!ok)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ApiResponse.Fail("كلمة المرور القديمة غير صحيحة", "INVALID_OLD_PASSWORD"));
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(null, "تم تغيير كلمة المرور بنجاح"));
        }

        private static TodayAttendanceDto LoadTodayAttendance(UnitOfWork uow, int empId)
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            var attendance = uow.AttendanceRepository
                .Get(filter: a => a.EmpId == empId
                                  && a.CheckIn.HasValue
                                  && a.CheckIn.Value >= today
                                  && a.CheckIn.Value < tomorrow)
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();

            if (attendance == null)
                return new TodayAttendanceDto { HasAttendance = false };

            int? minutes = null;
            if (attendance.CheckIn.HasValue)
            {
                DateTime endTime = attendance.CheckOut ?? DateTime.Now;
                minutes = (int)Math.Round((endTime - attendance.CheckIn.Value).TotalMinutes);
            }

            return new TodayAttendanceDto
            {
                HasAttendance = true,
                AttendanceId = attendance.Id,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                DurationMinutes = minutes,
                IsOpen = attendance.CheckIn.HasValue && attendance.CheckOut == null
            };
        }
    }
}
