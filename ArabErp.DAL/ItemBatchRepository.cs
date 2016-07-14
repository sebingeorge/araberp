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


    }
}
