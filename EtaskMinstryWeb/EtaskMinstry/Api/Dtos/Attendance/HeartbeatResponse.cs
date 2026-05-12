using System;

namespace EtaskMinstry.Api.Dtos.Attendance
{
    /// <summary>
    /// Result of POST /api/v1/attendance/heartbeat.
    /// </summary>
    public class HeartbeatResponse
    {
        public int AttendanceId { get; set; }
        public DateTime LastHeartbeat { get; set; }
    }
}
