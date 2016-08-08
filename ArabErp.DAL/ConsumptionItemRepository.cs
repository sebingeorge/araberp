using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ConsumptionItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertConsumptionItem(ConsumptionItem objConsumptionItem, IDbConnection connection, IDbTransaction txn)
        {
            try
            {
                string sql = @"insert into ConsumptionItem(ConsumptionId,ItemId,Remarks,Amount) Values (@ConsumptionId,@ItemId,@Remarks,@Amount);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, objConsumptionItem, txn).Single();
                
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public ConsumptionItem GetConsumptionItem(int ConsumptionItemId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ConsumptionItem
                        where ConsumptionItemId=@ConsumptionItemId";

                var objConsumptionItem = connection.Query<ConsumptionItem>(sql, new
                {
                    ConsumptionItemId = ConsumptionItemId
                }).First<ConsumptionItem>();

                return objConsumptionItem;
            }
        }

        public List<ConsumptionItem> GetConsumptionItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from ConsumptionItem
                        where isActive=1";

                var objConsumptionItems = connection.Query<ConsumptionItem>(sql).ToList<ConsumptionItem>();

                return objConsumptionItems;
            }
        }



        public int DeleteConsumptionItem(Unit objConsumptionItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete ConsumptionItem  OUTPUT DELETED.ConsumptionItemId WHERE ConsumptionItemId=@ConsumptionItemId";


                var id = connection.Execute(sql, objConsumptionItem);
                return id;
            }
        }


    }
}