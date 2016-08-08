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
        public string ConnectionString()
        {
            return dataConnection;
        }

        public Unit InsertUnit(Unit objUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Unit();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO Unit (UnitRefNo,UnitName,CreatedBy,CreatedDate,OrganizationId,isActive) 
                               VALUES(@UnitRefNo,@UnitName,@CreatedBy,getDate(),@OrganizationId,1);
                               SELECT CAST(SCOPE_IDENTITY() as int)";



                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Unit).Name, "0", 1);
                    objUnit.UnitRefNo = "UN/" + internalid;

                    int id = connection.Query<int>(sql, objUnit, trn).Single();
                    objUnit.UnitId = id;
                    InsertLoginHistory(dataConnection, objUnit.CreatedBy, "Create", "Unit", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objUnit.UnitId = 0;
                    objUnit.UnitRefNo = null;

                }
                return objUnit;
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

        public Unit UpdateUnit(Unit objUnit)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Unit Set UnitRefNo=@UnitRefNo,UnitName=@UnitName OUTPUT INSERTED.UnitId WHERE UnitId=@UnitId";


                var id = connection.Execute(sql, objUnit);
                InsertLoginHistory(dataConnection, objUnit.CreatedBy, "Update", "Unit", id.ToString(), "0");
                return objUnit;
            }
        }

        public int DeleteUnit(Unit objUnit)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
               string sql = @" Update Unit Set isActive=0 WHERE UnitId=@UnitId";
                try
                {

                    var id = connection.Execute(sql, objUnit);
                    objUnit.UnitId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objUnit.CreatedBy, "Delete", "Unit", id.ToString(), "0");
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

   
        public string GetRefNo(Unit objUnit)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Unit();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Unit).Name, "0", 0);
                    RefNo = "UN/" + internalid;
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
