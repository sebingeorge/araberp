using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class StandardSellingRate
    {
       
        public List<StandardSellingRateItem> StandardSellingRateItems { get; set; }

    }
   public class StandardSellingRateItem
   {
       public int ItemId { get; set; }
       [Required]
       public string ItemName { get; set; }
       public string PartNo { get; set; }
       public decimal? Rate { get; set; }
   }
}
