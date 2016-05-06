using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class SupplyOrder
    {
        public int SupplyOrderId { get; set; }
        public DateTime? SupplyOrderDate { get; set; }
        public int SupplierId { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTerms { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
