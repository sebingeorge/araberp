using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class LocalPurchaseRequest
    {
        public int LocalPurchaseRequestId { get; set; }
        public string PurchaseRequestNo { get; set; }
        public DateTime? PurchaseRequestDate { get; set; }
        public string SpecialRemarks { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public List<LocalPurchaseRequestItem> items { get; set; }
    }
}
