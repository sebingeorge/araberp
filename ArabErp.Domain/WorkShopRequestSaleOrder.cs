using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class WorkShopRequestSaleOrder
    {
        public WorkShopRequestSaleOrder()
        {
            WorkShopRequestSaleOrderItems = new List<WorkShopRequestSaleOrderItem>();
        }
        public int WorkShopRequestSaleOrderId { get; set; }
        public int WorkShopRequestId { get; set; }
        public int SaleOrderId { get; set; }
        public string CustomerOrderRef { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }

        public List<WorkShopRequestSaleOrderItem> WorkShopRequestSaleOrderItems { get; set; }
    }
}
