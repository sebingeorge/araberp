using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class DirectPurchaseGRNItem
    {
        public int LocalPurchaseGRNItemId { get; set; }
        public int LocalPurchaseGRNId { get; set; }
        public int? SupplyOrderItemId { get; set; }
        public int? SlNo { get; set; }
        public string ItemName { get; set; }
        public int? ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public decimal? PendingQuantity { get; set; }
        public decimal? Quantity { get; set; }
        public string Unit { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Amount { get; set; }
    }
}
