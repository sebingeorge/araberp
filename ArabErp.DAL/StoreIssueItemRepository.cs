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

        public List<StoreIssueItem> GetStoreIssueDT(int StoreIssueId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT SH.StockPointId,SD.WorkShopRequestItemId,SD.StoreIssueId,IssuedQuantity CurrentIssuedQuantity INTO #STOREISSUE 
                                FROM StoreIssueItem SD
                                INNER JOIN StoreIssue SH ON SH.StoreIssueId=SD.StoreIssueId;

                                SELECT WorkShopRequestId, WorkShopRequestItemId, ItemId, Quantity RequiredQuantity INTO #WORK FROM WorkShopRequestItem;

                                SELECT SI.WorkShopRequestId, SII.WorkShopRequestItemId, WRI.ItemId, SUM(IssuedQuantity) IssuedQuantity INTO #ISSUE 
                                FROM StoreIssueItem SII 
                                INNER JOIN StoreIssue SI ON  SII.StoreIssueId = SI.StoreIssueId 
                                INNER JOIN WorkShopRequestItem WRI ON SII.WorkShopRequestItemId = WRI.WorkShopRequestItemId 
                                GROUP BY WRI.ItemId, SI.WorkShopRequestId, SII.WorkShopRequestItemId;
                
                                SELECT ItemId, ItemName, ItemUnitId INTO #ITEM FROM Item;

                                SELECT UnitId, UnitName INTO #UNIT FROM Unit;

                                SELECT ItemId,StockPointId,Sum(Quantity)StockQuantity INTO #STOCK FROM StockUpdate GROUP BY ItemId,StockPointId

                                SELECT W.WorkShopRequestItemId, ITEM.ItemId, ITEM.ItemName, W.RequiredQuantity, 
                                ISNULL(I.IssuedQuantity, 0) IssuedQuantity, ISNULL((W.RequiredQuantity-ISNULL(I.IssuedQuantity, 0)), 0) PendingQuantity,
                                (ST.StockQuantity+S.CurrentIssuedQuantity)StockQuantity ,S.CurrentIssuedQuantity,UNIT.UnitName
                                FROM #STOREISSUE S
                                INNER JOIN #WORK W ON W.WorkShopRequestItemId=S.WorkShopRequestItemId
                                LEFT JOIN #ISSUE I ON W.WorkShopRequestId = I.WorkShopRequestId AND W.WorkShopRequestItemId = I.WorkShopRequestItemId 
                                LEFT JOIN #ITEM ITEM ON W.ItemId = ITEM.ItemId 
                                LEFT JOIN #UNIT UNIT ON ITEM.ItemUnitId = UNIT.UnitId 
                                LEFT JOIN #STOCK ST ON ST.ItemId= W.ItemId AND ST.StockPointId=S.StockPointId
                                WHERE S.StoreIssueId =@StoreIssueId 

                                DROP TABLE #STOREISSUE;
                                DROP TABLE #ISSUE;
                                DROP TABLE #WORK;
                                DROP TABLE #ITEM;
                                DROP TABLE #UNIT;
                                DROP TABLE #STOCK;";

                return connection.Query<StoreIssueItem>(sql, new { StoreIssueId = StoreIssueId }).ToList();
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



        public List<StoreIssueItem> GetStoreIssueDTPrint(int StoreIssueId,int OrganizationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"       SELECT SH.StockPointId,SD.WorkShopRequestItemId,SD.StoreIssueId,IssuedQuantity CurrentIssuedQuantity,OrganizationId INTO #STOREISSUE 
      FROM StoreIssueItem SD
      INNER JOIN StoreIssue SH ON SH.StoreIssueId=SD.StoreIssueId;

      SELECT WorkShopRequestId, WorkShopRequestItemId, ItemId, Quantity RequiredQuantity INTO #WORK FROM WorkShopRequestItem;

      SELECT SI.WorkShopRequestId, SII.WorkShopRequestItemId, WRI.ItemId, SUM(IssuedQuantity) IssuedQuantity INTO #ISSUE 
      FROM StoreIssueItem SII 
      INNER JOIN StoreIssue SI ON  SII.StoreIssueId = SI.StoreIssueId 
      INNER JOIN WorkShopRequestItem WRI ON SII.WorkShopRequestItemId = WRI.WorkShopRequestItemId 
      GROUP BY WRI.ItemId, SI.WorkShopRequestId, SII.WorkShopRequestItemId;
                
      SELECT ItemId, ItemName, ItemUnitId INTO #ITEM FROM Item;

      SELECT * INTO #Organization FROM Organization;

      SELECT UnitId, UnitName INTO #UNIT FROM Unit;

      SELECT ItemId,StockPointId,Sum(Quantity)StockQuantity INTO #STOCK FROM StockUpdate GROUP BY ItemId,StockPointId

      SELECT W.WorkShopRequestItemId, ITEM.ItemId, ITEM.ItemName, W.RequiredQuantity, 
      ISNULL(I.IssuedQuantity, 0) IssuedQuantity, ISNULL((W.RequiredQuantity-ISNULL(I.IssuedQuantity, 0)), 0) PendingQuantity,
      (ST.StockQuantity+S.CurrentIssuedQuantity)StockQuantity ,S.CurrentIssuedQuantity,UNIT.UnitName
      FROM #STOREISSUE S
      INNER JOIN #WORK W ON W.WorkShopRequestItemId=S.WorkShopRequestItemId
      INNER JOIN #Organization O ON O.OrganizationId=S.OrganizationId
      LEFT JOIN #ISSUE I ON W.WorkShopRequestId = I.WorkShopRequestId AND W.WorkShopRequestItemId = I.WorkShopRequestItemId 
      LEFT JOIN #ITEM ITEM ON W.ItemId = ITEM.ItemId 
      LEFT JOIN #UNIT UNIT ON ITEM.ItemUnitId = UNIT.UnitId 
      LEFT JOIN #STOCK ST ON ST.ItemId= W.ItemId AND ST.StockPointId=S.StockPointId
      WHERE S.StoreIssueId =@StoreIssueId 

                                DROP TABLE #STOREISSUE;
                                DROP TABLE #ISSUE;
                                DROP TABLE #WORK;
                                DROP TABLE #ITEM;
                                DROP TABLE #UNIT;
                                DROP TABLE #STOCK;
                                DROP TABLE #Organization;";

                return connection.Query<StoreIssueItem>(sql, new { StoreIssueId = StoreIssueId, OrganizationId = OrganizationId }).ToList();
            }
        }
    }
}