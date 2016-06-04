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
        public int InsertStoreIssueItem(StoreIssueItem objStoreIssueItem, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string sql = @"INSERT INTO StoreIssueItem(
                                WorkShopRequestItemId,
                                StoreIssueId,
                                IssuedQuantity,
                                isActive) 
                            Values (
                                @WorkShopRequestItemId,
                                @StoreIssueId,
                                @CurrentIssuedQuantity,
                                1);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var id = connection.Query<int>(sql, objStoreIssueItem, txn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
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