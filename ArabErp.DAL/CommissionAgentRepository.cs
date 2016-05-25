using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class CommissionAgentRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertCommissionAgent(CommissionAgent objCommissionAgent)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO CommissionAgent (CommissionAgentRefNo,CommissionAgentName,Address1,Address2,Address3,Phone,CreatedBy,CreatedDate,OrganizationId) VALUES(@CommissionAgentRefNo,@CommissionAgentName,@Address1,@Address2,@Address3,@Phone,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objCommissionAgent).Single();
            return id;
        }
        }

        public IEnumerable<CommissionAgent> FillCommissionAgentList()
        {
            return connection.Query<CommissionAgent>("SELECT CommissionAgentRefNo,CommissionAgentName FROM CommissionAgent").ToList();
        }

        public CommissionAgent GetCommissionAgent(int CommissionAgentId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

            string sql = @"select * from CommissionAgent
                        where CommissionAgentId=@CommissionAgentId";

            var objCommissionAgent = connection.Query<CommissionAgent>(sql, new
            {
                CommissionAgentId = CommissionAgentId
            }).First<CommissionAgent>();

            return objCommissionAgent;
        }
        }

        public List<CommissionAgent> GetCommissionAgents()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
            string sql = @"select * from CommissionAgent
                        where isActive=1";

            var objCommissionAgents = connection.Query<CommissionAgent>(sql).ToList<CommissionAgent>();

            return objCommissionAgents;
        }
        }


    }
}