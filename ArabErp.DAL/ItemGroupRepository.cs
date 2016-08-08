using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ItemGroupRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public ItemGroup InsertItemGroup(ItemGroup objItemGroup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new ItemGroup();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO ItemGroup (ItemGroupRefNo,ItemGroupName,ItemCategoryId,CreatedBy,CreatedDate,OrganizationId) 
                               VALUES(@ItemGroupRefNo,@ItemGroupName,@ItemCategoryId,@CreatedBy,getDate(),@OrganizationId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ItemGroup).Name, "0", 1);
                    objItemGroup.ItemGroupRefNo = "ITMGRP/" + internalid;

                    int id = connection.Query<int>(sql, objItemGroup, trn).Single();
                    objItemGroup.ItemGroupId = id;
                    InsertLoginHistory(dataConnection, objItemGroup.CreatedBy, "Create", "Item Group", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objItemGroup.ItemGroupId = 0;
                    objItemGroup.ItemGroupRefNo = null;

                }
                return objItemGroup;
            }
        }


        public IEnumerable<ItemGroup> FillItemGroupList()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ItemGroup>("SELECT ItemGroupId,ItemGroupRefNo,ItemGroupName,CategoryName FROM itemGroup IG INNER JOIN ItemCategory ON ItemCategoryId=itmCatId  where IG.isActive=1").ToList();
            }
        }

        public IEnumerable<Dropdown> FillCategory()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<Dropdown>("select itmCatId Id,CategoryName Name from ItemCategory").ToList();
            }
        }

        public ItemGroup GetItemGroup(int ItemGroupId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemGroup
                        where ItemGroupId=@ItemGroupId";

                var objItemGroup = connection.Query<ItemGroup>(sql, new
                {
                    ItemGroupId = ItemGroupId
                }).First<ItemGroup>();

                return objItemGroup;
            }
        }

        public List<ItemGroup> GetItemGroups()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemGroup
                        where isActive=1";

                var objItemGroups = connection.Query<ItemGroup>(sql).ToList<ItemGroup>();

                return objItemGroups;
            }
        }

        public ItemGroup UpdateItemGroup(ItemGroup objItemGroup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update ItemGroup Set ItemGroupRefNo=@ItemGroupRefNo,ItemGroupName=@ItemGroupName,ItemCategoryId=@ItemCategoryId OUTPUT INSERTED.ItemGroupId WHERE ItemGroupId=@ItemGroupId";


                var id = connection.Execute(sql, objItemGroup);
                InsertLoginHistory(dataConnection, objItemGroup.CreatedBy, "Update", "Item Group", id.ToString(), "0");
                return objItemGroup;
            }
        }

        public int DeleteItemGroup(ItemGroup objItemGroup)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update ItemGroup Set isActive=0 WHERE ItemGroupId=@ItemGroupId";
                try
                {

                    var id = connection.Execute(sql, objItemGroup);
                    objItemGroup.ItemGroupId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objItemGroup.CreatedBy, "Delete", "Item Group", id.ToString(), "0");
                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
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

        public string GetRefNo(ItemGroup objItemGroup)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new ItemGroup();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ItemGroup).Name, "0", 0);
                    RefNo = "ITMGRP/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }


    }
}