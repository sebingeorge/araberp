using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class WorkshopRequestRegister
    {
       public string WorkShopRequestRefNo { get; set; }
       public DateTime WorkShopRequestDate { get; set; }
       public string ItemName { get; set; }
       public string UnitName { get; set; }
       public decimal Quantity { get; set; }


    }
}
