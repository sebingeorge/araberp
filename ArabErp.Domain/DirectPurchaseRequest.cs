using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class DirectPurchaseRequest
    {
        public DirectPurchaseRequest()
       {
           items = new List<DirectPurchaseRequestItem>();
       }
        public int DirectPurchaseRequestId { get; set; }
        public string PurchaseRequestNo { get; set; }
        public DateTime? PurchaseRequestDate { get; set; }
        public string SpecialRemarks { get; set; }
        [Required]
        [ValidateDateGreaterThan("PurchaseRequestDate")]
        public DateTime? RequiredDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string toast { get; set; }
        public string SoOrJc { get; set; }
        public int? SaleOrderId { get; set; }
        public int? JobCardId { get; set; }
        public List<DirectPurchaseRequestItem> items { get; set; }

        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string JobCardNo { get; set; }
        public DateTime JobCardDate { get; set; }
        public int Ageing { get; set; }
        public int DaysLeft { get; set; }
    }
}
