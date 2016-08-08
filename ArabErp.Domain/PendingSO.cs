using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingSO
    {
        public int? SaleOrderId { get; set; }
        public int? SaleOrderItemId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerOrderRef { get; set; }
        public string VehicleModelName { get; set; }
        public string WorkDescription { get; set; }
        public string SaleOrderHoldReason { get; set; }
        public DateTime SaleOrderHoldDate { get; set; }
        public DateTime EDateArrival { get; set; }
        public DateTime EDateDelivery { get; set; }
        public int? Ageing { get; set; }
        public int? Remaindays { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int? IsPaymentApprovedForJobOrder { get; set; }
        public string QuotationNoDate { get; set; }
        public string EmployeeName { get; set; }
        

        
    }
}
