using System;
using System.Collections.Generic;

namespace EtaskMinstry.Api.Dtos.Tasks
{
    /// <summary>
    /// Full task payload for /api/v1/tasks/{id}.
    /// </summary>
    public class TaskDetailDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DeliverDate { get; set; }
        public decimal? ExpectedTime { get; set; }
        public decimal? ActualTime { get; set; }
        public int? TimeUnitId { get; set; }
        public string TimeUnitName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; }
        public string PriorityColor { get; set; }
        public int? EmpId { get; set; }
        public string EmpName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool IsArchived { get; set; }
        public bool IsRecurrence { get; set; }
        public bool IsDelayed { get; set; }
        public List<TaskStatusLogDto> StatusLog { get; set; }
    }
}
