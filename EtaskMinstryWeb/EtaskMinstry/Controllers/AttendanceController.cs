using EtaskMinstry.AppCode;
using EtaskMinstry.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TaskManagementModel;
using QvLib.QVUtil;
using Microsoft.Reporting.WebForms;
using System.Globalization;
using System.Security.Cryptography;

namespace EtaskMinstry.Controllers
{
    public class AttendanceController : Controller
    {
        private AttendanceReportService attendanceReportService;
        private SharedService sharedService;
        private UnitOfWork _unitOfWork;

        public AttendanceController()
        {
            attendanceReportService = new AttendanceReportService();
            sharedService = new SharedService();
            _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }
        public ActionResult AttendanceReport(int? companyId)
        {
            ViewBag.companyId = companyId;
            ViewBag.Providers = sharedService.GetProviders();
            if (companyId==null)
            {
                // ViewBag.companies = attendanceReportService.GetCompanies();
                ViewBag.companies = Enumerable.Empty<SelectListItem>();
                ViewBag.employees=Enumerable.Empty<SelectListItem>();
            }
            else
            {
                ViewBag.employees = attendanceReportService.GetEmployeesByCompanyId((int)companyId);
            }
            int currentYear = DateTime.Now.Year;
            int yearsBack =5;
            ViewBag.years= Enumerable.Range(currentYear - yearsBack + 1, yearsBack)
                         .OrderByDescending(y => y)
                         .Select(y => new SelectListItem
                         {
                             Value = y.ToString(),
                             Text = y.ToString()
                         });
            return View(companyId);
        }

