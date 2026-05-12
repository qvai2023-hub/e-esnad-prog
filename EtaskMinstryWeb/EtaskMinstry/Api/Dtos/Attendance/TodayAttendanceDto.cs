using System;

namespace EtaskMinstry.Api.Dtos.Attendance
{
    /// <summary>
    /// Current state of an employee's attendance for today.
    /// Returned by GET /api/v1/attendance/today and embedded in GET /api/v1/me.
    /// </summary>
    public class TodayAttendanceDto
    {
        public bool HasAttendance { get; set; }
        public int? AttendanceId { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsOpen { get; set; }   // true if CheckIn set and CheckOut null
    }
}
