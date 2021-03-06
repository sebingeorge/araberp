﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkShopRequestItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        /// <summary>
        /// Insert WorkShopRequestItem
        /// </summary>
        /// <param name="model"></param>
        /// <param name="connection"></param>
        /// <param name="trn"></param>
        /// <returns></returns>
        public int InsertWorkShopRequestItem(WorkShopRequestItem model, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"INSERT INTO WorkShopRequestItem(WorkShopRequestId,ItemId,Remarks,Slno,Quantity,isActive,isAddtionalMaterialRequest) VALUES( @WorkShopRequestId ,@ItemId,@Remarks,@Slno,@Quantity,1,@isAddtionalMaterialRequest);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, model, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public int InsertWorkShopRequestAdditionalMaterial(WorkShopRequestItem model, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"INSERT INTO WorkShopRequestItem   (WorkShopRequestId,ItemId,Remarks,Quantity,isActive,isAddtionalMaterialRequest) VALUES( @WorkShopRequestId ,@ItemId,@Remarks,@Quantity,1,1);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, model, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public WorkShopRequestItem GetWorkShopRequestItem(int WorkShopRequestItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from WorkShopRequestItem
                        where WorkShopRequestItemId=@WorkShopRequestItemId";

                var objWorkShopRequestItem = connection.Query<WorkShopRequestItem>(sql, new
                {
                    WorkShopRequestItemId = WorkShopRequestItemId
                }).First<WorkShopRequestItem>();

                return objWorkShopRequestItem;
            }
        }

//        public List<WorkShopRequestItem> GetWorkShopRequestItems()
//        {
//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {
//                string sql = @"select * from WorkShopRequestItem
//                        where isActive=1";

//                var objWorkShopRequestItems = connection.Query<WorkShopRequestItem>(sql).ToList<WorkShopRequestItem>();

//                return objWorkShopRequestItems;
//            }
//        }

        public List<WorkShopRequestItem> WorkShopRequestDT(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
//                string sql = @"SELECT Slno,I.ItemId,I.ItemName,I.PartNo,WRI.Remarks,Quantity,U.UnitName FROM WorkShopRequestItem WRI
//                               INNER JOIN Item I ON I.ItemId=WRI.ItemId
//                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
//                               WHERE WRI.WorkShopRequestId=@WorkShopRequestId";

                string sql = @"SELECT Slno,I.ItemId,I.ItemName,I.PartNo,WRI.Remarks,Quantity,U.UnitName ,WRI.WorkShopRequestItemId,CASE WHEN SI.StoreIssueItemId IS NULL THEN 0 ELSE 1 END isIssueUsed FROM WorkShopRequestItem WRI
                               INNER JOIN Item I ON I.ItemId=WRI.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
							   LEFT JOIN StoreIssueItem SI ON SI.WorkShopRequestItemId=WRI.WorkShopRequestItemId
                               WHERE WRI.WorkShopRequestId=@WorkShopRequestId";

                return connection.Query<WorkShopRequestItem>(sql, new { WorkShopRequestId = WorkShopRequestId }).ToList();
            }
        }

        public int UpdateWorkShopRequestItem(WorkShopRequestItem objWorkShopRequestItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update WorkShopRequestItem Set WorkShopRequestItemRefNo=@WorkShopRequestItemRefNo,WorkShopRequestItemName=@WorkShopRequestItemName OUTPUT INSERTED.WorkShopRequestItemId WHERE WorkShopRequestItemId=@WorkShopRequestItemId";


                var id = connection.Execute(sql, objWorkShopRequestItem);
                return id;
            }
        }

      
        /// <summary>
        /// Insert additional items in workshop request item details table (WorkShopRequestItem table)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertAdditionalWorkshopRequestItem(WorkShopRequestItem model, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string query = @"INSERT INTO WorkShopRequestItem(
                                WorkShopRequestId,
                                Slno,
                                ItemId,
                                Remarks,
                                Quantity,
                                isActive) 
                            VALUES(
                                @WorkShopRequestId,
                                @Slno,
                                @ItemId,
                                @Remarks,
                                @Quantity,
                                1);

                            SELECT CAST(SCOPE_IDENTITY() AS INT)";

                return connection.Query<int>(query, model, txn).First();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}