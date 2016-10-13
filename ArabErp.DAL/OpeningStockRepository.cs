using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class OpeningStockRepository : BaseRepository
    {
        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");

        public OpeningStockRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public IEnumerable<Dropdown> FillStockpoint()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT StockPointId Id,StockPointName Name FROM Stockpoint").ToList();
            }
        }

        public IEnumerable<Dropdown> FillItem()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT ItemId Id,ItemName Name FROM Item").ToList();
            }
        }

        public int DeleteOpeningStock(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete OpeningStock  OUTPUT DELETED.StockPointId WHERE StockPointId=@StockPointId";
                var id = connection.Execute(sql, objOpeningStock);
                InsertLoginHistory(dataConnection, objOpeningStock.CreatedBy, "Update", "Opening Stock", id.ToString(), "0");
                return id;
            }
        }

        public int InsertOpeningStock(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                int id = 0;
                foreach (var item in objOpeningStock.OpeningStockItem)
                {
                    if (item.ItemId == null || item.ItemId == 0) continue;
                    string sql = @"insert  into OpeningStock(StockPointId,ItemId,Quantity,CreatedBy,CreatedDate,OrganizationId) 
                           Values (@stockpointId,@ItemId,@Quantity,@CreatedBy,@CreatedDate,@OrganizationId);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

                    //id = connection.Query<int>(sql, objOpeningStock).Single();

                    id = connection.Query<int>(sql, new
                    {
                        stockpointId = objOpeningStock.stockpointId,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        CreatedBy = objOpeningStock.CreatedBy,
                        CreatedDate = objOpeningStock.CreatedDate,
                        OrganizationId = objOpeningStock.OrganizationId
                    }).Single();
                    InsertLoginHistory(dataConnection, objOpeningStock.CreatedBy, "Create", "Opening Stock", id.ToString(), "0");
                }


                return id;

            }
        }

        public int DeleteStockUpdate(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StockUpdate  OUTPUT DELETED.StockPointId WHERE StockPointId=@StockPointId AND StockType='OpeningStock'";
                var id = connection.Execute(sql, objOpeningStock);
                return id;
            }
        }

        public int InsertStockUpdate(OpeningStock objOpeningStock, IDbConnection connection, IDbTransaction txn)
        {
            //using (IDbConnection connection = OpenConnection(dataConnection))
            //{
            int id = 0;
            foreach (var item in objOpeningStock.OpeningStockItem)
            {
                if (item.ItemId == null || item.ItemId == 0) continue;
                string sql = @"insert  into StockUpdate(StockPointId,stocktrnDate,ItemId,Quantity,
                                 StockType,StockInOut,CreatedBy,CreatedDate,OrganizationId) 
                                 Values (@stockpointId,@CreatedDate,@ItemId,@Quantity,'OpeningStock','IN',
                                 @CreatedBy,@CreatedDate,@OrganizationId);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";

                //id = connection.Query<int>(sql, objOpeningStock).Single();

                id = connection.Query<int>(sql, new
                {
                    stockpointId = objOpeningStock.stockpointId,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    CreatedBy = objOpeningStock.CreatedBy,
                    CreatedDate = objOpeningStock.CreatedDate,
                    OrganizationId = objOpeningStock.OrganizationId
                }, txn).Single();

            }


            return id;

            //}
        }

        public IEnumerable<OpeningStockItem> GetItem(int? StockPointId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query
                //string sql = @"SELECT ItemId,Quantity  FROM OpeningStock WHERE StockPointId=@StockPointId"; 
                #endregion

                string query = @"SELECT DISTINCT OS.OpeningStockId, ItemId, Quantity, CASE WHEN IB.ItemBatchId IS NOT NULL THEN 1 ELSE 0 END AS isUsed
                                FROM OpeningStock OS
	                                LEFT JOIN ItemBatch IB ON OS.OpeningStockId = IB.OpeningStockId
                                WHERE StockPointId=@StockPointId";

                //var objItem = connection.Query<OpeningStock>(sql, new
                //{
                //    StockPointId = StockPointId
                //}).OpeningStockItem();

                return connection.Query<OpeningStockItem>(query, new
                {
                    StockPointId = StockPointId
                }).ToList();
            }
        }

        public IEnumerable<OpeningStockReport> GetClosingStockData(int stockPointId, int itemCategoryId, int itemId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT I.ItemName,ISnull(I.PartNo,'-')PartNo,Sum(Quantity)Quantity,U.UnitName FROM StockUpdate S
                                INNER JOIN Item I ON I.ItemId=S.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                WHERE StockType='OpeningStock' AND I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) AND 
                                I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) and S.OrganizationId=@OrganizationId AND 
                                S.StockPointId = ISNULL(NULLIF(@stkid, 0), S.StockPointId) 
                                GROUP BY  I.ItemName,I.PartNo,U.UnitName
                                ORDER BY I.ItemName";
                return connection.Query<OpeningStockReport>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId }).ToList();
            }
        }

        public int UpdateOpeningStock(OpeningStock model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    var list = (from OpeningStockItem p in model.OpeningStockItem
                                where !p.isUsed
                                select p).ToList();

                    string qry = @"UPDATE OpeningStock SET
	                            [ItemId]=@ItemId,
	                            [Quantity]=@Quantity
                            WHERE OpeningStockId = @OpeningStockId";
                    var id = 0;
                    foreach (var item in list)
                    {
                        id = connection.Execute(qry, new
                                {
                                    ItemId = item.ItemId,
                                    Quantity = item.Quantity,
                                    OpeningStockId = item.OpeningStockId
                                }, txn);
                    }

                    qry = @"DELETE FROM StockUpdate  WHERE StockpointId = @StockpointId AND StockType='OpeningStock'";

                    connection.Execute(qry, new { StockpointId = model.stockpointId }, txn);

                    InsertStockUpdate(model, connection, txn);

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", typeof(OpeningStock).Name, id.ToString(), model.OrganizationId.ToString());

                    txn.Commit();

                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }

            }
        }
    }
}