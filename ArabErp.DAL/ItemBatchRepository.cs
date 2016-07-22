using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

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

                string sql = @"insert  into ItemBatch(GRNItemId,SerialNo,CreatedBy,CreatedDate,OrganizationId, isActive) Values (@GRNItemId,@SerialNo,@CreatedBy,@CreatedDate,@OrganizationId,1);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    foreach (var item in model)
                    {
                        var id = connection.Query<int>(sql, item, txn).Single();
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


        public ItemBatch GetItemBatch(int ItemBatchId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemBatch
                        where ItemBatchId=@ItemBatchId";

                var objItemBatch = connection.Query<ItemBatch>(sql, new
                {
                    ItemBatchId = ItemBatchId
                }).First<ItemBatch>();

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



        public int DeleteItemBatch(Unit objItemBatch)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete ItemBatch  OUTPUT DELETED.ItemBatchId WHERE ItemBatchId=@ItemBatchId";


                var id = connection.Execute(sql, objItemBatch);
                return id;
            }
        }

        public IEnumerable<ItemBatch> PendingGRNItems()
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT GI.GRNItemId,
	                            I.ItemId, 
	                            I.ItemName, 
	                            GI.Quantity, 
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
                            LEFT JOIN ItemBatch P ON P.GRNItemId=GI.GRNItemId 
                            WHERE P.GRNItemId IS NULL
							AND I.BatchRequired = 1;";
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

        public void ReserveItemBatch(IList<ItemBatch> model)
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
        public IEnumerable<PendingForSOIReservation> GetUnreservedItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
									GI.ItemId,
									BT.SaleOrderItemId,
									COUNT(BT.SaleOrderItemId) ReservedQuantity
								INTO #RESERVED
								FROM ItemBatch BT
								INNER JOIN GRNItem GI ON BT.GRNItemId = GI.GRNItemId
								WHERE BT.SaleOrderItemId IS NOT NULL
								GROUP BY GI.ItemId, BT.SaleOrderItemId;

								SELECT
	                                SOI.SaleOrderItemId,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                ISNULL(SOI.Quantity, 0) Quantity,
	                                WD.WorkDescriptionRefNo,
                                    I.ItemId,
									R.ReservedQuantity,
	                                I.ItemName,
									WD.WorkDescrShortName
                                FROM SaleOrderItem SOI
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                                INNER JOIN Item I ON WI.ItemId = I.ItemId
								LEFT JOIN #RESERVED R ON SOI.SaleOrderItemId = R.SaleOrderItemId AND I.ItemId = R.ItemId
								LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId = SII.SaleOrderItemId
                                WHERE ISNULL(SOI.isActive, 1) = 1
                                AND SO.isActive = 1 AND SOI.isActive = 1 AND SO.SaleOrderApproveStatus = 1
								--AND IB.SaleOrderItemId IS NULL
								AND ISNULL(ReservedQuantity, 0) < ISNULL(SOI.Quantity, 0)
								AND SII.SaleOrderItemId IS NULL
                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC;

								DROP TABLE #RESERVED;";

                return connection.Query<PendingForSOIReservation>(query).ToList();
            }
        }

        public IEnumerable<ItemBatch> GetItemBatchForReservation(int saleOrderItemId, int materialId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                IB.ItemBatchId,
	                                IB.SerialNo,
	                                GI.ItemId
                                INTO #BATCH
                                FROM ItemBatch IB
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId AND IB.SaleOrderItemId IS NULL
                                WHERE ItemId = @item;

								SELECT
	                                GI.ItemId,
	                                COUNT(IB.SaleOrderItemId) ReservedQuantity
                                INTO #RESERVED
                                FROM ItemBatch IB
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
								WHERE ItemId = @item AND IB.SaleOrderItemId = @id
                                GROUP BY GI.ItemId;

                                SELECT
	                                SOI.SaleOrderItemId,
	                                SOI.Quantity - ISNULL(R.ReservedQuantity, 0) Quantity,
	                                B.SerialNo,
	                                B.ItemBatchId,
	                                I.ItemName
									--ISNULL(R.ReservedQuantity, 0) ReservedQuantity
                                FROM SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                                INNER JOIN #BATCH B ON WI.ItemId = B.ItemId
                                INNER JOIN Item I ON B.ItemId = I.ItemId
								LEFT JOIN #RESERVED R ON B.ItemId = R.ItemId
                                WHERE SOI.SaleOrderItemId = @id AND WI.ItemId = @item;

                                DROP TABLE #BATCH;
								DROP TABLE #RESERVED;";

                return connection.Query<ItemBatch>(query, new { id = saleOrderItemId, item = materialId }).ToList();
            }
        }

        public IEnumerable<PendingForSOIReservation> GetReservedItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                DISTINCT SO.SaleOrderId,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
									
									STUFF((SELECT ', ' + CAST(W.WorkDescrShortName AS VARCHAR(10)) [text()]
									FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
									WHERE SI.SaleOrderId = SO.SaleOrderId
									FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescrShortName,

									STUFF((SELECT ', ' + CAST(BTCH.SerialNo AS VARCHAR(10)) [text()]
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
                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC";
                return connection.Query<PendingForSOIReservation>(query).ToList();
            }
        }

        public IEnumerable<ItemBatch> GetItemBatchForUnReservation(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                ItemBatchId,
	                                I.ItemName,
	                                IB.SerialNo
                                FROM ItemBatch IB
                                INNER JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                INNER JOIN Item I ON GI.ItemId = I.ItemId
                                WHERE SO.SaleOrderId = @id
                                AND IB.isActive = 1";
                return connection.Query<ItemBatch>(query, new { id = SaleOrderId }).ToList();
            }
        }

        public int UnReserveItems(List<int> selected)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"UPDATE ItemBatch SET SaleOrderItemId = NULL WHERE ItemBatchId IN @id";
                    connection.Execute(query, new { id = selected });
                    return 1;
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
        public IEnumerable<ItemBatch> GetMaterialList(string serialno, int item, int type, int saleorder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                IB.ItemBatchId,
	                                G.GRNNo,
	                                CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
	                                ISNULL(SO.SaleOrderRefNo, '-') SaleOrderRefNo,
	                                ISNULL(CONVERT(VARCHAR, SO.SaleOrderDate, 106), '-') SaleOrderDate,
	                                I.ItemName,
	                                IB.SerialNo,
	                                U.UserName AS CreatedBy
                                FROM ItemBatch IB
                                LEFT JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                LEFT JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                INNER JOIN GRN G ON GI.GRNId = G.GRNId
                                INNER JOIN Item I ON GI.ItemId = I.ItemId
                                LEFT JOIN [User] U ON IB.CreatedBy = U.UserId
                                WHERE ISNULL(IB.isActive, 1) = 1
								AND IB.SerialNo LIKE '%'+@serialno+'%'
								AND I.ItemId = ISNULL(NULLIF(@item, 0), I.ItemId)
								AND ISNULL(SO.SaleOrderId, 0) = ISNULL(NULLIF(ISNULL(@saleorder, 0), 0), ISNULL(SO.SaleOrderId, 0))
                                ORDER BY G.GRNDate DESC, IB.CreatedDate DESC;";
                if (type == 1)
                {
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SO.SaleOrderId IS NOT NULL ");
                }
                else if (type == 2)
                {
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SO.SaleOrderId IS NULL ");
                }
                else if (type == 3)
                {
                    query = query.Insert(query.IndexOf("WHERE"), "LEFT JOIN SalesInvoiceItem SI ON SOI.SaleOrderItemId = SI.SaleOrderItemId ");
                    query = query.Insert(query.IndexOf("ORDER BY"), "AND SI.SaleOrderItemId IS NOT NULL ");
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
                string query = @"SELECT
	                                G.GRNNo,
	                                CONVERT(VARCHAR, G.GRNDate, 106) GRNDate,
	                                WD.WorkDescriptionRefNo,
	                                WD.WorkDescrShortName,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate
                                FROM ItemBatch IB
                                INNER JOIN GRNItem GI ON IB.GRNItemId = GI.GRNItemId
                                INNER JOIN GRN G ON GI.GRNId = G.GRNId
                                INNER JOIN SaleOrderItem SOI ON IB.SaleOrderItemId = SOI.SaleOrderItemId
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                WHERE SOI.SaleOrderItemId = @id AND IB.isActive = 1";
                if (type == 0)
                {
                    query = query.Replace("SOI.SaleOrderItemId = @id", "SO.SaleOrderId = @id");
                }
                return connection.Query<ItemBatch>(query, new { id = id }).First();
            }
        }
    }
}
