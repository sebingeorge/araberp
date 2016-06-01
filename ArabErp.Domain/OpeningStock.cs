using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class OpeningStock
    {
        public int? OpeningStockId { get; set; }
        public int? stockpointId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public List<OpeningStockItem> OpeningStockItem { get; set; }
    }
   public class OpeningStockItem
    {
        public int? ItemId { get; set; }
        public decimal? Quantity { get; set; }
       
    }
}
