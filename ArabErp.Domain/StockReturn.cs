using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class StockReturn
    {
        public int StockReturnId { get; set; }
        public string StockReturnRefNo { get; set; }
        public DateTime? StockReturnDate { get; set; }
        public int JobCardId { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public List<StockReturnItem> Items { get; set; }
    }
}
