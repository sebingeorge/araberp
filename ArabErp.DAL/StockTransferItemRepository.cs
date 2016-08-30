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
    class StockTransferItemRepository
    {
        internal int InsertTransferItems(StockTransferItem item, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string query = @"INSERT INTO StockTransferItem
                            (
	                            StockTransferId,
	                            ItemId,
	                            Quantity,
	                            isActive
                            )
                            VALUES
                            (
	                            @StockTransferId,
	                            @ItemId,
	                            @Quantity,
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
