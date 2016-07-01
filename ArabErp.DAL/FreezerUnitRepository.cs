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


        public FreezerUnit InsertFreezerUnit(FreezerUnit objFreezerUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new FreezerUnit();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO FreezerUnit (FreezerUnitRefNo,FreezerUnitName,CreatedBy,CreatedDate,OrganizationId)
                               VALUES(@FreezerUnitRefNo,@FreezerUnitName,@CreatedBy,@CreatedDate,@OrganizationId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(FreezerUnit).Name, "0", 1);
                    objFreezerUnit.FreezerUnitRefNo = "FU/" + internalid;

                    int id = connection.Query<int>(sql, objFreezerUnit, trn).Single();
                    objFreezerUnit.FreezerUnitId = id;
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objFreezerUnit.FreezerUnitId = 0;
                    objFreezerUnit.FreezerUnitRefNo = null;

                }
                return objFreezerUnit;
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

        //public List<Dropdown> FillFreezerUnit()
        //{
        //    using (IDbConnection connection = OpenConnection(dataConnection))
        //    {
        //        var param = new DynamicParameters();
        //        return connection.Query<Dropdown>("select FreezerUnitId Id,FreezerUnitName Name from FreezerUnit").ToList();
        //    }
        //}

        public IEnumerable<FreezerUnit> FillFreezerUnit()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<FreezerUnit>("SELECT FreezerUnitId,FreezerUnitRefNo,FreezerUnitName FROM FreezerUnit WHERE isActive=1").ToList();
            }
        }


        public FreezerUnit UpdateFreezerUnit(FreezerUnit objFreezerUnit)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update FreezerUnit Set FreezerUnitRefNo=@FreezerUnitRefNo,FreezerUnitName=@FreezerUnitName OUTPUT INSERTED.FreezerUnitId WHERE FreezerUnitId=@FreezerUnitId";


                var id = connection.Execute(sql, objFreezerUnit);
                return objFreezerUnit;
            }
        }

        public int DeleteFreezerUnit(FreezerUnit objFreezerUnit)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql =  @"Update FreezerUnit Set isActive=0 WHERE FreezerUnitId=@FreezerUnitId";
                try
                {

                    var id = connection.Execute(sql, objFreezerUnit);
                    objFreezerUnit.FreezerUnitId = id;
                    result = 0;

                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
                    {
                        switch (ex.Errors[0].Number)
                        {
                            case 547: // Foreign Key violation
                                result = 1;
                                break;

                            default:
                                result = 2;
                                break;
                        }
                    }

                }

                return result;
            }
        }


        public string GetRefNo(FreezerUnit objFreezerUnit)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new FreezerUnit();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(FreezerUnit).Name, "0", 0);
                    RefNo = "FU/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }

    }
}