﻿using System;
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
                    string sql = @"insert into SupplyOrder(SupplyOrderNo,SupplyOrderDate,SupplierId,QuotaionNoAndDate,SpecialRemarks,
                                   PaymentTerms,DeliveryTerms,RequiredDate,CreatedBy,CreatedDate,OrganizationId, CurrencyId) 
                                   Values (@SupplyOrderNo,@SupplyOrderDate,@SupplierId,@QuotaionNoAndDate,@SpecialRemarks,@PaymentTerms,
                                   @DeliveryTerms,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId, @CurrencyId);
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
                    InsertLoginHistory(dataConnection, objSupplyOrder.CreatedBy, "Create", "LPO", id.ToString(), "0");
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
	                                    i.ItemName,i.ItemId,
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
        //public List<SupplyOrderItem> GetSupplierItemRateSetting(int Id)
        //{
        //    using (IDbConnection connection = OpenConnection(dataConnection))
        //    {
        //        string sql = @"select * from SaleOrderItem where SaleOrderId=@SaleOrderId";
        //        return connection.Query<SaleOrderItem>(sql, new { SaleOrderId = SaleOrderId }).ToList();
        //    }
        //}

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
                InsertLoginHistory(dataConnection, objSupplyOrder.CreatedBy, "Update", "LPO", id.ToString(), "0");
                return id;
            }
        }
        public int DeleteSupplyOrder(Unit objSupplyOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplyOrder  OUTPUT DELETED.SupplyOrderId WHERE SupplyOrderId=@SupplyOrderId";


                var id = connection.Execute(sql, objSupplyOrder);
                InsertLoginHistory(dataConnection, objSupplyOrder.CreatedBy, "Delete", "LPO", id.ToString(), "0");
                return id;
            }
        }
        /// <summary>
        /// Return all approved supply orders
        /// </summary>
        /// <returns></returns>
        public IList<SupplyOrderPreviousList> GetPreviousList(int OrganizationId,int id, int supid, DateTime? from, DateTime? to)
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
                                SELECT SO.SupplyOrderId,
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
                                WHERE ISNULL(SO.isActive, 1) = 1 and SO.OrganizationId=@OrganizationId
                                and SO.SupplyOrderId = ISNULL(NULLIF(@id, 0), SO.SupplyOrderId)
                                and SUP.SupplierId = ISNULL(NULLIF(@supid, 0), SUP.SupplierId)
                                and SO.SupplyOrderDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
								ORDER BY SupplyOrderDate DESC, SO.CreatedDate DESC;
                                DROP TABLE #SUPPLY_ITEM;";

                return connection.Query<SupplyOrderPreviousList>(query, new { OrganizationId = OrganizationId,id = id, supid = supid, to = to, from = from }).ToList<SupplyOrderPreviousList>();
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

        public int Approve(int supplyOrderId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"UPDATE SupplyOrder SET isApproved = 1 WHERE SupplyOrderId = @supplyOrderId;";

                    connection.Execute(query, new { supplyOrderId = supplyOrderId });

                    return 1;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public SupplyOrderItem GetSupplierItemRate(int Id, string ItemId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {

                    string query = String.Format("select ItemId,FixedRate  from SupplierItemRate where SupplierId = {0} and ItemId in ({1});", Id, ItemId);
                    return connection.Query<SupplyOrderItem>(query).First<SupplyOrderItem>();
                      
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int CHECK(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT Count(S.SupplyOrderId)Count FROM SupplyOrder S
                                INNER JOIN SupplyOrderItem SI ON S.SupplyOrderId=SI.SupplyOrderId
                                INNER JOIN GRNItem GI ON GI.SupplyOrderItemId=SI.SupplyOrderItemId 
                                WHERE S.SupplyOrderId=@SupplyOrderId";

                var id = connection.Query<int>(sql, new { SupplyOrderId = SupplyOrderId }).FirstOrDefault();

                return id;

            }

        }

        /// <summary>
        /// Delete SO HD Details
        /// </summary>
        /// <returns></returns>
        public int DeleteSOHD(int Id)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM SupplyOrder WHERE SupplyOrderId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }
        /// <summary>
        /// Delete SO DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteSODT(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM SupplyOrderItem WHERE SupplyOrderId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

        public string GetPaymentTerm(int supplierid)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                Supplier Supplier = connection.Query<Supplier>("select * from Supplier where SupplierId = " + supplierid).FirstOrDefault();

                string PaymentTerms = "";
                if (Supplier != null)
                {
                    PaymentTerms = Supplier.PaymentTerms;
                }
                return PaymentTerms;
            }
        }
    }
}