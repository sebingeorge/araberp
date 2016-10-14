using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ArabErp.Domain
{
   public  class Stockpoint
    {
       public int StockPointId { get; set; }
       public string StockPointRefNo { get; set; }
       [Required]
       public string StockPointName { get; set; }
       [Required]
       public string StockPointShrtName { get; set; }
       [Required]
       public string StockPointDoorNo { get; set; }
       [Required]
       public string StockPointZip { get; set; }
       [Required]
       public string StockPointArea { get; set; }
       [Required]
       public string StockPointPhone { get; set; }
       [Required]
       public string StockPointCity { get; set; }
       [Required]
       public string StockPointFax { get; set; }
       public string CreatedBy { get; set; }
       public DateTime? CreatedDate { get; set; }
       public int OrganizationId { get; set; }
    }
}
