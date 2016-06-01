using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class JobCardQC
    {
        public int JobCardQCId { get; set; }
        public int JobCardId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime JobCardQCDate { get; set; }
        public bool? IsQCPassed { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
