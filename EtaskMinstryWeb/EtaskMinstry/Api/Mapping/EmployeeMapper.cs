using EtaskMinstry.Api.Dtos.Employees;
using TaskManagementModel;

namespace EtaskMinstry.Api.Mapping
{
    public static class EmployeeMapper
    {
        public static EmployeeListItemDto ToListItem(Employee e)
        {
            if (e == null) return null;
            return new EmployeeListItemDto
            {
                EmpId = e.EmpID,
                Name = e.Name,
                JobTitle = e.JobTitle,
                Email = e.Email,
                Mobile = e.Mobile
            };
        }
    }
}
