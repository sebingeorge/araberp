using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ConsumptionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertConsumption(Consumption objConsumption)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into Consumption(ConsumptionNo,ConsumptionDate,JobCardId,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@ConsumptionNo,@ConsumptionDate,@JobCardId,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objConsumption).Single();
                return id;
            }
        }


        public Consumption GetConsumption(int ConsumptionId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Consumption
                        where ConsumptionId=@ConsumptionId";

                var objConsumption = connection.Query<Consumption>(sql, new
                {
                    ConsumptionId = ConsumptionId
                }).First<Consumption>();

                return objConsumption;
            }
        }

        public List<Consumption> GetConsumptions()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Consumption
                        where isActive=1";

                var objConsumptions = connection.Query<Consumption>(sql).ToList<Consumption>();

                return objConsumptions;
            }
        }



        public int DeleteConsumption(Unit objConsumption)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete Consumption  OUTPUT DELETED.ConsumptionId WHERE ConsumptionId=@ConsumptionId";


                var id = connection.Execute(sql, objConsumption);
                return id;
            }
        }


    }
}
