using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class DashboardMonthlySalesOrders
    {
        public string SODate { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class DashboardTotalSalesQuotations
    {
        public string SODate { get; set; }
        public int Quotations { get; set; }
    }
}
