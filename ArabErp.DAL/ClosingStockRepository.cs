using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ClosingStockRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<ClosingStock> GetClosingStockData(DateTime? Ason, int stkid,int itmcatid, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"select ItemName,SUM(Quantity)Quantity,UnitName from StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               inner join Unit U ON U.UnitId=I.ItemUnitId
                               where  SU.ItemId = ISNULL(NULLIF(@itmid, 0), SU.ItemId) and I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) and SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId) AND SU.stocktrnDate<=@Ason
                               GROUP BY ItemName,UnitName";
                

                             


                return connection.Query<ClosingStock>(qry, new { stkid = stkid,itmcatid=itmcatid, itmid = itmid, OrganizationId = OrganizationId, Ason = Ason}).ToList();
            }
        }
    }
}