        [HttpPost]
        public ActionResult GetEmployees(int companyId) {
           var lst= attendanceReportService.GetEmployeesByCompanyId(companyId);
           return Json(lst, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult ViewReport(int? CompanyId, int? EmployeeId, string FromDate, string ToDate)
        //{

        //    var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, FromDate, ToDate);
        //    //if (EmployeeId != null)
        //    //{
        //    //    data[0].filteredEmployee = data[0].employeeName;
        //    //}
        //    ReportAgent.ReportDataSources.Clear();
        //    ReportAgent.AddReportDataSources(new ReportDataSource("DS_attendance", data));
        //    return Redirect("/Reports/Attendance");
        //}
        //public ActionResult ViewReport(int? CompanyId, int? EmployeeId
        //    , string FromYear ,string fromMonth, string toYear, string toMonth)
        //{

        //    string FromDate = new DateTime(int.Parse(FromYear), int.Parse(fromMonth), 1).ToString("dd/MM/yyyy");
        //    int days= DateTime.DaysInMonth(int.Parse(toYear), int.Parse(toMonth));
        //    string ToDate = new DateTime(int.Parse(toYear), int.Parse(toMonth) ,days).ToString("dd/MM/yyyy");
        //    var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, FromDate, ToDate);
        //    string reportMonthName = "";
        //    if (toMonth== fromMonth)
        //    {
        //        reportMonthName = new DateTime(int.Parse(FromYear), int.Parse(fromMonth), 1)
        //                        .ToString("MMMM yyyy", new CultureInfo("ar-EG"));
        //    }
        //    else
        //    {
        //        reportMonthName=" من " + new DateTime(int.Parse(FromYear), int.Parse(fromMonth), 1)
        //                        .ToString("MMMM yyyy", new CultureInfo("ar-EG")) + " الى " + new DateTime(int.Parse(toYear), int.Parse(toMonth), 1)
        //                        .ToString("MMMM yyyy", new CultureInfo("ar-EG"));
        //    }
        //        //if (EmployeeId != null)
        //        //{
        //        //    data[0].filteredEmployee = data[0].employeeName;
        //        //}
        //        ReportAgent.ReportDataSources.Clear();
        //    //ReportAgent.l.ReportParameters("ReportMonth", reportMonthName);
        //    //repv.LocalReport.SetParameters(new ReportParameter("ReportDateTime", currentDateTime));
        //    //ReportAgent.AddReportParameter("ReportMonth", "reportMonthName");

        //    ReportAgent.ReportParameters.Add(new ReportParameter("ReportMonth", reportMonthName));



        //    ReportAgent.AddReportDataSources(new ReportDataSource("DS_attendance", data));
        //    return Redirect("/Reports/Attendance");
        //}

        public ActionResult ViewReport(int? CompanyId, int? EmployeeId, string FromDate, string ToDate)
        {

            var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, FromDate, ToDate);
            //if (EmployeeId != null)
            //{
            //    data[0].filteredEmployee = data[0].employeeName;
            //}
            ReportAgent.ReportDataSources.Clear();
            ReportAgent.AddReportDataSources(new ReportDataSource("DS_attendance", data));
            return Redirect("/Reports/Attendance");
        }
        [HttpPost]
        public ActionResult GetCompanies(string provider)
        {
            var lst = sharedService.GetCompaniesByproviderId(provider);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetMonths(int Year)
        {
            int currentYear = DateTime.Now.Year;
            int maxMonth = (Year == currentYear) ? DateTime.Now.Month : 12;

            var lst= Enumerable.Range(1, maxMonth)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = CultureInfo.GetCultureInfo("ar-EG")
                                       .DateTimeFormat.GetMonthName(m)   // يناير…ديسمبر
                });
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

        #region Activity Tracking APIs

        /// <summary>
        /// Helper method to get today's active attendance for an employee
        /// SQL Server 2008 R2 compatible - fetch then filter in memory
        /// </summary>
        private Attendance GetTodayActiveAttendance(int empId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Fetch open attendance records for this employee, then filter by date in memory
            var openAttendances = _unitOfWork.AttendanceRepository.Get(
                a => a.EmpId == empId && a.CheckOut == null
            ).ToList();

            // Filter by today's date in memory (avoids LINQ to Entities date comparison issues)
            return openAttendances
                .Where(a => a.CheckIn.HasValue &&
                           a.CheckIn.Value >= today &&
                           a.CheckIn.Value < tomorrow)
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Logs user activity and returns current attendance ID
        /// Called periodically by JavaScript tracker
        /// Uses direct SQL for ActivityLog (not in EDMX model)
        /// </summary>
        [HttpPost]
        public JsonResult LogActivity(string activityType)
        {
            try
            {
                if (MvcApplication.userData == null || MvcApplication.userData.isCompany)
                {
                    return Json(new { success = false, message = "Not an employee session" });
                }

                int empId = MvcApplication.userData.userId;
                var attendance = GetTodayActiveAttendance(empId);

                if (attendance == null)
                {
                    return Json(new { success = false, message = "No active attendance" });
                }

                // Log activity using direct SQL (ActivityLog not in EDMX)
                // Extract SQL connection string from EF connection string
                var efConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString();
                var entityBuilder = new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr);
                string connStr = entityBuilder.ProviderConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"INSERT INTO ActivityLog (EmpId, AttendanceId, LastActivityTime, ActivityType)
                                   VALUES (@EmpId, @AttendanceId, @LastActivityTime, @ActivityType)";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmpId", empId);
                        cmd.Parameters.AddWithValue("@AttendanceId", attendance.Id);
                        cmd.Parameters.AddWithValue("@LastActivityTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ActivityType", activityType ?? "heartbeat");
                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, attendanceId = attendance.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Beacon checkout - called when browser/tab closes
        /// Uses sendBeacon API which works even during page unload
        /// </summary>
        [HttpPost]
        public JsonResult BeaconCheckout(int? attendanceId, string lastActivityTime)
        {
            try
            {
                if (MvcApplication.userData == null || MvcApplication.userData.isCompany)
                {
                    return Json(new { success = false });
                }

                int empId = MvcApplication.userData.userId;
                Attendance attendance = null;

                if (attendanceId.HasValue)
                {
                    attendance = _unitOfWork.AttendanceRepository.GetByID(attendanceId.Value);
                }
                else
                {
                    attendance = GetTodayActiveAttendance(empId);
                }

                if (attendance == null || attendance.CheckOut.HasValue)
                {
                    return Json(new { success = false });
                }

                // Parse last activity time if provided, otherwise use now
                DateTime checkoutTime = DateTime.Now;
                if (!string.IsNullOrEmpty(lastActivityTime))
                {
                    if (DateTime.TryParse(lastActivityTime, out DateTime parsedTime))
                    {
                        checkoutTime = parsedTime;
                    }
                }

                attendance.CheckOut = checkoutTime;
                _unitOfWork.AttendanceRepository.Update(attendance);
                _unitOfWork.Save();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Inactivity checkout - called when user confirms leaving after inactivity warning
        /// Sets checkout time to last recorded activity
        /// </summary>
        [HttpPost]
        public JsonResult InactivityCheckout(int? attendanceId)
        {
            try
            {
                if (MvcApplication.userData == null || MvcApplication.userData.isCompany)
                {
                    return Json(new { success = false, message = "غير مصرح - userData null أو شركة" });
                }

                int empId = MvcApplication.userData.userId;
                Attendance attendance = null;

                if (attendanceId.HasValue)
                {
                    attendance = _unitOfWork.AttendanceRepository.GetByID(attendanceId.Value);
                }
                else
                {
                    attendance = GetTodayActiveAttendance(empId);
                }

                if (attendance == null)
                {
                    return Json(new { success = false, message = "لا يوجد حضور نشط - attendanceId: " + attendanceId });
                }

                if (attendance.CheckOut.HasValue)
                {
                    return Json(new { success = false, message = "تم تسجيل الخروج مسبقاً في: " + attendance.CheckOut.Value.ToString("HH:mm:ss") });
                }

                // Get last activity time using direct SQL (ActivityLog not in EDMX)
                // Extract SQL connection string from EF connection string
                DateTime? lastActivityTime = null;
                var efConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString();
                var entityBuilder = new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr);
                string connStr = entityBuilder.ProviderConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT TOP 1 LastActivityTime FROM ActivityLog WHERE AttendanceId = @AttendanceId ORDER BY LastActivityTime DESC";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AttendanceId", attendance.Id);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastActivityTime = (DateTime)result;
                        }
                    }
                }

                DateTime checkoutTime = lastActivityTime ?? DateTime.Now;
                attendance.CheckOut = checkoutTime;
                _unitOfWork.AttendanceRepository.Update(attendance);
                _unitOfWork.Save();

                return Json(new { success = true, checkoutTime = checkoutTime.ToString("HH:mm:ss") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get current attendance status for JavaScript tracker
        /// </summary>
        [HttpGet]
        public JsonResult GetCurrentAttendance()
        {
            try
            {
                if (MvcApplication.userData == null || MvcApplication.userData.isCompany)
                {
                    return Json(new {
                        hasAttendance = false,
                        debug = "userData is null or isCompany",
                        isNull = MvcApplication.userData == null,
                        isCompany = MvcApplication.userData?.isCompany
                    }, JsonRequestBehavior.AllowGet);
                }

                int empId = MvcApplication.userData.userId;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Debug: Get all attendance records for this employee (fetch then filter)
                var allAttendance = _unitOfWork.AttendanceRepository.Get(
                    a => a.EmpId == empId
                ).ToList();

                // Debug: Filter today's records in memory
                var todayRecords = allAttendance
                    .Where(a => a.CheckIn.HasValue &&
                               a.CheckIn.Value >= today &&
                               a.CheckIn.Value < tomorrow)
                    .ToList();

                var attendance = GetTodayActiveAttendance(empId);

                if (attendance == null)
                {
                    return Json(new {
                        hasAttendance = false,
                        debug = "No active attendance found",
                        empId = empId,
                        today = today.ToString("yyyy-MM-dd"),
                        totalRecords = allAttendance.Count,
                        todayRecordsCount = todayRecords.Count,
                        todayRecordsInfo = todayRecords.Select(r => new {
                            id = r.Id,
                            checkIn = r.CheckIn.HasValue ? r.CheckIn.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                            checkOut = r.CheckOut.HasValue ? r.CheckOut.Value.ToString("yyyy-MM-dd HH:mm:ss") : null
                        })
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    hasAttendance = true,
                    attendanceId = attendance.Id,
                    checkInTime = attendance.CheckIn?.ToString("HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new {
                    hasAttendance = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
