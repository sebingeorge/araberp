using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class MISReportsRepository : BaseRepository
    {
        string dataConnection = GetConnectionString("arab");
        public IEnumerable GetDCReport(int OrganizationId, int? month, int? year, string ChassisNo = "", string UnitSlNo = "", string Customer = "", string JobcardNo = "", string InstallType = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT 
	                                    JobCardId,
	                                Sum(CAST((ISNULL(JCTM.MinimumRate, 0)*ISNULL(JCT.ActualHours, 0))AS DECIMAL(18,2)) ) LabourCost
                                    INTO #HourlyCost
                                    FROM JobCardTask JCT
                                    INNER JOIN JobCardTaskMaster JCTM ON JCT.JobCardTaskMasterId = JCTM.JobCardTaskMasterId
                                    INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                                    GROUP BY JobCardId

                                    SELECT
	                                    JC.JobCardNo,
	                                    CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                    CUS.CustomerName,
	                                    VIP.RegistrationNo,
	                                    VIP.ChassisNo,
	                                    DC.DeliveryChallanRefNo,
	                                    CONVERT(VARCHAR, DC.DeliveryChallanDate, 106) DeliveryChallanDate,
	                                    SI.SalesInvoiceRefNo InvoiceNo,
	                                    CONVERT(VARCHAR, SI.SalesInvoiceDate, 106) InvoiceDate,
	                                    SI.TotalAmount Amount,
	                                    BOX.ItemName BoxName,
	                                    FREEZER.ItemName FreezerName,
	                                    IB.SerialNo UnitSerialNo,
	                                    CASE WHEN ISNULL(JC.isService, 0) = 1 THEN 'Service' ELSE 'New Installation' END InstallationType,
	                                    HC.LabourCost
                                    FROM JobCard JC
                                    INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
                                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                                    INNER JOIN VehicleInPass VIP ON JC.InPassId = VIP.VehicleInPassId
                                    LEFT JOIN DeliveryChallan DC ON JC.JobCardId = DC.JobCardId
                                    LEFT JOIN SalesInvoice SI ON JC.SaleOrderId = SI.SaleOrderId
                                    LEFT JOIN Item BOX ON JC.BoxId = BOX.ItemId
                                    LEFT JOIN Item FREEZER ON JC.FreezerUnitId = FREEZER.ItemId
                                    LEFT JOIN ItemBatch IB ON JC.SaleOrderItemId = IB.SaleOrderItemId
                                    LEFT JOIN #HourlyCost HC ON JC.JobCardId = HC.JobCardId
                                    WHERE JC.OrganizationId = @org
									AND MONTH(JC.JobCardDate) = ISNULL(@month, MONTH(GETDATE())) 
									AND YEAR(JC.JobCardDate) = ISNULL(@year, YEAR(GETDATE()))
                                    AND Concat(VIP.RegistrationNo,'/',VIP.ChassisNo) LIKE '%'+@ChassisNo+'%'
                                    AND isnull(IB.SerialNo,0)  LIKE '%'+@UnitSlNo+'%'
                                    AND isnull(CUS.CustomerName,'')  LIKE '%'+@Customer+'%'
                                    AND isnull( JC.JobCardNo,'')  LIKE '%'+@JobcardNo+'%'
--                                  AND isnull( JC.isService,'')  LIKE '%'+@Installation+'%'
                                    AND  ISNULL(JC.isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(JC.isService, 0) END
                                    DROP TABLE #HourlyCost";
                    return connection.Query<DCReport>(query, new { org = OrganizationId, month = month, year = year, ChassisNo = ChassisNo, UnitSlNo = UnitSlNo, Customer = Customer, JobcardNo = JobcardNo, InstallType = InstallType });
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public IEnumerable GetPendingLPO(int item, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT
	                                    PR.PurchaseRequestNo,
	                                    CONVERT(VARCHAR, PR.PurchaseRequestDate, 106) PurchaseRequestDate,
	                                    PRI.Quantity PRQuantity,
	                                    PRI.Quantity - ISNULL(SOI.Quantity, 0) PendingQuantity
                                    FROM PurchaseRequestItem PRI
                                    LEFT JOIN (SELECT
				                                    PurchaseRequestItemId,
				                                    SUM(OrderedQty) Quantity
			                                    FROM SupplyOrderItem
			                                    GROUP BY PurchaseRequestItemId) SOI ON PRI.PurchaseRequestItemId = SOI.PurchaseRequestItemId
                                    INNER JOIN Item I ON PRI.ItemId = I.ItemId
                                    INNER JOIN PurchaseRequest PR ON PRI.PurchaseRequestId = PR.PurchaseRequestId
                                    WHERE I.ItemId = @item AND PR.OrganizationId = @org AND ISNULL(SOI.Quantity, 0) < PRI.Quantity AND SOI.PurchaseRequestItemId IS NULL";
                    return connection.Query<PendingPurchaseRequest>(query, new { org = OrganizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public object GetLastPurchaseBill(int item, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT DISTINCT TOP 3
	                                    PB.PurchaseBillRefNo,
	                                    CONVERT(VARCHAR, PB.PurchaseBillDate, 106) PurchaseBillDate,
	                                    PB.PurchaseBillAmount
                                    FROM PurchaseBill PB
	                                    INNER JOIN PurchaseBillItem PBI ON PB.PurchaseBillId = PBI.PurchaseBillId
	                                    INNER JOIN GRNItem GI ON PBI.GRNItemId = GI.GRNItemId
                                    WHERE GI.ItemId = @item AND PB.OrganizationId = @org";
                    return connection.Query<PurchaseBill>(query, new { org = organizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public IEnumerable GetPendingGRN(int item, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT
	                                    SO.SupplyOrderNo + ' - ' + CONVERT(VARCHAR, SO.SupplyOrderDate, 106) SoNoWithDate,
	                                    S.SupplierName,
	                                    SOI.OrderedQty Quantity,
	                                    SOI.OrderedQty - ISNULL(GI.Quantity, 0) PendingQuantity
                                    FROM SupplyOrderItem SOI
                                    LEFT JOIN (SELECT
				                                    SupplyOrderItemId,
				                                    ItemId,
				                                    SUM(Quantity) Quantity
			                                    FROM GRNItem
			                                    WHERE ItemId = @item
			                                    GROUP BY SupplyOrderItemId, ItemId) GI ON SOI.SupplyOrderItemId = GI.SupplyOrderItemId
                                    INNER JOIN Item I ON GI.ItemId = I.ItemId
                                    INNER JOIN SupplyOrder SO ON SOI.SupplyOrderId = SO.SupplyOrderId
                                    INNER JOIN Supplier S ON SO.SupplierId = S.SupplierId
                                    WHERE ISNULL(GI.Quantity, 0) < SOI.OrderedQty AND SO.OrganizationId = @org AND GI.SupplyOrderItemId IS NULL";
                    return connection.Query<PendingForGRN>(query, new { org = OrganizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public object GetPendingIssue(int item, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT
	                                    WR.WorkShopRequestRefNo,
	                                    CONVERT(VARCHAR, WR.WorkShopRequestDate, 106) WorkShopRequestDate,
	                                    WRI.Quantity,
	                                    WRI.Quantity - ISNULL(SII.Quantity, 0) PendingQuantity
                                    FROM WorkShopRequestItem WRI
                                    LEFT JOIN (SELECT
				                                    SII.WorkShopRequestItemId,
				                                    SUM(SII.IssuedQuantity) Quantity
			                                    FROM StoreIssueItem SII
				                                    INNER JOIN WorkShopRequestItem WRI ON SII.WorkShopRequestItemId = WRI.WorkShopRequestItemId
			                                    WHERE WRI.ItemId = @item
			                                    GROUP BY SII.WorkShopRequestItemId) SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                                    INNER JOIN WorkShopRequest WR ON WRI.WorkShopRequestId = WR.WorkShopRequestId
                                    INNER JOIN Item I ON WRI.ItemId = I.ItemId
                                    WHERE WRI.ItemId = @item AND SII.WorkShopRequestItemId IS NULL AND WR.OrganizationId = @org
                                    AND ISNULL(SII.Quantity, 0) < WRI.Quantity";
                    return connection.Query<PendingWorkShopRequest>(query, new { org = OrganizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public object GetLastPurchaseRequest(int item, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT TOP 3
	                                    PR.PurchaseRequestNo,
	                                    CONVERT(VARCHAR, PR.PurchaseRequestDate, 106) PurchaseRequestDate,
	                                    SUM(PRI.Quantity) PRQuantity
                                    FROM PurchaseRequest PR
	                                    INNER JOIN PurchaseRequestItem PRI ON PR.PurchaseRequestId = PRI.PurchaseRequestId
                                    WHERE PRI.ItemId = @item AND PR.OrganizationId = @org
                                    GROUP BY PR.PurchaseRequestNo, PR.PurchaseRequestDate
                                    ORDER BY PR.PurchaseRequestDate DESC";
                    return connection.Query<PendingPurchaseRequest>(query, new { org = OrganizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public object GetLastLPO(int item, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT TOP 3
	                                    SO.SupplyOrderNo + ' - ' + CONVERT(VARCHAR, SO.SupplyOrderDate, 106) SoNoWithDate,
	                                    SUM(SOI.OrderedQty) Quantity
                                    FROM SupplyOrder SO
	                                    INNER JOIN SupplyOrderItem SOI ON SO.SupplyOrderId = SOI.SupplyOrderId
	                                    LEFT JOIN PurchaseRequestItem PRI ON SOI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                                    WHERE PRI.ItemId = @item AND SO.OrganizationId = @org
                                    GROUP BY SO.SupplyOrderNo,SO.SupplyOrderDate
                                    ORDER BY SO.SupplyOrderDate DESC";
                    return connection.Query<PendingSupplyOrder>(query, new { org = OrganizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public object GetLastGRN(int item, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT TOP 3
	                                    GRN.GRNNo,
	                                    CONVERT(VARCHAR, GRN.GRNDate, 106) GRNDate,
	                                    SUM(GI.Quantity) Quantity
                                    FROM GRN
	                                    INNER JOIN GRNItem GI ON GRN.GRNId = GI.GRNId
                                    WHERE GI.ItemId = @item AND GRN.OrganizationId = @org
                                    GROUP BY GRN.GRNNo, GRN.GRNDate
                                    ORDER BY GRN.GRNDate DESC";
                    return connection.Query<PendingGRN>(query, new { org = OrganizationId, item = item }).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
