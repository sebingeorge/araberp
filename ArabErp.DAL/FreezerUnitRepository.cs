using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class FreezerUnitRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertFreezerUnit(FreezerUnit objFreezerUnit)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO FreezerUnit (FreezerUnitRefNo,FreezerUnitName,CreatedBy,CreatedDate,OrganizationId) VALUES(@FreezerUnitRefNo,@FreezerUnitName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objFreezerUnit).Single();
                return id;
            }
        }


        public FreezerUnit GetFreezerUnit(int FreezerUnitId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from FreezerUnit
                        where FreezerUnitId=@FreezerUnitId";

                var objFreezerUnit = connection.Query<FreezerUnit>(sql, new
                {
                    FreezerUnitId = FreezerUnitId
                }).First<FreezerUnit>();

                return objFreezerUnit;
            }
        }

        public List<Dropdown> FillFreezerUnit()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select FreezerUnitId Id,FreezerUnitName Name from FreezerUnit").ToList();
            }
        }

        public List<FreezerUnit> GetFreezerUnits()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from FreezerUnit
                        where isActive=1";

                var objFreezerUnits = connection.Query<FreezerUnit>(sql).ToList<FreezerUnit>();

                return objFreezerUnits;
            }
        }

        public int UpdateFreezerUnit(FreezerUnit objFreezerUnit)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update FreezerUnit Set FreezerUnitRefNo=@FreezerUnitRefNo,FreezerUnitName=@FreezerUnitName OUTPUT INSERTED.FreezerUnitId WHERE FreezerUnitId=@FreezerUnitId";


                var id = connection.Execute(sql, objFreezerUnit);
                return id;
            }
        }

        public int DeleteFreezerUnit(Unit objFreezerUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete FreezerUnit  OUTPUT DELETED.FreezerUnitId WHERE FreezerUnitId=@FreezerUnitId";


                var id = connection.Execute(sql, objFreezerUnit);
                return id;
            }
        }


    }
}