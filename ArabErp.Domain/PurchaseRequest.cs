using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class PurchaseRequest
    {
        public int PurchaseRequestId { get; set; }
        public string PurchaseRequestNo { get; set; }
        [Required]
        public DateTime PurchaseRequestDate { get; set; }
        [Required]
        public int WorkShopRequestId { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public string SaleOrderRefNo { get; set; }
        public string CustomerOrderRef { get; set; }
        public string CustomerName { get; set; }
        public string SpecialRemarks { get; set; }
        [Required]
        [ValidateDateGreaterThan("PurchaseRequestDate")]
        public DateTime RequiredDate { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public List<PurchaseRequestItem>items { get; set; }

    }
}
