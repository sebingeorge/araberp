using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplyOrderRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public string InsertSupplyOrder(SupplyOrder objSupplyOrder)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    objSupplyOrder.SupplyOrderNo = "LPO/0/" + DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SupplyOrder).Name, "0", 1);
                    string sql = @"insert  into SupplyOrder(SupplyOrderNo,SupplyOrderDate,SupplierId,QuotaionNoAndDate,SpecialRemarks,PaymentTerms,DeliveryTerms,RequiredDate,CreatedBy,CreatedDate,OrganizationId, CurrencyId) Values (@SupplyOrderNo,@SupplyOrderDate,@SupplierId,@QuotaionNoAndDate,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId, @CurrencyId);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, objSupplyOrder, trn).Single<int>();

                    var supplyorderitemrepo = new SupplyOrderItemRepository();
                    foreach (var item in objSupplyOrder.SupplyOrderItems)
                    {
                        if (item.OrderedQty > 0)
                        {
                            item.SupplyOrderId = id;
                            item.OrganizationId = objSupplyOrder.OrganizationId;
                            supplyorderitemrepo.InsertSupplyOrderItem(item, connection, trn); 
                        }
                    }

                    trn.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    trn.Rollback();
                    throw;
                }
                return objSupplyOrder.SupplyOrderNo;
            }
        }

        public List<SupplyOrderItem> GetPurchaseRequestItems(List<int> selectedpurchaserequests)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//                string sql = @"SELECT
