using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class OpeningStock
    {
        public int OpeningStockId { get; set; }
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
