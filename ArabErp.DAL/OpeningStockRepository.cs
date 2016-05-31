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
        static string dataConnection = GetConnectionString("arab");

        public int InsertOpeningStock(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into OpeningStock(ItemId,Quantity,CreatedBy,CreatedDate,OrganizationId) Values (@ItemId,@Quantity,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objOpeningStock).Single();
                return id;
            }
        }


 

    }
}