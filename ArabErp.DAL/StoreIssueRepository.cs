using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StoreIssueRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertStoreIssue(StoreIssue objStoreIssue)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into StoreIssue(StoreIssueDate,WorkShopRequestId,EmployeeId,CreatedBy,CreatedDate,OrganizationId,isActive) Values (@StoreIssueDate,@WorkShopRequestId,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId,@isActive);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objStoreIssue).Single();
                return id;
            }
        }


        public StoreIssue GetStoreIssue(int StoreIssueId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StoreIssue
                        where StoreIssueId=@StoreIssueId";

                var objStoreIssue = connection.Query<StoreIssue>(sql, new
                {
                    StoreIssueId = StoreIssueId
                }).First<StoreIssue>();

                return objStoreIssue;
            }
        }

        public List<StoreIssue> GetStoreIssues()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StoreIssue
                        where isActive=1";

                var objStoreIssues = connection.Query<StoreIssue>(sql).ToList<StoreIssue>();

                return objStoreIssues;
            }
        }



        public int DeleteStoreIssue(Unit objStoreIssue)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StoreIssue  OUTPUT DELETED.StoreIssueId WHERE StoreIssueId=@StoreIssueId";


                var id = connection.Execute(sql, objStoreIssue);
                return id;
            }
        }


    }
}
