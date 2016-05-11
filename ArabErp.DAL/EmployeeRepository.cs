using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;

namespace ArabErp.DAL
{
   public class EmployeeRepository : IDisposable
    {
        private SqlConnection connection;

        public EmployeeRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }

        public Employee NewEmployee()
        {
            var emp = new Employee();

            emp.Designations = connection.GetList<Designation>().ToList<Designation>();
            return emp;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
