namespace EtaskMinstry.Api.Dtos.Tasks
{
    /// <summary>
    /// Body for POST /api/v1/tasks/{id}/time
    /// Wraps TaskManger.EmpUpdateTaskTime — value is in hours (TimeUnit.Hour=1).
    /// </summary>
    public class UpdateTimeDto
    {
        public decimal Time { get; set; }
    }
}
