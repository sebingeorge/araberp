using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
  public  class CostingParametersRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public CostingParameters InsertBox(CostingParameters objCosting)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new CostingParameters();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO CostingParameters (Description,CostingRefNo,CreatedBy,CreatedDate,OrganizationId,isActive) 
                              VALUES(@Description,@CostingRefNo,@CreatedBy,@CreatedDate,@OrganizationId,1);
                              SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(CostingParameters).Name, "0", 1);
                    objCosting.CostingRefNo = "B/" + internalid;

                    int id = connection.Query<int>(sql, objCosting, trn).Single();
                    objCosting.CostingId = id;
                    InsertLoginHistory(dataConnection, objCosting.CreatedBy, "Create", "Box", internalid.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objCosting.CostingId = 0;
                    objCosting.CostingRefNo = null;

                }
                return objCosting;
            }
        }
       
        public string GetRefNo(CostingParameters objCosting)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new CostingParameters();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(CostingParameters).Name, "0", 0);
                    RefNo = "B/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }

        public IEnumerable<CostingParameters> FillBox()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<CostingParameters>("select CostingId,CostingRefNo,Description  from CostingParameters where isActive=1").ToList();
            }
        }

        public CostingParameters UpdateBox(CostingParameters objCosting)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update CostingParameters Set CostingRefNo=@CostingRefNo,Description=@Description OUTPUT INSERTED.CostingId WHERE CostingId=@CostingId";
                var id = connection.Execute(sql, objCosting);
                InsertLoginHistory(dataConnection, objCosting.CreatedBy, "Update", "CostingParameters", objCosting.CostingId.ToString(), "0");
                return objCosting;
            }
        }

        public CostingParameters GetCosting(int CostingId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from CostingParameters
                        where CostingId=@CostingId";

                var objBox = connection.Query<CostingParameters>(sql, new
                {
                    CostingId = CostingId
                }).First<CostingParameters>();

                return objBox;
            }
        }
        public int DeleteBox(CostingParameters objCosting)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update CostingParameters Set isActive=0 WHERE CostingId=@CostingId";
                try
                {

                    var id = connection.Execute(sql, objCosting);
                    objCosting.CostingId = id;
                    InsertLoginHistory(dataConnection, objCosting.CreatedBy, "Delete", "CostingId", objCosting.CostingId.ToString(), "0");
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
