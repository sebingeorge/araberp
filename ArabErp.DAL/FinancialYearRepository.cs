using ArabErp.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace ArabErp.DAL
{
    public class FinancialYearRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<FinancialYear> GetFinancialYear()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<FinancialYear>("select * from FinancialYear");
            }
        }
    }
}
