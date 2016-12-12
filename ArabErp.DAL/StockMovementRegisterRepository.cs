using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class StockMovementRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<ClosingStock> GetStockMovementData(DateTime? from, DateTime? to, int itmcatid, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"select I.ItemId,I.ItemRefNo,I.ItemName,U.UnitName,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='IN' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid ) INQTY,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='OUT' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid )*(-1) OUTQTY ,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE  StockType='OpeningStock' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid ) OPENINGSTOCK
                            from StockUpdate S  inner join Item I on I.ItemId=S.ItemId INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                            where S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid GROUP BY  I.ItemId,I.ItemRefNo,I.ItemName,ItemCategoryId,U.UnitName";



                return connection.Query<ClosingStock>(qry, new { itmcatid = itmcatid, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();

               
                
            }
        }
    }
}
