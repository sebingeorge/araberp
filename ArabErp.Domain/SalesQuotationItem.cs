using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class SalesQuotationItem
    {
        public int SalesQuotationItemId { get; set; }
        public int SalesQuotationId { get; set; }
        public int? SlNo { get; set; }
        public int WorkDescriptionId { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public int Quantity { get; set; }
        public int UnitId { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
