using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class WorkShopRequestSaleOrderItem
    {
        public int WorkShopRequestSaleOrderItemId { get; set; }
        public int WorkShopRequestSaleOrderId { get; set; }
        public int SaleOrderItemId { get; set; }
        public int ItemId { get; set; }
        public decimal RequiredQuantity { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
    }
}
