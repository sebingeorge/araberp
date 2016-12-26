using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockReportSummary
    {
        public int? ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal InQuantity { get; set; }
        public decimal OutQuantity { get; set; }
        public decimal Balance { get; set; }
        public string PartNo { get; set; }
        public int itmCatId { get; set; }
        public int itmGrpId { get; set; }
        public int itmSubGrpId { get; set; }
    }
    public class StockSummaryDrillDown
    {
        public string ItemName { get; set; }
        public string StockType { get; set; }
        public string TrnNo { get; set; }
        public decimal INQty { get; set; }
        public decimal OUTQty { get; set; }
    }
}
