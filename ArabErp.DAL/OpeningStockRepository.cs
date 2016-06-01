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


 

    }
}