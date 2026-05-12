using EtaskMinstry.Api.Dtos.Projects;
using TaskManagementModel;

namespace EtaskMinstry.Api.Mapping
{
    public static class ProjectMapper
    {
        public static ProjectListItemDto ToListItem(Project p)
        {
            if (p == null) return null;
            return new ProjectListItemDto
            {
                ProjectId = p.ProjectID,
                Name = p.Name,
                Description = p.Description,
                CompanyId = p.CompanyID
            };
        }
    }
}
