using System;
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
                return objItemCategory;
            }
        }

        public int DeleteItemCategory(ItemCategory objItemCategory)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update ItemCategory Set isActive=0 WHERE itmCatId=@itmCatId";
                try
                {

                    var id = connection.Execute(sql, objItemCategory);
                    objItemCategory.itmCatId = id;
                    result = 0;

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

        public IEnumerable<ItemCategory> FillItemCategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ItemCategory>("SELECT itmCatId,itmCatRefNo,CategoryName FROM ItemCategory WHERE isActive=1").ToList();
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
