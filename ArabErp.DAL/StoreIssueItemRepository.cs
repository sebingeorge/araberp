using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StoreIssueItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertStoreIssueItem(StoreIssueItem objStoreIssueItem)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into StoreIssueItem(WorkShopRequestItemId,StoreIssueId,IssuedQuantity,OrganizationId,isActive) Values (@WorkShopRequestItemId,@StoreIssueId,@IssuedQuantity,@OrganizationId,@isActive);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objStoreIssueItem).Single();
                return id;
            }
        }


        public StoreIssueItem GetStoreIssueItem(int StoreIssueItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StoreIssueItem
                        where StoreIssueItemId=@StoreIssueItemId";

                var objStoreIssueItem = connection.Query<StoreIssueItem>(sql, new
                {
                    StoreIssueItemId = StoreIssueItemId
                }).First<StoreIssueItem>();

                return objStoreIssueItem;
            }
        }

        public List<StoreIssueItem> GetStoreIssueItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from StoreIssueItem
                        where isActive=1";

                var objStoreIssueItems = connection.Query<StoreIssueItem>(sql).ToList<StoreIssueItem>();

                return objStoreIssueItems;
            }
        }



        public int DeleteStoreIssueItem(Unit objStoreIssueItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StoreIssueItem  OUTPUT DELETED.StoreIssueItemId WHERE StoreIssueItemId=@StoreIssueItemId";


                var id = connection.Execute(sql, objStoreIssueItem);
                return id;
            }
        }


    }
}