using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingWorkShopRequest
    {
        public int WorkShopRequestId { get; set; }
        public string WorkShopRequestNo { get; set; }
        public DateTime? WorkShopRequestDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderRef { get; set; }
        public string CustomerName { get; set; }
    }
}
