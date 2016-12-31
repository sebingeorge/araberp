using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class PendingGRN
    {
      public int GRNId { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string SupplierName { get; set; }
        public int SupplierId { get; set; }
        public string SupplyOrderNo { get; set; }
        public DateTime SupplyOrderDate { get; set; }
        public int  Ageing { get; set; }
        public string StockPointName { get; set; }
        public decimal? GrandTotal { get; set; }
       public bool Select { get; set; }
       public decimal Quantity { get; set; }
    }
}
