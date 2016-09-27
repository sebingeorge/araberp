using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{

    public class StockJournalItemsRepository :BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertStockJournalItem(StockJournalItem model, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string query = @"INSERT INTO StockJournalItem (StockJournalId,SlNo,ItemId,Quantity) VALUES (@StockJournalId,@SlNo,@ItemId,@Quantity);
                            SELECT CAST(SCOPE_IDENTITY() AS INT)
                            INSERT INTO StockUpdate(StockPointId,StocktrnId,StockUserId,stocktrnDate,ItemId,Quantity,StockType,StockInOut,StockDescription,CreatedBy,CreatedDate,OrganizationId)
                            SELECT @StockPointId,@StockJournalId,@StockJournalRefno,StockJournalDate,@ItemId,@Quantity,'StockJournal',CASE WHEN @Quantity < 0 THEN 'OUT' WHEN @Quantity > 0 THEN 'IN' END ,@Remarks,@CreatedBy,GETDATE(),@OrganizationId
                            FROM StockJournal   where StockJournalId=@StockJournalId";

                return connection.Query<int>(query, model, txn).First();
            }
            catch (Exception) 
            {
                throw;
            }
        }

        public List<StockJournalItem> GetStockJournalDT(int StockJournalId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT SlNo,S.ItemId,I.PartNo,Sum(SU.Quantity)Stock,S.Quantity,UnitName UoM
                                FROM StockJournalItem S
                                INNER JOIN Item I ON I.ItemId=S.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                LEFT JOIN StockUpdate SU ON SU.ItemId=S.ItemId
                                WHERE StockJournalId=@StockJournalId
                                GROUP BY SlNo,S.ItemId,I.PartNo,S.Quantity,UnitName";

                return connection.Query<StockJournalItem>(sql, new { StockJournalId = StockJournalId }).ToList();
            }
        }

    }
}
