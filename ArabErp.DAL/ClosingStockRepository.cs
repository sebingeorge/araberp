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
        public IEnumerable<ClosingStock> GetClosingStockData1(DateTime? asOn, int stockPointId, int itemCategoryId, string itemId, int OrganizationId, string partno, int itmGroup, int itmSubgroup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT I.ItemId,ItemRefNo,ISNULL(PartNo,'-')PartNo,ItemName,SUM(Quantity)Quantity,UnitName 
                                FROM StockUpdate SU 
                                INNER JOIN Item I ON I.ItemId=SU.ItemId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
								INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                                WHERE  ISNULL(I.isConsumable,0)=0 and I.ItemName LIKE '%'+@itmid+'%' AND I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                                AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId) AND 
                                I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId) and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                                AND CONVERT(DATE, SU.stocktrnDate, 106)<=CONVERT(DATE, @Ason, 106)
                                and isnull(I.PartNo,'') like '%'+@partno+'%'
                                GROUP BY I.ItemId,ItemRefNo,PartNo,ItemName,UnitName";
                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmcatid = itemCategoryId, itmid = itemId, OrganizationId = OrganizationId, Ason = asOn, partno = partno, itmGroup = itmGroup, itmSubgroup = itmSubgroup }).ToList();
            }
        }
        public IEnumerable<ClosingStockDrillDown> GetItemWiseDetails(DateTime? from, DateTime? to, int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = "exec ItemWiseDetails " + itemId.ToString();
                //string sql = "exec ItemWiseDetails " + from.Value + ',' + to.Value + ',' + itemId.ToString();
               string qry = @" SELECT DISTINCT I.ItemName,PurchaseRequestNo RefNo,PurchaseRequestDate Date,Quantity,'Direct Purchase' Type,U.UserName
                            FROM DirectPurchaseRequestItem DI
                            INNER JOIN DirectPurchaseRequest D ON D.DirectPurchaseRequestId=DI.DirectPurchaseRequestId
                            INNER JOIN Item I ON I.ItemId=DI.ItemId
                            INNER JOIN [User] U ON U.UserId=D.CreatedBy
                            WHERE I.ItemId=@itemId AND PurchaseRequestDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,PurchaseRequestNo,PurchaseRequestDate,Quantity,'Purchase Request'Type,U.UserName
                            FROM PurchaseRequestItem PI
                            INNER JOIN PurchaseRequest P ON P.PurchaseRequestId=PI.PurchaseRequestId
                            INNER JOIN Item I ON I.ItemId=PI.ItemId
                            INNER JOIN [User] U ON U.UserId=P.CreatedBy
                            WHERE PI.ItemId=@itemId AND PurchaseRequestDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,S.SupplyOrderNo,S.SupplyOrderDate,SI.OrderedQty,'Supply Order'Type,U.UserName
                            FROM SupplyOrderItem SI
                            INNER JOIN SupplyOrder S ON S.SupplyOrderId=SI.SupplyOrderId
                            INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestItemId=SI.PurchaseRequestItemId
                            INNER JOIN Item I ON I.ItemId=PI.ItemId
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy
                            WHERE I.ItemId=@itemId AND S.SupplyOrderDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,P.PurchaseBillRefNo,P.PurchaseBillDate,PI.Quantity,'Purchase Bill'Type,U.UserName 
                            FROM PurchaseBillItem PI
                            INNER JOIN PurchaseBill P ON P.PurchaseBillId=PI.PurchaseBillId
                            INNER JOIN GRNItem GI ON GI.GRNItemId=PI.GRNItemId
                            INNER JOIN Item I ON I.ItemId=GI.ItemId
                            INNER JOIN [User] U ON U.UserId=P.CreatedBy
                            WHERE GI.ItemId=@itemId AND PurchaseBillDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,G.GRNNo,G.GRNDate,GI.Quantity,'GRN'Type,U.UserName 
                            FROM GRNItem GI
                            INNER JOIN GRN G ON G.GRNId=GI.GRNId
                            INNER JOIN Item I ON I.ItemId=GI.ItemId
                            INNER JOIN [User] U ON U.UserId=G.CreatedBy
                            WHERE GI.ItemId=@itemId AND GRNDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,WorkShopRequestRefNo,WorkShopRequestDate,WI.Quantity,'WorkShop/Material-Request'Type,U.UserName
                            FROM WorkShopRequest W
                            INNER JOIN WorkShopRequestItem WI ON W.WorkShopRequestId=WI.WorkShopRequestId
                            INNER JOIN Item I ON I.ItemId=WI.ItemId
                            INNER JOIN [User] U ON U.UserId=W.CreatedBy
                            WHERE WI.ItemId=@itemId AND WorkShopRequestDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,StoreIssueRefNo,StoreIssueDate,SI.IssuedQuantity,'Store Issue'Type,U.UserName
                            FROM StoreIssue S
                            INNER JOIN StoreIssueItem SI ON S.StoreIssueId=SI.StoreIssueId
                            INNER JOIN WorkShopRequestItem WI ON WI.WorkShopRequestItemId=SI.WorkShopRequestItemId
                            INNER JOIN Item I ON I.ItemId=WI.ItemId
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy
                            WHERE WI.ItemId=@itemId AND StoreIssueDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,S.SaleOrderRefNo,S.SaleOrderDate,SI.Quantity,'Sale Order'Type ,U.UserName
                            FROM SaleOrder S
                            INNER JOIN SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                            INNER JOIN Item I ON I.ItemId=W.FreezerUnitId
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy 
                            WHERE W.FreezerUnitId=@itemId AND SaleOrderDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,S.SaleOrderRefNo,S.SaleOrderDate,SI.Quantity,'Sale Order'Type ,U.UserName
                            FROM SaleOrder S
                            INNER JOIN SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                            INNER JOIN Item I ON  I.ItemId=W.BoxId
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy
                            WHERE W.BoxId=@itemId AND SaleOrderDate BETWEEN @from AND @to
                            
                            UNION ALL

                            SELECT DISTINCT U.ItemName,SalesInvoiceRefNo,SalesInvoiceDate,SI.Quantity,'Sales Invoice'Type ,U1.UserName
                            FROM SalesInvoice S
                            INNER JOIN SalesInvoiceItem SI ON S.SalesInvoiceId=SI.SalesInvoiceId
                            INNER JOIN SaleOrderItem SOI ON SOI.SaleOrderItemId=SI.SaleOrderItemId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=SOI.WorkDescriptionId
                            INNER JOIN Item U ON U.ItemId=W.FreezerUnitId 
                            INNER JOIN [User] U1 ON U1.UserId=S.CreatedBy
                            WHERE W.FreezerUnitId=@itemId AND SalesInvoiceDate BETWEEN @from AND @to 

                            UNION ALL

                            SELECT DISTINCT B.ItemName,SalesInvoiceRefNo,SalesInvoiceDate,SI.Quantity,'Sales Invoice'Type,U.UserName 
                            FROM SalesInvoice S
                            INNER JOIN SalesInvoiceItem SI ON S.SalesInvoiceId=SI.SalesInvoiceId
                            INNER JOIN SaleOrderItem SOI ON SOI.SaleOrderItemId=SI.SaleOrderItemId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=SOI.WorkDescriptionId
                            INNER JOIN Item B ON B.ItemId=W.BoxId 
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy
                            WHERE W.BoxId=@itemId AND SalesInvoiceDate BETWEEN @from AND @to
                         
                            UNION ALL

                            SELECT DISTINCT I.ItemName,DeliveryChallanRefNo,DeliveryChallanDate,1 Quantity,'Delivery Note'Type,U.UserName
                            FROM DeliveryChallan D
                            INNER JOIN JobCard J ON J.JobCardId=D.JobCardId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=J.WorkDescriptionId
                            INNER JOIN Item I ON I.ItemId=W.FreezerUnitId 
                            INNER JOIN [User] U ON U.UserId=D.CreatedBy
                            WHERE W.FreezerUnitId=@itemId AND DeliveryChallanDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,DeliveryChallanRefNo,DeliveryChallanDate,1 Quantity,'Delivery Note'Type,U.UserName
                            FROM DeliveryChallan D
                            INNER JOIN JobCard J ON J.JobCardId=D.JobCardId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=J.WorkDescriptionId
                            INNER JOIN Item I ON I.ItemId=W.BoxId
                            INNER JOIN [User] U ON U.UserId=D.CreatedBy
                            WHERE  W.BoxId=@itemId AND DeliveryChallanDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,QuotationRefNo,QuotationDate,SI.Quantity,'Sales Quotation'Type ,U.UserName
                            FROM SalesQuotation S
                            INNER JOIN SalesQuotationItem SI ON S.SalesQuotationId=SI.SalesQuotationId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                            INNER JOIN Item I ON I.ItemId=W.FreezerUnitId
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy
                            WHERE W.FreezerUnitId=@itemId AND QuotationDate BETWEEN @from AND @to

                            UNION ALL

                            SELECT DISTINCT I.ItemName,QuotationRefNo,QuotationDate,SI.Quantity,'Sales Quotation'Type ,U.UserName
                            FROM SalesQuotation S
                            INNER JOIN SalesQuotationItem SI ON S.SalesQuotationId=SI.SalesQuotationId
                            INNER JOIN WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                            INNER JOIN Item I ON I.ItemId=W.BoxId
                            INNER JOIN [User] U ON U.UserId=S.CreatedBy
                            WHERE  W.BoxId=@itemId AND QuotationDate BETWEEN @from AND @to

                            ORDER BY Date Desc,Type";


               return connection.Query<ClosingStockDrillDown>(qry, new { itemId = itemId, from = from, to = to }).ToList();
            }
        }
        public IEnumerable<ClosingStock> GetClosingStockWithAvgRate(DateTime? asOn, int stockPointId, string itemId, int OrganizationId, string partno)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"SELECT ItemRefNo,ISNULL(PartNo,'-')PartNo,ItemName,I.ItemId,SUM(Quantity)Quantity,UnitName,0 AverageRate INTO #TEMP FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                                       INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                       INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
        							   INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                                       WHERE ISNULL(I.isConsumable,0)=0  and I.ItemName LIKE '%'+@itmid+'%'  
                                       AND SU.OrganizationId=@OrganizationId AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId)
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

                return connection.Query<ClosingStock>(qry, new { stkid = stockPointId, itmid = itemId, OrganizationId = OrganizationId, Ason = asOn, partno = partno}).ToList();
            }
        }

        public IEnumerable<ClosingStock> GetCurrentStockData(int stockPointId, int itemCategoryId, string itemId,string partno,int itmGroup, int itmSubgroup, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"SELECT ItemName,PartNo,SUM(Quantity)Quantity,UnitName FROM StockUpdate SU INNER JOIN Item I ON I.ItemId=SU.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
							   INNER JOIN ItemGroup  IG ON IG.ItemGroupId=I.ItemGroupId
							   INNER JOIN ItemSubGroup IGS ON IGS.ItemSubGroupId=I.ItemSubGroupId
                               WHERE  
							   I.ItemCategoryId=ISNULL(NULLIF(@itmcatid, 0), I.ItemCategoryId) 
                               AND SU.OrganizationId=@OrganizationId 
							   AND SU.StockPointId = ISNULL(NULLIF(@stkid, 0), SU.StockPointId)
							   AND I.ItemGroupId=ISNULL(NULLIF(@itmGroup,0),I.ItemGroupId)
							   and I.ItemSubGroupId=ISNULL(NULLIF(@itmSubgroup,0),I.ItemSubGroupId)
                               and isnull(I.PartNo,'') like '%'+@partno+'%'
                               and isnull(I.ItemName,'') like '%'+@itmid+'%'
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
                            -- and I.PartNo=ISNULL(NULLIF(@PartNo, 0), I.PartNo)
                               and isnull(I.PartNo,'') like '%'+@partno+'%'
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
