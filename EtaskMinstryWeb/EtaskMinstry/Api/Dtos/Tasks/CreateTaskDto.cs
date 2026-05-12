using System;

namespace EtaskMinstry.Api.Dtos.Tasks
{
    /// <summary>
    /// Body for POST /api/v1/tasks (Company creates and assigns a task).
    /// Dates are ISO 8601 Gregorian — mobile clients handle any Hijri display.
    /// </summary>
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string BriefTaskName { get; set; }
        public int PriorityId { get; set; }
        public int EmpId { get; set; }
        public int? ProjectId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? ExpectedTime { get; set; }
        public int? TimeUnitId { get; set; }
    }
}
