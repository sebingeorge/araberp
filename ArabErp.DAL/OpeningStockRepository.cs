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

                }
//            

                return id;
              
            }
        }

        public IEnumerable< OpeningStockItem> GetItem(int? StockPointId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT ItemId,Quantity  FROM OpeningStock WHERE StockPointId=@StockPointId";

                //var objItem = connection.Query<OpeningStock>(sql, new
                //{
                //    StockPointId = StockPointId
                //}).OpeningStockItem();

                return connection.Query<OpeningStockItem>(sql, new
                {
                    StockPointId = StockPointId
                }).ToList();
            }
        }





    }
}