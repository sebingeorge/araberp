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
    public class SupplierCategoryRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
         public SupplierCategory InsertSupplierCategory(SupplierCategory objSupplierCategory)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 var result = new SupplierCategory();
                 IDbTransaction trn = connection.BeginTransaction();

                 string sql = @"INSERT INTO SupplierCategory(SupCategoryRefNo,SupCategoryName,SupCategoryShortName,CreatedBy,CreatedDate,OrganizationId) 
                                VALUES(@SupCategoryRefNo,@SupCategoryName,@SupCategoryShortName,@CreatedBy,getDate(),@OrganizationId);
                                SELECT CAST(SCOPE_IDENTITY() as int)";

                 try
                 {
                     int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SupplierCategory).Name, "0", 1);
                     objSupplierCategory.SupCategoryRefNo = "SUPCAT/" + internalid;

                     int id = connection.Query<int>(sql, objSupplierCategory, trn).Single();
                     objSupplierCategory.SupCategoryId = id;
                     //connection.Dispose();
                     trn.Commit();
                 }
                 catch (Exception ex)
                 {
                     trn.Rollback();
                     objSupplierCategory.SupCategoryId = 0;
                     objSupplierCategory.SupCategoryRefNo = null;

                 }
                 return objSupplierCategory;
             }
         }
         public IEnumerable<SupplierCategory> FillSupplierCategoryList()
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {

                 return connection.Query<SupplierCategory>("SELECT SupCategoryId,SupCategoryRefNo,SupCategoryName,SupCategoryShortName FROM SupplierCategory WHERE isActive=1").ToList();
             }
         }
         public SupplierCategory GetSupplierCategory(int SupCategoryId)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"select * from SupplierCategory
                                    where SupCategoryId=@SupCategoryId";

                 var objSupplierCategory = connection.Query<SupplierCategory>(sql, new
                 {
                     SupCategoryId = SupCategoryId
                 }).First<SupplierCategory>();

                 return objSupplierCategory;
             }
         }

         public List<SupplierCategory> GetSupplierCategory()
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"select * from SupplierCategory
                        where OrganizationId>0";

                 var objSupplierCategory = connection.Query<SupplierCategory>(sql).ToList<SupplierCategory>();

                 return objSupplierCategory;
             }
         }

         public SupplierCategory UpdateSupplierCategory(SupplierCategory objSupplierCategory)
         {
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @"Update SupplierCategory Set SupCategoryRefNo=@SupCategoryRefNo,SupCategoryName=@SupCategoryName,SupCategoryShortName=@SupCategoryShortName OUTPUT INSERTED.SupCategoryId WHERE SupCategoryId=@SupCategoryId";


                 var id = connection.Execute(sql, objSupplierCategory);
                 return objSupplierCategory;
             }
         }

         public int DeleteSupplierCategory(SupplierCategory objSupplierCategory)
         {
             int result = 0;
             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string sql = @" Update SupplierCategory Set isActive=0 WHERE SupCategoryId=@SupCategoryId";
                 try
                 {

                     var id = connection.Execute(sql, objSupplierCategory);
                     objSupplierCategory.SupCategoryId = id;
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


         public string GetRefNo(SupplierCategory objSupplierCategory)
         {

             using (IDbConnection connection = OpenConnection(dataConnection))
             {
                 string RefNo = "";
                 var result = new SupplierCategory();

                 IDbTransaction trn = connection.BeginTransaction();

                 try
                 {
                     int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(SupplierCategory).Name, "0", 0);
                     RefNo = "SUPCAT/" + internalid;
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
