using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class BoxRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public Box InsertBox(Box objBox)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Box();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO Box (BoxRefNo,BoxName,CreatedBy,CreatedDate,OrganizationId) 
                              VALUES(@BoxRefNo,@BoxName,@CreatedBy,@CreatedDate,@OrganizationId);
                              SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Box).Name, "0", 1);
                    objBox.BoxRefNo = "B/" + internalid;

                    int id = connection.Query<int>(sql, objBox, trn).Single();
                    objBox.BoxId = id;
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objBox.BoxId = 0;
                    objBox.BoxRefNo = null;

                }
                return objBox;
            }
        }

        public Box GetBox(int BoxId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Box
                        where BoxId=@BoxId";

                var objBox = connection.Query<Box>(sql, new
                {
                    BoxId = BoxId
                }).First<Box>();

                return objBox;
            }
        }

        public List<Box> GetBoxs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Box
                        where isActive=1";

                var objBoxs = connection.Query<Box>(sql).ToList<Box>();

                return objBoxs;
            }
        }
        public IEnumerable<Box> FillBox()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Box>("select BoxId,BoxRefNo,BoxName  from Box where isActive=1").ToList();
            }
        }

        public Box UpdateBox(Box objBox)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update Box Set BoxRefNo=@BoxRefNo,BoxName=@BoxName OUTPUT INSERTED.BoxId WHERE BoxId=@BoxId";


                var id = connection.Execute(sql, objBox);
                return objBox;
            }
        }

        public int DeleteBox(Box objBox)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update Box Set isActive=0 WHERE BoxId=@BoxId";
                try
                {

                    var id = connection.Execute(sql, objBox);
                    objBox.BoxId = id;
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


        public string GetRefNo(Box objBox)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new Box();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Box).Name, "0", 0);
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
