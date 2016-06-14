using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class PurchaseBillItem
    {
        public int PurchaseBillItemId { get; set; }
        public int PurchaseBillId { get; set; }
        public int GRNItemId { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public bool isActive { get; set; }
        public int OrganizationId { get; set; }
    }
}
