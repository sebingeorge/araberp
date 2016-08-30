using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockTransfer : StockTransferItem
    {
        public int StockTransferId { get; set; }
        public string StockTransferRefNo { get; set; }
        public DateTime StockTransferDate { get; set; }
        public int FromStockpointId { get; set; }
        public string FromStockpointName { get; set; }
        public int ToStockpointId { get; set; }
        public string ToStockpointName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public List<StockTransferItem> Items { get; set; }
    }
}
