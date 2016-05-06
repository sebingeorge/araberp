using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class JobCardTaskEmployee
    {
        public int JobCardTaskEmployeeId { get; set; }
        public int JobCardTaskId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime TaskDate { get; set; }
        public decimal Hours { get; set; }
        public decimal? ActualHours { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
