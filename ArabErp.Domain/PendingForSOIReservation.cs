using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingForSOIReservation
    {
        public int SaleOrderItemId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public int Quantity { get; set; }
        public string WorkDescriptionRefNo { get; set; }
        public string ItemName { get; set; }
    }
}
