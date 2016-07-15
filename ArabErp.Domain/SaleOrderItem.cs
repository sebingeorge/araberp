using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class SaleOrderItem
    {
        public int? SaleOrderItemId { get; set; }
        public int? SaleOrderId { get; set; }
        public int? SlNo { get; set; }
        public int? WorkDescriptionId { get; set; }       
        public int? VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
        public int? Quantity { get; set; }
        public int? ActualQuantity { get; set; }
       public int? UnitId { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? OrganizationId { get; set; }
        public bool? IsPaymentApprovedForWorkshopRequest { get; set; }
       [Required]
        public string PaymentApprovedForWorkshopRequestReceiptNoAndDate { get; set; }
        public bool? IsPaymentApprovedForJobOrder { get; set; }
       [Required]
        public string PaymentApprovedForJobOrderReceiptNoAndDate { get; set; }
        public bool? IsPaymentApprovedForDelivery { get; set; }
       [Required]
        public string PaymentApprovedForDeliveryReceiptNoAndDate { get; set; }
        public string PaymentApprovedForWorkshopRequestCreatedBy { get; set; }
        public DateTime? PaymentApprovedForWorkshopRequestCreatedDate { get; set; }
        public string PaymentApprovedForJobOrderCreatedBy { get; set; }
        public DateTime? PaymentApprovedForJobOrderCreatedDate { get; set; }
        public string PaymentApprovedForDeliveryCreatedBy { get; set; }
        public DateTime? PaymentApprovedForDeliveryCreatedDate { get; set; }

    }
}
