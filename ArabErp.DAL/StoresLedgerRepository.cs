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
                string qry = @"SELECT StockPointId,stocktrnDate,StockUpdateId,StockUserId,StockType,StockInOut,0 INQTY, 0 OUTQTY  INTO #TEMP  
                               FROM StockUpdate S  inner join Item I on I.ItemId=S.ItemId
                               WHERE stocktrnDate >= @from AND stocktrnDate <= @to AND S.StockPointId=ISNULL(NULLIF(@stkid,0),S.StockPointId)
                               AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) AND S.ItemId=ISNULL(NULLIF(@itmid, 0), S.ItemId) 
                               AND S.OrganizationId=@OrganizationId ;
                 
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
