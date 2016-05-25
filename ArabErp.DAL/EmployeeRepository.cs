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
        public IEnumerable<Dropdown> FillTaskDropdown()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<Dropdown>("select TaskId Id,TaskName Name from Task").ToList();
            }
        }


       

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
