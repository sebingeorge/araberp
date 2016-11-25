using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class DirectPurchaseItemRepository
    {
        internal int InsertDirectPurchaseRequestItem(DirectPurchaseRequestItem item, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                item.Amount = item.Rate * item.Quantity;
                string query = @"INSERT INTO DirectPurchaseRequestItem
                                (
	                                [DirectPurchaseRequestId],
	                                [SlNo],
	                                [ItemId],
	                                [Remarks],
	                                [Quantity],
	                                [Rate],
	                                [Amount],
	                                [isActive]
                                )
                                VALUES
                                (
	                                @DirectPurchaseRequestId,
	                                @SlNo,
	                                @ItemId,
	                                @Remarks,
	                                @Quantity,
	                                @Rate,
	                                @Amount,
	                                1
                                );
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
               return connection.Query<int>(query, item, txn).First();
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal int InsertPurchaseIndentItem(DirectPurchaseRequestItem item, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string sql = @"insert  into PurchaseRequestItem(PurchaseRequestId,SlNo,ItemId,Remarks,Quantity,isActive) Values (@DirectPurchaseRequestId,@SlNo,@ItemId,@Remarks,@Quantity,1);
                SELECT CAST(SCOPE_IDENTITY() as int)";
                var id = connection.Query<int>(sql, item, txn).FirstOrDefault();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
