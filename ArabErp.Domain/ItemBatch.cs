using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class ItemBatch
    {
        public int ItemBatchId { get; set; }
        public int GRNItemId { get; set; }
        public int? SaleOrderItemId { get; set; }
        public int? StoreIssueItemId { get; set; }
        public string SerialNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
