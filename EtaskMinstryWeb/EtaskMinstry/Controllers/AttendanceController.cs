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

        public ActionResult ViewReport(int? CompanyId, int? EmployeeId, string FromDate, string ToDate, int? calendarType)
        {
            // Convert Hijri dates to Gregorian before passing to service
            string fromDateForSP = FromDate;
            string toDateForSP = ToDate;

            if (calendarType.HasValue && calendarType.Value == 0 && !string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                // Hijri → Gregorian using same pattern as CompanyTasks ReportController
                DateTime gregFrom = QvLib.QVUtil.Date.hijritodate(FromDate).Date;
                DateTime gregTo = QvLib.QVUtil.Date.hijritodate(ToDate).Date;
                fromDateForSP = gregFrom.ToString("dd/MM/yyyy");
                toDateForSP = gregTo.ToString("dd/MM/yyyy");
            }

            var data = attendanceReportService.GetAttendence(CompanyId, EmployeeId, fromDateForSP, toDateForSP);

            // Convert AttendanceDate for display (Hijri or Gregorian)
            CultureInfo hijriDisplayCulture = new CultureInfo("ar-SA");
            hijriDisplayCulture.DateTimeFormat.Calendar = new System.Globalization.UmAlQuraCalendar();
            foreach (var row in data)
            {
                if (row.AttendanceDate.HasValue)
                {
                    if (calendarType.HasValue && calendarType.Value == 0)
                        row.DisplayDate = row.AttendanceDate.Value.ToString("yyyy/MM/dd", hijriDisplayCulture);
                    else
                        row.DisplayDate = row.AttendanceDate.Value.ToString("yyyy/MM/dd");
                }
            }

            ReportAgent.ReportDataSources.Clear();
            ReportAgent.ReportParameters.Clear();

            // Format date range for report header
            string startDateDisplay = "";
            string endDateDisplay = "";
            string reportPeriod = "";

            if (!string.IsNullOrEmpty(fromDateForSP) && !string.IsNullOrEmpty(toDateForSP))
            {
                DateTime fromDt = DateTime.ParseExact(fromDateForSP, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime toDt = DateTime.ParseExact(toDateForSP, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (calendarType.HasValue && calendarType.Value == 0)
                {
                    CultureInfo hijriCulture = new CultureInfo("ar-SA");
                    hijriCulture.DateTimeFormat.Calendar = new System.Globalization.UmAlQuraCalendar();
                    startDateDisplay = fromDt.ToString("yyyy/MM/dd", hijriCulture);
                    endDateDisplay = toDt.ToString("yyyy/MM/dd", hijriCulture);
                    reportPeriod = "شهر " + fromDt.ToString("MMMM yyyy", hijriCulture);
                }
                else
                {
                    CultureInfo arCulture = new CultureInfo("ar-SA");
                    arCulture.DateTimeFormat.Calendar = new GregorianCalendar();
                    startDateDisplay = fromDt.ToString("yyyy/MM/dd");
                    endDateDisplay = toDt.ToString("yyyy/MM/dd");
                    reportPeriod = "شهر " + fromDt.ToString("MMMM yyyy", arCulture);
                }
            }

            // Add report parameters
            ReportAgent.ReportParameters.Add(new ReportParameter("StartDate", startDateDisplay));
            ReportAgent.ReportParameters.Add(new ReportParameter("EndDate", endDateDisplay));
            ReportAgent.ReportParameters.Add(new ReportParameter("ReportPeriod", reportPeriod));
            ReportAgent.ReportParameters.Add(new ReportParameter("CalendarType", (calendarType ?? 1).ToString()));

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

                // Update LastHeartbeat on Attendance record using direct SQL
                var efConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString();
                var entityBuilder = new System.Data.EntityClient.EntityConnectionStringBuilder(efConnStr);
                string connStr = entityBuilder.ProviderConnectionString;
                var now = DateTime.Now;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sqlUpdate = "UPDATE Attendance SET LastHeartbeat = @LastHeartbeat WHERE Id = @Id";
                    using (var cmd = new SqlCommand(sqlUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@LastHeartbeat", now);
                        cmd.Parameters.AddWithValue("@Id", attendance.Id);
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

                // Clear session flag so tracker won't load on next page
                if (Session != null)
                {
                    Session["HasActiveAttendance"] = false;
                }

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

                // Use LastHeartbeat from Attendance record
                DateTime checkoutTime = attendance.LastHeartbeat ?? DateTime.Now;
                attendance.CheckOut = checkoutTime;
                _unitOfWork.AttendanceRepository.Update(attendance);
                _unitOfWork.Save();

                // Clear session flag so tracker won't load on next page
                if (Session != null)
                {
                    Session["HasActiveAttendance"] = false;
                }

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

                // Get today's attendance records for this employee (filtered at DB level)
                var allAttendance = _unitOfWork.AttendanceRepository.Get(
                    filter: a => a.EmpId == empId
                               && a.CheckIn.HasValue
                               && a.CheckIn.Value >= today
                               && a.CheckIn.Value < tomorrow
                ).ToList();

                var todayRecords = allAttendance
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
