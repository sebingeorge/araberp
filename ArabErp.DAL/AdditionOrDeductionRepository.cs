using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class AdditionOrDeductionRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<AdditionOrDeduction> GetAdditionDeduction()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = "select AddDedId, AddDedName, AddDedType, AddDedRemarks from AdditionDeduction";
                return connection.Query<AdditionOrDeduction>(qry);
            }
        }
        public int Insert(AdditionOrDeduction model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = "Insert into AdditionDeduction(AddDedName, AddDedType, AddDedRemarks)";
                qry += " values(@AddDedName, @AddDedType, @AddDedRemarks);";
                qry += " SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(qry, model).Single();
                return id;
            }
        }
    }
}
