using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ProfitabilityReportRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<ProfitabilityReport> GetProfitabilityReport()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ProfitabilityReport>("exec ProfitabilityReport");
            }
        }
    }
}
