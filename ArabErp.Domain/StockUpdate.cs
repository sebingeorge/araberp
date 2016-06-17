using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class StockUpdate
    {
        public int StockUpdateId { get; set; }
        public int StockPointId { get; set; }
        public int? ItemId { get; set; }
        public decimal? Quantity { get; set; }
        public string StockType { get; set; }
        public string StockInOut { get; set; }
        public DateTime? stocktrnDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int StocktrnId { get; set; }
        public string StockUserId { get; set; }
    }
}
