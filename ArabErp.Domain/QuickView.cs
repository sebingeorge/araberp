using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class QuickView
    {
        public bool PendingProjectQuotations { get; set; }
        public int NoOfProjectQuotations { get; set; }
        public bool PendingTransQuotations { get; set; }
        public int NoOfTransQuotations { get; set; }
        public bool PendingSupplyOrders { get; set; }
        public int NoOfSupplyOrders { get; set; }
        public bool PendingWorkshopRequests { get; set; }
        public int NoOfWorkShopRequests { get; set; }
        public bool PendingDirectPurchaseRequests { get; set; }
        public int NoOfPurchaseRequests { get; set; }
        public bool PendingProjectSaleOrdersForJobCard { get; set; }
        public int NoOfProjectSaleOrdersForJobCard { get; set; }
        public bool PendingTransSaleOrdersForJobCard { get; set; }
        public int NoOfTransSaleOrdersForJobCard { get; set; }
        public bool MaterialStockBelowMinLevel { get; set; }
        public int NoOfMaterialStockBelowMinLevel { get; set; }
        public bool PendingVechicleInpass { get; set; }
        public int NoOfVechicleInpass { get; set; }
        public bool PendingWRForStoreIssue { get; set; }
        public int NoOfWRForStoreIssue { get; set; }
        public bool PendingJobcardQC { get; set; }
        public int NoOfJobcardQC { get; set; }


    }
}
