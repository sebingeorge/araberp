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
    public class CustomerCategoryRepository
    {
         private SqlConnection connection;

         public CustomerCategoryRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

         public int InsertCustomerCategory(CustomerCategory objCustomerCategory)
         {
             string sql = @"INSERT INTO CustomerCategory(CusCategoryRefNo,CusCategoryName,CusCategoryShortName,CreatedBy,CreatedDate,OrganizationId) VALUES(@CusCategoryRefNo,@CusCategoryName,@CusCategoryShortName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
             var id = connection.Query<int>(sql, objCustomerCategory).Single();
             return id;
         }

         public IEnumerable<CustomerCategory> FillCustomerCategoryList()
         {

             return connection.Query<CustomerCategory>("SELECT CusCategoryRefNo,CusCategoryName,CusCategoryShortName  FROM CustomerCategory ").ToList();
         }

         public CustomerCategory GetCustomerCategory(int CustomerCategoryId)
         {

             string sql = @"select * from CustomerCategory
                                    where CusCategoryId=@CusCategoryId";

             var objCustomerCategory = connection.Query<CustomerCategory>(sql, new
             {
                 CustomerCategoryId = CustomerCategoryId
             }).First<CustomerCategory>();

             return objCustomerCategory;
         }

         public List<CustomerCategory> GetCustomerCategory()
         {
             string sql = @"select * from CustomerCategory
                        where OrganizationId>0";

             var objCustomerCategory = connection.Query<CustomerCategory>(sql).ToList<CustomerCategory>();

             return objCustomerCategory;
         }

         public void Dispose()
         {
             connection.Dispose();
         }
    }
}
