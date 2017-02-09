using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SupplyOrderPreviousList
    {
        public int SupplyOrderId { get; set; }
        public string SupplyOrderNo { get; set; }
        public string SupplierName { get; set; }
        public string QuotationNoAndDate { get; set; }
        public DateTime SupplyOrderDate { get; set; }
        public int RequestedQuantity { get; set; }
        public int SuppliedQuantity { get; set; }
        public int BalanceQuantity { get; set; }
        public decimal Amount { get; set; }
        public string ItemName { get; set; }
        public string PartNo { get; set; }
        public int GRNQty { get; set; }
    }
}
