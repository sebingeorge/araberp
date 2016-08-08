using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Web;
//using ArabErp.Web.Models;
using System.Collections;

namespace ArabErp.DAL
{
    public class BayRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public Bay GetBay(int BayId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Bay
                        where BayId=@BayId";

                var objBay = connection.Query<Bay>(sql, new
                {
                    BayId = BayId
                }).First<Bay>();

                return objBay;
            }
        }

        public List<Bay> GetBays()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select BayId,BayRefNo,BayName, CASE WHEN BayType=0 THEN 'Transport' ELSE 'Bus' END AS BayType from Bay where isActive=1";

                var objBays = connection.Query<Bay>(sql).ToList<Bay>();

                return objBays;
            }
        }

        public Bay InsertBay(Bay objBay)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Bay();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO Bay (BayRefNo,BayName,BayType,CreatedBy,CreatedDate,OrganizationId) 
                               VALUES(@BayRefNo,@BayName,@BayType,@CreatedBy,@CreatedDate,@OrganizationId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Bay).Name, "0", 1);
                    objBay.BayRefNo = "B/" + internalid;

                    int id = connection.Query<int>(sql, objBay, trn).Single();
                    objBay.BayId = id;
                    //connection.Dispose();
                    InsertLoginHistory(dataConnection, objBay.CreatedBy, "Create", "Bay", internalid.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objBay.BayId = 0;
                    objBay.BayRefNo = null;

                }
                return objBay;
            }
        }

        public Bay UpdateBay(Bay objBay)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Bay Set BayRefNo=@BayRefNo,BayName=@BayName,BayType=@BayType OUTPUT INSERTED.BayId WHERE BayId=@BayId";

                var id = connection.Execute(sql, objBay);
                InsertLoginHistory(dataConnection, objBay.CreatedBy, "Update", "Bay", objBay.BayId.ToString(), "0");
                return objBay;
            }
        }

        public int DeleteBay(Bay objBay)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update Bay Set isActive=0 WHERE BayId=@BayId";
                try
                {

                    var id = connection.Execute(sql, objBay);
                    objBay.BayId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objBay.CreatedBy, "Delete", "Bay", objBay.BayId.ToString(), "0");
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

        public string GetRefNo(Bay objBay)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Bay();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Bay).Name, "0", 0);
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

    

    }
}
