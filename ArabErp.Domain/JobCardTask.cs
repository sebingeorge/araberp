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
        public int JobCardTaskMasterId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime TaskDate { get; set; }
        public decimal Hours { get; set; }
        public decimal? ActualHours { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int SlNo { get; set; }
        public string Employee { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartTime { get; set; }
        public decimal EndTime { get; set; }
        public string JobCardTaskName { get; set; }
        public bool isTaskUsed { get; set; }
    }
}