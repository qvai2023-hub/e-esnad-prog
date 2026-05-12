using EtaskMinstry.Api.Dtos.Attendance;

namespace EtaskMinstry.Api.Dtos.Auth
{
    /// <summary>
    /// Combined response for GET /api/v1/me — user summary + today's attendance.
    /// </summary>
    public class MeResponse
    {
        public UserSummaryDto User { get; set; }
        public TodayAttendanceDto Attendance { get; set; }
    }
}
