using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Web.Models
{
    public class PagedSaleOrderViewModel
    {
        public IEnumerable<ArabErp.Domain.SaleOrder> SaleOrders { get; set; }
        public Pager Pager { get; set; }
     
      
    }
}