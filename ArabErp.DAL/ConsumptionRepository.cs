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
        public string InsertConsumption(Consumption objConsumption)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(Consumption).Name, "0", 1);

                    objConsumption.ConsumptionNo = "CON/" + internalId;

                    objConsumption.TotalAmount = objConsumption.ConsumptionItems.Sum(m => m.Amount);

                    string sql = @"insert into Consumption(ConsumptionNo,ConsumptionDate,JobCardId,TotalAmount,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId) Values (@ConsumptionNo,@ConsumptionDate,@JobCardId,@TotalAmount,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objConsumption, txn).Single();

                    foreach (ConsumptionItem item in objConsumption.ConsumptionItems)
                    {
                        item.ConsumptionId = id;
                        new ConsumptionItemRepository().InsertConsumptionItem(item, connection, txn);
                    }

                    txn.Commit();

                    return id + "|CON/" + internalId;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "0";
                }
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
