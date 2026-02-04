
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EtaskMinstry.Models
{
    public class BriefTasksReportModel
    {
        public int Id { get; set; }
        public string BriefTaskName { get; set; }
        public string status { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public DateTime CreatedDate { get; set; }
        public string  IsActive { get; set; }
    }
}