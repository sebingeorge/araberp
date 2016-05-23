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
       [Required(ErrorMessage = "Please Enter Code")]
       public string StockPointRefNo { get; set; }
       [Required(ErrorMessage = "Please Enter Name")]
       public string StockPointName { get; set; }
       [Required(ErrorMessage = "Please Enter Short Name")]
       public string StockPointShrtName { get; set; }
       [Required(ErrorMessage = "Please Enter Door No")]
       public string StockPointDoorNo { get; set; }
       [Required(ErrorMessage = "Please Enter Zip")]
       public string StockPointZip { get; set; }
       [Required(ErrorMessage = "Please Enter Area")]
       public string StockPointArea { get; set; }
       [Required(ErrorMessage = "Please Enter Phone No")]
       public string StockPointPhone { get; set; }
       [Required(ErrorMessage = "Please Enter City")]
       public string StockPointCity { get; set; }
       [Required(ErrorMessage = "Please Enter Fax")]
       public string StockPointFax { get; set; }
       public string CreatedBy { get; set; }
       public DateTime? CreatedDate { get; set; }
       public int OrganizationId { get; set; }
    }
}
