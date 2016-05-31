using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockReturnItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        /// <summary>
        /// Insert stock return into details table (StockReturnItem table)
        /// </summary>
        /// <param name="objStockReturnItem"></param>
        /// <param name="connection"></param>
        /// <param name="txn"></param>
        /// <returns></returns>
        public int InsertStockReturnItem(StockReturnItem objStockReturnItem, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string sql = @"insert  into StockReturnItem(StockReturnId,SlNo,ItemId,Quantity,Remarks) Values (@StockReturnId,@SlNo,@ItemId,@Quantity,@Remarks);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
                var id = connection.Query<int>(sql, objStockReturnItem, txn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public StockReturnItem GetStockReturnItem(int StockReturnItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StockReturnItem
                        where StockReturnItemId=@StockReturnItemId";

                var objStockReturnItem = connection.Query<StockReturnItem>(sql, new
                {
                    StockReturnItemId = StockReturnItemId
                }).First<StockReturnItem>();

                return objStockReturnItem;
            }
        }

        public List<StockReturnItem> GetStockReturnItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StockReturnItem
                        where isActive=1";

                var objStockReturnItems = connection.Query<StockReturnItem>(sql).ToList<StockReturnItem>();

                return objStockReturnItems;
            }
        }

        public int UpdateStockReturnItem(StockReturnItem objStockReturnItem)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE StockReturnItem SET StockReturnId = @StockReturnId ,SlNo = @SlNo ,ItemId = @ItemId ,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.StockReturnItemId  WHERE StockReturnItemId = @StockReturnItemId";


                var id = connection.Execute(sql, objStockReturnItem);
                return id;
            }
        }

        public int DeleteStockReturnItem(Unit objStockReturnItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StockReturnItem  OUTPUT DELETED.StockReturnItemId WHERE StockReturnItemId=@StockReturnItemId";


                var id = connection.Execute(sql, objStockReturnItem);
                return id;
            }
        }
        /// <summary>
        /// Get the unit of a given item
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public string GetItemUnit(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "SELECT U.UnitName FROM Item I INNER JOIN Unit U ON I.ItemUnitId = U.UnitId AND I.ItemId = @ItemId";
                return connection.Query<string>(query,
                    new { ItemId = itemId }).First<string>();
            }
        }
    }
}