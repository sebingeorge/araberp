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
        public Employee Insert(Employee model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"insert  into Employee(EmployeeRefNo,EmployeeName,GenderId,DesignationId,CategoryId,LocationId,HourlyCost,CreatedBy,CreatedDate,OrganizationId)  Values (@EmployeeRefNo,@EmployeeName,@GenderId,@DesignationId,@CategoryId,@LocationId,@HourlyCost,@CreatedBy,@CreatedDate,@OrganizationId);          
                        SELECT CAST(SCOPE_IDENTITY() as int)";
                int id=0;

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(Employee).Name, "0", 1);
                    model.EmployeeRefNo = "EMP/" + internalid;
                     id = connection.Query<int>(sql, model, trn).Single();
                     model.EmployeeId = id;
                     
                     trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.EmployeeId = 0;
                    model.EmployeeRefNo = null;
                }
                return model;
            }
        }

        public Employee GetEmployee(int EmployeeId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Employee where EmployeeId=@EmployeeId";

                var model = connection.Query<Employee>(sql, new
                {
                    EmployeeId = EmployeeId
                }).First<Employee>();

                return model;
            }
        }
        public Employee UpdateEmployee(Employee model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Employee SET EmployeeName = @EmployeeName,GenderId = @GenderId,DesignationId = @DesignationId,CategoryId = @CategoryId,LocationId = @LocationId,HourlyCost = @HourlyCost,CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId OUTPUT INSERTED.EmployeeId  WHERE EmployeeId = @EmployeeId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.EmployeeId = id;
                }
                catch (Exception ex)
                {

                    model.EmployeeId = 0;
                    
                }
                return model;
            }
        }
        public Employee DeleteEmployee(Employee model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE Employee SET isActive = 0 OUTPUT INSERTED.EmployeeId  WHERE EmployeeId = @EmployeeId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.EmployeeId = id;
                }
                catch (Exception ex)
                {

                    model.EmployeeId = 0;

                }
                return model;
            }
        }
       public IEnumerable <Employee> FillEmployeeList()
        {
           using (IDbConnection connection=OpenConnection (dataConnection))
           {
               return connection.Query<Employee>("SELECT EmployeeId,EmployeeRefNo,EmployeeName,CASE WHEN GenderId=0 then 'Male' Else 'Female' End AS  Gender,DesignationName FROM Employee E INNER JOIN Designation D ON D.DesignationId=E.DesignationId where E.isActive = 1").ToList();
           }
         
        }
        public void Dispose()
        {
            connection.Dispose();
        }
       
        
    }
}
