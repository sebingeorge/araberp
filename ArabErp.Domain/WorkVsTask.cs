using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class WorkVsTask
    {
        public int WorkVsTaskId { get; set; }
        public int WorkDescriptionId { get; set; }
        public int JobCardTaskId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
