namespace EtaskMinstry.Api.Dtos.Projects
{
    /// <summary>
    /// Project row for /api/v1/projects (used by both Employee and Company for task pickers).
    /// </summary>
    public class ProjectListItemDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CompanyId { get; set; }
    }
}
