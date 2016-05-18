using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class FreezerUnitRepository : BaseRepository
    {

        public int InsertFreezerUnit(FreezerUnit objFreezerUnit)
        {
            string sql = @"INSERT INTO FreezerUnit (FreezerUnitRefNo,FreezerUnitName,CreatedBy,CreatedDate,OrganizationId) VALUES(@FreezerUnitRefNo,@FreezerUnitName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objFreezerUnit).Single();
            return id;
        }


        public FreezerUnit GetFreezerUnit(int FreezerUnitId)
        {

            string sql = @"select * from FreezerUnit
                        where FreezerUnitId=@FreezerUnitId";

            var objFreezerUnit = connection.Query<FreezerUnit>(sql, new
            {
                FreezerUnitId = FreezerUnitId
            }).First<FreezerUnit>();

            return objFreezerUnit;
        }

        public List<FreezerUnit> GetFreezerUnits()
        {
            string sql = @"select * from FreezerUnit
                        where isActive=1";

            var objFreezerUnits = connection.Query<FreezerUnit>(sql).ToList<FreezerUnit>();

            return objFreezerUnits;
        }

        public int UpdateFreezerUnit(FreezerUnit objFreezerUnit)
        {
            string sql = @"Update FreezerUnit Set FreezerUnitRefNo=@FreezerUnitRefNo,FreezerUnitName=@FreezerUnitName OUTPUT INSERTED.FreezerUnitId WHERE FreezerUnitId=@FreezerUnitId";


            var id = connection.Execute(sql, objFreezerUnit);
            return id;
        }

        public int DeleteFreezerUnit(Unit objFreezerUnit)
        {
            string sql = @"Delete FreezerUnit  OUTPUT DELETED.FreezerUnitId WHERE FreezerUnitId=@FreezerUnitId";


            var id = connection.Execute(sql, objFreezerUnit);
            return id;
        }


    }
}