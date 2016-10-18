using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
   public class EmployeeLocationRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public IEnumerable<EmployeeLocation> FillEmployeeLocationList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<EmployeeLocation>("SELECT LocationId,LocationRefNo,LocationName FROM EmployeeLocation WHERE isActive=1").ToList();
            }
        }
        public EmployeeLocation GetEmployeeLocation(int LocationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from EmployeeLocation
                        where LocationId=@LocationId";

                var objDesignation = connection.Query<EmployeeLocation>(sql, new
                {
                    LocationId = LocationId
                }).First<EmployeeLocation>();

                return objDesignation;
            }
        }
        public string GetRefNo(EmployeeLocation objEmployeeLocation)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new EmployeeLocation();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(EmployeeLocation).Name, "0", 0);
                    RefNo = "L/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }

        public EmployeeLocation InsertEmployeeLocation(EmployeeLocation objEmployeeLocation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new EmployeeLocation();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO EmployeeLocation(LocationRefNo,LocationName,CreatedBy,CreatedDate,OrganizationId,isActive) 
                               VALUES(@LocationRefNo,@LocationName,@CreatedBy,@CreatedDate,@OrganizationId,1);
                               SELECT CAST(SCOPE_IDENTITY() as int)";
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(EmployeeLocation).Name, "0", 1);
                    objEmployeeLocation.LocationRefNo = "L/" + internalid;

                    int id = connection.Query<int>(sql, objEmployeeLocation, trn).Single();
                    objEmployeeLocation.LocationId = id;
                    //connection.Dispose();
                    InsertLoginHistory(dataConnection, objEmployeeLocation.CreatedBy, "Create", "EmployeeLocation", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objEmployeeLocation.LocationId = 0;
                    objEmployeeLocation.LocationRefNo = null;

                }
                return objEmployeeLocation;
            }
        }

        public EmployeeLocation UpdateEmployeeLocation(EmployeeLocation objEmployeeLocation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update EmployeeLocation Set LocationRefNo=@LocationRefNo,LocationName=@LocationName OUTPUT INSERTED.LocationId WHERE LocationId=@LocationId";

                var id = connection.Execute(sql, objEmployeeLocation);
                InsertLoginHistory(dataConnection, objEmployeeLocation.CreatedBy, "Update", "EmployeeLocation", id.ToString(), "0");
                return objEmployeeLocation;
            }
        }
        public int DeleteEmployeeLocation(EmployeeLocation objEmployeeLocation)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update EmployeeLocation Set isActive=0 WHERE LocationId=@LocationId";
                try
                {

                    var id = connection.Execute(sql, objEmployeeLocation);
                    objEmployeeLocation.LocationId = id;
                    InsertLoginHistory(dataConnection, objEmployeeLocation.CreatedBy, "Delete", "EmployeeLocation", id.ToString(), "0");
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


    }
}
