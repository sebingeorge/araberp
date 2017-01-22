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
        public IEnumerable<ClosingStock> GetStockMovementData(DateTime? from, DateTime? to, int stkid, int itmcatid, int itmid, int OrganizationId, int ItemGroupId = 0, int ItemSubGroupId = 0,string PartNo="")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
//                string qry = @"select I.ItemId,I.ItemRefNo,I.ItemName,U.UnitName,
//                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='IN' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid ) INQTY,
//                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='OUT' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid )*(-1) OUTQTY ,
//                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE  StockType='OpeningStock' AND S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid ) OPENINGSTOCK
//                            from StockUpdate S  inner join Item I on I.ItemId=S.ItemId INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
//                            where S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid
//	                        AND S.StockPointId=ISNULL(NULLIF(@stkid,0),S.StockPointId)
//							AND I.ItemGroupId=ISNULL(NULLIF(@ItemGroupId,0),I.ItemGroupId)
//							AND I.ItemSubGroupId=ISNULL(NULLIF(@ItemSubGroupId, 0), I.ItemSubGroupId)
//							AND I.ItemId=ISNULL(NULLIF(@itmid, 0), I.ItemId)
//                            and I.PartNo=ISNULL(NULLIF(@PartNo, 0), I.PartNo)
//							GROUP BY  I.ItemId,I.ItemRefNo,I.ItemName,ItemCategoryId,U.UnitName";


                string qry = @"select I.ItemId,I.ItemRefNo,I.ItemName,U.UnitName,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='IN' AND S.ItemId=isnull(@itmid,S.ItemId) AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=isnull(@itmcatid,I.ItemCategoryId) ) INQTY,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='OUT' AND S.ItemId=isnull(@itmid,S.ItemId) AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=isnull(@itmcatid,I.ItemCategoryId) )*(-1) OUTQTY ,
                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE  StockType='OpeningStock' AND S.ItemId=isnull(@itmid,S.ItemId) AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=isnull(@itmcatid,I.ItemCategoryId) ) OPENINGSTOCK
                            from StockUpdate S  inner join Item I on I.ItemId=S.ItemId INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                            where S.ItemId=@itmid AND  S.OrganizationId=@OrganizationId AND I.ItemCategoryId=@itmcatid
	                        AND S.StockPointId=ISNULL(NULLIF(@stkid,0),S.StockPointId)
							AND I.ItemGroupId=ISNULL(NULLIF(@ItemGroupId,0),I.ItemGroupId)
							AND I.ItemSubGroupId=ISNULL(NULLIF(@ItemSubGroupId, 0), I.ItemSubGroupId)
							AND I.ItemId=ISNULL(NULLIF(@itmid, 0), I.ItemId)
                            and I.PartNo=ISNULL(NULLIF(@PartNo, 0), I.PartNo)
							GROUP BY  I.ItemId,I.ItemRefNo,I.ItemName,ItemCategoryId,U.UnitName";

//                string qry = @"select I.ItemId,I.ItemRefNo,I.ItemName,U.UnitName,
//                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='IN' AND S.ItemId=ISNULL(NULLIF( @itmid, 0),S.ItemId) AND  S.OrganizationId=1 AND I.ItemCategoryId=ISNULL(NULLIF( @itmcatid, 0),I.ItemCategoryId) ) INQTY,
//                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE StockInOut='OUT' AND S.ItemId=ISNULL(NULLIF( @itmid, 0),S.ItemId) AND  S.OrganizationId=1 AND I.ItemCategoryId=ISNULL(NULLIF( @itmcatid, 0),I.ItemCategoryId))*(-1) OUTQTY ,
//                            (SELECT SUM(S.Quantity)Quantity from StockUpdate S WHERE  StockType='OpeningStock' AND S.ItemId=ISNULL(NULLIF( @itmid, 0),S.ItemId) AND  S.OrganizationId=1 AND I.ItemCategoryId=ISNULL(NULLIF( @itmcatid, 0),I.ItemCategoryId)) OPENINGSTOCK
//                            from StockUpdate S  inner join Item I on I.ItemId=S.ItemId INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
//                            where S.ItemId=ISNULL(NULLIF( @itmid, 0),S.ItemId) AND  S.OrganizationId=1 AND I.ItemCategoryId=ISNULL(NULLIF( @itmcatid, 0),I.ItemCategoryId)
//	                        AND S.StockPointId=ISNULL(NULLIF(@stkid,0),S.StockPointId)
//							AND I.ItemGroupId=ISNULL(NULLIF(@ItemGroupId,0),I.ItemGroupId)
//							AND I.ItemSubGroupId=ISNULL(NULLIF(@ItemSubGroupId, 0), I.ItemSubGroupId)
//							AND I.ItemId=ISNULL(NULLIF(@itmid, 0), I.ItemId)
//                            and I.PartNo=ISNULL(NULLIF(@PartNo, 0), I.PartNo)
//							GROUP BY  I.ItemId,I.ItemRefNo,I.ItemName,ItemCategoryId,U.UnitName";

                return connection.Query<ClosingStock>(qry, new { itmcatid = itmcatid, stkid = stkid, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to, ItemGroupId = ItemGroupId, ItemSubGroupId = ItemSubGroupId, PartNo = PartNo }).ToList();

               
                
            }
        }
    }
}
