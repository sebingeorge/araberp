using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class WorkVsItem
    {
        public int WorkVsItemId { get; set; }
        public int ItemId { get; set; }
        public int JobCardTaskMasterId { get; set; }
        public decimal Quantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
