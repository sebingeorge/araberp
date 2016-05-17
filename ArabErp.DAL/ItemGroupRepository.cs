﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class ItemGroupRepository : BaseRepository
    {

        public int InsertItemGroup(ItemGroup objItemGroup)
        {
            string sql = @"INSERT INTO ItemGroup (ItemGroupRefNo,ItemGroupName,ItemCategoryId,CreatedBy,CreatedDate,OrganizationId) VALUES(@ItemGroupRefNo,@ItemGroupName,@ItemCategoryId,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objItemGroup).Single();
            return id;
        }


        public ItemGroup GetItemGroup(int ItemGroupId)
        {

            string sql = @"select * from ItemGroup
                        where ItemGroupId=@ItemGroupId";

            var objItemGroup = connection.Query<ItemGroup>(sql, new
            {
                ItemGroupId = ItemGroupId
            }).First<ItemGroup>();

            return objItemGroup;
        }

        public List<ItemGroup> GetItemGroups()
        {
            string sql = @"select * from ItemGroup
                        where isActive=1";

            var objItemGroups = connection.Query<ItemGroup>(sql).ToList<ItemGroup>();

            return objItemGroups;
        }

        public int UpdateItemGroup(ItemGroup objItemGroup)
        {
            string sql = @"Update ItemGroup Set ItemGroupRefNo=@ItemGroupRefNo,ItemGroupName=@ItemGroupName,ItemCategoryId=@ItemCategoryId OUTPUT INSERTED.ItemGroupId WHERE ItemGroupId=@ItemGroupId";


            var id = connection.Execute(sql, objItemGroup);
            return id;
        }

        public int DeleteItemGroup(Unit objItemGroup)
        {
            string sql = @"Delete ItemGroup  OUTPUT DELETED.ItemGroupId WHERE ItemGroupId=@ItemGroupId";


            var id = connection.Execute(sql, objItemGroup);
            return id;
        }


    }
}