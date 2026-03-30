namespace TaskManagementModel
{
    /// <summary>
    /// Partial class to add display-formatted date for Hijri/Gregorian support in reports.
    /// </summary>
    public partial class sp_Attendance_Result
    {
        /// <summary>
        /// Pre-formatted date string for report display.
        /// Populated in AttendanceController based on calendarType.
        /// </summary>
        public string DisplayDate { get; set; }
    }
}
