using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class JobCardTaskMaster
    {
        public int JobCardTaskMasterId { get; set; }
        public string JobCardTaskRefNo { get; set; }
        [Required]
        public string JobCardTaskName { get; set; }
        [Required]
        public Decimal MinimumRate { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
