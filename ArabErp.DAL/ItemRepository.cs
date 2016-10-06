﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class ItemRepository :BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public List<Dropdown> FillItemSubGroup(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
            var param = new DynamicParameters();
            
            //return connection.Query<Dropdown>("x",
            // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
            return connection.Query<Dropdown>("select ItemSubGroupId Id,ItemSubGroupName Name from ItemSubGroup WHERE isActive=1 AND ItemGroupId=@ID", new { ID = Id }).ToList();
            }

        }
        public List<Dropdown> FillItemCategory()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                //return connection.Query<Dropdown>("x",
                // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
                return connection.Query<Dropdown>("select itmCatId Id,CategoryName Name from ItemCategory WHERE isActive=1").ToList();

            }
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="Id"></param>
       /// <returns></returns>
        public List<Dropdown> FillItemGroup( int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                //return connection.Query<Dropdown>("x",
                // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
                return connection.Query<Dropdown>("select ItemGroupId Id,ItemGroupName Name from ItemGroup WHERE isActive=1 AND ItemCategoryId=@ID", new { ID = Id }).ToList();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Dropdown> FillUnit()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select UnitId Id,UnitName Name from Unit WHERE isActive=1").ToList();
            }
             }
        public List<Dropdown> FillItem()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select ItemId Id,ItemName Name from Item where isActive=1").ToList();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="OItem"></param>
        /// <returns></returns>
        public ArrayList SaveItem(Item OItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();

                param.Add("@PartNo", OItem.PartNo, dbType: DbType.String);
                param.Add("@ItemName", OItem.ItemName, dbType: DbType.String);
                param.Add("@ItemPrintName", OItem.ItemPrintName, dbType: DbType.String);
                param.Add("@ItemShortName", OItem.ItemShortName, dbType: DbType.String);
                param.Add("@ItemGroupId", OItem.ItemGroupId, dbType: DbType.Int32);
                param.Add("@ItemSubGroupId", OItem.ItemSubGroupId, dbType: DbType.Int32);
                param.Add("@ItemCategoryId", OItem.ItemCategoryId, dbType: DbType.Int32);
                param.Add("@ItemUnitId", OItem.ItemUnitId, dbType: DbType.Int32);
                param.Add("@CommodityId", OItem.CommodityId, dbType: DbType.Int32);
                param.Add("@MinLevel", OItem.MinLevel, dbType: DbType.Int32);
                param.Add("@ReorderLevel", OItem.ReorderLevel, dbType: DbType.Int32);
                param.Add("@MaxLevel", OItem.MaxLevel, dbType: DbType.Int32);

                param.Add("@RESULT", direction: ParameterDirection.Output, dbType: DbType.String, size: 50);
                param.Add("@RID", direction: ParameterDirection.Output, dbType: DbType.Int32);
                connection.Query("USP_SAVE_ITEM", param, commandType: CommandType.StoredProcedure);

                ArrayList result = new ArrayList()
                { 
                    param.Get<string>("@RESULT"), 
                    param.Get<int>("@RID") 
                };
                return result;
            }
        }
        public Item InsertItem(Item objItem)
        {

            
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new Item();
                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"insert  into Item(ItemRefNo,PartNo,ItemName,ItemPrintName,ItemShortName,ItemGroupId,ItemSubGroupId,
                                                ItemCategoryId,ItemUnitId,MinLevel,ReorderLevel,MaxLevel,BatchRequired,StockRequired,
                                                CriticalItem,FreezerUnit,Box,OrganizationId,CreatedBy,CreatedDate) Values
                                                (@ItemRefNo,@PartNo,@ItemName,@ItemPrintName,@ItemShortName,@ItemGroupId,@ItemSubGroupId,
                                                @ItemCategoryId,@ItemUnitId,@MinLevel,@ReorderLevel,@MaxLevel,@BatchRequired,@StockRequired,
                                                @CriticalItem,@FreezerUnit,@Box,@OrganizationId,@CreatedBy,@CreatedDate);
                                                SELECT CAST(SCOPE_IDENTITY() as int)";
               
                
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Item).Name, "0", 1);
                    objItem.ItemRefNo = "ITM/"+internalid;
                    
                    int id = connection.Query<int>(sql, objItem,trn).Single();
                    objItem.ItemId = id;
                    InsertLoginHistory(dataConnection, objItem.CreatedBy, "Create", "Item", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objItem.ItemId = 0;
                    objItem.ItemRefNo = null;
                   
                }
                return objItem;
            }
        }


        public Item GetItem(int ItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Item
                        where ItemId=@ItemId";

                var objItem = connection.Query<Item>(sql, new
                {
                    ItemId = ItemId
                }).First<Item>();

                return objItem;
            }
        }

        public List<Item> GetItems(string name)

        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT ItemId,PartNo,ItemName,CategoryName,ItemGroupName,ItemSubGroupName,UnitName FROM Item I
                               INNER JOIN ItemCategory ON itmCatId=ItemCategoryId
                               INNER JOIN ItemGroup G ON I.ItemGroupId=G.ItemGroupId
                               INNER JOIN ItemSubGroup S ON I.ItemSubGroupId=S.ItemSubGroupId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               WHERE I.isActive=1 AND ItemName LIKE '%'+@name+'%'";

                var objItems = connection.Query<Item>(sql, new { name = name }).ToList<Item>();

                return objItems;
            }
        }

        public Item UpdateItem(Item objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Item SET PartNo = @PartNo ,ItemName = @ItemName ,ItemPrintName = @ItemPrintName ,
                               ItemShortName = @ItemShortName,ItemGroupId = @ItemGroupId,ItemSubGroupId = @ItemSubGroupId,
                               ItemCategoryId = @ItemCategoryId,ItemUnitId = @ItemUnitId ,MinLevel = @MinLevel,MaxLevel = @MaxLevel,
                               ReorderLevel = @ReorderLevel,BatchRequired = @BatchRequired ,StockRequired = @StockRequired,
                               CriticalItem=@CriticalItem,FreezerUnit=@FreezerUnit,Box=@Box OUTPUT INSERTED.ItemId  WHERE ItemId = @ItemId";

                try
                {
                    var id = connection.Execute(sql, objItem);
                    objItem.ItemId = id;
                    InsertLoginHistory(dataConnection, objItem.CreatedBy, "Update", "Item", id.ToString(), "0");
                }
                catch (Exception ex)
                {
                   
                    objItem.ItemId = 0;
                    throw ex;
                }
                return objItem;
            }
        }

        public int DeleteItem(Item objItem)
        {
            int result=0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Item SET isActive = 0  OUTPUT INSERTED.ItemId  WHERE ItemId = @ItemId";
                try
                {

                    var id = connection.Execute(sql, objItem);
                    objItem.ItemId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objItem.CreatedBy, "Delete", "Item", id.ToString(), "0");
                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count >0) // Assume the interesting stuff is in the first error
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

        public string GetPartNoUnit(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<string>("SELECT ISNULL(PartNo,'')+'|'+ISNULL(UnitName,'') FROM Item I INNER JOIN Unit U ON I.ItemUnitId = U.UnitId WHERE ItemId = @itemId",
                new { itemId = itemId }).First<string>();
            }
        }

        public string GetUnit(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<string>("SELECT UnitName FROM Item I INNER JOIN Unit U ON I.ItemUnitId = U.UnitId WHERE ItemId = @itemId",
                new { itemId = itemId }).First<string>();
            }
        }
    }
}
