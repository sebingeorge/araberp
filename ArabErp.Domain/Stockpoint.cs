using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public  class Stockpoint
    {
       public int StockPointId { get; set; }
       public string StockPointRefNo { get; set; }
       public string StockPointName { get; set; }
       public string StockPointShrtName { get; set; }
       public string StockPointDoorNo { get; set; }
       public string StockPointZip { get; set; }
       public string StockPointArea { get; set; }
       public string StockPointPhone { get; set; }
       public string StockPointCity { get; set; }
       public string StockPointFax { get; set; }
       public string CreatedBy { get; set; }
       public DateTime? CreatedDate { get; set; }
       public int OrganizationId { get; set; }
    }
}
