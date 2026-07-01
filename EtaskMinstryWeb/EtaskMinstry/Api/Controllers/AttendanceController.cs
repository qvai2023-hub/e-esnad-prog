using System;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Attendance;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Models.Login;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API attendance endpoints. URL prefix: /api/v1/attendance/
    ///   POST check-in   â€” idempotent; returns existing open row, else creates new
    ///   POST heartbeat  â€” updates LastHeartbeat (mirrors web AttendanceController.LogActivity)
    ///   POST check-out  â€” closes the latest open row for today
    ///   GET  today      â€” returns today's latest attendance state
    ///
    /// Employee-only (Company users get 403 EMPLOYEE_ONLY). Mirrors the web's
    /// behavior in AttendanceController.LogActivity which short-circuits when
    /// userData.isCompany is true.
    /// </summary>
    [JwtAuthorize]
    public class AttendanceController : ApiController
    {
        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ POST check-in â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        [HttpPost]
        [ActionName("check-in")]
        public HttpResponseMessage CheckIn()
        {
            var employeeGuard = RequireEmployee();
            if (employeeGuard != null) return employeeGuard;

            int empId = MvcApplication.userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var existing = GetTodayLatest(uow, empId);
            if (existing != null && existing.CheckOut == null)
            {
                // Idempotent: still checked in â€” return existing row.
                return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(new CheckInResponse
                {
                    AttendanceId = existing.Id,
                    CheckIn = existing.CheckIn ?? DateTime.Now,
                    IsNew = false
                }));
            }

            // No open row â†’ create new one via the existing web logic so any
            // side effects (LastHeartbeat seed, session flag) stay identical.
            try
            {
                new UserAccountVM().Checkin(empId);
            }
            catch
            {
                // Mirror web behavior â€” Checkin swallows LastHeartbeat SQL errors.
            }

            // Re-query so we return the row we just created.
            var fresh = GetTodayLatest(
                new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString),
                empId);
            if (fresh == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    ApiResponse.Fail("ØªØ¹Ø°Ø± Ø¥Ù†Ø´Ø§Ø¡ Ø³Ø¬Ù„ Ø§Ù„Ø­Ø¶ÙˆØ±", "CHECKIN_FAILED"));
            }

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(new CheckInResponse
            {
                AttendanceId = fresh.Id,
                CheckIn = fresh.CheckIn ?? DateTime.Now,
                IsNew = true
            }));
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ POST heartbeat â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        [HttpPost]
        [ActionName("heartbeat")]
        public HttpResponseMessage Heartbeat()
        {
            var employeeGuard = RequireEmployee();
            if (employeeGuard != null) return employeeGuard;

            int empId = MvcApplication.userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var attendance = GetTodayOpen(uow, empId);
            if (attendance == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound,
                    ApiResponse.Fail("Ù„Ø§ ÙŠÙˆØ¬Ø¯ Ø³Ø¬Ù„ Ø­Ø¶ÙˆØ± Ù†Ø´Ø·", "NO_ACTIVE_ATTENDANCE"));
            }

            // Mirrors the web's AttendanceController.LogActivity exactly:
            // direct UPDATE on LastHeartbeat (column is [NotMapped] in EF).
            DateTime now = DateTime.Now;
            using (var conn = OpenSqlConnection())
            {
                using (var cmd = new SqlCommand(
                    "UPDATE Attendance SET LastHeartbeat = @LastHeartbeat WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@LastHeartbeat", now);
                    cmd.Parameters.AddWithValue("@Id", attendance.Id);
                    cmd.ExecuteNonQuery();
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(new HeartbeatResponse
            {
                AttendanceId = attendance.Id,
                LastHeartbeat = now
            }));
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ POST check-out â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        [HttpPost]
        [ActionName("check-out")]
        public HttpResponseMessage CheckOut()
        {
            var employeeGuard = RequireEmployee();
            if (employeeGuard != null) return employeeGuard;

            int empId = MvcApplication.userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var attendance = GetTodayOpen(uow, empId);
            if (attendance == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound,
                    ApiResponse.Fail("Ù„Ø§ ÙŠÙˆØ¬Ø¯ Ø³Ø¬Ù„ Ø­Ø¶ÙˆØ± Ù†Ø´Ø·", "NO_ACTIVE_ATTENDANCE"));
            }

            attendance.CheckOut = DateTime.Now;
            uow.AttendanceRepository.Update(attendance);
            uow.Save();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(BuildDto(uow, attendance, empId), "ØªÙ… ØªØ³Ø¬ÙŠÙ„ Ø§Ù„Ø®Ø±ÙˆØ¬"));
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ GET today â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        [HttpGet]
        [ActionName("today")]
        public HttpResponseMessage Today()
        {
            if (MvcApplication.userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("ØºÙŠØ± Ù…ØµØ±Ø­"));

            // Company users: no attendance. Mirror web's "hasAttendance = false".
            if (MvcApplication.userData.isCompany)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    ApiResponse.Ok(new TodayAttendanceDto { HasAttendance = false }));
            }

            int empId = MvcApplication.userData.userId;
            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var attendance = GetTodayLatest(uow, empId);

            if (attendance == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    ApiResponse.Ok(new TodayAttendanceDto { HasAttendance = false }));
            }

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(BuildDto(uow, attendance, empId)));
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// 403 EMPLOYEE_ONLY when the caller is a Company user; null otherwise.
        /// Caller short-circuits with the returned response.
        /// </summary>
        private HttpResponseMessage RequireEmployee()
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("ØºÙŠØ± Ù…ØµØ±Ø­"));
            if (userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("Ù…ØªØ§Ø­ Ù„Ù„Ù…ÙˆØ¸ÙÙŠÙ† ÙÙ‚Ø·", "EMPLOYEE_ONLY"));
            return null;
        }

        private static TaskManagementModel.Attendance GetTodayLatest(UnitOfWork uow, int empId)
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            return uow.AttendanceRepository
                .Get(filter: a => a.EmpId == empId
                                  && a.CheckIn.HasValue
                                  && a.CheckIn.Value >= today
                                  && a.CheckIn.Value < tomorrow)
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();
        }

        private static TaskManagementModel.Attendance GetTodayOpen(UnitOfWork uow, int empId)
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);
            return uow.AttendanceRepository
                .Get(filter: a => a.EmpId == empId
                                  && a.CheckOut == null
                                  && a.CheckIn.HasValue
                                  && a.CheckIn.Value >= today
                                  && a.CheckIn.Value < tomorrow)
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();
        }

        private static TodayAttendanceDto BuildDto(UnitOfWork uow, TaskManagementModel.Attendance latest, int empId)
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            int closedMinutes = uow.AttendanceRepository
                .Get(filter: a => a.EmpId == empId
                                  && a.CheckIn.HasValue
                                  && a.CheckOut.HasValue
                                  && a.CheckIn.Value >= today
                                  && a.CheckIn.Value < tomorrow)
                .ToList()
                .Sum(a => Math.Max(0, (int)Math.Round((a.CheckOut.Value - a.CheckIn.Value).TotalMinutes)));

            return new TodayAttendanceDto
            {
                HasAttendance = true,
                AttendanceId = latest.Id,
                CheckIn = latest.CheckIn,
                CheckOut = latest.CheckOut,
                DurationMinutes = closedMinutes,
                IsOpen = latest.CheckIn.HasValue && latest.CheckOut == null
            };
        }

        private static SqlConnection OpenSqlConnection()
        {
            string efConnStr = ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString;
            var builder = new EntityConnectionStringBuilder(efConnStr);
            var conn = new SqlConnection(builder.ProviderConnectionString);
            conn.Open();
            return conn;
        }
    }
}

