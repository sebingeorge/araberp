using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockCreationFinishedGood
    {
        public int StockCreationFinishedGoodId { get; set; }
        public int StockCreationId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
    }
}
