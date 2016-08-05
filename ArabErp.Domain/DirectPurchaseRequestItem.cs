using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class DirectPurchaseRequestItem
    {
        public int DirectPurchaseRequestItemId { get; set; }
        public int DirectPurchaseRequestId { get; set; }
        public int? SlNo { get; set; }
        public string ItemName { get; set; }
        public int? ItemId { get; set; }
        public string Remarks { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public string txtPartNo { get; set; }
        public string txtUoM { get; set; }
    }
}
