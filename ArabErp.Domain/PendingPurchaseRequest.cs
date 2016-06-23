using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class PendingPurchaseRequest
    {
        public int PurchaseRequestId { get; set; }
        public string PurchaseRequestNo { get; set; }
        public DateTime PurchaseRequestDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public bool Select { get; set; }
        public int Ageing { get; set; }
    }
}
