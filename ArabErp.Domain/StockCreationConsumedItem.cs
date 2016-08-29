using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockCreationConsumedItem
    {
        public int StockCreationConsumedItemId { get; set; }
        public int StockCreationId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string UnitName { get; set; }
        public string Unit { get; set; }
        public decimal Rate { get; set; }

        public int StockQuantity { get; set; }
    }
}
