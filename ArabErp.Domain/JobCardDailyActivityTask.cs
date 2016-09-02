using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class JobCardDailyActivityTask
    {
        public int JobCardDailyActivityTaskId { get; set; }
        public int JobCardDailyActivityId { get; set; }
        public int JobCardTaskId { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskEndDate { get; set; }
        public decimal? ActualHours { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }

        public string JobCardTaskName { get; set; }

        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int JobCardTaskMasterId { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