//	                            CONCAT(PurchaseRequestNo,' - ',
//	                            CONVERT (VARCHAR(15),PurchaseRequestDate,106)) PRNODATE,
//	                            PurchaseRequestItemId,
//	                            i.ItemName,
//	                            i.PartNo,
//	                            CAST(PI.Quantity AS INT) as BalQty,
//	                            CAST(PI.Quantity AS INT) AS OrderedQty,
//                                0.00 AS Rate,
//                                0.00 AS Discount,
//                                0.00 AS Amount
//                            FROM PurchaseRequest P 
//                            INNER JOIN PurchaseRequestItem PI ON P.PurchaseRequestId=PI.PurchaseRequestId
//                            INNER JOIN Item i ON PI.ItemId=i.ItemId
//                            WHERE P.PurchaseRequestId in @selectedpurchaserequests";
                string sql = @"select distinct SI.PurchaseRequestItemId, SUM(SI.OrderedQty) SuppliedQuantity 
                                    INTO #SUPPLY
                                    from [dbo].[SupplyOrderItem] SI
                                    WHERE ISNULL(isActive, 1) = 1
                                    GROUP BY SI.PurchaseRequestItemId;
                                    SELECT
	                                    CONCAT(PurchaseRequestNo,' - ',
	                                    CONVERT (VARCHAR(15),PurchaseRequestDate,106)) PRNODATE,
	                                    PI.PurchaseRequestItemId,
	                                    i.ItemName,
	                                    i.PartNo,
	                                    CAST(ISNULL(PI.Quantity, 0) - ISNULL(SUP.SuppliedQuantity, 0) AS INT) as BalQty,
	                                    CAST(ISNULL(PI.Quantity, 0) - ISNULL(SUP.SuppliedQuantity, 0) AS INT) AS OrderedQty,
                                        0.00 AS Rate,
                                        0.00 AS Discount,
                                        0.00 AS Amount
                                    FROM PurchaseRequest P 
                                    INNER JOIN PurchaseRequestItem PI ON P.PurchaseRequestId=PI.PurchaseRequestId
                                    INNER JOIN Item i ON PI.ItemId=i.ItemId
                                    LEFT JOIN #SUPPLY SUP ON PI.PurchaseRequestItemId = SUP.PurchaseRequestItemId
                                    WHERE P.PurchaseRequestId in @selectedpurchaserequests
                                    AND (SUP.PurchaseRequestItemId IS NULL OR ISNULL(SUP.SuppliedQuantity, 0) < ISNULL(PI.Quantity, 0));
                                    DROP TABLE #SUPPLY;";

                var objPendingPurchaseRequests = connection.Query<SupplyOrderItem>(sql, new { selectedpurchaserequests = selectedpurchaserequests }).ToList<SupplyOrderItem>();

                return objPendingPurchaseRequests;
            }
        }

        public IEnumerable<PendingPurchaseRequest> GetPendingPurchaseRequest()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select distinct SI.PurchaseRequestItemId, SUM(SI.OrderedQty) SuppliedQuantity 
                                INTO #SUPPLY
                                from [dbo].[SupplyOrderItem] SI
                                -- join [dbo].[PurchaseRequestItem] 
                                --PRI on SI.PurchaseRequestItemId=PRI.PurchaseRequestItemId join [dbo].[PurchaseRequest] PR on PRI.PurchaseRequestId=PR.PurchaseRequestId
                                WHERE ISNULL(isActive, 1) = 1
                                GROUP BY SI.PurchaseRequestItemId;
                                select 
	                                DISTINCT
	                                P.PurchaseRequestId,
	                                PurchaseRequestNo,
	                                PurchaseRequestDate,
	                                P.RequiredDate,
	                                ISNULL(P.SpecialRemarks, '-') SpecialRemarks,
	                                ISNULL(WRK.WorkShopRequestRefNo, '')+' - '+CONVERT(VARCHAR, WRK.WorkShopRequestDate, 106) WRNoAndDate,
	                                DATEDIFF(dd,P.PurchaseRequestDate,GETDATE ()) Ageing,
	                                P.PurchaseRequestDate, P.CreatedDate
                                from PurchaseRequest P
                                INNER JOIN PurchaseRequestItem PRI ON P.PurchaseRequestId = PRI.PurchaseRequestId
                                INNER JOIN WorkShopRequest WRK ON P.WorkShopRequestId = WRK.WorkShopRequestId
                                LEFT JOIN #SUPPLY SUP ON PRI.PurchaseRequestItemId = SUP.PurchaseRequestItemId
                                WHERE P.isActive=1 and 
                                (SUP.PurchaseRequestItemId IS NULL OR ISNULL(SUP.SuppliedQuantity, 0) < ISNULL(PRI.Quantity, 0))
                                ORDER BY P.PurchaseRequestDate DESC, P.CreatedDate DESC;
                                DROP TABLE #SUPPLY;";

                var objPendingPurchaseRequests = connection.Query<PendingPurchaseRequest>(sql).ToList<PendingPurchaseRequest>();

                return objPendingPurchaseRequests;
            }
        }

        public SupplyOrder GetSupplyOrder(int SupplyOrderId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT 
                                    SupplyOrderId,
	                                SupplyOrderNo,
	                                CONVERT(DATETIME, SupplyOrderDate, 106) SupplyOrderDate,
	                                SupplierId,
	                                QuotaionNoAndDate,
	                                SpecialRemarks,
	                                PaymentTerms,
	                                DeliveryTerms,
	                                RequiredDate,
	                                CurrencyId
                                FROM SupplyOrder
                                WHERE SupplyOrderId = @SupplyOrderId
	                                AND ISNULL(isActive, 1) = 1";

                    var objSupplyOrder = connection.Query<SupplyOrder>(query, new
                    {
                        SupplyOrderId = SupplyOrderId
                    }).First<SupplyOrder>();

                    objSupplyOrder.SupplyOrderItems = GetPurchaseRequestItems(new List<int>(SupplyOrderId));

                    return objSupplyOrder;
                }
            }
            catch (InvalidOperationException iox)
            {
                throw iox;
            }
            catch (SqlException sx)
            {
                throw sx;
            }
            catch (NullReferenceException nx)
            {
                throw nx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SupplyOrder> GetSupplyOrders()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrder
                        where isActive=1";

                var objSupplyOrders = connection.Query<SupplyOrder>(sql).ToList<SupplyOrder>();

                return objSupplyOrders;
            }
        }

        public int UpdateSupplyOrder(SupplyOrder objSupplyOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SupplyOrder SET SupplyOrderDate = @SupplyOrderDate ,SupplierId = @SupplierId ,QuotaionNoAndDate = @QuotaionNoAndDate ,SpecialRemarks = @SpecialRemarks,PaymentTerms = @PaymentTerms,DeliveryTerms = @DeliveryTerms,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.SupplyOrderId  WHERE SupplyOrderId = @SupplyOrderId";


                var id = connection.Execute(sql, objSupplyOrder);
                return id;
            }
        }
        public int DeleteSupplyOrder(Unit objSupplyOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplyOrder  OUTPUT DELETED.SupplyOrderId WHERE SupplyOrderId=@SupplyOrderId";


                var id = connection.Execute(sql, objSupplyOrder);
                return id;
            }
        }
        /// <summary>
        /// Return all approved supply orders
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SupplyOrderPreviousList> GetPreviousList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SupplyOrderId,
	                                SUM(PRI.Quantity) RequestedQuantity,
	                                SUM(SOI.OrderedQty) SuppliedQuantity,
	                                SUM(PRI.Quantity - SOI.OrderedQty) BalanceQuantity,
	                                SUM(SOI.Amount) Amount
                                INTO #SUPPLY_ITEM
                                FROM SupplyOrderItem SOI
                                INNER JOIN PurchaseRequestItem PRI ON SOI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                                WHERE ISNULL(SOI.isActive, 1) = 1 AND ISNULL(PRI.isActive, 1) = 1
                                GROUP BY SupplyOrderId;
                                SELECT 
	                                SO.SupplyOrderNo,
	                                CONVERT(DATETIME, SO.SupplyOrderDate, 106) SupplyOrderDate,
	                                SUP.SupplierName,
	                                ISNULL(SO.QuotaionNoAndDate, '-') QuotationNoAndDate,
	                                SI.RequestedQuantity,
	                                SI.SuppliedQuantity,
	                                SI.BalanceQuantity,
	                                SI.Amount,
									SO.CreatedDate
                                FROM SupplyOrder SO
                                INNER JOIN Supplier SUP ON SO.SupplierId = SUP.SupplierId
                                INNER JOIN #SUPPLY_ITEM SI ON SO.SupplyOrderId = SI.SupplyOrderId
                                WHERE ISNULL(SO.isActive, 1) = 1
                                    AND ISNULL(isApproved, 0) = 1
								ORDER BY SupplyOrderDate DESC, SO.CreatedDate DESC;
                                DROP TABLE #SUPPLY_ITEM;";

                return connection.Query<SupplyOrderPreviousList>(query).ToList<SupplyOrderPreviousList>();
            }
        }
        /// <summary>
        /// Return all unapproved supply orders
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SupplyOrderPreviousList> GetPendingApproval()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SupplyOrderId,
	                                SUM(PRI.Quantity) RequestedQuantity,
	                                SUM(SOI.OrderedQty) SuppliedQuantity,
	                                SUM(PRI.Quantity - SOI.OrderedQty) BalanceQuantity,
	                                SUM(SOI.Amount) Amount
                                INTO #SUPPLY_ITEM
                                FROM SupplyOrderItem SOI
                                INNER JOIN PurchaseRequestItem PRI ON SOI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                                WHERE ISNULL(SOI.isActive, 1) = 1 AND ISNULL(PRI.isActive, 1) = 1
                                GROUP BY SupplyOrderId;
                                SELECT 
									SO.SupplyOrderId,
	                                SO.SupplyOrderNo,
	                                CONVERT(DATETIME, SO.SupplyOrderDate, 106) SupplyOrderDate,
	                                SUP.SupplierName,
	                                ISNULL(SO.QuotaionNoAndDate, '-') QuotationNoAndDate,
	                                SI.RequestedQuantity,
	                                SI.SuppliedQuantity,
	                                SI.BalanceQuantity,
	                                SI.Amount,
									SO.CreatedDate
                                FROM SupplyOrder SO
                                INNER JOIN Supplier SUP ON SO.SupplierId = SUP.SupplierId
                                INNER JOIN #SUPPLY_ITEM SI ON SO.SupplyOrderId = SI.SupplyOrderId
                                WHERE ISNULL(SO.isActive, 1) = 1
                                    AND ISNULL(isApproved, 0) = 0
								ORDER BY SupplyOrderDate DESC, SO.CreatedDate DESC;
                                DROP TABLE #SUPPLY_ITEM;";

                return connection.Query<SupplyOrderPreviousList>(query).ToList<SupplyOrderPreviousList>();
            }
        }
    }
}