using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class StockReturnRepository : BaseRepository
    {

        public int InsertStockReturn(StockReturn objStockReturn)
        {
            string sql = @"insert  into StockReturn(StockReturnDate,JobCardId,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@StockReturnDate,@JobCardId,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objStockReturn).Single();
            return id;
        }


        public StockReturn GetStockReturn(int StockReturnId)
        {

            string sql = @"select * from StockReturn
                        where StockReturnId=@StockReturnId";

            var objStockReturn = connection.Query<StockReturn>(sql, new
            {
                StockReturnId = StockReturnId
            }).First<StockReturn>();

            return objStockReturn;
        }

        public List<StockReturn> GetStockReturns()
        {
            string sql = @"select * from StockReturn
                        where isActive=1";

            var objStockReturns = connection.Query<StockReturn>(sql).ToList<StockReturn>();

            return objStockReturns;
        }

        public int UpdateStockReturn(StockReturn objStockReturn)
        {
            string sql = @"UPDATE StockReturn SET StockReturnDate = @StockReturnDate ,JobCardId = @JobCardId ,SpecialRemarks = @SpecialRemarks ,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.StockReturnId  WHERE StockReturnId = @StockReturnId";


            var id = connection.Execute(sql, objStockReturn);
            return id;
        }

        public int DeleteStockReturn(Unit objStockReturn)
        {
            string sql = @"Delete StockReturn  OUTPUT DELETED.StockReturnId WHERE StockReturnId=@StockReturnId";


            var id = connection.Execute(sql, objStockReturn);
            return id;
        }


    }
}
