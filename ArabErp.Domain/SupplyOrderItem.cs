using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class SupplyOrderItem
    {
        public int SupplyOrderItemId { get; set; }
        public int SupplyOrderId { get; set; }
        public int PurchaseRequestItemId { get; set; }
        public int ItemId { get; set; }
        public int? SlNo { get; set; }
        
        public decimal BalQty { get; set; }
        public decimal? OrderedQty { get; set; }
       [Required]
        public decimal Rate { get; set; }
        public decimal FixedRate { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string PartNo { get; set; }
        public string Remarks { get; set; }
        public decimal PendingQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string Unit { get; set; }
        public string PRNODATE { get; set; }
        public string ItemRefNo { get; set; }
        public string UnitName { get; set; }

       

    }
}
