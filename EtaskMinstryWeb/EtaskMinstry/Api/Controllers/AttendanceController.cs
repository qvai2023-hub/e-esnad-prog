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
    ///   POST check-in   — idempotent; returns existing open row, else creates new
    ///   POST heartbeat  — updates LastHeartbeat (mirrors web AttendanceController.LogActivity)
    ///   POST check-out  — closes the latest open row for today
    ///   GET  today      — returns today's latest attendance state
    ///
    /// Employee-only (Company users get 403 EMPLOYEE_ONLY). Mirrors the web's
    /// behavior in AttendanceController.LogActivity which short-circuits when
    /// userData.isCompany is true.
    /// </summary>
    [JwtAuthorize]
    public class AttendanceController : ApiController
    {
        // ───────────────────────── POST check-in ─────────────────────────
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
                // Idempotent: still checked in — return existing row.
                return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(new CheckInResponse
                {
                    AttendanceId = existing.Id,
                    CheckIn = existing.CheckIn ?? DateTime.Now,
                    IsNew = false
                }));
            }

            // No open row → create new one via the existing web logic so any
            // side effects (LastHeartbeat seed, session flag) stay identical.
            try
            {
                new UserAccountVM().Checkin(empId);
            }
            catch
            {
                // Mirror web behavior — Checkin swallows LastHeartbeat SQL errors.
            }

            // Re-query so we return the row we just created.
            var fresh = GetTodayLatest(
                new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString),
                empId);
            if (fresh == null)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    ApiResponse.Fail("تعذر إنشاء سجل الحضور", "CHECKIN_FAILED"));
            }

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(new CheckInResponse
            {
                AttendanceId = fresh.Id,
                CheckIn = fresh.CheckIn ?? DateTime.Now,
                IsNew = true
            }));
        }

        // ───────────────────────── POST heartbeat ─────────────────────────
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
                    ApiResponse.Fail("لا يوجد سجل حضور نشط", "NO_ACTIVE_ATTENDANCE"));
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

        // ───────────────────────── POST check-out ─────────────────────────
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
                    ApiResponse.Fail("لا يوجد سجل حضور نشط", "NO_ACTIVE_ATTENDANCE"));
            }

            attendance.CheckOut = DateTime.Now;
            uow.AttendanceRepository.Update(attendance);
            uow.Save();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(BuildDto(attendance), "تم تسجيل الخروج"));
        }

        // ───────────────────────── GET today ─────────────────────────
        [HttpGet]
        [ActionName("today")]
        public HttpResponseMessage Today()
        {
            if (MvcApplication.userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

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

            return Request.CreateResponse(HttpStatusCode.OK, ApiResponse.Ok(BuildDto(attendance)));
        }

        // ───────────────────────── helpers ─────────────────────────

        /// <summary>
        /// 403 EMPLOYEE_ONLY when the caller is a Company user; null otherwise.
        /// Caller short-circuits with the returned response.
        /// </summary>
        private HttpResponseMessage RequireEmployee()
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));
            if (userData.isCompany)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("متاح للموظفين فقط", "EMPLOYEE_ONLY"));
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

        private static TodayAttendanceDto BuildDto(TaskManagementModel.Attendance a)
        {
            int? minutes = null;
            if (a.CheckIn.HasValue)
            {
                DateTime endTime = a.CheckOut ?? DateTime.Now;
                minutes = (int)Math.Round((endTime - a.CheckIn.Value).TotalMinutes);
            }
            return new TodayAttendanceDto
            {
                HasAttendance = true,
                AttendanceId = a.Id,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                DurationMinutes = minutes,
                IsOpen = a.CheckIn.HasValue && a.CheckOut == null
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
