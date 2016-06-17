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
    public class StockUpdateRepository
    {
        public int InsertStockUpdate(StockUpdate model, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string sql = @"INSERT INTO StockUpdate(
                                StockPointId, 
                                ItemId, 
                                Quantity,
                                StockType,
                                StockInOut,
                                stocktrnDate,
                                StocktrnId,
                                StockUserId,
                                CreatedBy, 
                                CreatedDate,
                                OrganizationId)
                            VALUES(
                                @StockPointId, 
                                @ItemId, 
                                (@Quantity),
                                @StockType,
                                @StockInOut, 
                                @stocktrnDate,
                                @StocktrnId,
                                @StockUserId,
                                @CreatedBy, 
                                @CreatedDate,
                                @OrganizationId);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var id = connection.Query<int>(sql, model, txn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
