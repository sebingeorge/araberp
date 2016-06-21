using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class SupplyOrder
    {
       public SupplyOrder()
       {
           SupplyOrderItems = new List<SupplyOrderItem>();
       }
        public int SupplyOrderId { get; set; }
        public DateTime SupplyOrderDate { get; set; }
        [Required]
        public int PurchaseRequestId { get; set; }
        public int SupplierId { get; set; }
        public string SupplyOrderNo { get; set; }
        public string QuotaionNoAndDate { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string DeliveryTerms { get; set; }
        public DateTime RequiredDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        public int OrganizationId { get; set; }
        public string SupplierName { get; set; }
        public string SoNoWithDate { get; set; }
        public List<SupplyOrderItem> SupplyOrderItems { get; set; }
       // public string SoNoWithDate
       // {
       //     get
       //     {
       //         return string.Format("{0},{1}", SupplyOrderId, SupplyOrderDate.ToShortDateString());
       //     }
       //     set
       //     {

       //     }
       //}

    }
}
