using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplyOrderItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertSupplyOrderItem(SupplyOrderItem objSupplyOrderItem, IDbConnection connection, IDbTransaction trn)
        {
           
                string sql = @"insert  into SupplyOrderItem(SupplyOrderId,PurchaseRequestItemId,SlNo,BalQty,OrderedQty,Rate,Discount,Amount,CreatedBy,CreatedDate,OrganizationId,isActive,CreatedDate,OrganizationId) Values (@SupplyOrderId,@PurchaseRequestItemId,@SlNo,@BalQty,@OrderedQty,@Rate,@Discount,@Amount,@CreatedBy,@CreatedDate,@OrganizationId,@isActive,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSupplyOrderItem,trn).Single();
                return id;
            
        }


        public SupplyOrderItem GetSupplyOrderItem(int SupplyOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrderItem
                        where SupplyOrderItemId=@SupplyOrderItemId";

                var objSupplyOrderItem = connection.Query<SupplyOrderItem>(sql, new
                {
                    SupplyOrderItemId = SupplyOrderItemId
                }).First<SupplyOrderItem>();

                return objSupplyOrderItem;
            }
        }
        public List<SupplyOrderItem> GetSupplyOrderItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrderItem
                        where isActive=1";

                var objSupplyOrderItems = connection.Query<SupplyOrderItem>(sql).ToList<SupplyOrderItem>();

                return objSupplyOrderItems;
            }
        }

        public int UpdateSupplyOrderItem(SupplyOrderItem objSupplyOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SupplyOrderItem SET SupplyOrderId = @SupplyOrderId ,PurchaseRequestItemId = @PurchaseRequestItemId ,SlNo = @SlNo ,BalQty = @BalQty,OrderedQty = @OrderedQty,Rate = @Rate,Discount = @Discount,Amount = @Amount,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate   OUTPUT INSERTED.SupplyOrderItemId  WHERE SupplyOrderItemId = @SupplyOrderItemId";


                var id = connection.Execute(sql, objSupplyOrderItem);
                return id;
            }
        }
        public int DeleteSupplyOrderItem(Unit objSupplyOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplyOrderItem  OUTPUT DELETED.SupplyOrderItemId WHERE SupplyOrderItemId=@SupplyOrderItemId";


                var id = connection.Execute(sql, objSupplyOrderItem);
                return id;
            }
        }


    }
}
