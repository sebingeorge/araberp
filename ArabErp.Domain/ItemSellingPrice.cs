using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class ItemSellingPrice
    {
        public int ItemSellingPriceId { get; set; }
       public int ItemId { get; set; }
        [Required]
        public string ItemName { get; set; }

        public string PartNo { get; set; }

        public string CategoryName { get; set; }
        public string ItemGroupName { get; set; }
        public string ItemSubGroupName { get; set; }
        public string UnitName { get; set; }
        public decimal? SellingPrice { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
