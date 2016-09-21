using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class SalesQuotationMaterial
    {
       public int SalesQuotationMaterialId  { get; set; }
        public int SalesQuotationId{ get; set; }
        public int  ItemId{ get; set; }
        public string ItemName { get; set; }
        public int Quantity{ get; set; }
        public decimal Rate { get; set; }
        public int? SlNo { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public string UnitName { get; set; }
        public decimal Amount { get; set; }

    }
}
