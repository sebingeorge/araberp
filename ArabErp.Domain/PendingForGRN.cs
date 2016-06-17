using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingForGRN
    {
        public int? SupplyOrderId { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SoNoWithDate { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public int? DirectPurchaseRequestId { get; set; }
        public string RequestNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public int? Age { get; set; }
        public int? TotalAmount { get; set; }
        public bool isChecked { get; set; }
        public bool isDirectPurchase { get; set; }
    }
}
