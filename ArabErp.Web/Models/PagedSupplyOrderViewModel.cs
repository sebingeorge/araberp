using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabErp.Web.Models
{
    public class PagedSupplyOrderViewModel
    {
        public IEnumerable<ArabErp.Domain.SupplyOrder> SupplyOrders { get; set; }
        public Pager Pager { get; set; }
     
    }
}