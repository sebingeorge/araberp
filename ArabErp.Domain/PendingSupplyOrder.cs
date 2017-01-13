using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingSupplyOrder
    {
        public int SupplyOrderId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SoNoWithDate { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public bool isChecked { get; set; }
        public decimal Quantity { get; set; }
    }
}
