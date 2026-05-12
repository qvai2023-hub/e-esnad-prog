using System;

namespace EtaskMinstry.Api.Dtos.Attendance
{
    /// <summary>
    /// Result of POST /api/v1/attendance/check-in.
    /// `IsNew` is false when an existing open row was returned (idempotent call).
    /// </summary>
    public class CheckInResponse
    {
        public int AttendanceId { get; set; }
        public DateTime CheckIn { get; set; }
        public bool IsNew { get; set; }
    }
}
