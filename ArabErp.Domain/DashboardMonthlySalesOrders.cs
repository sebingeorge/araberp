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
    public class DashboardPurchaseSales
    {
        public string InvoiceDate { get; set; }
        public decimal Amount { get; set; }
    }
    public class DashboardFGAllocated
    {
        public string TotalFG { get; set; }
        public string AllocatedFG { get; set; }
    }
    public class DashboardSaleOrderAllocated
    {
        public string TotalFG { get; set; }
        public string AllocatedSaleOrders { get; set; }
        public string TotalSaleOrders { get; set; }
    }
}
