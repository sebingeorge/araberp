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
        public int InsertItemCategory(ItemCategory objItemCategory)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO ItemCategory (itmCatRefNo,CategoryName,CreatedBy,CreatedDate,OrganizationId) VALUES(@itmCatRefNo,@CategoryName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objItemCategory).Single();
                return id;
            }
        }

        public IEnumerable<ItemCategory> FillItemCategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ItemCategory>("SELECT itmCatId,itmCatRefNo,CategoryName FROM ItemCategory").ToList();
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
                string sql = @"select * from ItemCategory
                        where OrganizationId>0";

                var objItemCategory = connection.Query<ItemCategory>(sql).ToList<ItemCategory>();

                return objItemCategory;
            }
        }

    }
}
