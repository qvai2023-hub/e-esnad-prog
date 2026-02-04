using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EtaskMinstry.Areas.Company.Models
{
    public class ReportEmployeeTasks
    {
        public List<int> employees { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public string FromEndDate { get; set; }

        public string ToEndDate { get; set; }
    }
}