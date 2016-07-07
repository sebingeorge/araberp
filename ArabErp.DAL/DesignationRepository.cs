using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class DesignationRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public Designation InsertDesignation(Designation objDesignation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Designation();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO Designation(DesignationRefNo,DesignationName,CreatedBy,CreatedDate,OrganizationId) 
                               VALUES(@DesignationRefNo,@DesignationName,@CreatedBy,getDate(),@OrganizationId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Designation).Name, "0", 1);
                    objDesignation.DesignationRefNo = "D/" + internalid;

                    int id = connection.Query<int>(sql, objDesignation, trn).Single();
                    objDesignation.DesignationId = id;
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objDesignation.DesignationId = 0;
                    objDesignation.DesignationRefNo = null;

                }
                return objDesignation;
            }
        }

        public IEnumerable<Designation> FillDesignationList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Designation>("SELECT DesignationId,DesignationRefNo,DesignationName FROM Designation WHERE isActive=1").ToList();
            }
        }

        public Designation GetDesignation(int DesignationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Designation
                        where DesignationId=@DesignationId";

                var objDesignation = connection.Query<Designation>(sql, new
                {
                    DesignationId = DesignationId
                }).First<Designation>();

                return objDesignation;
            }
        }

        public List<Designation> GetDesignation()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Designation
                        where OrganizationId>0";

                var objDesignation = connection.Query<Designation>(sql).ToList<Designation>();

                return objDesignation;
            }
        }

        public Designation UpdateDesignation(Designation objDesignation)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Designation Set DesignationRefNo=@DesignationRefNo,DesignationName=@DesignationName OUTPUT INSERTED.DesignationId WHERE DesignationId=@DesignationId";

                var id = connection.Execute(sql, objDesignation);
                return objDesignation;
            }
        }

        public int DeleteDesignation(Designation objDesignation)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update Designation Set isActive=0 WHERE DesignationId=@DesignationId";
                try
                {

                    var id = connection.Execute(sql, objDesignation);
                    objDesignation.DesignationId = id;
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


        public string GetRefNo(Designation objDesignation)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Designation();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Designation).Name, "0", 0);
                    RefNo = "D/" + internalid;
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
