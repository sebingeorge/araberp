using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockReturnRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertStockReturn(StockReturn objStockReturn)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into StockReturn(StockReturnDate,JobCardId,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@StockReturnDate,@JobCardId,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objStockReturn).Single();
                return id;
            }
        }

        public StockReturn GetStockReturn(int StockReturnId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StockReturn
                        where StockReturnId=@StockReturnId";

                var objStockReturn = connection.Query<StockReturn>(sql, new
                {
                    StockReturnId = StockReturnId
                }).First<StockReturn>();

                return objStockReturn;
            }
        }

        public List<StockReturn> GetStockReturns()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StockReturn
                        where isActive=1";

                var objStockReturns = connection.Query<StockReturn>(sql).ToList<StockReturn>();

                return objStockReturns;
            }
        }

        public int UpdateStockReturn(StockReturn objStockReturn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE StockReturn SET StockReturnDate = @StockReturnDate ,JobCardId = @JobCardId ,SpecialRemarks = @SpecialRemarks ,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.StockReturnId  WHERE StockReturnId = @StockReturnId";


                var id = connection.Execute(sql, objStockReturn);
                return id;
            }
        }

        public int DeleteStockReturn(Unit objStockReturn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StockReturn  OUTPUT DELETED.StockReturnId WHERE StockReturnId=@StockReturnId";


                var id = connection.Execute(sql, objStockReturn);
                return id;
            }
        }


    }
}
