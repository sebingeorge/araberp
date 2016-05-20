using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class SaleOrder
    {
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderRef { get; set; }
        public int VehicleModelId { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTerms { get; set; }
        public int? CommissionAgentId { get; set; }
        public decimal? CommisionAmount { get; set; }
        public int? SalesExecutiveId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
