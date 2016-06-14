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
    }
}
