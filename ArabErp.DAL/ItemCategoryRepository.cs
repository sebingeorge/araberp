using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class ItemCategoryRepository:BaseRepository 
    {
        public int InsertItemCategory(ItemCategory objItemCategory)
        {
            string sql = @"INSERT INTO ItemCategory (itmCatRefNo,CategoryName,CreatedBy,CreatedDate,OrganizationId) VALUES(@itmCatRefNo,@CategoryName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objItemCategory).Single();
            return id;
        }

        public IEnumerable<ItemCategory> FillItemCategoryList()
        {

            return connection.Query<ItemCategory>("SELECT itmCatId,itmCatRefNo,CategoryName FROM ItemCategory").ToList();
        }

        
        public ItemCategory GetItemCategory(int itmCatId)
        {

            string sql = @"select * from ItemCategory
                        where itmCatId=@itmCatId";

            var objItemCategory = connection.Query<ItemCategory>(sql, new
            {
                itmCatId = itmCatId
            }).First<ItemCategory>();

            return objItemCategory;
        }

        public List<ItemCategory> GetItemCategory()
        {
            string sql = @"select * from ItemCategory
                        where OrganizationId>0";

            var objItemCategory = connection.Query<ItemCategory>(sql).ToList<ItemCategory>();

            return objItemCategory;
        }

    }
}
