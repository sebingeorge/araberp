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
        public DateTime StockReturnDate { get; set; }
        public int StockPointId { get; set; }
        public int JobCardId { get; set; }
        public String CustomerName { get; set; }
        public String WorkDescr { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public List<StockReturnItem> Items { get; set; }

        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public string ItemName { get; set; }
    }
}
