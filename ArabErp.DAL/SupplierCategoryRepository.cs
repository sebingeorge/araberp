using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SupplierCategoryRepository
    {
         private SqlConnection connection;

         public SupplierCategoryRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

         public int InsertSupplierCategory(SupplierCategory objSupplierCategory)
         {
             string sql = @"INSERT INTO SupplierCategory(SupCategoryRefNo,SupCategoryName,SupCategoryShortName,CreatedBy,CreatedDate,OrganizationId) VALUES(@SupCategoryRefNo,@SupCategoryName,@SupCategoryShortName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
             var id = connection.Query<int>(sql, objSupplierCategory).Single();
             return id;
         }
         public IEnumerable<SupplierCategory> FillSupplierCategoryList()
         {

             return connection.Query<SupplierCategory>("SELECT SupCategoryRefNo,SupCategoryName,SupCategoryShortName FROM SupplierCategory").ToList();
         }

         public SupplierCategory GetSupplierCategory(int SupplierCategoryId)
         {

             string sql = @"select * from SupplierCategory
                                    where SupCategoryId=@SupCategoryId";

             var objSupplierCategory = connection.Query<SupplierCategory>(sql, new
             {
                 SupplierCategoryId = SupplierCategoryId
             }).First<SupplierCategory>();

             return objSupplierCategory;
         }

         public List<SupplierCategory> GetSupplierCategory()
         {
             string sql = @"select * from SupplierCategory
                        where OrganizationId>0";

             var objSupplierCategory = connection.Query<SupplierCategory>(sql).ToList<SupplierCategory>();

             return objSupplierCategory;
         }

         public void Dispose()
         {
             connection.Dispose();
         }
    }
}
