using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class SalesInvoiceItem
    {
        public int SalesInvoiceItemId { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public int SalesInvoiceId { get; set; }
        public string WorkDescription { get; set; }
        public string WorkDescriptionRefNo { get; set; }
        public int? SlNo { get; set; }
        public int? ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string PartNo { get; set; }
        public int? Quantity { get; set; }
        public int? QuantityTxt { get; set; }
        public string Unit { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int SaleOrderId { get; set; }
        public string VehicleModelName { get; set; }
        public string RegistrationNo { get; set; }
        public string ChassisNo { get; set; }
        public bool SelectStatus { get; set; }
        public int SaleOrderItemId { get; set; }
        public int JobCardId { get; set; }
        public string invType { get; set; }
        public string DeliveryChallanRefNo { get; set; }
        public DateTime DeliveryChallanDate { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
    }
}
