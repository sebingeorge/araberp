using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;


namespace ArabErp.DAL
{
    public class EmployeeCategoryRepository:BaseRepository 
    {
        public int InsertEmployeeCategory(EmployeeCategory objEmployeeCategory)
        {
            string sql = @"INSERT INTO EmpolyeeCategory(EmpCategoryRefNo,EmpCategoryName,CreatedBy,CreatedDate,OrganizationId) VALUES(@EmpCategoryRefNo,@EmpCategoryName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = connection.Query<int>(sql, objEmployeeCategory).Single();
            return id;
        }

        public EmployeeCategory GetEmployeeCategory(int EmpCategoryId)
        {

            string sql = @"select * from EmpolyeeCategory
                        where EmpCategoryId=@EmpCategoryId";

            var objEmployeeCategory = connection.Query<EmployeeCategory>(sql, new
            {
                EmpCategoryId = EmpCategoryId
            }).First<EmployeeCategory>();

            return objEmployeeCategory;
        }

        public List<EmployeeCategory> GetEmployeeCategory()
        {
            string sql = @"select * from EmpolyeeCategory
                        where OrganizationId>0";

            var objEmployeeCategory = connection.Query<EmployeeCategory>(sql).ToList<EmployeeCategory>();

            return objEmployeeCategory;
        }
  
}
}
