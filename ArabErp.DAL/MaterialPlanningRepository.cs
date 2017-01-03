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
        public IEnumerable<MaterialPlanning> GetMaterialPlanning(int? itmid,string partNo)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"select I.ItemId,I.PartNo,ItemName,UnitName,isnull(MinLevel,0)MinLevel
                ,ISNULL(sum(S.Quantity),0)CurrentStock,0WRQTY,0 WRPndIssQty ,0TotalQty,0InTransitQty,0PendingPRQty,0ShortorExcess,BatchRequired INTO #TEMP FROM item I
                INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                INNER JOIN ItemCategory IC ON IC.itmCatId=I.ItemCategoryId
                LEFT JOIN StockUpdate S ON I.ItemId=S.ItemId
                WHERE isnull(I.BatchRequired,0)=0 AND (isnull(I.FreezerUnit,0)=0 OR isnull(I.Box,0)=0)
                GROUP BY I.ItemId,I.PartNo,ItemName,UnitName,MinLevel,BatchRequired;
                           
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
                
                update T set T.TotalQty = ((T.WRPndIssQty+T.MinLevel)-T1.CurrentStock) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                
                SELECT ItemId,SUM(ISNULL(GI.Quantity,0))GRNQTY INTO #TEMP2 FROM GRNItem GI WHERE  DirectPurchaseRequestItemId IS NULL
                GROUP BY ItemId ;
                
                SELECT PI.ItemId,SUM(SI.OrderedQty)SOQTY  INTO #TEMP1 from SupplyOrderItem SI INNER JOIN PurchaseRequestItem PI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                GROUP BY PI.ItemId ;
                
                with TR as (
                SELECT T1.ItemId,(SOQTY-isnull(GRNQTY,0))INTRANS FROM #TEMP1 T1  LEFT JOIN #TEMP2 T2 ON T2.ItemId =T1.ItemId
                )
                update T set T.InTransitQty = INTRANS from TR inner join  #TEMP T on T.ItemId = TR.ItemId;
                
                with PR as (
                SELECT PI.ItemId,(SUM(ISNULL(PI.Quantity,0))-T.SOQTY)PRQty  FROM PurchaseRequestItem PI  LEFT JOIN #TEMP1 T ON T.ItemId =PI.ItemId
                GROUP BY  PI.ItemId,T.SOQTY
                )
                update T set T.PendingPRQty = isnull(PR.PRQty,0)  from PR INNER JOIN #TEMP T  on T.ItemId = PR.ItemId;
                
                update T set T.ShortorExcess = (T.InTransitQty+T.PendingPRQty)-(T.TotalQty) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                SELECT * FROM #TEMP where ItemId = ISNULL(NULLIF(@itmid, 0),ItemId);
              


                drop table #TEMP;
                DROP TABLE #TEMP1;
                DROP TABLE #TEMP2;";


                //where ItemId in (select W.ItemId from SaleOrderItem SI inner join WorkVsItem W on SI.WorkDescriptionId=W.WorkDescriptionId WHERE SaleOrderId=@Id)
                return connection.Query<MaterialPlanning>(sql, new { itmid = itmid, partNo = partNo}).ToList();
            }
        }
        public IEnumerable<MaterialPlanning> GetMaterialPlanningDTPrint(int itmid, string batch = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"select I.ItemId,I.PartNo,ItemName,UnitName,isnull(MinLevel,0)MinLevel
                ,ISNULL(sum(S.Quantity),0)CurrentStock,0WRQTY,0 WRPndIssQty ,0TotalQty,0InTransitQty,0PendingPRQty,0ShortorExcess,BatchRequired INTO #TEMP FROM item I
                INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                INNER JOIN ItemCategory IC ON IC.itmCatId=I.ItemCategoryId
                LEFT JOIN StockUpdate S ON I.ItemId=S.ItemId
                WHERE isnull(I.BatchRequired,0)=0 AND (isnull(I.FreezerUnit,0)=0 OR isnull(I.Box,0)=0)
                GROUP BY I.ItemId,I.PartNo,ItemName,UnitName,MinLevel,BatchRequired;
                           
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
                
                update T set T.TotalQty = ((T.WRPndIssQty+T.MinLevel)-T1.CurrentStock) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                
                SELECT ItemId,SUM(ISNULL(GI.Quantity,0))GRNQTY INTO #TEMP2 FROM GRNItem GI WHERE  DirectPurchaseRequestItemId IS NULL
                GROUP BY ItemId ;
                
                SELECT PI.ItemId,SUM(SI.OrderedQty)SOQTY  INTO #TEMP1 from SupplyOrderItem SI INNER JOIN PurchaseRequestItem PI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                GROUP BY PI.ItemId ;
                
                with TR as (
                SELECT T1.ItemId,(SOQTY-isnull(GRNQTY,0))INTRANS FROM #TEMP1 T1  LEFT JOIN #TEMP2 T2 ON T2.ItemId =T1.ItemId
                )
                update T set T.InTransitQty = INTRANS from TR inner join  #TEMP T on T.ItemId = TR.ItemId;
                
                with PR as (
                SELECT PI.ItemId,(SUM(ISNULL(PI.Quantity,0))-T.SOQTY)PRQty  FROM PurchaseRequestItem PI  LEFT JOIN #TEMP1 T ON T.ItemId =PI.ItemId
                GROUP BY  PI.ItemId,T.SOQTY
                )
                update T set T.PendingPRQty = isnull(PR.PRQty,0)  from PR INNER JOIN #TEMP T  on T.ItemId = PR.ItemId;
                
                update T set T.ShortorExcess = (T.InTransitQty+T.PendingPRQty)-(T.TotalQty) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                SELECT * FROM #TEMP where ItemId = ISNULL(NULLIF(@itmid, 0),ItemId);
              

               

                drop table #TEMP;
                DROP TABLE #TEMP1;
                DROP TABLE #TEMP2;";


                //where ItemId in (select W.ItemId from SaleOrderItem SI inner join WorkVsItem W on SI.WorkDescriptionId=W.WorkDescriptionId WHERE SaleOrderId=@Id)
                return connection.Query<MaterialPlanning>(sql, new { itmid = itmid, batch = batch }).ToList();
            }
        }
        public IEnumerable<MaterialPlanning> GetMaterialPlanningFG(int? itmid)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"select I.ItemId,I.PartNo,ItemName,UnitName,isnull(MinLevel,0)MinLevel
                ,ISNULL(sum(S.Quantity),0)CurrentStock,0SOQTY,0WRQTY,0PENWRQTY,0 WRPndIssQty ,0TotalQty,0InTransitQty,0PendingPRQty,0ShortorExcess,BatchRequired INTO #TEMP FROM item I
                INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                LEFT JOIN StockUpdate S ON I.ItemId=S.ItemId
                WHERE I.BatchRequired=1 AND (I.FreezerUnit=1 OR I.Box=1)
                GROUP BY I.ItemId,I.PartNo,ItemName,UnitName,MinLevel,BatchRequired;
                
                with S as (
                select ItemId, sum(Quantity)Quantity from SaleOrderItem S
                INNER JOIN WorkDescription W ON W.WorkDescriptionId = S.WorkDescriptionId
                INNER JOIN Item I ON I.ItemId=W.FreezerUnitId
                group by ItemId
                UNION ALL
                select ItemId, sum(Quantity)Quantity from SaleOrderItem S
                INNER JOIN WorkDescription W ON W.WorkDescriptionId = S.WorkDescriptionId
                INNER JOIN Item I ON I.ItemId=W.BoxId
                group by ItemId
                UNION ALL

                select ItemId,sum(Quantity)Quantity from SaleOrderMaterial S
                group by ItemId
                )
                update T set T.SOQTY = ISNULL(S.Quantity,0) from S inner join #TEMP T on T.ItemId = S.ItemId;
                           
                with W as (
                select ItemId, sum(Quantity)Quantity from WorkShopRequestItem group by ItemId
                )
                update T set T.WRQTY = W.Quantity from W inner join #TEMP T on T.ItemId = W.ItemId;
                
                update T set T.PENWRQTY =(T.SOQTY - T.WRQTY )from #TEMP T;
                
                with S as (
                SELECT  ItemId,sum(IssuedQuantity)IssuedQuantity FROM StoreIssueItem SI 
                INNER JOIN WorkShopRequestItem WI ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
                group by ItemId
                )
                update T set T.WRPndIssQty =  (T.WRQTY-S.IssuedQuantity) from S inner join #TEMP T on T.ItemId = S.ItemId;
                
                update T set T.TotalQty = ((T.PENWRQTY + T.WRPndIssQty+T.MinLevel)-T1.CurrentStock) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                
                SELECT ItemId,SUM(ISNULL(GI.Quantity,0))GRNQTY INTO #TEMP2 FROM GRNItem GI WHERE  DirectPurchaseRequestItemId IS NULL
                GROUP BY ItemId ;
                
                SELECT PI.ItemId,SUM(SI.OrderedQty)SOQTY  INTO #TEMP1 from SupplyOrderItem SI INNER JOIN PurchaseRequestItem PI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                GROUP BY PI.ItemId ;
                
                with TR as (
                SELECT T1.ItemId,(SOQTY-isnull(GRNQTY,0))INTRANS FROM #TEMP1 T1  LEFT JOIN #TEMP2 T2 ON T2.ItemId =T1.ItemId
                )
                update T set T.InTransitQty = INTRANS from TR inner join  #TEMP T on T.ItemId = TR.ItemId;
                
                with PR as (
                SELECT PI.ItemId,(SUM(ISNULL(PI.Quantity,0))-T.SOQTY)PRQty  FROM PurchaseRequestItem PI  LEFT JOIN #TEMP1 T ON T.ItemId =PI.ItemId
                GROUP BY  PI.ItemId,T.SOQTY
                )
                update T set T.PendingPRQty = (PR.PRQty) from PR INNER JOIN #TEMP T  on T.ItemId = PR.ItemId;
                
                update T set T.ShortorExcess = (T.InTransitQty+T.PendingPRQty)-(T.TotalQty) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                                
                SELECT * FROM #TEMP  where ItemId = ISNULL(NULLIF(@itmid, 0),ItemId);
         

                drop table #TEMP;
                DROP TABLE #TEMP1;
                DROP TABLE #TEMP2;";
                return connection.Query<MaterialPlanning>(sql, new { itmid = itmid}).ToList();
            }
        }
        public IEnumerable<MaterialPlanning> GetInTransitDetails(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"select SI.SupplyOrderItemId,S.SupplyOrderNo,SS.SupplierName,DATEDIFF(day,S.SupplyOrderDate, GETDATE()) Age,S.RequiredDate,DATEDIFF(day, GETDATE(),S.RequiredDate) DaysLeft,S.SupplyOrderDate,PI.itemid,OrderedQty,OrderedQty-isnull((SELECT Quantity  FROM GRNItem G WHERE  G.ItemId=PI.ItemId and SI.SupplyOrderItemId= G.SupplyOrderItemId ),0) InTrans
                               INTO #TEMP from SupplyOrderItem SI 
                               INNER JOIN SupplyOrder S ON S.SupplyOrderId=SI.SupplyOrderId
                               INNER JOIN PurchaseRequestItem PI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                               INNER JOIN Supplier SS ON SS.SupplierId=S.SupplierId
                               where PI.itemid=@id;
                               SELECT * FROM #TEMP WHERE InTrans>0";
                           

                return connection.Query<MaterialPlanning>(sql, new { id = id }).ToList();
            }
        }

        public IEnumerable<MaterialPlanning> GetMaterialPlanningFGPrint(int itmid)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"select I.ItemId,I.PartNo,ItemName,UnitName,isnull(MinLevel,0)MinLevel
                ,ISNULL(sum(S.Quantity),0)CurrentStock,0SOQTY,0WRQTY,0PENWRQTY,0 WRPndIssQty ,0TotalQty,0InTransitQty,0PendingPRQty,0ShortorExcess,BatchRequired INTO #TEMP FROM item I
                INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                LEFT JOIN StockUpdate S ON I.ItemId=S.ItemId
                WHERE I.BatchRequired=1 AND (I.FreezerUnit=1 OR I.Box=1)
                GROUP BY I.ItemId,I.PartNo,ItemName,UnitName,MinLevel,BatchRequired;
                
                with S as (
                select ItemId, sum(Quantity)Quantity from SaleOrderItem S
                INNER JOIN WorkDescription W ON W.WorkDescriptionId = S.WorkDescriptionId
                INNER JOIN Item I ON I.ItemId=W.FreezerUnitId
                group by ItemId
                UNION ALL
                select ItemId, sum(Quantity)Quantity from SaleOrderItem S
                INNER JOIN WorkDescription W ON W.WorkDescriptionId = S.WorkDescriptionId
                INNER JOIN Item I ON I.ItemId=W.BoxId
                group by ItemId
                UNION ALL

                select ItemId,sum(Quantity)Quantity from SaleOrderMaterial S
                group by ItemId
                )
                update T set T.SOQTY = ISNULL(S.Quantity,0) from S inner join #TEMP T on T.ItemId = S.ItemId;
                           
                with W as (
                select ItemId, sum(Quantity)Quantity from WorkShopRequestItem group by ItemId
                )
                update T set T.WRQTY = W.Quantity from W inner join #TEMP T on T.ItemId = W.ItemId;
                
                update T set T.PENWRQTY =(T.SOQTY - T.WRQTY )from #TEMP T;
                
                with S as (
                SELECT  ItemId,sum(IssuedQuantity)IssuedQuantity FROM StoreIssueItem SI 
                INNER JOIN WorkShopRequestItem WI ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
                group by ItemId
                )
                update T set T.WRPndIssQty =  (T.WRQTY-S.IssuedQuantity) from S inner join #TEMP T on T.ItemId = S.ItemId;
                
                update T set T.TotalQty = ((T.PENWRQTY + T.WRPndIssQty+T.MinLevel)-T1.CurrentStock) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                
                SELECT ItemId,SUM(ISNULL(GI.Quantity,0))GRNQTY INTO #TEMP2 FROM GRNItem GI WHERE  DirectPurchaseRequestItemId IS NULL
                GROUP BY ItemId ;
                
                SELECT PI.ItemId,SUM(SI.OrderedQty)SOQTY  INTO #TEMP1 from SupplyOrderItem SI INNER JOIN PurchaseRequestItem PI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                GROUP BY PI.ItemId ;
                
                with TR as (
                SELECT T1.ItemId,(SOQTY-isnull(GRNQTY,0))INTRANS FROM #TEMP1 T1  LEFT JOIN #TEMP2 T2 ON T2.ItemId =T1.ItemId
                )
                update T set T.InTransitQty = INTRANS from TR inner join  #TEMP T on T.ItemId = TR.ItemId;
                
                with PR as (
                SELECT PI.ItemId,(SUM(ISNULL(PI.Quantity,0))-T.SOQTY)PRQty  FROM PurchaseRequestItem PI  LEFT JOIN #TEMP1 T ON T.ItemId =PI.ItemId
                GROUP BY  PI.ItemId,T.SOQTY
                )
                update T set T.PendingPRQty = (PR.PRQty) from PR INNER JOIN #TEMP T  on T.ItemId = PR.ItemId;
                
                update T set T.ShortorExcess = (T.InTransitQty+T.PendingPRQty)-(T.TotalQty) from #TEMP T1 inner join #TEMP T on T.ItemId = T1.ItemId;
                
                SELECT * FROM #TEMP where ItemId = ISNULL(NULLIF(@itmid, 0),ItemId);
              

               

                drop table #TEMP;
                DROP TABLE #TEMP1;
                DROP TABLE #TEMP2;";


               return connection.Query<MaterialPlanning>(sql, new { itmid = itmid }).ToList();
            }
        }
    }
}
