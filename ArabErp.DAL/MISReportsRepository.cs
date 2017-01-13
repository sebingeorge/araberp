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
                    #region old query 12.01.2017 
//                    string query = @"SELECT 
//	                                    JobCardId,
//	                                Sum(CAST((ISNULL(JCTM.MinimumRate, 0)*ISNULL(JCT.ActualHours, 0))AS DECIMAL(18,2)) ) LabourCost
//                                    INTO #HourlyCost
//                                    FROM JobCardTask JCT
//                                    INNER JOIN JobCardTaskMaster JCTM ON JCT.JobCardTaskMasterId = JCTM.JobCardTaskMasterId
//                                    INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
//                                    GROUP BY JobCardId
//
//                                    SELECT
//	                                JC.JobCardId,JC.JobCardNo,SO.SaleOrderId,JC.SaleOrderItemId,
//	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
//	                                CUS.CustomerName,
//	                                VIP.RegistrationNo,
//	                                VIP.ChassisNo,
//	                                DC.DeliveryChallanId,DC.DeliveryChallanRefNo,
//	                                CONVERT(VARCHAR, DC.DeliveryChallanDate, 106) DeliveryChallanDate,
//	                                SI.SalesInvoiceRefNo InvoiceNo,
//	                                CONVERT(VARCHAR, SI.SalesInvoiceDate, 106) InvoiceDate,
//	                                SI.TotalAmount Amount,
//	                                BOX.ItemName BoxName,
//	                                FREEZER.ItemName FreezerName,
//
//	                                STUFF((SELECT ', ' + IB.SerialNo
//                                    FROM  ItemBatch IB
//                                    WHERE JC.SaleOrderItemId = IB.SaleOrderItemId
//                                    FOR XML PATH('')), 1, 1, '') AS UnitSerialNo,
//
//	                                CASE WHEN ISNULL(JC.isService, 0) = 1 THEN 'Service' ELSE 'New Installation' END InstallationType,
//	                                HC.LabourCost,0 MaterialCost,JC.isService, JC.OrganizationId INTO #Result
//                                    FROM JobCard JC
//                                    INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
//                                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
//                                    INNER JOIN VehicleInPass VIP ON JC.InPassId = VIP.VehicleInPassId
//                                    LEFT JOIN DeliveryChallan DC ON JC.JobCardId = DC.JobCardId
//                                    LEFT JOIN SalesInvoice SI ON JC.SaleOrderId = SI.SaleOrderId
//                                    LEFT JOIN Item BOX ON JC.BoxId = BOX.ItemId
//                                    LEFT JOIN Item FREEZER ON JC.FreezerUnitId = FREEZER.ItemId
//                                    LEFT JOIN #HourlyCost HC ON JC.JobCardId = HC.JobCardId
//
//                                    SELECT SO.SaleOrderId,WRI.ItemId,SUM(SII.IssuedQuantity)Quantity,0 Rate INTO #TEMP
//                                    FROM WorkShopRequest WR
//                                    INNER JOIN SaleOrder SO ON WR.SaleOrderId = SO.SaleOrderId
//                                    INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
//                                    INNER JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
//                                    GROUP BY SO.SaleOrderId, WRI.ItemId;
//
//                                    with A as(
//                                    SELECT I.ItemId,I.ItemName,
//                                    ISNULL(GI.Rate, ISNULL(SR.Rate, 0)) Rate
//                                    FROM GRNItem GI
//                                    INNER JOIN (SELECT MAX(GRNItemId)GRNItemId FROM GRNItem GROUP BY ItemId) T1 ON GI.GRNItemId = T1.GRNItemId
//                                    INNER JOIN GRN G ON GI.GRNId = G.GRNId
//                                    RIGHT OUTER JOIN Item I ON GI.ItemId = I.ItemId
//                                    LEFT JOIN StandardRate SR ON GI.ItemId = SR.ItemId
//                                    )
//                                    update T set T.Rate = A.Rate from A inner join #TEMP T on T.ItemId = A.ItemId;
//
//                                    update R set R.MaterialCost = (T.Quantity*T.Rate) from #TEMP T inner join #Result R on R.SaleOrderId = T.SaleOrderId 
//
//                                    SELECT R.JobCardId,R.JobCardNo,R.SaleOrderId,R.SaleOrderItemId,R.JobCardDate,
//                                    R.CustomerName,R.RegistrationNo,R.ChassisNo,R.DeliveryChallanId,
//                                    R.DeliveryChallanRefNo,R.DeliveryChallanDate,R.InvoiceNo,R.InvoiceDate,
//                                    R.Amount,R.BoxName,R.FreezerName,R.UnitSerialNo,R.InstallationType,R.LabourCost,
//                                    R.MaterialCost,R.isService,R.OrganizationId,
//
//                                    STUFF((SELECT DISTINCT ', ' + I.ItemName
//                                    FROM  WorkShopRequest W 
//                                    INNER JOIN WorkShopRequestItem WI ON WI.WorkShopRequestId=W.WorkShopRequestId
//                                    INNER JOIN (SELECT WorkShopRequestItemId,sum(IssuedQuantity)Qty FROM StoreIssueItem 
//                                    GROUP BY WorkShopRequestItemId) SI 
//                                    ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
//                                    INNER JOIN SaleOrderMaterial SM ON SM.SaleOrderId=R.SaleOrderId AND SM.itemid =WI.ItemId
//                                    INNER JOIN item I ON I.ItemId=SM.ItemId
//                                    WHERE R.JobCardId=W.JobCardId OR R.SaleOrderItemId=W.SaleOrderItemId
//                                    FOR XML PATH('')), 1, 1, '') AS Accessories
//                                    FROM #Result R
//                                   
//                                    WHERE OrganizationId = @org
//									AND MONTH(JobCardDate) = ISNULL(@month, MONTH(GETDATE())) 
//									AND YEAR(JobCardDate) = ISNULL(@year, YEAR(GETDATE()))
//                                    AND Concat(RegistrationNo,'/',ChassisNo) LIKE '%'+@ChassisNo+'%'
//                                    AND isnull(UnitSerialNo,0)  LIKE '%'+@UnitSlNo+'%'
//                                    AND isnull(CustomerName,'')  LIKE '%'+@Customer+'%'
//                                    AND isnull(JobCardNo,'')  LIKE '%'+@JobcardNo+'%'
//--                                  AND isnull(isService,'')  LIKE '%'+@Installation+'%'
//                                    AND ISNULL(isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(isService, 0) END
//                                    ORDER BY DeliveryChallanId desc
//                                    DROP TABLE #HourlyCost";

                    #endregion

                    string query = @"SELECT 
	                                JobCardId,
	                                Sum(CAST((ISNULL(JCTM.MinimumRate, 0)*ISNULL(JCT.ActualHours, 0))AS DECIMAL(18,2)) ) LabourCost
                                    INTO #HourlyCost
                                    FROM JobCardTask JCT
                                    INNER JOIN JobCardTaskMaster JCTM ON JCT.JobCardTaskMasterId = JCTM.JobCardTaskMasterId
                                    INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                                    GROUP BY JobCardId

                                    SELECT JC.JobCardId,JC.JobCardNo,SO.SaleOrderId,JC.SaleOrderItemId,
                                    CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
                                    CUS.CustomerName,VIP.RegistrationNo,VIP.ChassisNo,
                                    DC.DeliveryChallanId,DC.DeliveryChallanRefNo,
                                    CONVERT(VARCHAR, DC.DeliveryChallanDate, 106) DeliveryChallanDate,
                                    SI.SalesInvoiceRefNo InvoiceNo,CONVERT(VARCHAR, SI.SalesInvoiceDate, 106) InvoiceDate,
                                    SI.TotalAmount Amount,BOX.ItemName BoxName,FREEZER.ItemName FreezerName,
                                    STUFF((SELECT ', ' + IB.SerialNo
                                    FROM  ItemBatch IB
                                    WHERE JC.SaleOrderItemId = IB.SaleOrderItemId
                                    FOR XML PATH('')), 1, 1, '') AS UnitSerialNo,

                                    CASE WHEN ISNULL(JC.isService, 0) = 1 THEN 'Service' ELSE 'New Installation' END InstallationType,
                                    HC.LabourCost,0 MaterialCost,JC.isService, JC.OrganizationId INTO #Result
                                    FROM DeliveryChallan DC
                                    INNER JOIN JobCard JC ON JC.JobCardId = DC.JobCardId 
                                    INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId
                                    INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                                    INNER JOIN VehicleInPass VIP ON JC.InPassId = VIP.VehicleInPassId
                                    LEFT JOIN SalesInvoiceItem SII ON SII.JobCardId=JC.JobCardId
                                    LEFT JOIN SalesInvoice SI ON SII.SalesInvoiceId = SI.SalesInvoiceId
                                    LEFT JOIN Item BOX ON JC.BoxId = BOX.ItemId
                                    LEFT JOIN Item FREEZER ON JC.FreezerUnitId = FREEZER.ItemId
                                    LEFT JOIN #HourlyCost HC ON JC.JobCardId = HC.JobCardId
                                    SELECT SO.SaleOrderId,WRI.ItemId,SUM(SII.IssuedQuantity)Quantity,0 Rate INTO #TEMP
                                    FROM WorkShopRequest WR
                                    INNER JOIN SaleOrder SO ON WR.SaleOrderId = SO.SaleOrderId
                                    INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                                    INNER JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                                    GROUP BY SO.SaleOrderId, WRI.ItemId;

                                    with A as(
                                    SELECT I.ItemId,I.ItemName,
                                    ISNULL(GI.Rate, ISNULL(SR.Rate, 0)) Rate
                                    FROM GRNItem GI
                                    INNER JOIN (SELECT MAX(GRNItemId)GRNItemId FROM GRNItem GROUP BY ItemId) T1 ON GI.GRNItemId = T1.GRNItemId
                                    INNER JOIN GRN G ON GI.GRNId = G.GRNId
                                    RIGHT OUTER JOIN Item I ON GI.ItemId = I.ItemId
                                    LEFT JOIN StandardRate SR ON GI.ItemId = SR.ItemId
                                    )
                                    update T set T.Rate = A.Rate from A inner join #TEMP T on T.ItemId = A.ItemId;

                                    update R set R.MaterialCost = (T.Quantity*T.Rate) from #TEMP T inner join #Result R on R.SaleOrderId = T.SaleOrderId 

                                    SELECT R.JobCardId,R.JobCardNo,R.SaleOrderId,R.SaleOrderItemId,R.JobCardDate,
                                    R.CustomerName,R.RegistrationNo,R.ChassisNo,R.DeliveryChallanId,
                                    R.DeliveryChallanRefNo,R.DeliveryChallanDate,R.InvoiceNo,R.InvoiceDate,
                                    R.Amount,R.BoxName,R.FreezerName,R.UnitSerialNo,R.InstallationType,R.LabourCost,
                                    R.MaterialCost,R.isService,R.OrganizationId,

                                    STUFF((SELECT DISTINCT ', ' + I.ItemName
                                    FROM  WorkShopRequest W 
                                    INNER JOIN WorkShopRequestItem WI ON WI.WorkShopRequestId=W.WorkShopRequestId
                                    INNER JOIN (SELECT WorkShopRequestItemId,sum(IssuedQuantity)Qty FROM StoreIssueItem 
                                    GROUP BY WorkShopRequestItemId) SI ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
                                    INNER JOIN SaleOrderMaterial SM ON SM.SaleOrderId=R.SaleOrderId AND SM.itemid =WI.ItemId
                                    INNER JOIN item I ON I.ItemId=SM.ItemId
                                    WHERE R.JobCardId=W.JobCardId OR R.SaleOrderItemId=W.SaleOrderItemId
                                    FOR XML PATH('')), 1, 1, '') AS Accessories
                                    FROM #Result R
                                   
                                    WHERE OrganizationId = @org
									AND MONTH(JobCardDate) = ISNULL(@month, MONTH(GETDATE())) 
									AND YEAR(JobCardDate) = ISNULL(@year, YEAR(GETDATE()))
                                    AND Concat(RegistrationNo,'/',ChassisNo) LIKE '%'+@ChassisNo+'%'
                                    AND isnull(UnitSerialNo,0)  LIKE '%'+@UnitSlNo+'%'
                                    AND isnull(CustomerName,'')  LIKE '%'+@Customer+'%'
                                    AND isnull(JobCardNo,'')  LIKE '%'+@JobcardNo+'%'
--                                  AND isnull(isService,'')  LIKE '%'+@Installation+'%'
                                    AND ISNULL(isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(isService, 0) END
                                    ORDER BY DeliveryChallanId desc
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
