using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingSaleOrderForTransactionApproval
    {
        public int? SaleOrderId { get; set; }
        public int? SaleOrderItemId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string CustomerName { get; set; }
        public string WorkDescr { get; set; }
        public decimal Amount { get; set; }
        public string freezerUnit { get; set; }
        public string Box { get; set; }
        public bool? IsPaymentApprovedForWorkshopRequest { get; set; }
        public bool? IsPaymentApprovedForJobOrder { get; set; }
        public bool? IsPaymentApprovedForDelivery { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public int JodCardCompleteStatus { get; set; }
        public int IsQCPassed { get; set; }
    }
}
