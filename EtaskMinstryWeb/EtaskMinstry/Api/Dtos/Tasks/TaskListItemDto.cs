using System;

namespace EtaskMinstry.Api.Dtos.Tasks
{
    /// <summary>
    /// Compact task row for /api/v1/tasks list responses.
    /// </summary>
    public class TaskListItemDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; }
        public string PriorityColor { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? ExpectedTime { get; set; }
        public decimal? ActualTime { get; set; }
        public string ProjectName { get; set; }
        public bool IsDelayed { get; set; }
        public bool IsAccepted { get; set; }
    }
}
