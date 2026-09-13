namespace EtaskMinstry.Api.Dtos.Internal
{
    /// <summary>
    /// One item of the response <c>data</c> array for
    /// GET /api/internal/companies/{companyId}/attachments.
    ///
    /// Everything <see cref="OpsAttachmentListItemDto"/> carries, plus the task
    /// and employee context the Ops Portal needs to build a folder-per-employee
    /// export without a second round trip per task.
    ///
    /// As with the base type, the formatter runs <c>NullValueHandling.Ignore</c>,
    /// so an unknown field is absent from the JSON rather than null.
    /// </summary>
    public class OpsCompanyAttachmentItemDto : OpsAttachmentListItemDto
    {
        /// <summary>Task.EmpID — the employee the task is assigned to. Null for an unassigned task.</summary>
        public int? EmpId { get; set; }

        /// <summary>Employee.Name, for the export's folder label. Null when the task is unassigned.</summary>
        public string EmpName { get; set; }

        public string TaskTitle { get; set; }

        /// <summary>
        /// Task.StartDate as a plain <c>yyyy-MM-dd</c> string — the column the date
        /// range filters on.
        ///
        /// Deliberately not a DateTime: the formatter runs
        /// <c>DateTimeZoneHandling.Utc</c>, which would either stamp this
        /// server-local value "Z" without shifting it (a lie) or shift it and move
        /// the item to the previous day, so that an attachment matching
        /// <c>fromDate=2026-08-14</c> would report <c>2026-08-13</c>. This is a
        /// business date the caller matches against its own filter values, not an
        /// instant, so it is sent in the same format the filter parameters use.
        /// Null when the task has no start date.
        /// </summary>
        public string TaskStartDate { get; set; }
    }
}
