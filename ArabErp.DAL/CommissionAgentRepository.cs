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

        public CommissionAgent InsertCommissionAgent(CommissionAgent model)

        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"INSERT INTO CommissionAgent (CommissionAgentRefNo,CommissionAgentName,Address1,Address2,Address3,Phone,CreatedBy,CreatedDate,OrganizationId) VALUES(@CommissionAgentRefNo,@CommissionAgentName,@Address1,@Address2,@Address3,@Phone,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                int id = 0;
                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(CommissionAgent).Name, "0", 1);
                    model.CommissionAgentRefNo = "CA/" + internalid;
                    id = connection.Query<int>(sql, model, trn).Single();
                    model.CommissionAgentId = id;

                    trn.Commit();
                }
                catch (Exception e)
                {
                    trn.Rollback();
                    model.CommissionAgentId = 0;
                    model.CommissionAgentRefNo = null;
                }
                return model;
        }
        }

        public CommissionAgent UpdateCommissionAgent(CommissionAgent model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE CommissionAgent SET CommissionAgentName = @CommissionAgentName,Address1 = @Address1 ,Address2 = @Address2,Address3 = @Address3,Phone = @Phone, CreatedBy = @CreatedBy,CreatedDate= GETDATE(),OrganizationId = @OrganizationId OUTPUT INSERTED.CommissionAgentId  WHERE CommissionAgentId = @CommissionAgentId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.CommissionAgentId = id;
                }
                catch (Exception ex)
                {

                    model.CommissionAgentId = 0;

                }
                return model;
            }
        }
        public CommissionAgent DeleteCommissionAgent(CommissionAgent model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE CommissionAgent SET isActive = 0 OUTPUT INSERTED.CommissionAgentId  WHERE CommissionAgentId = @CommissionAgentId";

                try
                {
                    var id = connection.Execute(sql, model);
                    model.CommissionAgentId = id;
                }
                catch (Exception ex)
                {

                    model.CommissionAgentId = 0;

                }
                return model;
            }
        }

        public IEnumerable<CommissionAgent> FillCommissionAgentList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<CommissionAgent>("SELECT CommissionAgentId,CommissionAgentRefNo,CommissionAgentName FROM CommissionAgent where isActive = 1 ").ToList();
            }
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