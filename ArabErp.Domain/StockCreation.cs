using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockCreation
    {
        public int StockCreationId { get; set; }
        public string StockCreationRefNo { get; set; }
        public DateTime StockCreationDate { get; set; }

        public List<StockCreationFinishedGood> FinishedGoods { get; set; }
        public List<StockCreationConsumedItem> ConsumedItems { get; set; }
    }
}
