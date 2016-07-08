using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class MaterialPlanningRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<MaterialPlanning> GetMaterialPlanning(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select I.ItemId,I.PartNo,ItemName,UnitName,MinLevel
                ,ISNULL(sum(S.Quantity),0)CurrentStock,0WRQTY,0 WRPndIssQty ,0TotalQty,0InTransitQty,0PendingPRQty,0ShortorExcess INTO #TEMP FROM item I
                INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                LEFT JOIN StockUpdate S ON I.ItemId=S.ItemId
                GROUP BY I.ItemId,I.PartNo,ItemName,UnitName,MinLevel;
                           
                with W as (
                select ItemId, sum(Quantity)Quantity from WorkShopRequestItem group by ItemId
                )
                update T set T.WRQTY = W.Quantity from W inner join #TEMP T on T.ItemId = W.ItemId;

                with S as (
                SELECT  ItemId,sum(IssuedQuantity)IssuedQuantity FROM StoreIssueItem SI 
                INNER JOIN WorkShopRequestItem WI ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
                group by ItemId
                )
                update T set T.WRPndIssQty =  (T.WRQTY-S.IssuedQuantity) from S inner join #TEMP T on T.ItemId = S.ItemId;

                update T set T.TotalQty = (T.WRQTY+T.WRPndIssQty+T.MinLevel) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;

                with PR as (
                SELECT WI.ItemId,(SUM(WI.Quantity)-SUM(ISNULL(PI.Quantity,0)))PendingPRQty FROM WorkShopRequestItem WI
                LEFT JOIN PurchaseRequestItem PI ON WI.ItemId =PI.ItemId
                GROUP BY WI.ItemId
                )

                update T set T.PendingPRQty =  (PR.PendingPRQty) from PR inner join #TEMP T on T.ItemId = PR.ItemId;


                SELECT ItemId,SUM(ISNULL(GI.Quantity,0))GRNQTY INTO #TEMP2 FROM GRNItem GI WHERE  DirectPurchaseRequestItemId IS NULL
                GROUP BY ItemId ;

                SELECT PI.ItemId,SUM(SI.OrderedQty)SOQTY  INTO #TEMP1 from SupplyOrderItem SI INNER JOIN PurchaseRequestItem PI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                GROUP BY PI.ItemId ;

                with TR as (
                SELECT T1.ItemId,(SOQTY-GRNQTY)INTRANS FROM #TEMP2 T2 INNER JOIN #TEMP1 T1 ON T2.ItemId =T1.ItemId
                )
                update T set T.InTransitQty = INTRANS from TR inner join  #TEMP T on T.ItemId = TR.ItemId;



                update T set T.ShortorExcess = (T.CurrentStock+T.InTransitQty+T.PendingPRQty)-(T.TotalQty) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;

                SELECT * FROM #TEMP where ItemId in (select W.ItemId from SaleOrderItem SI inner join WorkVsItem W on SI.WorkDescriptionId=W.WorkDescriptionId WHERE SaleOrderId=@Id );

                drop table #TEMP;
                DROP TABLE #TEMP1;
                DROP TABLE #TEMP2;";



                return connection.Query<MaterialPlanning>(sql, new { Id = Id });
            }
        }
    }
}
