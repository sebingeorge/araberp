using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class EmployeeCategoryRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertEmployeeCategory(EmployeeCategory objEmployeeCategory)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO EmpolyeeCategory(EmpCategoryRefNo,EmpCategoryName,CreatedBy,CreatedDate,OrganizationId) VALUES(@EmpCategoryRefNo,@EmpCategoryName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
                var id = connection.Query<int>(sql, objEmployeeCategory).Single();
                return id;
            }
        }

        public IEnumerable<EmployeeCategory> FillEmployeeCategoryList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<EmployeeCategory>("SELECT EmpCategoryRefNo,EmpCategoryName  FROM EmpolyeeCategory").ToList();
            }
        }

        public EmployeeCategory GetEmployeeCategory(int EmpCategoryId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from EmpolyeeCategory
                        where EmpCategoryId=@EmpCategoryId";

                var objEmployeeCategory = connection.Query<EmployeeCategory>(sql, new
                {
                    EmpCategoryId = EmpCategoryId
                }).First<EmployeeCategory>();

                return objEmployeeCategory;
            }
        }

        public List<EmployeeCategory> GetEmployeeCategory()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from EmpolyeeCategory
                        where OrganizationId>0";

                var objEmployeeCategory = connection.Query<EmployeeCategory>(sql).ToList<EmployeeCategory>();

                return objEmployeeCategory;
            }
        }
  
}
}
