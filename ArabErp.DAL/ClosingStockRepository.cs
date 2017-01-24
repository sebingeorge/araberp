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
                string qry = @"SELECT ItemRefNo,ISNULL(PartNo,'-')PartNo,ItemName,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               WHERE  SU.ItemId = ISNULL(NULLIF(@itmid, 0), SU.ItemId) AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId) AND 
                               CONVERT(DATE, SU.stocktrnDate, 106)<=CONVERT(DATE, @Ason, 106)
                               GROUP BY ItemRefNo,PartNo,ItemName,UnitName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, Ason = asOn }).ToList();
            }
        }
        public IEnumerable<ClosingStock> GetClosingStockData1(DateTime? asOn, int stockPointId, int itemCategoryId, string itemId, int OrganizationId, string partno,int itmGroup, int itmSubgroup )
      {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                       
                string qry = @"SELECT ItemRefNo,ISNULL(PartNo,'-')PartNo,ItemName,I.ItemId,SUM(Quantity)Quantity,UnitName,0 AverageRate INTO #TEMP FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
								INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                               WHERE  ISNULL(I.isConsumable,0)=0 and I.ItemName LIKE '%'+@itmid+'%' AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId) AND 
                               I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId) and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                               AND CONVERT(DATE, SU.stocktrnDate, 106)<=CONVERT(DATE, @Ason, 106)
                                 and isnull(I.PartNo,'') like '%'+@partno+'%'
                               GROUP BY ItemRefNo,PartNo,ItemName,UnitName,I.ItemId

                                SELECT * into #A from 
                                (
                                SELECT MAX(GRNItemId)GRNItemId, ItemId, Rate FROM GRNItem GROUP BY ItemId, Rate
                                UNION
                                SELECT MAX(GRNItemId), ItemId, Rate FROM GRNItem WHERE GRNItemId NOT IN 
                                (SELECT MAX(GRNItemId) FROM GRNItem GROUP BY ItemId) GROUP BY ItemId, Rate
                                UNION
                                SELECT MAX(GRNItemId), ItemId, Rate FROM GRNItem WHERE GRNItemId NOT IN
                                (SELECT MAX(GRNItemId) FROM GRNItem GROUP BY ItemId
                                UNION
                                SELECT MAX(GRNItemId) FROM GRNItem WHERE GRNItemId NOT IN 
                                (SELECT MAX(GRNItemId) FROM GRNItem GROUP BY ItemId) GROUP BY ItemId) GROUP BY ItemId, Rate
                                )AS A;

                                with B as 
                                (
                                select  ItemId,(SUM(Rate)/count(ItemId))Average from #A  group by Rate,ItemId  
                                )
                                update T SET T.AverageRate=B.Average from B inner join #TEMP T ON T.ItemId=B.ItemId;
                                with C as 
                                (
                                select ItemId,Rate from StandardRate
                                )
                                update T SET T.AverageRate=C.Rate from C inner join #TEMP T ON T.ItemId=C.ItemId WHERE  T.AverageRate=0
                               
                                SELECT * FROM #TEMP";

                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, Ason = asOn, partno = partno, itmGroup = itmGroup, itmSubgroup = itmSubgroup }).ToList();
            }
        }


        public IEnumerable<ClosingStock> GetCurrentStockData(int stockPointId, int itemCategoryId, int itemId,string partno,int itmGroup, int itmSubgroup, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"SELECT ItemName,PartNo,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
							   INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
							   INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                               WHERE  I.ItemId=ISNULL(NULLIF(@itmid,0),I.ItemId) 
							   AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId 
							   AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId)
							   AND I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId)
							   and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                               and I.PartNo=ISNULL(NULLIF(@PartNo, 0), I.PartNo)
                               GROUP BY  I.ItemName,I.PartNo,U.UnitName,IG.ItemGroupName,IGS.ItemSubGroupName
                               ORDER BY I.ItemName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, partno = partno , itmGroup = itmGroup, itmSubgroup = itmSubgroup, OrganizationId = OrganizationId}).ToList();
            }
        }

        public IEnumerable<ClosingStock> GetCurrentStockDataDTPrint(int stockPointId, int itemCategoryId, int itemId, int OrganizationId,string partno,int itmGroup, int itmSubgroup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"SELECT ItemName,PartNo,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
							   INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
							   INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                               WHERE  I.ItemId=ISNULL(NULLIF(@itmid,0),I.ItemId) 
							   AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId 
							   AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId)
							   AND I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId)
							   and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                               and I.PartNo=ISNULL(NULLIF(@PartNo, 0), I.PartNo)
                               GROUP BY  I.ItemName,I.PartNo,U.UnitName,IG.ItemGroupName,IGS.ItemSubGroupName
                               ORDER BY I.ItemName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, partno = partno, itmGroup = itmGroup, itmSubgroup = itmSubgroup }).ToList();
            }
        }
        public IEnumerable<ClosingStock> GetClosingStockDataDTPrint( int stockPointId, int itemCategoryId, string itemId, int OrganizationId,string partno)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT ItemRefNo,ISNULL(PartNo,'-')PartNo,ItemName,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               WHERE ISNULL(I.isConsumable,0)=0 and  I.ItemName LIKE '%'+@itmid+'%' AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId) 
                               and isnull(I.PartNo,'') like '%'+@partno+'%'
                               GROUP BY ItemRefNo,PartNo,ItemName,UnitName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, partno = partno}).ToList();
            }
        }

    }
}
