﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ItemCategoryRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }

        public ItemCategory InsertItemCategory(ItemCategory objItemCategory)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new ItemCategory();

                IDbTransaction trn = connection.BeginTransaction();

                string sql = @"INSERT INTO ItemCategory (itmCatRefNo,CategoryName,CreatedBy,CreatedDate,OrganizationId) 
                             VALUES(@itmCatRefNo,@CategoryName,@CreatedBy,@CreatedDate,@OrganizationId);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ItemCategory).Name, "0", 1);
                    objItemCategory.itmCatRefNo = "ITMCAT/" + internalid;

                    int id = connection.Query<int>(sql, objItemCategory, trn).Single();
                    objItemCategory.itmCatId = id;
                    InsertLoginHistory(dataConnection, objItemCategory.CreatedBy, "Create", "Item Category", id.ToString(), "0");
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objItemCategory.itmCatId = 0;
                    objItemCategory.itmCatRefNo = null;

                }
                return objItemCategory;
            }
        }


        public ItemCategory UpdateItemCategory(ItemCategory objItemCategory)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update ItemCategory Set itmCatRefNo=@itmCatRefNo,CategoryName=@CategoryName OUTPUT INSERTED.itmCatId WHERE itmCatId=@itmCatId";

                var id = connection.Execute(sql, objItemCategory);
                InsertLoginHistory(dataConnection, objItemCategory.CreatedBy, "Update", "Item Category", id.ToString(), "0");
                return objItemCategory;
            }
        }


        public ItemCategory DeleteItemCategory(ItemCategory objItemCategory)
        {
          
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update ItemCategory Set isActive=0 WHERE itmCatId=@itmCatId";
                try
                {

                    var id = connection.Execute(sql, objItemCategory);
                    objItemCategory.itmCatId = id;
                    InsertLoginHistory(dataConnection, objItemCategory.CreatedBy, "Delete", "Item Category", id.ToString(), "0");
                    //result = 0;

                }
                catch (Exception ex)
                {

                    objItemCategory.itmCatId = 0;

                }
                return objItemCategory;
            }
        }

        public IEnumerable<ItemCategory> FillItemCategoryList(string name)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ItemCategory>("SELECT itmCatId,itmCatRefNo,CategoryName FROM ItemCategory WHERE isActive=1 AND CategoryName LIKE '%'+@name+'%'", new { name = name }).ToList();
            }
        }

        
        public ItemCategory GetItemCategory(int itmCatId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemCategory
                        where itmCatId=@itmCatId";

                var objItemCategory = connection.Query<ItemCategory>(sql, new
                {
                    itmCatId = itmCatId
                }).First<ItemCategory>();

                return objItemCategory;
            }
        }

        public List<ItemCategory> GetItemCategory()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ItemCategory where isActive=1";

                var objItemCategory = connection.Query<ItemCategory>(sql).ToList<ItemCategory>();

                return objItemCategory;
            }
        }

        public string GetRefNo(ItemCategory objItemCategory)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new ItemCategory();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ItemCategory).Name, "0", 0);
                    RefNo = "ITMCAT/" + internalid;
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
