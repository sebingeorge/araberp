using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SalesTargetRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public string ConnectionString()
        {
            return dataConnection;
        }

        public SalesTarget InsertSalesTarget(SalesTarget salestarget)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new SalesTarget();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO SalesTarget (SalesTargetRefNo,MonthId,WorkDescriptionId,Target,OrganizationId,FyId,CreatedBy,CreatedDate,isActive) 
                              VALUES(@SalesTargetRefNo,@MonthId,@WorkDescriptionId,@Target,@OrganizationId,@FyId,@CreatedBy,@CreatedDate,1);
                              SELECT CAST(SCOPE_IDENTITY() as int)";



                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesTarget).Name, "0", 1);
                    salestarget.SalesTargetRefNo = "ST/" + internalid;

                    int id = connection.Query<int>(sql, salestarget, trn).Single();
                    salestarget.SalesTargetId = id;
                    InsertLoginHistory(dataConnection, salestarget.CreatedBy, "Create", "SalesTarget", internalid.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    salestarget.SalesTargetId = 0;
                    salestarget.SalesTargetRefNo = null;

                }
                return (salestarget);
            }
        }

        public IEnumerable<Dropdown>FillMonth()
         {
        using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT MonthId Id,MonthName Name  FROM Month").ToList();
            
             }
          }

        public IEnumerable<Dropdown> FillWorkDescription()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT WorkDescriptionId Id,WorkDescrShortName  Name FROM WorkDescription").ToList();

            }
        }
        public SalesTarget GetSalesTarget(int SalesTargetId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT * FROM SalesTarget
                        WHERE SalesTargetId=@SalesTargetId";

                var objSalesTarget = connection.Query<SalesTarget>(sql, new
                {
                    SalesTargetId = SalesTargetId
                }).First<SalesTarget>();

                return objSalesTarget;
            }
        }

        public IEnumerable<SalesTarget> FillSalesTargetList()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<SalesTarget>("select SalesTarget.SalesTargetId, SalesTargetRefNo,MonthName,WorkDescrShortName,OrganizationName,Target  from SalesTarget inner join Month on SalesTarget.MonthId=Month.MonthId inner join WorkDescription on SalesTarget.WorkDescriptionId=WorkDescription.WorkDescriptionId inner join Organization on Organization.OrganizationId=SalesTarget.OrganizationId  WHERE SalesTarget.isActive=1").ToList();
            }
        }
        public SalesTarget UpdateSalesTarget(SalesTarget ObjSalesTarget)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SalesTarget Set SalesTargetRefNo=@SalesTargetRefNo,MonthId=@MonthId,WorkDescriptionId=@WorkDescriptionId,Target=@Target,OrganizationId=@OrganizationId,FyId=@FyId,CreatedBy=@CreatedBy,CreatedDate=@CreatedDate OUTPUT INSERTED.OrganizationId WHERE SalesTargetId=@SalesTargetId";


                var id = connection.Execute(sql, ObjSalesTarget);
                InsertLoginHistory(dataConnection, ObjSalesTarget.CreatedBy, "Update", "SalesTarget", id.ToString(), "0");
                return ObjSalesTarget;
            }
        }


//        public SalesTarget GetSalesTarget(int SalesTargetId)
//        {

//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {
//                string sql = @"SELECT * FROM SalesTarget
//                        WHERE SalesTargetId=@SalesTargetId";

//                var objSalesTarget = connection.Query<SalesTarget>(sql, new
//                {
//                    SalesTargetId = SalesTargetId
//                }).First<SalesTarget>();

//                return objSalesTarget;
//            }
//        }
        public int DeleteSalesTarget(SalesTarget objSalesTarget)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update SalesTarget Set isActive=0 WHERE SalesTargetId=@SalesTargetId";
                try
                {

                    var id = connection.Execute(sql, objSalesTarget);
                    objSalesTarget.SalesTargetId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objSalesTarget.CreatedBy, "Delete", "SalesTarget", id.ToString(), "0");
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
        public string GetRefNo(SalesTarget objSalesTarget)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new SalesTarget();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SalesTarget).Name, "0", 0);
                    RefNo = "ST/" + internalid;
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