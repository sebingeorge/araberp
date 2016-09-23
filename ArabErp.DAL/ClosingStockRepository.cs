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

        /// <summary>
        /// Get stock quantity based on stock point, item category, item and organization
        /// </summary>
        /// <param name="asOn">Stock as on this date</param>
        /// <param name="stockPointId">Stock Point Id</param>
        /// <param name="itemCategoryId">Item Category Id</param>
        /// <param name="itemId">Item Id</param>
        /// <param name="OrganizationId">Organization Id</param>
        /// <returns>List of items matching the values</returns>
        public IEnumerable<ClosingStock> GetClosingStockData(DateTime? asOn, int stockPointId, int itemCategoryId, int itemId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT ItemRefNo,ItemName,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               WHERE  SU.ItemId = ISNULL(NULLIF(@itmid, 0), SU.ItemId) AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId) AND 
                               CONVERT(DATE, SU.stocktrnDate, 106)<=CONVERT(DATE, @Ason, 106)
                               GROUP BY ItemRefNo,ItemName,UnitName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, Ason = asOn }).ToList();
            }
        }
      
     

        public IEnumerable<ClosingStock> GetCurrentStockData(int stockPointId, int itemCategoryId, int itemId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"SELECT ItemName,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               WHERE  SU.ItemId = ISNULL(NULLIF(@itmid, 0), SU.ItemId) AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId)
                               GROUP BY ItemName,UnitName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId}).ToList();
            }
        }
 

    }
}
