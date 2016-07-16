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
            throw new NotImplementedException();
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

        public IEnumerable<PendingForSOIReservation> GetUnreservedItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                SOI.SaleOrderItemId,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                ISNULL(SOI.Quantity, 0) Quantity,
	                                WD.WorkDescriptionRefNo,
	                                I.ItemName
                                FROM SaleOrderItem SOI
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                LEFT JOIN ItemBatch IB ON SOI.SaleOrderItemId = IB.SaleOrderItemId AND IB.SaleOrderItemId IS NULL
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                INNER JOIN WorkVsItem WI ON WD.WorkDescriptionId = WI.WorkDescriptionId
                                INNER JOIN Item I ON WI.ItemId = I.ItemId
                                WHERE ISNULL(SOI.isActive, 1) = 1
                                AND SO.isActive = 1 AND SOI.isActive = 1 AND SO.SaleOrderApproveStatus = 1
                                ORDER BY SO.SaleOrderDate DESC, SO.CreatedDate DESC";

                return connection.Query<PendingForSOIReservation>(query).ToList();
            }
        }

        public IEnumerable<ItemBatch> GetItemBatchForReservation()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT * from ItemBatch ";

                return connection.Query<ItemBatch>(query).ToList(); 
            }
        }
    }
}
