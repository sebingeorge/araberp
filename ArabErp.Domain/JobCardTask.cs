using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Domain
{
    public class JobCardTask
    {
        public int JobCardTaskId { get; set; }
        public int JobCardId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime TaskDate { get; set; }
        public decimal Hours { get; set; }
        public decimal? ActualHours { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool IsInsertedOnCompletion { get; set; }
    

    }
}