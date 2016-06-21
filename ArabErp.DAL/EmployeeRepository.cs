using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
   public class EmployeeRepository : BaseRepository
    {
        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");

        public EmployeeRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public IEnumerable<Dropdown> FillDesignationDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select DesignationId Id,DesignationName Name from Designation").ToList();
            }
        }
        public IEnumerable<Dropdown> FillCategoryDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select EmpCategoryId Id ,EmpCategoryName Name from EmpolyeeCategory").ToList();
            }
        }
        public IEnumerable<Dropdown> FillLocationDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select StockPointId Id, StockPointName Name From Stockpoint").ToList();
            }
        }

       /// <summary>
       /// Fn for Insert Employee
       /// </summary>
       /// <param name="model"></param>
       /// <returns></returns>
        public int Insert(Employee model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into Employee(EmployeeRefNo,EmployeeName,GenderId,DesignationId,CategoryId,LocationId,HourlyCost,CreatedBy,CreatedDate,OrganizationId)  Values (@EmployeeRefNo,@EmployeeName,@GenderId,@DesignationId,@CategoryId,@LocationId,@HourlyCost,@CreatedBy,@CreatedDate,@OrganizationId);          
                        SELECT CAST(SCOPE_IDENTITY() as int)";
                int id=0;

                try
                {
                     id = connection.Query<int>(sql, model).Single();
                }
                catch (Exception e)
                { }
                return id;
            }
        }
       

       public IEnumerable <Employee> FillEmployeeList()
        {
           using (IDbConnection connection=OpenConnection (dataConnection))
           {
               return connection.Query<Employee>("SELECT EmployeeRefNo,EmployeeName,CASE WHEN GenderId=1 then 'Male' Else 'Female' End AS  Gender,DesignationName FROM Employee E INNER JOIN Designation D ON D.DesignationId=E.DesignationId").ToList();
           }
         
        }
        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
