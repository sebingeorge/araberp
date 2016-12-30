using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class GRNRegister
    {
        public int GRNId { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string SupplierName { get; set; }
        public string SupplyOrderNo { get; set; }
        public string ItemName { get; set; }
        public string PartNo { get; set; }
        public string Quantity { get; set; }
        public string Rate { get; set; }
        public string Amount { get; set; }
        public string UnitName { get; set; }

    }
}
