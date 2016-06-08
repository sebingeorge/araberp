using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingSO
    {
        public int? SaleOrderId { get; set; }
        public int? SaleOrderItemId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerOrderRef { get; set; }
        public string VehicleModelName { get; set; }
        public string WorkDescr { get; set; }
    }
}
