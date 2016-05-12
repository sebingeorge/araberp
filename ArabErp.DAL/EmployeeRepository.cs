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
            emp.Designations = connection.Query<Designation>("select DesignationId,DesignationName from Designation").ToList();

            return emp;
        }

        public IEnumerable<EmployeeCategoryDropDown> CategoryDropdownList()
        {
            var list = connection.Query<EmployeeCategoryDropDown>("select EmpCategoryId Code,EmpCategoryName Name from EmpolyeeCategory").ToList();
            return list;
        }


        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
