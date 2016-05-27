using System;
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
        public List<Dropdown> FillItemSubGroup(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
            var param = new DynamicParameters();
            
            //return connection.Query<Dropdown>("x",
            // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
            return connection.Query<Dropdown>("select ItemSubGroupId Id,ItemSubGroupName Name from ItemSubGroup where ItemGroupId=@ID", new { ID = Id }).ToList();
            }

        }
        public List<Dropdown> FillItemCategory()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                //return connection.Query<Dropdown>("x",
                // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
                return connection.Query<Dropdown>("select itmCatId Id,CategoryName Name from ItemCategory").ToList();

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
                return connection.Query<Dropdown>("select ItemGroupId Id,ItemGroupName Name from ItemGroup where ItemCategoryId=@ID", new { ID = Id }).ToList();
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
                return connection.Query<Dropdown>("select UnitId Id,UnitName Name from Unit").ToList();
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
        public int InsertItem(Item objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into Item(PartNo,ItemName,ItemPrintName,ItemShortName,ItemGroupId,ItemSubGroupId,ItemCategoryId,ItemUnitId,MinLevel,ReorderLevel,MaxLevel,ExpiryDate,BatchRequired,StockRequired,OrganizationId,CreatedBy,CreatedDate) Values
                                            (@PartNo,@ItemName,@ItemPrintName,@ItemShortName,@ItemGroupId,@ItemSubGroupId,@ItemCategoryId,@ItemUnitId,@MinLevel,@ReorderLevel,@MaxLevel,@ExpiryDate,@BatchRequired,@StockRequired,@OrganizationId,@CreatedBy,@CreatedDate);
            SELECT CAST(SCOPE_IDENTITY() as int)";
               
                var id = 0;
                try
                {
                    id = connection.Query<int>(sql, objItem).Single();
                    //connection.Dispose();
                }
                catch (Exception ex)
                {

                }
                return id;
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

        public List<Item> GetItems()

        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT PartNo,ItemName,ItemCategoryId,ItemGroupId,ItemSubGroupId FROM Item
                        WHERE isActive=1";

                var objItems = connection.Query<Item>(sql).ToList<Item>();

                return objItems;
            }
        }

        public int UpdateItem(Item objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Item SET PartNo = @PartNo ,ItemName = @ItemName ,ItemPrintName = @ItemPrintName ,ItemShortName = @ItemShortName,ItemGroupId = @ItemGroupId,ItemSubGroupId = @ItemSubGroupId,ItemCategoryId = @ItemCategoryId,ItemUnitId = @ItemUnitId,ItemQualityId = @ItemQualityId  OUTPUT INSERTED.ItemId  WHERE ItemId = @ItemId";


                var id = connection.Execute(sql, objItem);
                return id;
            }
        }

        public int DeleteItem(Unit objItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete Item  OUTPUT DELETED.ItemId WHERE ItemId=@ItemId";


                var id = connection.Execute(sql, objItem);
                return id;
            }
        }

    }
}
