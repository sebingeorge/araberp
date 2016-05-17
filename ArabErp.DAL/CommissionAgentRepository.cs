using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class CommissionAgentRepository : BaseRepository
    {

        public int InsertCommissionAgent(CommissionAgent objCommissionAgent)
        {
            string sql = @"INSERT INTO table_name (CommissionAgentId,CommissionAgentRefNo,CommissionAgentName,Address1,Address2,Address3,Phone,CreatedBy,CreatedDate,OrganizationId) VALUES(@CommissionAgentId,@CommissionAgentRefNo,@CommissionAgentName,@Address1,@Address2,@Address3,@Phone,@CreatedBy,@CreatedDate,@OrganizationId));
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objCommissionAgent).Single();
            return id;
        }


        public CommissionAgent GetCommissionAgent(int CommissionAgentId)
        {

            string sql = @"select * from CommissionAgent
                        where CommissionAgentId=@CommissionAgentId";

            var objCommissionAgent = connection.Query<CommissionAgent>(sql, new
            {
                CommissionAgentId = CommissionAgentId
            }).First<CommissionAgent>();

            return objCommissionAgent;
        }

        public List<CommissionAgent> GetCommissionAgents()
        {
            string sql = @"select * from CommissionAgent
                        where CommissionAgentId>0";

            var objCommissionAgents = connection.Query<CommissionAgent>(sql).ToList<CommissionAgent>();

            return objCommissionAgents;
        }


    }
}