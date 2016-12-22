using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class ItemBatchRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertItemBatch(IList<ItemBatch> model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                string sql = string.Empty;

                sql = model[0].isOpeningStock == 0 ?
                    @"insert  into ItemBatch(GRNItemId,SerialNo,CreatedBy,CreatedDate,OrganizationId, isActive) Values (@GRNItemId,LTRIM(RTRIM(@SerialNo)),@CreatedBy,@CreatedDate,@OrganizationId,1);
                      SELECT CAST(SCOPE_IDENTITY() as int)" :
                    @"insert  into ItemBatch(OpeningStockId,SerialNo,CreatedBy,CreatedDate,OrganizationId, isActive) Values (@OpeningStockId,LTRIM(RTRIM(@SerialNo)),@CreatedBy,@CreatedDate,@OrganizationId,1);
                      SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    foreach (var item in model)
                    {
                        var id = connection.Query<int>(sql, item, txn).Single();
                    }
                    InsertLoginHistory(dataConnection, model[0].CreatedBy, "Create", "Item Batch", "0", model[0].OrganizationId.ToString());
                    txn.Commit();
                    return 1;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public IEnumerable<ItemBatch> GetItemBatch(int id, int OrganizationId, int type = 0)//type=0 means GRNItemId, type=1 means OpeningStockId
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
	                                IB.*,
									CASE WHEN IB.GRNItemId IS NULL THEN 1 ELSE 0 END AS isOpeningStock,
	                                --ISNULL(IB.ItemBatchId, 1) AS isOpeningStock,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                DC.DeliveryChallanRefNo,
	                                CONVERT(VARCHAR, DC.DeliveryChallanDate, 106) DeliveryChallanDate,
	                                PC.ProjectCompletionRefNo,
	                                CONVERT(VARCHAR, PC.ProjectCompletionDate, 106) ProjectCompletionDate,
									ISNULL(GI.Quantity, OS.Quantity) Quantity,
									S.StockPointName,
									I.ItemName,
									G.GRNNo,
									CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
									CASE WHEN G.isDirectPurchaseGRN = 1 THEN 'DIRECT GRN' ELSE 'GRN' END AS isDirect
                                FROM ItemBatch IB
	                                LEFT JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
	                                LEFT JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
	                                LEFT JOIN DeliveryChallan DC ON IB.DeliveryChallanId = DC.DeliveryChallanId
	                                LEFT JOIN ProjectCompletion PC ON IB.ProjectCompletionId = PC.ProjectCompletionId 
									LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
									LEFT JOIN GRN G ON GI.GRNId = G.GRNId
									LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
									LEFT JOIN Stockpoint S ON (G.WareHouseId = S.StockPointId OR OS.StockPointId = S.StockPointId)
									LEFT JOIN Item I ON (GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId)
                                WHERE " + (type == 0 ? "IB.GRNItemId" : "IB.OpeningStockId") + "=@id AND IB.OrganizationId = @OrganizationId";

                var objItemBatch = connection.Query<ItemBatch>(query, new
                {
                    id = id,
                    type = type,
                    OrganizationId = OrganizationId
                }).ToList<ItemBatch>();

                return objItemBatch;
            }
        }

        public List<ItemBatch> GetItemBatchs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemBatch
                        where isActive=1";

                var objItemBatchs = connection.Query<ItemBatch>(sql).ToList<ItemBatch>();

                return objItemBatchs;
            }
        }

        public IEnumerable<ItemBatch> DeleteItemBatch(int id, int type, int UserId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM ItemBatch OUTPUT deleted.SerialNo WHERE " + (type == 0 ? "GRNItemId" : "OpeningStockId") + " = @id";
                    var list = connection.Query<ItemBatch>(query, new { id = id }, txn).ToList();
                    txn.Commit();
                    return list;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public IEnumerable<ItemBatch> PendingGRNItems(int type = 0)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = string.Empty;
                    if (type == 0)
                    {



                        query = @"SELECT GI.GRNItemId,
                        	                            I.ItemId, 
                        	                            I.ItemName, 
                        	                            GI.Quantity, 
                        	                            ISNULL(GI.Remarks, '-') Remarks, 
                        	                            CASE G.isDirectPurchaseGRN WHEN 1 THEN 'DIRECT GRN' ELSE 'GRN' END isDirect, 
                        	                            G.GRNNo,
                        	                            CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
                        	                            S.SupplierName,
                        	                            DATEDIFF(dd,G.GRNDate,GETDATE ()) Ageing,
                        	                            ST.StockPointName 
                                                    FROM GRN G 
                                                    INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                                                    INNER JOIN Supplier S ON G.SupplierId=S.SupplierId
                                                    INNER JOIN Stockpoint ST ON G.WareHouseId = ST.StockPointId
                        							INNER JOIN Item I ON GI.ItemId = I.ItemId
                                                    LEFT JOIN ItemBatch P ON P.GRNItemId=GI.GRNItemId 
                                                    WHERE P.GRNItemId IS NULL
                        							AND I.BatchRequired = 1
                        							ORDER BY G.GRNDate DESC, G.CreatedDate DESC;";
                    }
                    else if (type == 1)
                    {
                        query = @"SELECT OS.OpeningStockId,
	                            I.ItemId, 
	                            I.ItemName, 
	                            ST.StockPointName,
								OS.Quantity
                            FROM OpeningStock OS
                            INNER JOIN Stockpoint ST ON OS.StockPointId = ST.StockPointId
							INNER JOIN Item I ON OS.ItemId = I.ItemId
                            LEFT JOIN ItemBatch P ON P.OpeningStockId = OS.OpeningStockId 
                            WHERE P.OpeningStockId IS NULL
							AND I.BatchRequired = 1
							ORDER BY OS.CreatedDate DESC";


                    }
                    return connection.Query<ItemBatch>(query).ToList();
                }
            }
            catch (InvalidOperationException)
            {
                return new List<ItemBatch>();
            }
            catch (SqlException sx)
            {
                throw sx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReserveItemBatch(IList<ItemBatch> model, string CreatedBy)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                string sql = @"Update  ItemBatch  Set SaleOrderItemId=@SaleOrderItemId OUTPUT INSERTED.ItemBatchId WHERE ItemBatchId=@ItemBatchId";

                try
                {
                    foreach (var item in model)
                    {
                        var id = connection.Query<int>(sql, item, txn).Single();
                    }
                    InsertLoginHistory(dataConnection, CreatedBy, "Create", "Reserve Item", "0", "0");
                    txn.Commit();

                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public ItemBatch GetGRNItem(int grnItemId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT GI.GRNItemId,
	                                    I.ItemName, 
	                                    GI.Quantity,
	                                    GI.Rate,
	                                    GI.Discount,
	                                    GI.Amount,
	                                    GI.Remarks, 
	                                    CASE G.isDirectPurchaseGRN WHEN 1 THEN 'DIRECT GRN' ELSE 'GRN' END isDirect, 
	                                    G.GRNNo,
	                                    CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
	                                    S.SupplierName,
	                                    DATEDIFF(dd,G.GRNDate,GETDATE ()) Ageing,
	                                    ST.StockPointName 
                                   FROM GRN G 
                                   INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                                   INNER JOIN Supplier S ON G.SupplierId=S.SupplierId
                                   INNER JOIN Stockpoint ST ON G.WareHouseId = ST.StockPointId
							       INNER JOIN Item I ON GI.ItemId = I.ItemId
							       WHERE GI.GRNItemId = @id
                                   ORDER BY G.GRNDate DESC, G.CreatedDate DESC";

                    return connection.Query<ItemBatch>(query, new { id = grnItemId }).First();
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Return all sale order items that doesnt have a reserved material
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingForSOIReservation> GetUnreservedItems(string saleOrder, string itemName)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query
                //                string query = @"SELECT
                //									GI.ItemId,
                //									BT.SaleOrderItemId,
                //									COUNT(BT.SaleOrderItemId) ReservedQuantity
                //								INTO #RESERVED
                //								FROM ItemBatch BT
                //								INNER JOIN GRNItem GI ON BT.GRNItemId = GI.GRNItemId
                //								WHERE BT.SaleOrderItemId IS NOT NULL
                //								GROUP BY GI.ItemId, BT.SaleOrderItemId;
                //
                //								SELECT
                //	                                SOI.SaleOrderItemId,
                //	                                SO.SaleOrderRefNo,
                //	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
                //	                                ISNULL(SOI.Quantity, 0) Quantity,
                //	                                WD.WorkDescriptionRefNo,
                //                                    I.ItemId,
                //									R.ReservedQuantity,
                //	                                I.ItemName,
                //									WD.WorkDescrShortName
                //                                FROM SaleOrderItem SOI
                //                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                //                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                //                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                //                                INNER JOIN Item I ON WI.ItemId = I.ItemId
                //								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
                //								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
                //                                WHERE ISNULL(SOI.isActive, 1) = 1
                //                                AND SO.isActive = 1 AND SOI.isActive = 1 AND SO.SaleOrderApproveStatus = 1
                //								--AND IB.SaleOrderItemId IS NULL
                //								AND ISNULL(ReservedQuantity, 0) < ISNULL(SOI.Quantity, 0)
                //								AND SII.SaleOrderItemId IS NULL
                //                                AND I.BatchRequired = 1
                //                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                //                                AND I.ItemName LIKE '%'+@itemName+'%'
                //								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'
                //                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC;
                //
                //								DROP TABLE #RESERVED;"; 
                #endregion

                #region old query (16.9.2016 3.18p)
                //                string query = @"SELECT
                //									ISNULL(GI.ItemId, OS.ItemId) ItemId,
                //									BT.SaleOrderItemId,
                //									COUNT(BT.SaleOrderItemId) ReservedQuantity
                //								INTO #RESERVED
                //								FROM ItemBatch BT
                //								LEFT JOIN GRNItem GI ON BT.GRNItemId = GI.GRNItemId
                //								LEFT JOIN OpeningStock OS ON BT.OpeningStockId = OS.OpeningStockId
                //								WHERE BT.SaleOrderItemId IS NOT NULL
                //								GROUP BY GI.ItemId, OS.ItemId, BT.SaleOrderItemId;
                //
                //								SELECT
                //	                                SOI.SaleOrderItemId,
                //	                                SO.SaleOrderRefNo,
                //	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
                //	                                ISNULL(SOI.Quantity, 0) Quantity,
                //	                                WD.WorkDescriptionRefNo,
                //                                    I.ItemId,
                //									R.ReservedQuantity,
                //	                                I.ItemName,
                //									WD.WorkDescrShortName
                //                                FROM SaleOrderItem SOI
                //                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                //                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                //                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                //                                INNER JOIN Item I ON WI.ItemId = I.ItemId
                //								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
                //								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
                //                                WHERE ISNULL(SOI.isActive, 1) = 1
                //                                AND SO.isActive = 1 AND SOI.isActive = 1 AND SO.SaleOrderApproveStatus = 1
                //								--AND IB.SaleOrderItemId IS NULL
                //								AND ISNULL(ReservedQuantity, 0) < ISNULL(SOI.Quantity, 0)
                //								AND SII.SaleOrderItemId IS NULL
                //                                AND I.BatchRequired = 1
                //                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                //                                AND I.ItemName LIKE '%'+@itemName+'%'
                //								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'
                //                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC;
                //
                //								DROP TABLE #RESERVED;"; 
                #endregion

                #region old query (26.10.2016 5.01p)
                //                string query = @"SELECT
                //									ISNULL(GI.ItemId, OS.ItemId) ItemId,
                //									BT.SaleOrderItemId,
                //									COUNT(BT.SaleOrderItemId) ReservedQuantity
                //								INTO #RESERVED1
                //								FROM ItemBatch BT
                //								LEFT JOIN GRNItem GI ON BT.GRNItemId = GI.GRNItemId
                //								LEFT JOIN OpeningStock OS ON BT.OpeningStockId = OS.OpeningStockId
                //								WHERE BT.SaleOrderItemId IS NOT NULL
                //								GROUP BY GI.ItemId, OS.ItemId, BT.SaleOrderItemId;
                //
                //                                SELECT 
                //                                    ItemId, 
                //                                    SaleOrderItemId, 
                //                                    SUM(ReservedQuantity) ReservedQuantity
                //								INTO #RESERVED
                //								FROM #RESERVED1 R1
                //								GROUP BY R1.ItemId, R1.SaleOrderItemId;
                //
                //								DROP TABLE #RESERVED1;
                //
                //								SELECT
                //	                                SOI.SaleOrderItemId,
                //	                                SO.SaleOrderRefNo,
                //	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
                //	                                ISNULL(WI.Quantity, 0) Quantity,
                //	                                WD.WorkDescriptionRefNo,
                //                                    I.ItemId,
                //									R.ReservedQuantity,
                //	                                I.ItemName,
                //									WD.WorkDescrShortName
                //                                FROM SaleOrderItem SOI
                //                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                //                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                //                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                //                                INNER JOIN Item I ON WI.ItemId = I.ItemId
                //								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
                //								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
                //                                WHERE ISNULL(SOI.isActive, 1) = 1
                //                                AND SO.isActive = 1 AND SOI.isActive = 1 AND SO.SaleOrderApproveStatus = 1
                //								--AND IB.SaleOrderItemId IS NULL
                //								AND ISNULL(ReservedQuantity, 0) < ISNULL(WI.Quantity, 0)
                //								AND SII.SaleOrderItemId IS NULL
                //                                AND I.BatchRequired = 1
                //                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                //                                AND I.ItemName LIKE '%'+@itemName+'%'
                //								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'
                //                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC;
                //
                //								DROP TABLE #RESERVED;"; 
                #endregion

                string query = @"SELECT
									ISNULL(GI.ItemId, OS.ItemId) ItemId,
									BT.SaleOrderItemId,
									COUNT(BT.SaleOrderItemId) ReservedQuantity
								INTO #RESERVED1
								FROM ItemBatch BT
								LEFT JOIN GRNItem GI ON BT.GRNItemId = GI.GRNItemId
								LEFT JOIN OpeningStock OS ON BT.OpeningStockId = OS.OpeningStockId
								WHERE BT.SaleOrderItemId IS NOT NULL
								GROUP BY GI.ItemId, OS.ItemId, BT.SaleOrderItemId;

                                SELECT 
                                    ItemId, 
                                    SaleOrderItemId, 
                                    SUM(ReservedQuantity) ReservedQuantity
								INTO #RESERVED
								FROM #RESERVED1 R1
								GROUP BY R1.ItemId, R1.SaleOrderItemId;

								DROP TABLE #RESERVED1;

								SELECT
	                                SOI.SaleOrderItemId,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
									CUS.CustomerName,
									STUFF((SELECT ', '+T1.WorkShopRequestRefNo FROM WorkShopRequest T1 
									    WHERE T1.JobCardId = JC.JobCardId OR (T1.SaleOrderId = SO.SaleOrderId AND T1.JobCardId IS NULL) FOR XML PATH('')), 1, 2, '')WorkShopRequestRefNo,
									STUFF((SELECT ', '+T1.StoreIssueRefNo FROM StoreIssue T1 LEFT JOIN WorkShopRequest T2 ON T1.WorkShopRequestId = T2.WorkShopRequestId
										WHERE T2.SaleOrderId = SO.SaleOrderId FOR XML PATH('')), 1, 2, '')StoreIssueRefNo,
									JC.JobCardNo,
									STUFF((SELECT ', '+T1.SerialNo FROM ItemBatch T1 LEFT JOIN GRNItem T2 ON T1.GRNItemId = T2.GRNItemId
										LEFT JOIN OpeningStock T3 ON T1.OpeningStockId = T3.OpeningStockId
										WHERE T1.SaleOrderItemId IS NULL AND (T2.ItemId = I.ItemId OR T3.ItemId = I.ItemId) FOR XML PATH('')), 1, 2, '')SerialNo,
	                                ISNULL(WI.Quantity, 0) Quantity,
	                                --WD.WorkDescriptionRefNo,
                                    I.ItemId,
									R.ReservedQuantity,
	                                I.ItemName,
									WD.WorkDescrShortName,
									SO.SaleOrderDate
									
                                FROM SaleOrderItem SOI
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                                INNER JOIN Item I ON WI.ItemId = I.ItemId
								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
								INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
								LEFT JOIN JobCard JC ON SOI.SaleOrderItemId = JC.SaleOrderItemId
                                WHERE ISNULL(SOI.isActive, 1) = 1
                                AND SO.isActive = 1 AND SOI.isActive = 1 AND SO.SaleOrderApproveStatus = 1
								--AND IB.SaleOrderItemId IS NULL
								AND ISNULL(ReservedQuantity, 0) < ISNULL(WI.Quantity, 0)
								AND SII.SaleOrderItemId IS NULL
                                AND I.BatchRequired = 1
                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                                AND I.ItemName LIKE '%'+@itemName+'%'
								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'

								UNION ALL
                                ------------------------------------including freezer unit
								SELECT
									SOI.SaleOrderItemId,
									SO.SaleOrderRefNo,
									CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
									CUS.CustomerName,
									STUFF((SELECT ', '+T1.WorkShopRequestRefNo FROM WorkShopRequest T1 
									    WHERE T1.JobCardId = JC.JobCardId OR (T1.SaleOrderId = SO.SaleOrderId AND T1.JobCardId IS NULL) FOR XML PATH('')), 1, 2, '')WorkShopRequestRefNo,
									STUFF((SELECT ', '+T1.StoreIssueRefNo FROM StoreIssue T1 LEFT JOIN WorkShopRequest T2 ON T1.WorkShopRequestId = T2.WorkShopRequestId
										WHERE T2.SaleOrderId = SO.SaleOrderId FOR XML PATH('')), 1, 2, '')StoreIssueRefNo,
									JC.JobCardNo,
									STUFF((SELECT ', '+T1.SerialNo FROM ItemBatch T1 LEFT JOIN GRNItem T2 ON T1.GRNItemId = T2.GRNItemId
										LEFT JOIN OpeningStock T3 ON T1.OpeningStockId = T3.OpeningStockId
										WHERE T1.SaleOrderItemId IS NULL AND (T2.ItemId = I.ItemId OR T3.ItemId = I.ItemId) FOR XML PATH('')), 1, 2, '')SerialNo,
									SOI.Quantity,
									--WD.WorkDescriptionRefNo,
									I.ItemId,
									R.ReservedQuantity,
									I.ItemName,
									WD.WorkDescrShortName,
									SO.SaleOrderDate
								FROM SaleOrder SO
								INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
								INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
								INNER JOIN Item I ON WD.FreezerUnitId = I.ItemId
								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
								INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
								LEFT JOIN JobCard JC ON SOI.SaleOrderItemId = JC.SaleOrderItemId
								WHERE ISNULL(SOI.isActive, 1) = 1
                                AND SO.isActive = 1 AND SO.SaleOrderApproveStatus = 1
								--AND IB.SaleOrderItemId IS NULL
								AND ISNULL(ReservedQuantity, 0) < ISNULL(SOI.Quantity, 0)
								AND SII.SaleOrderItemId IS NULL
                                AND I.BatchRequired = 1
                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                                AND I.ItemName LIKE '%'+@itemName+'%'
								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'

								UNION ALL
                                ------------------------------------including box
								SELECT
									SOI.SaleOrderItemId,
									SO.SaleOrderRefNo,
									CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
									CUS.CustomerName,
									STUFF((SELECT ', '+T1.WorkShopRequestRefNo FROM WorkShopRequest T1 
									    WHERE T1.JobCardId = JC.JobCardId OR (T1.SaleOrderId = SO.SaleOrderId AND T1.JobCardId IS NULL) FOR XML PATH('')), 1, 2, '')WorkShopRequestRefNo,
									STUFF((SELECT ', '+T1.StoreIssueRefNo FROM StoreIssue T1 LEFT JOIN WorkShopRequest T2 ON T1.WorkShopRequestId = T2.WorkShopRequestId
										WHERE T2.SaleOrderId = SO.SaleOrderId FOR XML PATH('')), 1, 2, '')StoreIssueRefNo,
									JC.JobCardNo,
									STUFF((SELECT ', '+T1.SerialNo FROM ItemBatch T1 LEFT JOIN GRNItem T2 ON T1.GRNItemId = T2.GRNItemId
										LEFT JOIN OpeningStock T3 ON T1.OpeningStockId = T3.OpeningStockId
										WHERE T1.SaleOrderItemId IS NULL AND (T2.ItemId = I.ItemId OR T3.ItemId = I.ItemId) FOR XML PATH('')), 1, 2, '')SerialNo,
									SOI.Quantity,
									--WD.WorkDescriptionRefNo,
									I.ItemId,
									R.ReservedQuantity,
									I.ItemName,
									WD.WorkDescrShortName,
									SO.SaleOrderDate
								FROM SaleOrder SO
								INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
								INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
								INNER JOIN Item I ON WD.BoxId = I.ItemId
								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
								INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
								LEFT JOIN JobCard JC ON SOI.SaleOrderItemId = JC.SaleOrderItemId
								WHERE ISNULL(SOI.isActive, 1) = 1
                                AND SO.isActive = 1 AND SO.SaleOrderApproveStatus = 1
								--AND IB.SaleOrderItemId IS NULL
								AND ISNULL(ReservedQuantity, 0) < ISNULL(SOI.Quantity, 0)
								AND SII.SaleOrderItemId IS NULL
                                AND I.BatchRequired = 1
                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                                AND I.ItemName LIKE '%'+@itemName+'%'
								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'

                                ORDER BY SO.SaleOrderDate DESC, SO.SaleOrderRefNo DESC;

								DROP TABLE #RESERVED;";

                return connection.Query<PendingForSOIReservation>(query, new { saleOrder = saleOrder, itemName = itemName }).ToList();
            }
        }

        public IEnumerable<ItemBatch> GetItemBatchForReservation(int saleOrderItemId, int materialId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query
                //                string query = @"SELECT
                //	                                IB.ItemBatchId,
                //	                                IB.SerialNo,
                //	                                GI.ItemId,
                //									G.GRNNo,
                //									CONVERT(VARCHAR, G.GRNDate, 106) GRNDate
                //                                INTO #BATCH
                //                                FROM ItemBatch IB
                //                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId AND IB.SaleOrderItemId IS NULL
                //								INNER JOIN GRN G ON GI.GRNId = G.GRNId
                //                                WHERE ItemId = @item AND G.isActive = 1 AND GI.isActive = 1;
                //
                //								SELECT
                //	                                GI.ItemId,
                //	                                COUNT(IB.SaleOrderItemId) ReservedQuantity
                //                                INTO #RESERVED
                //                                FROM ItemBatch IB
                //                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                //								WHERE ItemId = @item AND IB.SaleOrderItemId = @id
                //                                GROUP BY GI.ItemId;
                //
                //                                SELECT
                //	                                SOI.SaleOrderItemId,
                //	                                SOI.Quantity - ISNULL(R.ReservedQuantity, 0) Quantity,
                //	                                B.SerialNo,
                //	                                B.ItemBatchId,
                //	                                I.ItemName,
                //									B.GRNDate,
                //									B.GRNNo,
                //									SO.SaleOrderRefNo,
                //									WD.WorkDescriptionRefNo WorkDescrRefNo
                //									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                //                                FROM SaleOrderItem SOI
                //                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                //                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                //                                INNER JOIN #BATCH B ON WI.ItemId = B.ItemId
                //                                INNER JOIN Item I ON B.ItemId = I.ItemId
                //								LEFT JOIN #RESERVED R ON B.ItemId = R.ItemId
                //								INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                //                                WHERE SOI.SaleOrderItemId = @id AND WI.ItemId = @item;
                //
                //                                DROP TABLE #BATCH;
                //								DROP TABLE #RESERVED;"; 
                #endregion

                #region old query (26.10.2016 5.27p)
                //                string query = @"SELECT
                //	                                IB.ItemBatchId,
                //	                                IB.SerialNo,
                //	                                ISNULL(GI.ItemId, OS.ItemId) ItemId,
                //									G.GRNNo,
                //									CONVERT(VARCHAR, G.GRNDate, 106) GRNDate
                //                                INTO #BATCH
                //                                FROM ItemBatch IB
                //                                LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                //								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                //								LEFT JOIN GRN G ON GI.GRNId = G.GRNId
                //                                WHERE (OS.ItemId = @item OR GI.ItemId = @item)-- AND G.isActive = 1 AND GI.isActive = 1
                //								AND IB.SaleOrderItemId IS NULL;
                //
                //                                SELECT
                //	                                @item ItemId,
                //	                                COUNT(ISNULL(GI.ItemId, OS.ItemId)) ReservedQuantity
                //                                INTO #RESERVED
                //                                FROM ItemBatch IB
                //                                LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                //								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                //								where ISNULL(IB.SaleOrderItemId, 0) = @id
                //										and (GI.ItemId = @item OR OS.ItemId = @item)
                //
                //                                SELECT
                //	                                SOI.SaleOrderItemId,
                //	                                WI.Quantity - ISNULL(R.ReservedQuantity, 0) Quantity,
                //	                                B.SerialNo,
                //	                                B.ItemBatchId,
                //	                                I.ItemName,
                //									B.GRNDate,
                //									B.GRNNo,
                //									SO.SaleOrderRefNo,
                //									WD.WorkDescriptionRefNo WorkDescrRefNo
                //									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                //                                FROM SaleOrderItem SOI
                //                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                //                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                //                                LEFT JOIN #BATCH B ON WI.ItemId = B.ItemId
                //                                LEFT JOIN Item I ON B.ItemId = I.ItemId
                //								LEFT JOIN #RESERVED R ON B.ItemId = R.ItemId
                //								INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                //                                WHERE SOI.SaleOrderItemId = @id AND WI.ItemId = @item;
                //
                //                                DROP TABLE #BATCH;
                //								DROP TABLE #RESERVED;"; 
                #endregion

                string query = @"SELECT
	                                IB.ItemBatchId,
	                                IB.SerialNo,
	                                ISNULL(GI.ItemId, OS.ItemId) ItemId,
									G.GRNNo,
									CONVERT(VARCHAR, G.GRNDate, 106) GRNDate
                                INTO #BATCH
                                FROM ItemBatch IB
                                LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
								LEFT JOIN GRN G ON GI.GRNId = G.GRNId
                                WHERE (OS.ItemId = @item OR GI.ItemId = @item)-- AND G.isActive = 1 AND GI.isActive = 1
								AND IB.SaleOrderItemId IS NULL;

                                SELECT
	                                @item ItemId,
	                                COUNT(ISNULL(GI.ItemId, OS.ItemId)) ReservedQuantity
                                INTO #RESERVED
                                FROM ItemBatch IB
                                LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
								where ISNULL(IB.SaleOrderItemId, 0) = @id
										and (GI.ItemId = @item OR OS.ItemId = @item)

                                SELECT
	                                SOI.SaleOrderItemId,
	                                WI.Quantity - ISNULL(R.ReservedQuantity, 0) Quantity,
	                                B.SerialNo,
	                                B.ItemBatchId,
	                                I.ItemName,
									B.GRNDate,
									B.GRNNo,
									SO.SaleOrderRefNo,
									WD.WorkDescriptionRefNo WorkDescrRefNo
									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                                FROM SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                                LEFT JOIN #BATCH B ON WI.ItemId = B.ItemId
                                LEFT JOIN Item I ON B.ItemId = I.ItemId
								LEFT JOIN #RESERVED R ON B.ItemId = R.ItemId
								INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                WHERE SOI.SaleOrderItemId = @id AND WI.ItemId = @item

								UNION ALL
								---------------------including freezer unit
								SELECT
	                                SOI.SaleOrderItemId,
	                                SOI.Quantity Quantity,
	                                B.SerialNo,
	                                B.ItemBatchId,
	                                I.ItemName,
									B.GRNDate,
									B.GRNNo,
									SO.SaleOrderRefNo,
									WD.WorkDescriptionRefNo WorkDescrRefNo
									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                                FROM SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                LEFT JOIN #BATCH B ON WD.FreezerUnitId = B.ItemId
                                LEFT JOIN Item I ON WD.FreezerUnitId = I.ItemId
								LEFT JOIN #RESERVED R ON WD.FreezerUnitId = R.ItemId
								INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                WHERE SOI.SaleOrderItemId = @id AND WD.FreezerUnitId = @item

								UNION ALL
								---------------------including box
								SELECT
	                                SOI.SaleOrderItemId,
	                                SOI.Quantity Quantity,
	                                B.SerialNo,
	                                B.ItemBatchId,
	                                I.ItemName,
									B.GRNDate,
									B.GRNNo,
									SO.SaleOrderRefNo,
									WD.WorkDescriptionRefNo WorkDescrRefNo
									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                                FROM SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                LEFT JOIN #BATCH B ON WD.BoxId = B.ItemId
                                LEFT JOIN Item I ON WD.BoxId = I.ItemId
								LEFT JOIN #RESERVED R ON WD.BoxId = R.ItemId
								INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                WHERE SOI.SaleOrderItemId = @id AND WD.BoxId = @item

                                DROP TABLE #BATCH;
								DROP TABLE #RESERVED;";
                return connection.Query<ItemBatch>(query, new { id = saleOrderItemId, item = materialId }).ToList();
            }
        }

        public IEnumerable<PendingForSOIReservation> GetReservedItems(string saleOrder, string serialNo)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                DISTINCT SO.SaleOrderId,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
									
									STUFF((SELECT ', ' + CAST(W.WorkDescrShortName AS VARCHAR(MAX)) [text()]
									FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
									WHERE SI.SaleOrderId = SO.SaleOrderId
									FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescrShortName,

									STUFF((SELECT ', ' + CAST(BTCH.SerialNo AS VARCHAR(MAX)) [text()]
									FROM ItemBatch BTCH inner join SaleOrderItem SAL on BTCH.SaleOrderItemId = SAL.SaleOrderItemId
									WHERE SAL.SaleOrderId = SOI.SaleOrderId
									FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') SerialNo,

									SO.SaleOrderDate,
									SO.CreatedDate

	                                --WD.WorkDescriptionRefNo,
	                                --WD.WorkDescrShortName,
	                                --IB.SerialNo
                                FROM ItemBatch IB
                                INNER JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                WHERE IB.SaleOrderItemId IS NOT NULL
                                AND ISNULL(IB.isActive, 1) = 1
                                AND IB.DeliveryChallanId IS NULL
                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                                AND IB.SerialNo LIKE '%'+@serialNo+'%'
								AND ISNULL(SO.SaleOrderClosed, '') <> 'CLOSED'
                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC";
                return connection.Query<PendingForSOIReservation>(query, new { saleOrder = saleOrder, serialNo = serialNo }).ToList();
            }
        }

        public IEnumerable<ItemBatch> GetItemBatchForUnReservation(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                ItemBatchId,
	                                I.ItemName,
	                                IB.SerialNo,
                                    SO.SaleOrderId,
									G.GRNNo,
									CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
									CASE WHEN IB.GRNItemId IS NULL THEN 1 ELSE 0 END isOpeningStock
                                FROM ItemBatch IB
                                INNER JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
								LEFT JOIN GRN G ON GI.GRNId = G.GRNId
								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                                LEFT JOIN Item I ON GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId
                                WHERE SO.SaleOrderId = @id
                                AND IB.isActive = 1
                                AND IB.DeliveryChallanId IS NULL";
                return connection.Query<ItemBatch>(query, new { id = SaleOrderId }).ToList();
            }
        }

        public int UnReserveItems(List<int> selected, string CreatedBy)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"UPDATE ItemBatch SET SaleOrderItemId = NULL WHERE ItemBatchId IN @id AND DeliveryChallanId IS NULL";
                    return connection.Execute(query, new { id = selected });
                    InsertLoginHistory(dataConnection, CreatedBy, "Create", "Un-Reserve Item", "0", "0");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Return all materials in [ItemBatch] table (i.e., items that have a serial number)
        /// </summary>
        /// <param name="serialno">Empty string for no serial number</param>
        /// <param name="item">0 for any item</param>
        /// <param name="type">0 for any type, 1 for reserved, 2 for unreserved, 3 for used</param>
        /// <param name="saleorder">0 for any sale order</param>
        /// <returns></returns>
        public IEnumerable<ItemBatch> GetMaterialList(string serialno, string item, int type, string saleorder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT SO.SaleOrderId,IB.ItemBatchId,
                                    --CONCAT('Opening Stock',' - ',ISNULL(CONVERT(VARCHAR,OS.CreatedDate,106),'-')) OSDATE,
	                                ISNULL(G.GRNNo, 'Opening Stock') GRNNo,
									CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
	                                SO.SaleOrderRefNo, CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                I.ItemName,IB.SerialNo,U.EmployeeName AS CreatedBy,
                                    ISNULL(IB.DeliveryChallanId, 0) DeliveryChallanId,
									ISNULL(DC.DeliveryChallanRefNo, '-') DeliveryChallanRefNo,
									CONVERT(VARCHAR, IB.WarrantyExpireDate, 106) WarrantyExpireDate,
									DATEDIFF(MONTH, GETDATE(), IB.WarrantyExpireDate) WarrantyLeft,
									ISNULL(C.CustomerName, '-') CustomerName
                                    FROM ItemBatch IB
                                    LEFT JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                    LEFT JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                    LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                    LEFT JOIN GRN G ON GI.GRNId = G.GRNId
									LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                                    LEFT JOIN Item I ON (GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId)
                                    LEFT JOIN Employee U ON IB.CreatedBy = U.EmployeeId
                                    LEFT JOIN Customer C ON SO.CustomerId = C.CustomerId
                                    LEFT JOIN DeliveryChallan DC ON IB.DeliveryChallanId = DC.DeliveryChallanId
                                    --LEFT JOIN OpeningStock OS ON OS.OpeningStockId=IB.OpeningStockId AND OS.ItemId=I.ItemId
                                    WHERE ISNULL(IB.isActive, 1) = 1
								    AND IB.SerialNo LIKE '%'+@serialno+'%'
                                    AND I.ItemName LIKE '%'+@item+'%'
								    --AND I.ItemId = ISNULL(NULLIF(@item, 0), I.ItemId)
                                    AND ISNULL(SO.SaleOrderRefNo,'') LIKE '%'+@saleorder+'%'
								    --AND ISNULL(SO.SaleOrderId, 0) = ISNULL(NULLIF(ISNULL(@saleorder, 0), 0), ISNULL(SO.SaleOrderId, 0))
                                    ORDER BY G.GRNDate DESC, G.GRNNo DESC;";
                if (type == 1)
                {
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SO.SaleOrderId IS NOT NULL AND IB.DeliveryChallanId IS NULL ");
                }
                else if (type == 2)
                {
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SO.SaleOrderId IS NULL ");
                }
                else if (type == 3)
                {
                    //query = query.Insert(query.IndexOf("WHERE"), "LEFT JOIN ItemBatch IB1 ON SOI.SaleOrderItemId = SI.SaleOrderItemId ");
                    //query = query.Insert(query.IndexOf("ORDER BY"), "AND SI.SaleOrderItemId IS NOT NULL ");
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND IB.DeliveryChallanId IS NOT NULL ");
                }
                return connection.Query<ItemBatch>(query, new { serialno = serialno, item = item, saleorder = saleorder }).ToList();
            }
        }

        /// <summary>
        /// Get Sale Order Ref. No. and Date, GRN Ref. No. and Date, Work Desc. Ref. No and Short Name
        /// </summary>
        /// <param name="id">SaleOrderId OR SaleOrderItemId</param>
        /// <param name="type">0 if SaleOrderId, 1 if SaleOrderItemId</param>
        /// <returns>ItemBatch</returns>
        public ItemBatch GetItemBatchDetails(int id, int type)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"--SELECT
	                                --IB.ItemBatchId,
	                                --IB.SerialNo,
	                                --GI.ItemId,
									--G.GRNNo,
									--CONVERT(VARCHAR, G.GRNDate, 106) GRNDate
                                --INTO #BATCH
                                --FROM ItemBatch IB
                                --INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId AND IB.SaleOrderItemId IS NULL
								--INNER JOIN GRN G ON GI.GRNId = G.GRNId
                                --WHERE ItemId = 19;

                                SELECT DISTINCT
									--B.GRNNo,
									--B.GRNDate,
									WD.WorkDescriptionRefNo WorkDescrRefNo,
									WD.WorkDescrShortName,
									SO.SaleOrderRefNo,
									CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate
									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                                FROM SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                                --INNER JOIN #BATCH B ON WI.ItemId = B.ItemId
								INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                WHERE SOI.SaleOrderItemId = @id;

                                --DROP TABLE #BATCH;";

                if (type == 0)
                {
                    query = query.Replace("SOI.SaleOrderItemId = @id", "SO.SaleOrderId = @id");
                }
                return connection.Query<ItemBatch>(query, new { id = id }).First();
            }
        }

        public FGTracking FGTrackingDetailsByItemBatchId(int itemBatchId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //                string query = @"SELECT 
                //	                                SI.SalesInvoiceRefNo,
                //	                                SI.SalesInvoiceId,
                //	                                CONVERT(VARCHAR, SI.SalesInvoiceDate, 106) SalesInvoiceDate,
                //	                                ISNULL(SII.Amount, 0) Amount,
                //	                                SI.SpecialRemarks
                //                                FROM SalesInvoice SI
                //                                INNER JOIN SalesInvoiceItem SII ON SI.SalesInvoiceId = SII.SalesInvoiceId
                //                                INNER JOIN ItemBatch IB ON SII.SaleOrderItemId = IB.SaleOrderItemId
                //                                WHERE IB.ItemBatchId = @itemBatchId";
                string query = @"SELECT 
									IB.ItemBatchId,
									IB.SerialNo,
									DC.DeliveryChallanRefNo,
									CONVERT(VARCHAR, DC.DeliveryChallanDate, 106) DeliveryChallanDate,
									CONVERT(VARCHAR,IB.WarrantyExpireDate, 106) WarrantyExpireDate,
									DATEDIFF(MONTH, GETDATE(), IB.WarrantyExpireDate) WarrantyLeft,
	                                SI.SalesInvoiceRefNo,
	                                SI.SalesInvoiceId,
	                                CONVERT(VARCHAR, SI.SalesInvoiceDate, 106) SalesInvoiceDate,
	                                ISNULL(SII.Amount, 0) SalesInvoiceAmount,
	                                SI.SpecialRemarks SalesInvoiceRemarks,
									I.ItemName,
									ISNULL(G.GRNNo, 'Opening Stock')GRNNo,
									G.GRNDate,
									S.SupplierName,
									GI.Amount GRNAmount,
									GI.Quantity GRNQuantity,
									GI.Rate GRNRate,
									SP.StockPointName,
									PB.PurchaseBillRefNo,
									CONVERT(VARCHAR, PB.PurchaseBillDate, 106) PurchaseBillDate,
									GI.Quantity GRNQuantity,
									PBI.Rate PurchaseBillRate,
									PBI.Amount PurchaseBillAmount,
									SO.SaleOrderRefNo,
									CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
									EMP.EmployeeName,
									CUS.CustomerName
                                FROM SalesInvoice SI
                                LEFT JOIN SalesInvoiceItem SII ON SI.SalesInvoiceId = SII.SalesInvoiceId
                                RIGHT JOIN ItemBatch IB ON SII.SaleOrderItemId = IB.SaleOrderItemId
								LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
								LEFT JOIN GRN G ON GI.GRNId = G.GRNId
								LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
								LEFT JOIN Item I ON (GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId)
								LEFT JOIN Supplier S ON G.SupplierId = S.SupplierId
								LEFT JOIN Stockpoint SP ON G.WareHouseId = SP.StockPointId
								LEFT JOIN PurchaseBillItem PBI ON GI.GRNItemId = PBI.GRNItemId
								LEFT JOIN PurchaseBill PB ON PBI.PurchaseBillId = PB.PurchaseBillId
								LEFT JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
								LEFT JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
								LEFT JOIN Employee EMP ON SO.SalesExecutiveId = EMP.EmployeeId
								LEFT JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
								LEFT JOIN DeliveryChallan DC ON IB.DeliveryChallanId = DC.DeliveryChallanId
                                WHERE IB.ItemBatchId = @itemBatchId";
                FGTracking model = connection.Query<FGTracking>(query, new { itemBatchId = itemBatchId }).First();

                #region old query for getting jobcard tasks and technician
                //                query = @"SELECT
                //	                            JC.JobCardNo,
                //	                            CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
                //	                            JCT.ActualHours,
                //	                            JM.JobCardTaskName,
                //	                            EMP.EmployeeName AS TaskEmployeeName
                //                            FROM ItemBatch IB
                //                            INNER JOIN JobCard JC ON IB.SaleOrderItemId = JC.SaleOrderItemId
                //                            INNER JOIN JobCardTask JCT ON JC.JobCardId = JCT.JobCardId
                //                            LEFT JOIN JobCardTaskMaster JM ON JCT.JobCardTaskMasterId = JM.JobCardTaskMasterId
                //                            LEFT JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                //                            INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                //                            INNER JOIN WorkDescription WD ON JC.WorkDescriptionId = WD.WorkDescriptionId
                //                            INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId AND WI.ItemId = GI.ItemId
                //                            WHERE IB.ItemBatchId = @itemBatchId";
                #endregion

                query = @"SELECT DISTINCT
	                            JC.JobCardNo,
	                            CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                            JCT.ActualHours,
	                            JM.JobCardTaskName,
	                            EMP.EmployeeName AS TaskEmployeeName
                            FROM ItemBatch IB
                            INNER JOIN JobCard JC ON IB.SaleOrderItemId = JC.SaleOrderItemId
                            INNER JOIN JobCardTask JCT ON JC.JobCardId = JCT.JobCardId
                            INNER JOIN JobCardTaskMaster JM ON JCT.JobCardTaskMasterId = JM.JobCardTaskMasterId
                            INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                            LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
							LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                            INNER JOIN WorkDescription WD ON JC.WorkDescriptionId = WD.WorkDescriptionId
                            INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId 
								AND (WI.ItemId = GI.ItemId OR WI.ItemId = OS.ItemId 
									OR WD.FreezerUnitId = GI.ItemId OR WD.FreezerUnitId = OS.ItemId
									OR WD.BoxId = GI.ItemId OR WD.BoxId = OS.ItemId)
                            WHERE IB.ItemBatchId = @itemBatchId";
                model.JobCardTasks = connection.Query<FGTracking>(query, new { itemBatchId = itemBatchId }).ToList(); 
                return model;
            }
        }

        public IList<ItemBatch> PreviousList(int OrganizationId, string serialno = "", string grnno = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query
                //                string query = @"SELECT DISTINCT
                //	                    G.GRNId,
                //	                    G.GRNNo,
                //	                    CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
                //						I.ItemName,
                //	                    --SOI.SaleOrderItemId,
                //	                    --DeliveryChallanId,
                //	
                //	                    STUFF((SELECT ', ' + CAST(T1.SerialNo AS VARCHAR(MAX)) [text()]
                //									                    FROM ItemBatch T1
                //									                    WHERE T1.GRNItemId = IB.GRNItemId
                //									                    FOR XML PATH('')),1,1,'') SerialNo,
                //
                //	                    CONVERT(VARCHAR, IB.CreatedDate, 106) CreatedDate,
                //						IB.CreatedDate
                //                    FROM ItemBatch IB
                //                    INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                //                    INNER JOIN GRN G ON GI.GRNId = G.GRNId
                //					INNER JOIN Item I ON GI.ItemId = I.ItemId
                //                    WHERE G.GRNId = ISNULL(NULLIF(CAST(@id AS INT), 0), G.GRNId)
                //                    AND IB.OrganizationId = @OrganizationId
                //                    AND IB.isActive = 1
                //					AND G.GRNDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                //					ORDER BY IB.CreatedDate DESC";
                #endregion

                string query =
                    @"SELECT DISTINCT
	                    ISNULL(G.GRNId, 0) GRNId,
						ISNULL(IB.GRNItemId, 0) GRNItemId,
						ISNULL(IB.OpeningStockId, 0) OpeningStockId,
						CASE WHEN IB.OpeningStockId IS NULL THEN 0 ELSE 1 END isOpeningStock,
	                    G.GRNNo,
	                    CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
						I.ItemName,
	
	                    STUFF((SELECT ', ' + CAST(T1.SerialNo AS VARCHAR(MAX)) [text()]
									                    FROM ItemBatch T1
									                    WHERE (T1.GRNItemId = IB.GRNItemId OR T1.OpeningStockId = IB.OpeningStockId)
														AND T1.SerialNo LIKE '%'+@serialno+'%'
									                    FOR XML PATH('')),1,1,'') SerialNo,

	                    CONVERT(VARCHAR, IB.CreatedDate, 106) CreatedDate
                    FROM ItemBatch IB
                    LEFT JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
					LEFT JOIN OpeningStock OS ON IB.OpeningStockId = OS.OpeningStockId
                    LEFT JOIN GRN G ON GI.GRNId = G.GRNId
					LEFT JOIN Item I ON (GI.ItemId = I.ItemId OR OS.ItemId = I.ItemId)
                    WHERE IB.OrganizationId = @OrganizationId
                    AND IB.isActive = 1
					AND IB.SerialNo LIKE '%'+@serialno+'%'
					AND ISNULL(G.GRNNo, 'Opening Stock') LIKE '%'+@grnno+'%'";
                var list = connection.Query<ItemBatch>(query, new
                {
                    OrganizationId = OrganizationId,
                    serialno = serialno,
                    grnno = grnno
                }).ToList();
                return list;
            }
        }

        public string IsSerialNoExists(List<string> serialNos)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT STUFF((SELECT ', ' + T1.SerialNo
                                FROM ItemBatch T1
                                WHERE SerialNo IN @list
                                ORDER BY T1.SerialNo
                                FOR XML PATH('')), 1, 1, '') AS SerialNos";
                return connection.Query<string>(query, new { list = serialNos }).First();
            }
        }

        public ItemBatch GetOpeningStockItem(int OpeningStockItemId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT
	                                    OS.OpeningStockId,
	                                    OS.ItemId,
	                                    I.ItemName,
	                                    OS.Quantity,
	                                    OS.StockPointId,
	                                    S.StockPointName
                                    FROM OpeningStock OS
	                                    INNER JOIN Item I ON OS.ItemId = I.ItemId
	                                    INNER JOIN Stockpoint S ON OS.StockPointId = S.StockPointId
                                    WHERE OS.OpeningStockId = @id";

                    return connection.Query<ItemBatch>(query, new { id = OpeningStockItemId }).First();
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int Update(IList<ItemBatch> model, int CreatedBy)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"UPDATE ItemBatch SET SerialNo = @SerialNo WHERE ItemBatchId = @id";
                    foreach (var item in model)
                    {
                        int i = connection.Execute(query, new { id = item.ItemBatchId, SerialNo = item.SerialNo }, txn);
                        InsertLoginHistory(dataConnection, CreatedBy.ToString(), "Edit", "Assign Serial No", i.ToString(), "0");
                    }
                    txn.Commit();
                    return 1;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }
    }
}
