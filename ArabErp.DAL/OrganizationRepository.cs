using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class OrganizationRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<Organization> GetOrganizations()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select OrganizationId, OrganizationName from Organization order by OrganizationName;";
                return connection.Query<Organization>(sql);
            }
        }
    }
}
