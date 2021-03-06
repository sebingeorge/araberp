﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class OpeningStockRepository : BaseRepository
    {
        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");

        public OpeningStockRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public IEnumerable<Dropdown> FillStockpoint()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT StockPointId Id,StockPointName Name FROM Stockpoint").ToList();
            }
        }

        public IEnumerable<Dropdown> FillItem()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("SELECT ItemId Id,ItemName Name FROM Item").ToList();
            }
        }

        public int DeleteOpeningStock(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete OpeningStock  OUTPUT DELETED.StockPointId WHERE StockPointId=@StockPointId";
                var id = connection.Execute(sql, objOpeningStock);
                InsertLoginHistory(dataConnection, objOpeningStock.CreatedBy, "Update", "Opening Stock", id.ToString(), "0");
                return id;
            }
        }

        public int InsertOpeningStock(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                int id = 0;
                foreach (var item in objOpeningStock.OpeningStockItem)
                {
                    if (item.ItemId == null || item.ItemId == 0) continue;
                    string sql = @"insert  into OpeningStock(StockPointId,ItemId,Quantity,CreatedBy,CreatedDate,OrganizationId) 
                           Values (@stockpointId,@ItemId,@Quantity,@CreatedBy,@CreatedDate,@OrganizationId);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

                    //id = connection.Query<int>(sql, objOpeningStock).Single();

                    id = connection.Query<int>(sql, new
                    {
                        stockpointId = objOpeningStock.stockpointId,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        CreatedBy = objOpeningStock.CreatedBy,
                        CreatedDate = objOpeningStock.CreatedDate,
                        OrganizationId = objOpeningStock.OrganizationId
                    }).Single();
                    InsertLoginHistory(dataConnection, objOpeningStock.CreatedBy, "Create", "Opening Stock", id.ToString(), "0");
                }


                return id;

            }
        }

        public int DeleteStockUpdate(OpeningStock objOpeningStock)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete StockUpdate  OUTPUT DELETED.StockPointId WHERE StockPointId=@StockPointId AND StockType='OpeningStock'";
                var id = connection.Execute(sql, objOpeningStock);
                return id;
            }
        }

        public int InsertStockUpdate(OpeningStock objOpeningStock, IDbConnection connection, IDbTransaction txn)
        {
            //using (IDbConnection connection = OpenConnection(dataConnection))
            //{
            int id = 0;
            foreach (var item in objOpeningStock.OpeningStockItem)
            {
                if (item.ItemId == null || item.ItemId == 0) continue;
                string sql = @"insert  into StockUpdate(StockPointId,stocktrnDate,ItemId,Quantity,
                                 StockType,StockInOut,CreatedBy,CreatedDate,OrganizationId) 
                                 Values (@stockpointId,@CreatedDate,@ItemId,@Quantity,'OpeningStock','IN',
                                 @CreatedBy,@CreatedDate,@OrganizationId);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";

                //id = connection.Query<int>(sql, objOpeningStock).Single();

                id = connection.Query<int>(sql, new
                {
                    stockpointId = objOpeningStock.stockpointId,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity,
                    CreatedBy = objOpeningStock.CreatedBy,
                    CreatedDate = objOpeningStock.CreatedDate,
                    OrganizationId = objOpeningStock.OrganizationId
                }, txn).Single();

            }


            return id;

            //}
        }

        public IEnumerable<OpeningStockItem> GetItem(int? StockPointId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query
                //string sql = @"SELECT ItemId,Quantity  FROM OpeningStock WHERE StockPointId=@StockPointId"; 
                #endregion

                string query = @"SELECT DISTINCT OS.OpeningStockId, OS.ItemId, Quantity, I.PartNo, CASE WHEN IB.ItemBatchId IS NOT NULL THEN 1 ELSE 0 END AS isUsed
                                FROM OpeningStock OS
	                                LEFT JOIN ItemBatch IB ON OS.OpeningStockId = IB.OpeningStockId
									LEFT JOIN Item I ON OS.ItemId = I.ItemId
                                WHERE StockPointId = @StockPointId";

                //var objItem = connection.Query<OpeningStock>(sql, new
                //{
                //    StockPointId = StockPointId
                //}).OpeningStockItem();

                return connection.Query<OpeningStockItem>(query, new
                {
                    StockPointId = StockPointId
                }).ToList();
            }
        }
        public List<OpeningStockItem> GetItem()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query
                //string sql = @"SELECT ItemId,Quantity  FROM OpeningStock WHERE StockPointId=@StockPointId"; 
                #endregion

                string query = @"select 0 OpeningStockId,I.ItemId,ItemName,0 Quantity,PartNo,0 isUsed from Item I LEFT join OpeningStock O on I.ItemId=O.ItemId  where OpeningStockId is null
                                 union all 
                                 SELECT DISTINCT OS.OpeningStockId, OS.ItemId,I.ItemName, Quantity, I.PartNo, CASE WHEN IB.ItemBatchId IS NOT NULL THEN 1 ELSE 0 END AS isUsed
                                 FROM OpeningStock OS
	                             LEFT JOIN ItemBatch IB ON OS.OpeningStockId = IB.OpeningStockId
								 left JOIN Item I ON OS.ItemId = I.ItemId";


                //var objItem = connection.Query<OpeningStock>(sql, new
                //{
                //    StockPointId = StockPointId
                //}).OpeningStockItem();

                return connection.Query<OpeningStockItem>(query).ToList();
            }
        }
        //        public int SaveStockUpdate(OpeningStock model)
        //        {
        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //            int id = 0;
        //            foreach (var item in model.OpeningStockItem)
        //            {
        //                if ( item.OpeningStockId > 0) continue;
        //                string sql = @"insert  into StockUpdate(StockPointId,stocktrnDate,ItemId,Quantity,
        //                                 StockType,StockInOut,CreatedBy,CreatedDate,OrganizationId) 
        //                                 Values (@stockpointId,@CreatedDate,@ItemId,@Quantity,'OpeningStock','IN',
        //                                 @CreatedBy,@CreatedDate,@OrganizationId);
        //                                 SELECT CAST(SCOPE_IDENTITY() as int)";

        //                //id = connection.Query<int>(sql, item).Single();

        //                id = connection.Query<int>(sql, new
        //                {
        //                    stockpointId = model.stockpointId,
        //                    ItemId = item.ItemId,
        //                    Quantity = item.Quantity,
        //                    CreatedBy = model.CreatedBy,
        //                    CreatedDate = model.CreatedDate,
        //                    OrganizationId = model.OrganizationId
        //                }).Single();

        //            }


        //            return id;

        //            }
        //        }
        public int SaveOpeningStock(OpeningStock model)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string sql = string.Empty;
                    foreach (var item in model.OpeningStockItem)
                    {
                        if (item.OpeningStockId == 0 && item.Quantity > 0)
                        {
                            sql = @"insert  into OpeningStock(ItemId,Quantity,
                                 CreatedBy,CreatedDate,OrganizationId,StockpointId) 
                                 Values (@ItemId,@Quantity,
                                 @CreatedBy,@CreatedDate,@OrganizationId,@StockpointId);
                                
                      
                                 insert  into StockUpdate(StockPointId,stocktrnDate,ItemId,Quantity,
                                 StockType,StockInOut,CreatedBy,CreatedDate,OrganizationId) 
                                 Values (@stockpointId,@CreatedDate,@ItemId,@Quantity,'OpeningStock','IN',
                                 @CreatedBy,@CreatedDate,@OrganizationId);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";
                      

                            int ObjStandardRate = connection.Query<int>(sql, new
                            {

                                StockPointId = model.stockpointId,
                                ItemId = item.ItemId,
                                Quantity = item.Quantity,
                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate,
                                OrganizationId = model.OrganizationId
                            }, txn).First();
                        }
                        else
                        {
                            if (!item.isUsed)
                            {
                                sql = @"update OpeningStock set ItemId=@ItemId,Quantity=@Quantity,CreatedBy= @CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId,StockpointId=@StockpointId where OpeningStockId=@OpeningStockId;";

                                  int ObjStandardRate = connection.Execute(sql, new
                                {
                                    StockPointId = model.stockpointId,
                                    OpeningStockId = item.OpeningStockId,
                                    ItemId = item.ItemId,
                                    Quantity = item.Quantity,
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = model.CreatedDate,
                                    OrganizationId = model.OrganizationId
                                }, txn);

                                sql = @"update StockUpdate set StockPointId=@StockPointId,stocktrnDate=@CreatedDate,ItemId=@ItemId,Quantity=@Quantity,CreatedBy= @CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId where StockType='OpeningStock' and StockInOut='IN' and ItemId=@ItemId and StockpointId=@StockpointId ;";
                          
                                int ObjReturn = connection.Execute(sql, new
                                {
                                    StockPointId = model.stockpointId,
                                    OpeningStockId = item.OpeningStockId,
                                    ItemId = item.ItemId,
                                    Quantity = item.Quantity,
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = model.CreatedDate,
                                    OrganizationId = model.OrganizationId
                                }, txn);
                                if (ObjReturn == 0 && item.Quantity > 0)
                                {

                                    sql = @" insert  into StockUpdate(StockPointId,stocktrnDate,ItemId,Quantity,
                                 StockType,StockInOut,CreatedBy,CreatedDate,OrganizationId) 
                                 Values (@stockpointId,@CreatedDate,@ItemId,@Quantity,'OpeningStock','IN',
                                 @CreatedBy,@CreatedDate,@OrganizationId);
                                 SELECT CAST(SCOPE_IDENTITY() as int)";
                      

                            int Objstckup = connection.Query<int>(sql, new
                            {

                                StockPointId = model.stockpointId,
                                ItemId = item.ItemId,
                                Quantity = item.Quantity,
                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate,
                                OrganizationId = model.OrganizationId
                            }, txn).First();
                                }

                            }
                        }

                    }
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }

                txn.Commit();
                return 1;
            }
        }
        public IEnumerable<OpeningStockReport> GetClosingStockData(int stockPointId, int itemCategoryId, string itemId, int itmGroup, int itmSubgroup, int OrganizationId, string partno)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT I.ItemName,ISnull(I.PartNo,'-')PartNo,Sum(Quantity)Quantity,U.UnitName,IG.ItemGroupName,IGS.ItemSubGroupName FROM StockUpdate S
                                INNER JOIN Item I ON I.ItemId=S.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
								INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                                WHERE StockType='OpeningStock'AND  I.ItemName LIKE '%'+@itmid+'%'   and isnull(I.PartNo,'') like '%'+@partno+'%' AND 
                                I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) and
                                I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId) and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                                and S.OrganizationId=@OrganizationId AND 
                                S.StockPointId = ISNULL(NULLIF(@stkid, 0), S.StockPointId) 
                                GROUP BY  I.ItemName,I.PartNo,U.UnitName,IG.ItemGroupName,IGS.ItemSubGroupName
                                ORDER BY I.ItemName";
                return connection.Query<OpeningStockReport>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, itmGroup = itmGroup, itmSubgroup = itmSubgroup, partno = partno }).ToList();
            }
        }

        public int UpdateOpeningStock(OpeningStock model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    var list = (from OpeningStockItem p in model.OpeningStockItem
                                where !p.isUsed
                                select p).ToList();

                    var openingStockId = (from OpeningStockItem p in list
                                          where p.OpeningStockId > 0
                                          select p.OpeningStockId).ToList();

                    IEnumerable<OpeningStockItem> original_list = GetItem(model.stockpointId);
                    var original_id = (from OpeningStockItem p in original_list
                                       select p.OpeningStockId).ToList();

                    var excluded_id = (from int p in original_id
                                       where !model.OpeningStockItem.Select(x => x.OpeningStockId).Contains(p)
                                       select p).ToList();

                    string query = @"DELETE FROM OpeningStock WHERE OpeningStockId IN @openingStockId";
                    connection.Execute(query, new { openingStockId = openingStockId }, txn);
                    query = @"DELETE FROM OpeningStock WHERE OpeningStockId IN @excluded_id";
                    connection.Execute(query, new { excluded_id = excluded_id }, txn);

                    query = @"INSERT INTO OpeningStock
                                (
	                                ItemId,
	                                Quantity,
	                                CreatedBy,
	                                CreatedDate,
	                                OrganizationId,
	                                StockPointId
                                )
                                VALUES
                                (
                                    @ItemId,
	                                @Quantity,
	                                @CreatedBy,
	                                @CreatedDate,
	                                @OrganizationId,
	                                @StockPointId
                                )";

                    var id = 0;
                    foreach (var item in list)
                    {
                        id = connection.Execute(query, new
                                {
                                    ItemId = item.ItemId,
                                    Quantity = item.Quantity,
                                    CreatedBy = model.CreatedBy,
                                    CreatedDate = model.CreatedDate,
                                    OrganizationId = model.OrganizationId,
                                    StockpointId = model.stockpointId
                                }, txn);
                    }

                    query = @"DELETE FROM StockUpdate WHERE StockpointId = @StockpointId AND StockType='OpeningStock'";

                    connection.Execute(query, new { StockpointId = model.stockpointId }, txn);

                    InsertStockUpdate(model, connection, txn);

                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", typeof(OpeningStock).Name, id.ToString(), model.OrganizationId.ToString());

                    txn.Commit();

                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }

            }
        }

        public IEnumerable<OpeningStockReport> GetClosingStockDataDTPrint(int stockPointId, int itemCategoryId, string itemId, int itmGroup, int itmSubgroup, int OrganizationId, string partno)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT I.ItemName,ISnull(I.PartNo,'-')PartNo,Sum(Quantity)Quantity,U.UnitName,IG.ItemGroupName,IGS.ItemSubGroupName FROM StockUpdate S
                                INNER JOIN Item I ON I.ItemId=S.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
								INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                                WHERE StockType='OpeningStock'AND  I.ItemName LIKE '%'+@itmid+'%'and isnull(I.PartNo,'') like '%'+@partno+'%' AND 
                                I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) and
                                I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId) and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                                and S.OrganizationId=@OrganizationId AND 
                                S.StockPointId = ISNULL(NULLIF(@stkid, 0), S.StockPointId) 
                                GROUP BY  I.ItemName,I.PartNo,U.UnitName,IG.ItemGroupName,IGS.ItemSubGroupName
                                ORDER BY I.ItemName";
                return connection.Query<OpeningStockReport>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, itmGroup = itmGroup, itmSubgroup = itmSubgroup, OrganizationId = OrganizationId, partno = partno }).ToList();
            }
        }

    }
}