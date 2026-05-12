namespace EtaskMinstry.Api.Dtos.Employees
{
    /// <summary>
    /// Employee row for /api/v1/employees (Company-only — used in the
    /// task-assignment picker on the mobile app).
    /// </summary>
    public class EmployeeListItemDto
    {
        public int EmpId { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}
