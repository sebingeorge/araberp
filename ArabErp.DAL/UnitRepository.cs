using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class UnitRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertUnit(Unit objUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO Unit (UnitRefNo,UnitName,CreatedBy,CreatedDate,OrganizationId,isActive) VALUES(@UnitRefNo,@UnitName,@CreatedBy,getDate(),@OrganizationId,1);
            SELECT CAST(SCOPE_IDENTITY() as int)";



                var id = connection.Query<int>(sql, objUnit).Single();
                return id;
            }
        }
        public IEnumerable<Unit> FillUnitList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<Unit>("SELECT UnitId,UnitRefNo,UnitName FROM Unit WHERE isActive=1").ToList();
            }
        }


        public Unit GetUnit(int UnitId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Unit
                        where UnitId=@UnitId";

                var objUnit = connection.Query<Unit>(sql, new
                {
                    UnitId = UnitId
                }).First<Unit>();

                return objUnit;
            }
        }

        public List<Unit> GetUnits()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Unit
                        where isActive=1";

                var objUnits = connection.Query<Unit>(sql).ToList<Unit>();

                return objUnits;
            }
        }

        public int UpdateUnit(Unit objUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Unit Set UnitRefNo=@UnitRefNo,UnitName=@UnitName OUTPUT INSERTED.UnitId WHERE UnitId=@UnitId";


                var id = connection.Execute(sql, objUnit);
                return id;
            }
        }

        public int DeleteUnit(Unit objUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update Unit Set isActive=0 WHERE UnitId=@UnitId";


                var id = connection.Execute(sql, objUnit);
                return id;
            }
        }

    }
}
