using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class ItemRepository :BaseRepository
    {
        public List<Dropdown> FillItemSubGroup()
        {
            var list = connection.Query<Dropdown>("SELECT ItemSubGroupId Id,ItemSubGroupName Name FROM [dbo].[ItemSubGroup]").ToList();

           return list;
            
        }
        public int InsertItem(Item objItem)
        {
            string sql = @"insert  into Item(PartNo,ItemName,ItemPrintName,ItemShortName,ItemGroupId,ItemSubGroupId,ItemCategoryId,ItemUnitId,ItemQualityId,CommodityId,MinLevel,ReorderLevel,MaxLevel,OrganizationId) Values (@PartNo,@ItemName,@ItemPrintName,@ItemShortName,@ItemGroupId,@ItemSubGroupId,@ItemCategoryId,@ItemUnitId,@ItemQualityId,@CommodityId,@MinLevel,@ReorderLevel,@MaxLevel,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objItem).Single();
            return id;
        }


        public Item GetItem(int ItemId)
        {

            string sql = @"select * from Item
                        where ItemId=@ItemId";

            var objItem = connection.Query<Item>(sql, new
            {
                ItemId = ItemId
            }).First<Item>();

            return objItem;
        }

        public List<Item> GetItems()
        {
            string sql = @"select * from Item
                        where isActive=1";

            var objItems = connection.Query<Item>(sql).ToList<Item>();

            return objItems;
        }

        public int UpdateItem(Item objItem)
        {
            string sql = @"UPDATE Item SET PartNo = @PartNo ,ItemName = @ItemName ,ItemPrintName = @ItemPrintName ,ItemShortName = @ItemShortName,ItemGroupId = @ItemGroupId,ItemSubGroupId = @ItemSubGroupId,ItemCategoryId = @ItemCategoryId,ItemUnitId = @ItemUnitId,ItemQualityId = @ItemQualityId  OUTPUT INSERTED.ItemId  WHERE ItemId = @ItemId";


            var id = connection.Execute(sql, objItem);
            return id;
        }

        public int DeleteItem(Unit objItem)
        {
            string sql = @"Delete Item  OUTPUT DELETED.ItemId WHERE ItemId=@ItemId";


            var id = connection.Execute(sql, objItem);
            return id;
        }

    }
}
