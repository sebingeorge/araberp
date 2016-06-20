using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class ExpenseRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public List<Dropdown> FillSupplier()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SupplierId Id,SupplierName Name from Supplier order by SupplierName").ToList();
            }
        }
        public List<Dropdown> FillAddition()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select AddDedId Id, AddDedName Name from AdditionDeduction where AddDedType = 1 order by AddDedName").ToList();
            }
        }
        public List<Dropdown> FillDeduction()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select AddDedId Id, AddDedName Name from AdditionDeduction where AddDedType = 0 order by AddDedName").ToList();
            }
        }
    }
}
