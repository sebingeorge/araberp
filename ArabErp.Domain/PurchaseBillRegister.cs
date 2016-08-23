using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class PurchaseBillRegister
    {
      
        public string PurchaseBillRefNo { get; set; }
        public string SupplierName { get; set; }
        public DateTime PurchaseBillDate { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public decimal Rate { get; set; }
        public string PurchaseBillNoDate { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public string UnitName { get; set; }
    }
}
