using System;

namespace EtaskMinstry.Api.Dtos.Tasks
{
    /// <summary>
    /// A single status-change entry from TaskTLog. Used inside TaskDetailDto.
    /// </summary>
    public class TaskStatusLogDto
    {
        public int LogId { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int? EmpId { get; set; }
        public string EmpName { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal? TimeCount { get; set; }
    }
}
