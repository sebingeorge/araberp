using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class GRNItem
    {
        public int GRNItemId { get; set; }
        public int GRNId { get; set; }
        public int? SupplyOrderId { get; set; }
        public int? SupplyOrderItemId { get; set; }
        public int? DirectPurchaseRequestId { get; set; }
        public int? DirectPurchaseRequestItemId { get; set; }
        public int? SlNo { get; set; }
        public string ItemName { get; set; }
        public int? ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public int? PendingQuantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public int? AcceptedQuantity { get; set; }
        public int? RejectedQuantity { get; set; }
        public string Unit { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
