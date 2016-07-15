using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingGRNItemsForBatch
    {
        public int GRNItemId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Remarks { get; set; }
        public string isDirect { get; set; }
        public string GRNNo { get; set; }
        public DateTime GRNDate { get; set; }
        public string SupplierName { get; set; }
        public int Ageing { get; set; }
        public string StockPointName { get; set; }
    }
}
