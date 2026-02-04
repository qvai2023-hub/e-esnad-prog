using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace EtaskMinstry.Models
{
    public class EmployeesReportModel
    {
        public int EmpId { get; set; }
        public string employeeName { get; set; }
        public string JobTitle { get; set; }
        public int CheckinCount { get; set; }
    }
}