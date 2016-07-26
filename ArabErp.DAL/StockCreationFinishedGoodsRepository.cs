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
    public class StockCreationFinishedGoodsRepository
    {
        public int InsertFinishedGoods(StockCreationFinishedGood model, IDbConnection connection, IDbTransaction txn)
        {
            string query = @"INSERT INTO StockCreationFinishedGoods
                            (
	                            StockCreationId,
	                            ItemId,
	                            Quantity,
	                            Rate
                            )
                            VALUES
                            (
                                @StockCreationId,
	                            @ItemId,
	                            @Quantity,
	                            @Rate
                            );
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return connection.Query<int>(query, model, txn).First();
        }
    }
}
