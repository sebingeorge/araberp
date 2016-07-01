using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class JobCardDailyActivity
    {
        public int JobCardDailyActivityId { get; set; }
        public string JobCardDailyActivityRefNo { get; set; }
        public DateTime JobCardDailyActivityDate { get; set; }
        public int JobCardId { get; set; }
        public string JobCardNo { get; set; }
        public string Remarks { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public List<JobCardDailyActivityTask> JobCardDailyActivityTask { get; set; }
    }
}
