using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class GINRegister
    {
        public int ItemId { get; set; }
        public string ItemRefNo { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public string PartNo { get; set; }
        public decimal Quantity { get; set; }
      
        public decimal ISSQTY { get; set; }
        public decimal BALQTY { get; set; }

        public decimal STOCK { get; set; }

       
    }
}
