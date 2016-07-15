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
        public int InsertItemBatch(ItemBatch objItemBatch)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into ItemBatch(GRNItemId,SaleOrderItemId,StoreIssueItemId,SerialNo,CreatedBy,CreatedDate,OrganizationId) Values (@GRNItemId,@SaleOrderItemId,@StoreIssueItemId,@SerialNo,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objItemBatch).Single();
                return id;
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

        public IEnumerable<PendingGRNItemsForBatch> PendingGRNItems()
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
                    return connection.Query<PendingGRNItemsForBatch>(query).ToList();
                }
            }
            catch (InvalidOperationException)
            {
                return new List<PendingGRNItemsForBatch>();
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
    }
}
