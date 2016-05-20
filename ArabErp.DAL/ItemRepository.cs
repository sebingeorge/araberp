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
        public List<Dropdown> FillItemSubGroup(int Id)
        {
            var param = new DynamicParameters();
            //return connection.Query<Dropdown>("x",
            // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
            return connection.Query<Dropdown>("select ItemSubGroupId Id,ItemSubGroupName Name from ItemSubGroup where ItemGroupId=@ID", new { ID = Id }).ToList();


        }
        public List<Dropdown> FillItemCategory()
        {
            var param = new DynamicParameters();
            //return connection.Query<Dropdown>("x",
            // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
            return connection.Query<Dropdown>("select itmCatId Id,CategoryName Name from ItemCategory").ToList();


        }
        public List<Dropdown> FillItemGroup( int Id)
        {
            var param = new DynamicParameters();
            //return connection.Query<Dropdown>("x",
            // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
            return connection.Query<Dropdown>("select ItemGroupId Id,ItemGroupName Name from ItemGroup where ItemCategoryId=@ID", new { ID=Id }).ToList();


        }
        public List<Dropdown> FillUnit()
        {
            return connection.Query<Dropdown>("select UnitId Id,UnitName Name from Unit").ToList();
        }
        public ArrayList SaveItem(Item OItem)
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
}
