using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockTransferItem
    {
        public int StockTransferItemId { get; set; }
        public int StockTransferId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public bool isActive { get; set; }
        public int StockQuantity { get; set; }
    }
}
