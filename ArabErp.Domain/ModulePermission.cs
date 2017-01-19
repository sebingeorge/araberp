using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ModulePermission
    {
        public bool Admin { get; set; }
        public bool Purchase { get; set; }
        public bool Sales { get; set; }
        public bool Project { get; set; }
        public bool Transportation { get; set; }
        public bool Finance { get; set; }
        public bool MISReports { get; set; }
        public bool Store { get; set; }
    }
    public class AlertPermission
    {
        public bool ProjectQuotApproval { get; set; }
        public bool TransportQuotApproval { get; set; }
        public bool PendingSupplyOrdForGrn { get; set; }
        public bool DirectPurchaseReqDorApproval { get; set; }
        public bool PendingSOForWorkshopReq { get; set; }
        public bool PendingProjectSaleOrdersForJobCard { get; set; }
        public bool PendingTransSaleOrdersForJobCard { get; set; }
        public bool MaterialStockBelowMinLevel { get; set; }
        public bool PendingVechicleInpass { get; set; }
        public bool PendingWRForStoreIssue { get; set; }
        public bool PendingJobcardQC { get; set; }
    }
    public class GraphPermission
    {
        public bool SaleQuotations { get; set; }
        public bool SaleOrders { get; set; }
        public bool SalesVsPurchase { get; set; }
        public bool FGStockVsAllocation { get; set; }
        public bool FGStockVsSOAllocation { get; set; }
        public bool JobCardCompletion7Days { get; set; }
    }
    public class FormPermission
    {
        public int FormId { get; set; }
        public string ControllerName { get; set; }
        public string Action { get; set; }
        public string FormName { get; set; }
        public int ModuleId { get; set; }
    }
}
