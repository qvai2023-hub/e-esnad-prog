namespace EtaskMinstry.Api.Dtos.Tasks
{
    /// <summary>
    /// Return body for task workflow actions (accept/reject/complete/approve/disapprove).
    /// `NewStatusId` reflects the task status after the action.
    /// </summary>
    public class TaskActionResultDto
    {
        public int TaskId { get; set; }
        public int NewStatusId { get; set; }
        public string NewStatusName { get; set; }
    }
}
