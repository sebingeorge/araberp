using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class CustomerCategoryRepository :BaseRepository
    {
         private SqlConnection connection;
         static string dataConnection = GetConnectionString("arab");
         public string ConnectionString()
         {
             return dataConnection;
         }

         public CustomerCategoryRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

         public CustomerCategory InsertCustomerCategory(CustomerCategory objCustomerCategory)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))

             {
                 var result = new CustomerCategory();

                 IDbTransaction trn = connection.BeginTransaction();

                 string sql = @"INSERT INTO CustomerCategory(CusCategoryRefNo,CusCategoryName,CusCategoryShortName,CreatedBy,CreatedDate,OrganizationId) 
                                VALUES(@CusCategoryRefNo,@CusCategoryName,@CusCategoryShortName,@CreatedBy,getDate(),@OrganizationId);
                                SELECT CAST(SCOPE_IDENTITY() as int)";

                 try
                 {
                     int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(CustomerCategory).Name, "0", 1);
                     objCustomerCategory.CusCategoryRefNo = "CUSCAT/" + internalid;

                     int id = connection.Query<int>(sql, objCustomerCategory, trn).Single();
                     objCustomerCategory.CusCategoryId = id;
                     //connection.Dispose();
                     trn.Commit();
                 }
                 catch (Exception ex)
                 {
                     trn.Rollback();
                     objCustomerCategory.CusCategoryId = 0;
                     objCustomerCategory.CusCategoryRefNo = null;

                 }
                 return objCustomerCategory;
             }
         }

         public CustomerCategory UpdateCustomerCategory(CustomerCategory objCustomerCategory)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"Update CustomerCategory Set CusCategoryRefNo=@CusCategoryRefNo,CusCategoryName=@CusCategoryName,CusCategoryShortName=@CusCategoryShortName OUTPUT INSERTED.CusCategoryId WHERE CusCategoryId=@CusCategoryId";


                 var id = connection.Execute(sql, objCustomerCategory);
                 return objCustomerCategory;
             }
         }

         public int DeleteCustomerCategory(CustomerCategory objCustomerCategory)
         {
             int result = 0;
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @" Update CustomerCategory Set isActive=0 WHERE CusCategoryId=@CusCategoryId";
                 try
                 {

                     var id = connection.Execute(sql, objCustomerCategory);
                     objCustomerCategory.CusCategoryId = id;
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


         public IEnumerable<CustomerCategory> FillCustomerCategoryList()
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 return connection.Query<CustomerCategory>("SELECT CusCategoryId,CusCategoryRefNo,CusCategoryName,CusCategoryShortName  FROM CustomerCategory WHERE isActive=1").ToList();
             }
         }

         public CustomerCategory GetCustomerCategory(int CusCategoryId)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"select * from CustomerCategory
                                    where CusCategoryId=@CusCategoryId";

                 var objCustomerCategory = connection.Query<CustomerCategory>(sql, new
                 {
                     CusCategoryId = CusCategoryId
                 }).First<CustomerCategory>();

                 return objCustomerCategory;

             }
         }
   
         public List<CustomerCategory> GetCustomerCategory()
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"select * from CustomerCategory
                        where OrganizationId>0";

                 var objCustomerCategory = connection.Query<CustomerCategory>(sql).ToList<CustomerCategory>();

                 return objCustomerCategory;
             }
         }

         public string GetRefNo(CustomerCategory objCustomerCategory)
         {

             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string RefNo = "";
                 var result = new CustomerCategory();

                 IDbTransaction trn = connection.BeginTransaction();

                 try
                 {
                     int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(CustomerCategory).Name, "0", 0);
                     RefNo = "CUSCAT/" + internalid;
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
