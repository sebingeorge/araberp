using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class Dashboard
    {
        public IEnumerable<DashboardMonthlySalesOrders> DashboardMonthlySalesOrders { get; set; }
        public IEnumerable<DashboardTotalSalesQuotations> DashboardTotalSalesQuotations { get; set; }
        public IEnumerable<DashboardTotalSalesQuotations> DashboardAcceptedSalesQuotations { get; set; }
        public IEnumerable<DashboardTotalSalesQuotations> DashboardAcceptedProjectSalesQuotations { get; set; }
        public IEnumerable<DashboardTotalSalesQuotations> DashboardAcceptedTransportationSalesQuotations { get; set; }
        public IEnumerable<DashboardPurchaseSales> DashboardSales { get; set; }
        public IEnumerable<DashboardPurchaseSales> DashboardPurchase { get; set; }
    }
}
