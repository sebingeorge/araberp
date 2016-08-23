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


        public decimal? Varperc { get; set; }
        public string MonthName { get; set; }
        public decimal? Total { get; set; }
        public decimal? Jan { get; set; }
        public decimal? Feb { get; set; }
        public decimal? Mar { get; set; }
        public decimal? Apr { get; set; }
        public decimal? May { get; set; }
        public decimal? Jun { get; set; }
        public decimal? Jul { get; set; }
        public decimal? Aug { get; set; }
        public decimal? Sep { get; set; }
        public decimal? Oct { get; set; }
        public decimal? Nov { get; set; }
        public decimal? Dece { get; set; }
    }
}
