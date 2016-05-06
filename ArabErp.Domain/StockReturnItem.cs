using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class StockReturnItem
    {
        public int StockReturnItemId { get; set; }
        public int StockReturnId { get; set; }
        public int SlNo { get; set; }
        public int ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string PartNo { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
