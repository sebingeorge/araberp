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
            string sql = @"INSERT INTO Unit (UnitRefNo,UnitName,CreatedBy,CreatedDate,OrganizationId) VALUES(@UnitRefNo,@UnitName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objUnit).Single();
            return id;
        }


        public Unit GetUnit(int UnitId)
        {

            string sql = @"select * from Unit
                        where UnitId=@UnitId";

            var objUnit= connection.Query<Unit>(sql, new
            {
                UnitId = UnitId
            }).First<Unit>();

            return objUnit;
        }

        public List<Unit> GetUnits()
        {
            string sql = @"select * from Unit
                        where OrganizationId>0";

            var objUnits = connection.Query<Unit>(sql).ToList<Unit>();

            return objUnits;
        }


    }
}
