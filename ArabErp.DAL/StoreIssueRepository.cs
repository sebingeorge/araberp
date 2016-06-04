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
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string sql = @"INSERT INTO StoreIssue(
                                StoreIssueRefNo, 
                                StoreIssueDate, 
                                WorkShopRequestId, 
                                EmployeeId,
                                Remarks, 
                                CreatedBy, 
                                CreatedDate, 
                                OrganizationId, 
                                isActive) 
                            VALUES (
                                @StoreIssueRefNo, 
                                @StoreIssueDate,
                                @WorkShopRequestId,
                                @EmployeeId,
                                @Remarks,
                                @CreatedBy,
                                @CreatedDate,
                                @OrganizationId, 
                                1);
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    var id = connection.Query<int>(sql, objStoreIssue, txn).Single();
                    foreach (var item in objStoreIssue.Items)
                    {
                        item.StoreIssueId = id;
                        new StoreIssueItemRepository().InsertStoreIssueItem(item, connection, txn);
                        new StockUpdateRepository().InsertStockUpdate(new StockUpdate
                        {
                            OrganizationId = objStoreIssue.OrganizationId,
                            CreatedBy = objStoreIssue.CreatedBy,
                            CreatedDate = objStoreIssue.CreatedDate,
                            StockPointId = objStoreIssue.StockpointId,
                            StockPointType = "StoreIssue",
                            StockPointInOut = "OUT",
                            ItemId = item.ItemId,
                            Quantity = item.CurrentIssuedQuantity
                        }, connection, txn);
                    }
                    txn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return 0;
                }
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

        /// <summary>
        /// Return items in a workshop request that are yet to receive the required quantity
        /// </summary>
        /// <param name="workshopRequestId"></param>
        /// <returns></returns>
        public IEnumerable<StoreIssueItem> PendingWorkshopRequestDetails(int workshopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<StoreIssueItem>(@"SELECT WorkShopRequestId, WorkShopRequestItemId, ItemId, Quantity RequiredQuantity INTO #WORK FROM WorkShopRequestItem;
                SELECT SI.WorkShopRequestId, SII.WorkShopRequestItemId, WRI.ItemId, SUM(IssuedQuantity) IssuedQuantity INTO #ISSUE FROM StoreIssueItem SII INNER JOIN StoreIssue SI ON  SII.StoreIssueId = SI.StoreIssueId INNER JOIN WorkShopRequestItem WRI ON SII.WorkShopRequestItemId = WRI.WorkShopRequestItemId GROUP BY WRI.ItemId, SI.WorkShopRequestId, SII.WorkShopRequestItemId;
                SELECT ItemId, ItemName, ItemUnitId INTO #ITEM FROM Item;
				SELECT UnitId, UnitName INTO #UNIT FROM Unit;
                SELECT /*W.WorkShopRequestId,*/ W.WorkShopRequestItemId, ITEM.ItemId, ITEM.ItemName, UNIT.UnitName, W.RequiredQuantity, ISNULL(I.IssuedQuantity, 0) IssuedQuantity, ISNULL((W.RequiredQuantity-ISNULL(I.IssuedQuantity, 0)), 0) PendingQuantity FROM #WORK W LEFT JOIN #ISSUE I ON W.WorkShopRequestId = I.WorkShopRequestId AND W.WorkShopRequestItemId = I.WorkShopRequestItemId INNER JOIN #ITEM ITEM ON W.ItemId = ITEM.ItemId INNER JOIN #UNIT UNIT ON ITEM.ItemUnitId = UNIT.UnitId WHERE W.WorkShopRequestId = 9 AND W.RequiredQuantity > ISNULL(I.IssuedQuantity, 0);
                DROP TABLE #ISSUE;
                DROP TABLE #WORK;
                DROP TABLE #ITEM;
                DROP TABLE #UNIT;", new { WorkShopRequestId = workshopRequestId }).ToList();
            }
        }
    }
}
