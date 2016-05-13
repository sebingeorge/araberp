using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class UnitRepository : BaseRepository
    {

    

        public int InsertUnit(Unit objUnit)
        {
            string sql = @"
            INSERT INTO [Unit] (UnitRefNo,UnitName,OrganizationId) VALUES (@UnitRefNo,@UnitName,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objUnit).Single();
            return id;
        }

     
    }
}
