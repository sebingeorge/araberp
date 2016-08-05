using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StoresLedgerRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<ClosingStock> GetStoresLedgerData(DateTime? from, DateTime? to, int stkid, int itmcatid, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"select stocktrnDate,StockUpdateId,StockUserId,StockType,StockInOut,0 INQTY, 0 OUTQTY  INTO #TEMP  from StockUpdate 
                               where ItemId=@itmid AND  stocktrnDate >= @from AND stocktrnDate <= @to AND OrganizationId=@OrganizationId;
                 
                with A as (
                select StockUpdateId, Quantity from StockUpdate WHERE StockInOut='IN'
                )
                update T set T.INQTY = A.Quantity from A inner join #TEMP T on T.StockUpdateId = A.StockUpdateId;

				with B as (
                select StockUpdateId,(Quantity)*(-1)Quantity from StockUpdate WHERE StockInOut='OUT'
                )
                update T set T.OUTQTY = B.Quantity from B inner join #TEMP T on T.StockUpdateId = B.StockUpdateId;

                select * from #TEMP";



                return connection.Query<ClosingStock>(qry, new { stkid = stkid, itmcatid = itmcatid, itmid = itmid, OrganizationId = OrganizationId, from = from,to=to }).ToList();
            }
        }
    }
}
