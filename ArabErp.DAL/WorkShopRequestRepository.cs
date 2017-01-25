using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkShopRequestRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");



        /// <summary>
        /// Insert WorkShopRequest
        /// </summary>
        /// <param name="model"></param>
        /// <returns>primary key of WorkShopRequest </returns>

        public string InsertWorkShopRequest(WorkShopRequest objWorkShopRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var internalId = "";
                    if (objWorkShopRequest.isProjectBased == 0)
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objWorkShopRequest.OrganizationId, 19, true, trn);
                    }
                    else
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objWorkShopRequest.OrganizationId, 31, true, trn);
                    }

                    objWorkShopRequest.WorkShopRequestRefNo = internalId;

                    string sql = @"insert  into WorkShopRequest(WorkShopRequestRefNo,WorkShopRequestDate,SaleOrderId,CustomerId,CustomerOrderRef,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId, SaleOrderItemId,SaleOrderItemUnitId,EvaConUnitId) 
                                    Values (@WorkShopRequestRefNo,@WorkShopRequestDate,@SaleOrderId,@CustomerId,@CustomerOrderRef,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId, @SaleOrderItemId,@SaleOrderItemUnitId,@EvaConUnitId);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                    var id = connection.Query<int>(sql, objWorkShopRequest, trn).Single();

                    foreach (WorkShopRequestItem item in objWorkShopRequest.Items)
                    {
                        item.WorkShopRequestId = id;
                        new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, trn);
                    }

                    InsertLoginHistory(dataConnection, objWorkShopRequest.CreatedBy, "Create", "Workshop Request", id.ToString(), "0");
                    trn.Commit();

                    return id + "|" + internalId;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return "0";
                }
            }
        }
        public List<WorkShopRequestItem> GetWorkShopRequestData(int SaleOrderId, int SaleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                #region old query 12.11.2016
                //                string query = @"SELECT I.ItemName,I.ItemId,I.PartNo,SUM(WI.Quantity)Quantity,UnitName 
                //                                       FROM WorkDescription W 
                //                                       INNER JOIN  WorkVsItem WI on W.WorkDescriptionId=WI.WorkDescriptionId
                //                                       INNER JOIN Item I ON WI.ItemId=I.ItemId 
                //                                       INNER JOIN Unit U on U.UnitId =I.ItemUnitId  
                //                                       INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId
                //                                       WHERE SI.SaleOrderId=@SaleOrderId GROUP BY I.ItemName,I.ItemId,I.PartNo,UnitName 
                //                                       UNION ALL SELECT I.ItemName,I.ItemId,I.PartNo,SUM(S.Quantity)Quantity,UnitName 
                //                                       FROM SaleOrderMaterial S INNER JOIN Item I ON I.ItemId=S.ItemId
                //                                       INNER JOIN Unit U on U.UnitId =I.ItemUnitId  
                //                                       WHERE S.SaleOrderId=@SaleOrderId 
                //                                       GROUP BY I.ItemName,I.ItemId,I.PartNo,UnitName
                //                                       ----------------------------Freezer Unit Set-----------------------------------
                //                                       UNION ALL 
                //                                       SELECT FU.ItemName,FU.ItemId,FU.PartNo,COUNT(ItemId) Quantity,UnitName 
                //                                       from WorkDescription W 
                //                                       INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId
                //                                       LEFT JOIN Item FU ON FU.ItemId=W.FreezerUnitId
                //                                       LEFT JOIN Unit U on U.UnitId =FU.ItemUnitId 
                //                                       WHERE SI.SaleOrderId=@SaleOrderId AND FU.FreezerUnit=1
                //                					   GROUP BY FU.ItemName,FU.ItemId,FU.PartNo,UnitName
                //                                       ------------------------------------Box Set-------------------------------------
                //                                       UNION ALL 
                //                                       SELECT B.ItemName,B.ItemId,B.PartNo,COUNT(ItemId) Quantity,UnitName 
                //                                       from WorkDescription W 
                //                                       INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId
                //                                       LEFT JOIN Item B ON B.ItemId=W.BoxId
                //                                       LEFT JOIN Unit U on U.UnitId =B.ItemUnitId
                //                                       WHERE SI.SaleOrderId=@SaleOrderId AND B.Box=1
                //                					   GROUP BY B.ItemName,B.ItemId,B.PartNo, UnitName"; 
                #endregion

                #region old query 16.12.2016 5.14p
//                                string query = @"SELECT T1.* 
//                                                INTO #TEMP1 FROM 
//                                                (SELECT
//                	                                    I_FRZR.ItemId, I_FRZR.ItemName, I_FRZR.PartNo, FRZR.Quantity, U.UnitName
//                                                    FROM SaleOrder SO
//                                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                                    INNER JOIN ItemVsBom FRZR ON WD.FreezerUnitId = FRZR.ItemId
//                                                    INNER JOIN Item I_FRZR ON FRZR.BomItemId = I_FRZR.ItemId
//                                                    INNER JOIN Unit U ON I_FRZR.ItemUnitId = U.UnitId
//                                                    WHERE SO.SaleOrderId=@SaleOrderId
//                                                    UNION ALL
//                                                    SELECT
//                	                                    I_BOX.ItemId, I_BOX.ItemName, I_BOX.PartNo, BOX.Quantity, U.UnitName
//                                                    FROM SaleOrder SO
//                                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                                    INNER JOIN ItemVsBom BOX ON WD.BoxId = BOX.ItemId
//                                                    INNER JOIN Item I_BOX ON BOX.BomItemId = I_BOX.ItemId
//                                                    INNER JOIN Unit U ON I_BOX.ItemUnitId = U.UnitId
//                                                    WHERE SO.SaleOrderId=@SaleOrderId
//                                                    UNION ALL
//                                                    SELECT
//                	                                    I1.ItemId, I1.ItemName, I1.PartNo, COUNT(ItemId), U.UnitName
//                                                    FROM SaleOrder SO
//                                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                                    INNER JOIN Item I1 ON WD.FreezerUnitId = I1.ItemId
//                                                    INNER JOIN Unit U ON I1.ItemUnitId = U.UnitId
//                                                    WHERE SO.SaleOrderId=@SaleOrderId
//                                                    GROUP BY I1.ItemId, I1.ItemName, I1.PartNo, U.UnitName
//                                                    UNION ALL
//                                                    SELECT
//                	                                    I2.ItemId, I2.ItemName, I2.PartNo, COUNT(ItemId), U.UnitName
//                                                    FROM SaleOrder SO
//                                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                                    INNER JOIN Item I2 ON WD.BoxId = I2.ItemId
//                                                    INNER JOIN Unit U ON I2.ItemUnitId = U.UnitId
//                                                    WHERE SO.SaleOrderId=@SaleOrderId
//                                                    GROUP BY I2.ItemId, I2.ItemName, I2.PartNo, U.UnitName
//                									UNION ALL
//                									SELECT
//                										I3.ItemId, I3.ItemName, I3.PartNo, SOM.Quantity, U.UnitName
//                									FROM SaleOrderMaterial SOM
//                									INNER JOIN SaleOrder SO ON SOM.SaleOrderId = SO.SaleOrderId
//                									INNER JOIN Item I3 ON SOM.ItemId = I3.ItemId
//                									INNER JOIN Unit U ON I3.ItemUnitId = U.UnitId
//                									WHERE SO.SaleOrderId = @SaleOrderId) T1;
//                
//                                                    SELECT
//                	                                    ItemId, ItemName, PartNo, SUM(Quantity) Quantity, UnitName
//                                                    FROM #TEMP1
//                                                    GROUP BY ItemId, ItemName, PartNo, UnitName
//                
//                                                    DROP TABLE #TEMP1;"; 
                #endregion

                #region old query 12.01.2017
//                string query = @"SELECT T1.* 
//                                INTO #TEMP1 FROM 
//                                (SELECT
//                	                    I_FRZR.ItemId, I_FRZR.ItemName, I_FRZR.PartNo, FRZR.Quantity, U.UnitName
//                                    FROM SaleOrder SO
//                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                    INNER JOIN ItemVsBom FRZR ON WD.FreezerUnitId = FRZR.ItemId
//                                    INNER JOIN Item I_FRZR ON FRZR.BomItemId = I_FRZR.ItemId
//                                    INNER JOIN Unit U ON I_FRZR.ItemUnitId = U.UnitId
//                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
//                                    UNION ALL
//                                    SELECT
//                	                    I_BOX.ItemId, I_BOX.ItemName, I_BOX.PartNo, BOX.Quantity, U.UnitName
//                                    FROM SaleOrder SO
//                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                    INNER JOIN ItemVsBom BOX ON WD.BoxId = BOX.ItemId
//                                    INNER JOIN Item I_BOX ON BOX.BomItemId = I_BOX.ItemId
//                                    INNER JOIN Unit U ON I_BOX.ItemUnitId = U.UnitId
//                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
//                                    UNION ALL
//                                    SELECT
//                	                    I1.ItemId, I1.ItemName, I1.PartNo, COUNT(ItemId), U.UnitName
//                                    FROM SaleOrder SO
//                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                    INNER JOIN Item I1 ON WD.FreezerUnitId = I1.ItemId
//                                    INNER JOIN Unit U ON I1.ItemUnitId = U.UnitId
//                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
//                                    GROUP BY I1.ItemId, I1.ItemName, I1.PartNo, U.UnitName
//                                    UNION ALL
//                                    SELECT
//                	                    I2.ItemId, I2.ItemName, I2.PartNo, COUNT(ItemId), U.UnitName
//                                    FROM SaleOrder SO
//                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
//                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
//                                    INNER JOIN Item I2 ON WD.BoxId = I2.ItemId
//                                    INNER JOIN Unit U ON I2.ItemUnitId = U.UnitId
//                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
//                                    GROUP BY I2.ItemId, I2.ItemName, I2.PartNo, U.UnitName
//                					UNION ALL
//                					SELECT
//                						I3.ItemId, I3.ItemName, I3.PartNo, SOM.Quantity, U.UnitName
//                					FROM SaleOrderMaterial SOM
//                					INNER JOIN SaleOrder SO ON SOM.SaleOrderId = SO.SaleOrderId
//                					INNER JOIN Item I3 ON SOM.ItemId = I3.ItemId
//                					INNER JOIN Unit U ON I3.ItemUnitId = U.UnitId
//                					WHERE SO.SaleOrderId = @SaleOrderId
//                                    UNION ALL
//                                    SELECT 
//                                        I.ItemId, I.ItemName, I.PartNo, IVB.Quantity, U.UnitName FROM ItemVsBom IVB
//                                    INNER JOIN Item I ON IVB.BomItemId = I.ItemId
//                                    INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
//                                    WHERE IVB.ItemId IN
//                                    (
//	                                    SELECT
//		                                    SOM.ItemId
//	                                    FROM SaleOrderMaterial SOM
//	                                    WHERE SOM.SaleOrderId = @SaleOrderId
//                                    )) T1;
//                
//                                    SELECT
//                	                    ItemId, ItemName, PartNo, SUM(Quantity) Quantity, UnitName
//                                    FROM #TEMP1
//                                    GROUP BY ItemId, ItemName, PartNo, UnitName
//                
//                                    DROP TABLE #TEMP1;";
 #endregion

                string query = @"SELECT T1.* 
                                INTO #TEMP1 FROM 
                                (SELECT
                	                    I_FRZR.ItemId, I_FRZR.ItemName, I_FRZR.PartNo, FRZR.Quantity, U.UnitName,'c' orderkey
                                    FROM SaleOrder SO
                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
                                    INNER JOIN ItemVsBom FRZR ON WD.FreezerUnitId = FRZR.ItemId
                                    INNER JOIN Item I_FRZR ON FRZR.BomItemId = I_FRZR.ItemId
                                    INNER JOIN Unit U ON I_FRZR.ItemUnitId = U.UnitId
                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
                                    UNION ALL
                                    SELECT
                	                    I_BOX.ItemId, I_BOX.ItemName, I_BOX.PartNo, BOX.Quantity, U.UnitName,'c' orderkey
                                    FROM SaleOrder SO
                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
                                    INNER JOIN ItemVsBom BOX ON WD.BoxId = BOX.ItemId
                                    INNER JOIN Item I_BOX ON BOX.BomItemId = I_BOX.ItemId
                                    INNER JOIN Unit U ON I_BOX.ItemUnitId = U.UnitId
                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
                                    UNION ALL
                                    SELECT
                	                    I1.ItemId, I1.ItemName, I1.PartNo, COUNT(ItemId), U.UnitName,'a' orderkey
                                    FROM SaleOrder SO
                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
                                    INNER JOIN Item I1 ON WD.FreezerUnitId = I1.ItemId
                                    INNER JOIN Unit U ON I1.ItemUnitId = U.UnitId
                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
                                    GROUP BY I1.ItemId, I1.ItemName, I1.PartNo, U.UnitName
                                    UNION ALL
                                    SELECT
                	                    I2.ItemId, I2.ItemName, I2.PartNo, COUNT(ItemId), U.UnitName,'b' orderkey
                                    FROM SaleOrder SO
                                    INNER JOIN SaleOrderItem SOI ON SO.SaleOrderId = SOI.SaleOrderId
                                    INNER JOIN WorkDescription WD ON WD.WorkDescriptionId = SOI.WorkDescriptionId
                                    INNER JOIN Item I2 ON WD.BoxId = I2.ItemId
                                    INNER JOIN Unit U ON I2.ItemUnitId = U.UnitId
                                    WHERE SOI.SaleOrderItemId=@SaleOrderItemId
                                    GROUP BY I2.ItemId, I2.ItemName, I2.PartNo, U.UnitName
                					UNION ALL
                					SELECT
                						I3.ItemId, I3.ItemName, I3.PartNo, SOM.Quantity, U.UnitName,'c' orderkey
                					FROM SaleOrderMaterial SOM
                					INNER JOIN SaleOrder SO ON SOM.SaleOrderId = SO.SaleOrderId
                					INNER JOIN Item I3 ON SOM.ItemId = I3.ItemId
                					INNER JOIN Unit U ON I3.ItemUnitId = U.UnitId
                					WHERE SO.SaleOrderId = @SaleOrderId
                                    UNION ALL
                                    SELECT 
                                        I.ItemId, I.ItemName, I.PartNo, IVB.Quantity, U.UnitName,'c' orderkey FROM ItemVsBom IVB
                                    INNER JOIN Item I ON IVB.BomItemId = I.ItemId
                                    INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
                                    WHERE IVB.ItemId IN
                                    (
	                                    SELECT
		                                    SOM.ItemId
	                                    FROM SaleOrderMaterial SOM
	                                    WHERE SOM.SaleOrderId = @SaleOrderId
                                    )) T1;
                
                                    SELECT
                	                    ItemId, ItemName, PartNo, SUM(Quantity) Quantity, UnitName
                                    FROM #TEMP1
                                    GROUP BY ItemId, ItemName, PartNo, UnitName,orderkey
                                    ORDER BY  orderkey    
                                    DROP TABLE #TEMP1;"; 
                return connection.Query<WorkShopRequestItem>(query,
                new { SaleOrderId = SaleOrderId, SaleOrderItemId = SaleOrderItemId }).ToList();
            }
        }
        public List<WorkShopRequestItem> GetWorkShopRequestDataForProject(int saleorderitem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {



                string query = @"select EvaporatorUnitId ITEMID into #T from SaleOrderItemUnit where SaleOrderItemId  = @saleorderitem
                                 union 
                                 select CondenserUnitId from SaleOrderItemUnit where SaleOrderItemId  = @saleorderitem
                                 union 
                                 select DoorId from SaleOrderItemDoor where SaleOrderItemId = @saleorderitem


                                SELECT T1.* 
                                INTO #TEMP1 FROM 
                                (  SELECT I.ItemId,I.ItemName,I.PartNo,U.Quantity,IU.UnitName,0 orderkey from SaleOrderItem SI 
                                    INNER JOIN SaleOrderItemUnit U ON SI.SaleOrderItemId=U.SaleOrderItemId
                                    INNER JOIN ITEM I ON I.ItemId=U.EvaporatorUnitId
                                    INNER JOIN Unit IU ON I.ItemUnitId = IU.UnitId
                                    where SI.SaleOrderItemId = @saleorderitem

                                    UNION ALL

                                    SELECT I.ItemId,I.ItemName,I.PartNo,U.Quantity,IU.UnitName,1 orderkey from SaleOrderItem SI 
                                    INNER JOIN SaleOrderItemUnit U ON SI.SaleOrderItemId=U.SaleOrderItemId
                                    INNER JOIN ITEM I ON I.ItemId=U.CondenserUnitId
                                    INNER JOIN Unit IU ON I.ItemUnitId = IU.UnitId
                                    where SI.SaleOrderItemId = @saleorderitem

                                    UNION ALL

                                    SELECT I.ItemId,I.ItemName,I.PartNo,U.Quantity,IU.UnitName,2 orderkey from SaleOrderItem SI 
                                    INNER JOIN SaleOrderItemDoor U ON SI.SaleOrderItemId=U.SaleOrderItemId
                                    INNER JOIN ITEM I ON I.ItemId=U.DoorId
                                    INNER JOIN Unit IU ON I.ItemUnitId = IU.UnitId
                                    where SI.SaleOrderItemId  = @saleorderitem

                                    UNION ALL

                                    SELECT I.ItemId,I.ItemName,I.PartNo,B.Quantity,IU.UnitName,3 orderkey FROM ItemVsBom B 
                                    INNER JOIN ITEM I ON I.ItemId = B.BomItemId
                                    INNER JOIN Unit IU ON I.ItemUnitId = IU.UnitId
                                    WHERE B.ItemId IN(SELECT ITEMID FROM #T) 
                                    ) T1;
                
                                    SELECT
                	                ItemId, ItemName, PartNo, SUM(Quantity) Quantity, UnitName
                                    FROM #TEMP1
                                    GROUP BY ItemId, ItemName, PartNo, UnitName,orderkey
                                    order by orderkey
                
                                    DROP TABLE #TEMP1
                                    DROP TABLE #T;";

                return connection.Query<WorkShopRequestItem>(query,
                new { saleorderitem = saleorderitem }).ToList();
            }
        }
        /// <summary>
        /// Sale order data for workshop request transaction
        /// </summary>
        /// <param name="SaleOrderId"></param>
        /// <returns></returns>
        public WorkShopRequest GetSaleOrderForWorkshopRequest(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"SELECT  SO.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,SO.SaleOrderDate SaleOrderDate,
                                SO.SaleOrderRefNo +' - '+ Replace(Convert(varchar,SaleOrderDate,106),' ','-') SaleOrderRefNo,SO.isProjectBased
                                FROM  SaleOrder SO  INNER JOIN Customer C  ON SO.CustomerId =C.CustomerId
                                WHERE SO.SaleOrderId =@SaleOrderId";
                var objSaleOrders = connection.Query<WorkShopRequest>(sql, new { SaleOrderId = SaleOrderId }).Single<WorkShopRequest>();

                return objSaleOrders;
            }
        }
        /// <summary>
        ///  select workshop description in workshop request transaction
        /// </summary>
        /// <param name="SaleOrderId"></param>
        /// <returns></returns>
        public WorkShopRequest GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"SELECT t.SaleOrderId,STUFF((SELECT DISTINCT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId  WHERE SO.SaleOrderId =@SaleOrderId
                             group by t.SaleOrderId";
                var objWorks = connection.Query<WorkShopRequest>(sql, new { SaleOrderId = SaleOrderId }).Single<WorkShopRequest>();

                return objWorks;
            }
        }
        public WorkShopRequest GetWorkShopRequest(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from WorkShopRequest
                        where WorkShopRequestId=@WorkShopRequestId";

                var objWorkShopRequest = connection.Query<WorkShopRequest>(sql, new
                {
                    WorkShopRequestId = WorkShopRequestId
                }).First<WorkShopRequest>();

                return objWorkShopRequest;
            }
        }

        public List<WorkShopRequest> GetWorkShopRequests()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkShopRequest
                        where isActive=1";

                var objWorkShopRequests = connection.Query<WorkShopRequest>(sql).ToList<WorkShopRequest>();

                return objWorkShopRequests;
            }
        }

//        public int UpdateWorkShopRequest(WorkShopRequest objWorkShopRequest)
//        {
//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {
//                string sql = string.Empty;
//                IDbTransaction txn = connection.BeginTransaction();


//                sql = @"UPDATE WorkShopRequest SET WorkShopRequestRefNo = @WorkShopRequestRefNo ,WorkShopRequestDate = @WorkShopRequestDate ,SaleOrderId = @SaleOrderId ,CustomerId = @CustomerId,CustomerOrderRef = @CustomerOrderRef,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,isAdditionalRequest=@isAdditionalRequest  WHERE WorkShopRequestId = @WorkShopRequestId;
//	                               
//                        DELETE FROM WorkShopRequestItem WHERE WorkShopRequestId = @WorkShopRequestId;";

//                try
//                {
//                    var id = connection.Execute(sql, objWorkShopRequest, txn);
//                    var saleorderitemrepo = new SalesQuotationItemRepository();

//                    foreach (var item in objWorkShopRequest.Items)
//                    {
//                        item.WorkShopRequestId = objWorkShopRequest.WorkShopRequestId;
//                        new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, txn);
//                    }


//                    InsertLoginHistory(dataConnection, objWorkShopRequest.CreatedBy, "Update", "Workshop Request", id.ToString(), objWorkShopRequest.OrganizationId.ToString());
//                    txn.Commit();
//                    return id;
//                }
//                catch (Exception ex)
//                {
//                    txn.Rollback();
//                    throw ex;
//                }
//            }
//        }
        public int UpdateWorkShopRequest(WorkShopRequest objWorkShopRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                IDbTransaction txn = connection.BeginTransaction();


                sql = @"UPDATE WorkShopRequest SET WorkShopRequestRefNo = @WorkShopRequestRefNo ,WorkShopRequestDate = @WorkShopRequestDate ,SaleOrderId = @SaleOrderId ,CustomerId = @CustomerId,CustomerOrderRef = @CustomerOrderRef,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,isAdditionalRequest=@isAdditionalRequest  WHERE WorkShopRequestId = @WorkShopRequestId;
	                               
                        --DELETE FROM WorkShopRequestItem WHERE WorkShopRequestId = @WorkShopRequestId;";

                try
                {
                    var id = connection.Execute(sql, objWorkShopRequest, txn);
                    #region get the list of items in use
                    var StoreIssued = objWorkShopRequest.Items.Where(x => x.isIssueUsed).Select(x => x.WorkShopRequestItemId).ToList();
                    #endregion

                    #region delete items that are not in use
                    sql = @"DELETE FROM WorkShopRequestItem WHERE WorkShopRequestId = @WorkShopRequestId AND WorkShopRequestItemId NOT IN @StoreIssued";
                    connection.Execute(sql, new { WorkShopRequestId = objWorkShopRequest.WorkShopRequestId, StoreIssued = StoreIssued }, txn);
                    #endregion

                    #region insert item that are not in use
                    var ItemList = objWorkShopRequest.Items.Where(x => !x.isIssueUsed || x.WorkShopRequestItemId == 0).Select(x => x).ToList();


                    foreach (var item in ItemList)
                    {
                        item.WorkShopRequestId = objWorkShopRequest.WorkShopRequestId;
                        item.WorkShopRequestItemId = item.WorkShopRequestItemId;
                        if (new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, txn) == 0)
                            throw new Exception("Some error occured while saving jobcard WorkshopRequestItem");
                    }
                    #endregion
                    //var saleorderitemrepo = new SalesQuotationItemRepository();

                    //foreach (var item in objWorkShopRequest.Items)
                    //{
                    //    item.WorkShopRequestId = objWorkShopRequest.WorkShopRequestId;
                    //    new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, txn);
                    //}


                    InsertLoginHistory(dataConnection, objWorkShopRequest.CreatedBy, "Update", "Workshop Request", id.ToString(), objWorkShopRequest.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }
        //        public int UpdateWorkShopRequest(WorkShopRequest objWorkShopRequest)
        //        {
        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                IDbTransaction txn = connection.BeginTransaction();
        //                string sql = @"UPDATE WorkShopRequest SET WorkShopRequestRefNo = @WorkShopRequestRefNo ,WorkShopRequestDate = @WorkShopRequestDate ,SaleOrderId = @SaleOrderId ,CustomerId = @CustomerId,CustomerOrderRef = @CustomerOrderRef,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkShopRequestId  WHERE WorkShopRequestId = @WorkShopRequestId;
        //
        //                  DELETE FROM WorkShopRequestItem WHERE WorkShopRequestId = @WorkShopRequestId;";

        //                var id = connection.Query<int>(sql, objWorkShopRequest, txn).Single();

        //                foreach (WorkShopRequestItem item in objWorkShopRequest.Items)
        //                {
        //                    item.WorkShopRequestId = id;
        //                    new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, txn);
        //                }


        //                InsertLoginHistory(dataConnection, objWorkShopRequest.CreatedBy, "Update", "Workshop Request", id.ToString(), "0");
        //                return id;
        //            }
        //        }


        public string DeleteWorkShopRequest(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string query = string.Empty;
                try
                {


                    query = @"DELETE FROM WorkShopRequestItem WHERE WorkShopRequestId = @WorkShopRequestId;
                               DELETE FROM WorkShopRequest OUTPUT deleted.WorkShopRequestRefNo WHERE WorkShopRequestId = @WorkShopRequestId;";
                    string output = connection.Query<string>(query, new { WorkShopRequestId = WorkShopRequestId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }


        /// <summary>
        /// Return details of a job card such as Sale Order No., Customer Name, Customer Order Ref. No.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WorkShopRequest GetJobCardDetails(int jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "SELECT SO.SaleOrderId, SO.SaleOrderRefNo, C.CustomerName, C.CustomerId, SO.CustomerOrderRef FROM JobCard JC INNER JOIN SaleOrder SO ON JC.SaleOrderId = SO.SaleOrderId INNER JOIN Customer C ON C.CustomerId = SO.CustomerId WHERE JC.JobCardId = @JobCardId";
                return connection.Query<WorkShopRequest>(query,
                    new { JobCardId = jobCardId }).Single();
            }
        }
        /// <summary>
        /// Returns the part number of a given item
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public string GetItemPartNo(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<string>("SELECT ISNULL(PartNo, '') FROM Item WHERE ItemId = @ItemId",
                new { ItemId = itemId }).First<string>();
            }
        }

        /// <summary>
        /// Insert additional workshop request head table (WorkShopRequest table)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertAdditionalWorkshopRequest(WorkShopRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string internalId = "";
                    if(model.isProjectBased==0)
                    {
                         internalId = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 20, true, txn);
                    }
                    else
                    {
                         internalId = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 43, true, txn);
                    }
                    model.WorkShopRequestRefNo = internalId.ToString();
                    string query = @"INSERT INTO WorkShopRequest(
                                    WorkShopRequestRefNo, 
                                    WorkShopRequestDate, 
                                    SaleOrderId, 
                                    CustomerId, 
                                    CustomerOrderRef, 
                                    SpecialRemarks, 
                                    RequiredDate, 
                                    CreatedBy, 
                                    CreatedDate, 
                                    OrganizationId, 
                                    isActive, 
                                    isAdditionalRequest, 
                                    JobCardId)
                                VALUES(
                                    @WorkShopRequestRefNo,
                                    @WorkShopRequestDate, 
                                    @SaleOrderId, 
                                    @CustomerId, 
                                    @CustomerOrderRef, 
                                    @SpecialRemarks, 
                                    @RequiredDate, 
                                    @CreatedBy, 
                                    @CreatedDate, 
                                    @OrganizationId, 
                                    1, 
                                    1, 
                                    @JobCardId);

                                SELECT CAST(SCOPE_IDENTITY() as int)";


                    int id = connection.Query<int>(query, model, txn).First();
                    foreach (var item in model.Items)
                    {
                        item.WorkShopRequestId = id;
                        new WorkShopRequestItemRepository().InsertAdditionalWorkshopRequestItem(item, connection, txn);
                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Additional Workshop Request", id.ToString(), "0");
                    txn.Commit();
                    return id;
                }
                catch (Exception)
                {

                    txn.Rollback();
                    return 0;
                }
            }
        }
        /// <summary>
        /// Returns all pending workshop requests
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WorkShopRequest> PendingWorkshopRequests(string Request = "", string Sale = "", string Customer = "", string jcno = "", string RegNo = "", string Type = "all")
       {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 30.12.2016 5.53p
                //                string sql = @"SELECT WorkShopRequestId, SUM(Quantity) Quantity INTO #WORK FROM WorkShopRequestItem 
                //                inner join ITEM I ON I.ItemId= WorkShopRequestItem.ItemId where isnull(I.isConsumable,0)=0 GROUP BY WorkShopRequestId;
                //                SELECT WorkShopRequestId, SUM(IssuedQuantity) IssuedQuantity INTO #ISSUE FROM StoreIssueItem SII INNER JOIN StoreIssue SI ON  SII.StoreIssueId = SI.StoreIssueId GROUP BY WorkShopRequestId;
                //                SELECT CustomerId, CustomerName INTO #CUSTOMER FROM Customer;
                //                SELECT SaleOrderId, ISNULL(SaleOrderRefNo, '')+' - '+CONVERT(VARCHAR, SaleOrderDate, 106) SoNoWithDate INTO #SALE FROM SaleOrder;
                //                SELECT distinct W.WorkShopRequestId,WR.isDirectRequest ,ISNULL(WR.WorkShopRequestRefNo, '')+' - '+CAST(CONVERT(VARCHAR, WR.WorkShopRequestDate, 106) AS VARCHAR) WorkShopRequestRefNo, CONVERT(DATETIME, WR.RequiredDate, 106) RequiredDate, C.CustomerName, S.SoNoWithDate,
                //                 DATEDIFF(day, WR.WorkShopRequestDate, GETDATE()) Ageing,
                //                 DATEDIFF(day, GETDATE(), WR.RequiredDate) DaysLeft,
                //
                //              --STUFF((SELECT ', '+T1.JobCardNo FROM JobCard T1 WHERE T1.SaleOrderId = S.SaleOrderId FOR XML PATH('')), 1, 2, '') JobCardNo,
                //                CASE WHEN WR.SaleOrderItemId = 0 THEN STUFF((SELECT ', '+T1.JobCardNo+' - '+CONVERT(VARCHAR, T1.JobCardDate, 106) FROM JobCard T1 WHERE ISNULL(T1.JobCardNo, '')LIKE '%'+@jcno+'%' AND T1.SaleOrderId = S.SaleOrderId FOR XML PATH('')), 1, 2, '')	
                //                ELSE (SELECT JobCardNo+' - '+CONVERT(VARCHAR, JobCardDate, 106) FROM JobCard WHERE SaleOrderItemId = WR.SaleOrderItemId AND ISNULL(JobCardNo, '')LIKE '%'+@jcno+'%') END JobCardNo,
                //
                //                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,ISNULL(ChassisNo,'')ChassisNo,ISNULL(RegistrationNo,'')RegistrationNo
                //                FROM #WORK W LEFT JOIN #ISSUE I ON W.WorkShopRequestId = I.WorkShopRequestId INNER JOIN WorkShopRequest WR ON W.WorkShopRequestId = WR.WorkShopRequestId left JOIN #CUSTOMER C ON WR.CustomerId = C.CustomerId left JOIN #SALE S ON WR.SaleOrderId = S.SaleOrderId 
                //                LEFT JOIN JobCard JC ON WR.JobCardId = JC.JobCardId
                //				LEFT JOIN VehicleInPass V ON V.VehicleInPassId=JC.InPassId 
                //				LEFT JOIN JobCard J ON S.SaleOrderId=J.SaleOrderId
                //				WHERE ISNULL(IssuedQuantity,0) < Quantity and  (case when isnull(WR.isDirectRequest,0)=1 then isnull(WR.isApproved,0)else 1 end)=1 
                //				AND  WorkShopRequestRefNo LIKE '%'+@Request+'%'
                //				AND ISNULL(SoNoWithDate,'') LIKE '%'+@Sale+'%'
                //				AND ISNULL(CustomerName,'') LIKE '%'+@Customer+'%'
                //				AND (ISNULL(V.RegistrationNo, '') LIKE '%'+@RegNo+'%'
                //				OR ISNULL(V.ChassisNo, '') LIKE '%'+@RegNo+'%')
                //				AND ISNULL(J.JobCardNo, '') LIKE '%'+@jcno+'%'
                //                --ORDER BY WR.WorkShopRequestDate DESC;
                //                DROP TABLE #ISSUE;
                //                DROP TABLE #WORK;
                //                DROP TABLE #CUSTOMER;
                //                DROP TABLE #SALE;"; 
                #endregion

                string sql = @"--SELECT WorkShopRequestId, SUM(Quantity) Quantity INTO #WORK FROM WorkShopRequestItem 
                                --inner join ITEM I ON I.ItemId= WorkShopRequestItem.ItemId where isnull(I.isConsumable,0)=0 GROUP BY WorkShopRequestId;

                                --SELECT WorkShopRequestId, SUM(IssuedQuantity) IssuedQuantity INTO #ISSUE FROM StoreIssueItem SII INNER JOIN StoreIssue SI ON  SII.StoreIssueId = SI.StoreIssueId GROUP BY WorkShopRequestId;

                                ------------------------------------------------------------------workshop requests with pending issue
                                SELECT
	                                WRI.WorkShopRequestId,
	                                WRI.ItemId,
	                                SUM(WRI.Quantity) Quantity
                                INTO #TEMP1
                                FROM WorkShopRequest WR
                                INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                                INNER JOIN Item I ON WRI.ItemId = I.ItemId
                                WHERE ISNULL(I.isConsumable, 0) = 0
                                GROUP BY WRI.ItemId, WRI.WorkShopRequestId

                                SELECT
	                                WRI.WorkShopRequestId,
	                                WRI.ItemId,
	                                SUM(SII.IssuedQuantity) Quantity
                                INTO #TEMP2
                                FROM WorkShopRequest WR
                                INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                                INNER JOIN Item I ON WRI.ItemId = I.ItemId
                                LEFT JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                                WHERE ISNULL(I.isConsumable, 0) = 0 
                                GROUP BY WRI.ItemId, WRI.WorkShopRequestId

                                SELECT DISTINCT
	                                #TEMP1.WorkShopRequestId
                                INTO #WORK
                                FROM #TEMP1
	                                LEFT JOIN #TEMP2 ON #TEMP1.ItemId = #TEMP2.ItemId AND #TEMP1.WorkShopRequestId = #TEMP2.WorkShopRequestId
                                WHERE #TEMP1.Quantity > ISNULL(#TEMP2.Quantity, 0) order by WorkShopRequestId

                                DROP TABLE #TEMP2;
                                DROP TABLE #TEMP1;
                                ------------------------------------------------------------------  

                                SELECT CustomerId, CustomerName INTO #CUSTOMER FROM Customer;

                                SELECT SaleOrderId, ISNULL(SaleOrderRefNo, '')+' - '+CONVERT(VARCHAR, SaleOrderDate, 106) SoNoWithDate,isProjectBased INTO #SALE FROM SaleOrder;

                                SELECT distinct W.WorkShopRequestId,WR.isDirectRequest ,ISNULL(WR.WorkShopRequestRefNo, '')+' - '+CAST(CONVERT(VARCHAR, WR.WorkShopRequestDate, 106) AS VARCHAR) WorkShopRequestRefNo, CONVERT(DATETIME, WR.RequiredDate, 106) RequiredDate, C.CustomerName, S.SoNoWithDate,
                                    DATEDIFF(day, WR.WorkShopRequestDate, GETDATE()) Ageing,
                                    DATEDIFF(day, GETDATE(), WR.RequiredDate) DaysLeft,
                
                              --STUFF((SELECT ', '+T1.JobCardNo FROM JobCard T1 WHERE T1.SaleOrderId = S.SaleOrderId FOR XML PATH('')), 1, 2, '') JobCardNo,
                                CASE WHEN WR.SaleOrderItemId = 0 THEN STUFF((SELECT ', '+T1.JobCardNo+' - '+CONVERT(VARCHAR, T1.JobCardDate, 106) FROM JobCard T1 WHERE ISNULL(T1.JobCardNo, '')LIKE '%'+@jcno+'%' AND T1.SaleOrderId = S.SaleOrderId FOR XML PATH('')), 1, 2, '')	
                                ELSE (SELECT JobCardNo+' - '+CONVERT(VARCHAR, JobCardDate, 106) FROM JobCard WHERE SaleOrderItemId = WR.SaleOrderItemId AND ISNULL(JobCardNo, '')LIKE '%'+@jcno+'%') END JobCardNo,
                
                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,ISNULL(ChassisNo,'')ChassisNo,ISNULL(RegistrationNo,'')RegistrationNo
                                FROM #WORK W 
                                INNER JOIN WorkShopRequest WR ON W.WorkShopRequestId = WR.WorkShopRequestId 
                                left JOIN #CUSTOMER C ON WR.CustomerId = C.CustomerId 
                                left JOIN #SALE S ON WR.SaleOrderId = S.SaleOrderId 
                                LEFT JOIN JobCard JC ON WR.JobCardId = JC.JobCardId
                                LEFT JOIN VehicleInPass V ON V.VehicleInPassId=JC.InPassId 
                                LEFT JOIN JobCard J ON S.SaleOrderId=J.SaleOrderId
                                WHERE /*ISNULL(IssuedQuantity,0) < Quantity and*/ (case when isnull(WR.isDirectRequest,0)=1 then isnull(WR.isApproved,0)else 1 end)=1 
 								AND ISNULL(S.isProjectBased, 0) = CASE @Type WHEN 'project' THEN 1 WHEN 'transport' THEN 0 WHEN 'all' THEN ISNULL(S.isProjectBased, 0) END
                				AND  WorkShopRequestRefNo LIKE '%'+@Request+'%'
                				AND ISNULL(SoNoWithDate,'') LIKE '%'+@Sale+'%'
                				AND ISNULL(CustomerName,'') LIKE '%'+@Customer+'%'
                				AND (ISNULL(V.RegistrationNo, '') LIKE '%'+@RegNo+'%'
                				OR ISNULL(V.ChassisNo, '') LIKE '%'+@RegNo+'%')
                				AND ISNULL(J.JobCardNo, '') LIKE '%'+@jcno+'%'
                                --ORDER BY WR.WorkShopRequestDate DESC;
                                DROP TABLE #WORK;
                                DROP TABLE #CUSTOMER;
                                DROP TABLE #SALE;";

                return connection.Query<WorkShopRequest>(sql, new { Request = Request, Sale = Sale, Customer = Customer, jcno = jcno, RegNo = RegNo, Type = Type }).ToList();
            }
        }
        public IEnumerable<WorkShopRequest> GetPrevious(int isProjectBased, DateTime? from, DateTime? to, string workshop, string customer, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"Select * from WorkShopRequest WR INNER JOIN Customer C on C.CustomerId=WR.CustomerId 
                               INNER JOIN SaleOrder S on S.SaleOrderId=WR.SaleOrderId
                               where  WR.WorkShopRequestRefNo LIKE '%'+@workshop+'%'
                               and C.CustomerName LIKE '%'+@customer+'%' and   WR.isActive=1 and WR.OrganizationId=@OrganizationId AND S.isProjectBased=@isProjectBased AND WR.WorkShopRequestDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())";
                return connection.Query<WorkShopRequest>(qry, new { isProjectBased = isProjectBased, workshop = workshop, customer = customer, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }
        public WorkShopRequest GetWorkshopRequestHdData(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"SELECT 
	                                WR.*,
	                                S.SaleOrderRefNo,
	                                S.EDateArrival,
	                                S.EDateDelivery,
	                                STUFF((SELECT DISTINCT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()] FROM SaleOrderItem SI 
	                                inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
	                                WHERE SI.SaleOrderId = S.SaleOrderId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,
	                                S.isProjectBased,
	                                C.CustomerName
                                from WorkShopRequest WR 
	                                INNER JOIN SaleOrder S on S.SaleOrderId=WR.SaleOrderId
	                                INNER JOIN  Customer C  ON S.CustomerId =C.CustomerId
                                WHERE WorkShopRequestId = @WorkShopRequestId";
                var objWrkshopRequests = connection.Query<WorkShopRequest>(sql, new { WorkShopRequestId = WorkShopRequestId }).Single<WorkShopRequest>();
                try
                {
                    sql = @"SELECT WorkShopRequestId FROM PurchaseRequest WHERE WorkShopRequestId=@WorkShopRequestId";
                    objWrkshopRequests.Isused = Convert.ToBoolean(connection.Query<int>(sql, new { WorkShopRequestId = WorkShopRequestId }).First());
                }
                catch
                {
                    objWrkshopRequests.Isused = false;

                }
                try
                {
                    sql = @"SELECT WorkShopRequestId FROM StoreIssue WHERE WorkShopRequestId=@WorkShopRequestId";
                    objWrkshopRequests.IsStoreused = Convert.ToBoolean(connection.Query<int>(sql, new { WorkShopRequestId = WorkShopRequestId }).First());

                }
                catch
                {

                    objWrkshopRequests.IsStoreused = false;
                }

                return objWrkshopRequests;
            }
        }
        public List<WorkShopRequestItem> GetWorkShopRequestDtData(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                #region old query - doesnt show the actual quantity given in work description
                //                string query = @"select 
                //                                    I.ItemId,
                //                                    I.ItemName,
                //                                    I.PartNo,
                //                                    WI.Remarks,
                //                                    WI.Quantity,
                //                                    UnitName,
                //                                    WI.isAddtionalMaterialRequest,
                //                                    WI.Quantity ActualQuantity
                //                                    from WorkShopRequestItem WI 
                //                                    INNER JOIN Item I ON WI.ItemId=I.ItemId
                //                                    INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                //                                    where WorkShopRequestId = @WorkShopRequestId"; 
                #endregion

                string query = @"SELECT * INTO #TABLE1 FROM(
                                    SELECT I.ItemName,I.ItemId,I.PartNo,SUM(WI.Quantity)Quantity,UnitName 
                                    FROM WorkDescription W 
                                    INNER JOIN  WorkVsItem WI on W.WorkDescriptionId=WI.WorkDescriptionId
                                    INNER JOIN Item I ON WI.ItemId=I.ItemId 
                                    INNER JOIN Unit U on U.UnitId =I.ItemUnitId  
                                    INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId
                                    WHERE SI.SaleOrderId=(SELECT SaleOrderId FROM WorkShopRequest WHERE WorkShopRequestId=@WorkShopRequestId) GROUP BY I.ItemName,I.ItemId,I.PartNo,UnitName 
                                    UNION ALL SELECT I.ItemName,I.ItemId,I.PartNo,SUM(S.Quantity)Quantity,UnitName 
                                    FROM SaleOrderMaterial S INNER JOIN Item I ON I.ItemId=S.ItemId
                                    INNER JOIN Unit U on U.UnitId =I.ItemUnitId  
                                    WHERE S.SaleOrderId=(SELECT SaleOrderId FROM WorkShopRequest WHERE WorkShopRequestId=@WorkShopRequestId) 
                                    GROUP BY I.ItemName,I.ItemId,I.PartNo,UnitName
                                    ----------------------------Freezer Unit Set-----------------------------------
                                    UNION ALL 
                                    SELECT FU.ItemName,FU.ItemId,FU.PartNo,COUNT(ItemId) Quantity,UnitName 
                                    from WorkDescription W 
                                    INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId
                                    LEFT JOIN Item FU ON FU.ItemId=W.FreezerUnitId
                                    LEFT JOIN Unit U on U.UnitId =FU.ItemUnitId 
                                    WHERE SI.SaleOrderId=(SELECT SaleOrderId FROM WorkShopRequest WHERE WorkShopRequestId=@WorkShopRequestId) AND FU.FreezerUnit=1
                                    GROUP BY FU.ItemName,FU.ItemId,FU.PartNo,UnitName
                                    ------------------------------------Box Set-------------------------------------
                                    UNION ALL 
                                    SELECT B.ItemName,B.ItemId,B.PartNo,COUNT(ItemId) Quantity,UnitName 
                                    from WorkDescription W 
                                    INNER JOIN SaleOrderItem SI ON SI.WorkDescriptionId = W.WorkDescriptionId
                                    LEFT JOIN Item B ON B.ItemId=W.BoxId
                                    LEFT JOIN Unit U on U.UnitId =B.ItemUnitId
                                    WHERE SI.SaleOrderId=(SELECT SaleOrderId FROM WorkShopRequest WHERE WorkShopRequestId=@WorkShopRequestId) AND B.Box=1
                                    GROUP BY B.ItemName,B.ItemId,B.PartNo, UnitName
                                    ) AS TABLE1

                                    select 
	                                    I.ItemId,
	                                    I.ItemName,
	                                    I.PartNo,
	                                    WI.Remarks,
	                                    WI.Quantity,
	                                    U.UnitName,
	                                    WI.isAddtionalMaterialRequest,
	                                    ISNULL(T1.Quantity, 0) ActualQuantity
                                    from WorkShopRequestItem WI 
	                                    INNER JOIN Item I ON WI.ItemId=I.ItemId
	                                    INNER JOIN Unit U on U.UnitId =I.ItemUnitId
	                                    LEFT JOIN #TABLE1 T1 ON WI.ItemId = T1.ItemId
                                    where WorkShopRequestId = @WorkShopRequestId

                                    DROP TABLE #TABLE1";

                return connection.Query<WorkShopRequestItem>(query,
                new { WorkShopRequestId = WorkShopRequestId }).ToList();
            }
        }



        public IEnumerable<WorkShopRequest> PreviousList(int isProjectBased, int OrganizationId, DateTime? from, DateTime? to, int id = 0, int customer = 0, int jobcard = 0)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = @"SELECT
	                                WR.WorkShopRequestId,
	                                WR.WorkShopRequestRefNo,
	                                CONVERT(VARCHAR, WR.WorkShopRequestDate, 106) WorkshopRequestDate,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                CUS.CustomerName,
	                                ISNULL(WR.SpecialRemarks ,'-') SpecialRemarks,
	                                JC.JobCardNo,
	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate
                                FROM WorkShopRequest WR
	                                INNER JOIN SaleOrder SO ON WR.SaleOrderId = SO.SaleOrderId
	                                INNER JOIN JobCard JC ON WR.JobCardId = JC.JobCardId
	                                INNER JOIN Customer CUS ON WR.CustomerId = CUS.CustomerId
                                WHERE isAdditionalRequest = 1
	                                AND WR.isActive = 1
	                                AND WR.OrganizationId = @OrganizationId and SO.isProjectBased=@isProjectBased 
                                    AND CONVERT(DATE, WR.WorkShopRequestDate, 106) BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                    AND WR.WorkShopRequestId = ISNULL(NULLIF(CAST(@id AS INT), 0), WR.WorkShopRequestId)
                                    AND WR.JobCardId = ISNULL(NULLIF(CAST(@jobcard AS INT), 0), WR.JobCardId)
                                    AND CUS.CustomerId = ISNULL(NULLIF(CAST(@customer AS INT), 0), CUS.CustomerId)";

                return connection.Query<WorkShopRequest>(query, new
                {
                    OrganizationId = OrganizationId,
                    from = from,
                    to = to,
                    customer = customer,
                    id = id,
                    jobcard = jobcard,
                    isProjectBased = isProjectBased
                }).ToList();
            }
        }

        public WorkShopRequest WorkShopRequestHD(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT  WR.*,JobCardId,SO.SaleOrderRefNo,C.CustomerName, SO.isProjectBased
                                FROM WorkShopRequest WR
                                INNER JOIN SaleOrder SO ON SO.SaleOrderId=WR.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=WR.CustomerId
                                WHERE WR.WorkShopRequestId=@WorkShopRequestId";

                var objWorkShopRequest = connection.Query<WorkShopRequest>(sql, new
                {
                    WorkShopRequestId = WorkShopRequestId
                }).First<WorkShopRequest>();
                try
                {
                    sql = @"SELECT WorkShopRequestId FROM PurchaseRequest WHERE WorkShopRequestId=@WorkShopRequestId";
                    objWorkShopRequest.Isused = Convert.ToBoolean(connection.Query<int>(sql, new { WorkShopRequestId = WorkShopRequestId }).First());
                }
                catch
                {
                    objWorkShopRequest.Isused = false;

                }
                try
                {
                    sql = @"SELECT WorkShopRequestId FROM StoreIssue WHERE WorkShopRequestId=@WorkShopRequestId";
                    objWorkShopRequest.IsStoreused = Convert.ToBoolean(connection.Query<int>(sql, new { WorkShopRequestId = WorkShopRequestId }).First());

                }
                catch
                {

                    objWorkShopRequest.IsStoreused = false;
                }
                return objWorkShopRequest;
            }
        }
        public WorkShopRequest GetWorkshopRequestHdDataPrint(int WorkShopRequestId, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT O.*,
	                                WR.*,
	                                S.SaleOrderRefNo,
	                                S.EDateArrival,
	                                S.EDateDelivery,
		                           ORR.CountryName,
	                                STUFF((SELECT DISTINCT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()] FROM SaleOrderItem SI 
	                                inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
	                                WHERE SI.SaleOrderId = S.SaleOrderId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,
	                                CASE WHEN S.isProjectBased = 1 THEN 'MATERIAL REQUEST' ELSE 'WORKSHOP REQUEST' END AS title,
	                                C.CustomerName
                                    from WorkShopRequest WR 
								    INNER JOIN Organization O ON O.OrganizationId=WR.OrganizationId
	                                INNER JOIN SaleOrder S on S.SaleOrderId=WR.SaleOrderId
	                                INNER JOIN  Customer C  ON S.CustomerId =C.CustomerId
									left  JOIN Country ORR ON ORR.CountryId=O.Country
                                    WHERE WorkShopRequestId = @WorkShopRequestId";
                var objWrkshopRequests = connection.Query<WorkShopRequest>(sql, new { WorkShopRequestId = WorkShopRequestId, organizationId = organizationId }).First<WorkShopRequest>();
                return objWrkshopRequests;
            }
        }
        public List<WorkShopRequestItem> GetWorkShopRequestDtDataPrint(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"		select 
                                    I.ItemId,
                                    I.ItemName,
                                    I.PartNo,
                                    WI.Remarks,
                                    WI.Quantity,
                                    UnitName                                    
									from WorkShopRequestItem WI                     
							        INNER JOIN Item I ON WI.ItemId=I.ItemId
                                    INNER JOIN Unit U on U.UnitId =I.ItemUnitId
                                    where WorkShopRequestId = @WorkShopRequestId";
                return connection.Query<WorkShopRequestItem>(query,
                new { WorkShopRequestId = WorkShopRequestId }).ToList();
            }
        }


        public string InsertDirectMaterialRequest(WorkShopRequest objWorkShopRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var internalId = "";
                   
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objWorkShopRequest.OrganizationId, 37, true, trn);
                 

                    objWorkShopRequest.WorkShopRequestRefNo = internalId;

                    string sql = @"insert  into WorkShopRequest(WorkShopRequestRefNo,WorkShopRequestDate,CustomerId,CustomerOrderRef,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId,isDirectRequest) 
                                    Values (@WorkShopRequestRefNo,@WorkShopRequestDate,@CustomerId,@CustomerOrderRef,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId,1);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                    var id = connection.Query<int>(sql, objWorkShopRequest, trn).Single();

                    foreach (WorkShopRequestItem item in objWorkShopRequest.Items)
                    {
                        item.WorkShopRequestId = id;
                        new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, trn);
                    }

                    InsertLoginHistory(dataConnection, objWorkShopRequest.CreatedBy, "Create", "Workshop Request", id.ToString(), "0");
                    trn.Commit();

                    return id + "|" + internalId;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return "0";
                }
            }
        }
        public object DirectMaterialRequestList(int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = @"SELECT WR.WorkShopRequestId,WR.WorkShopRequestRefNo,CONVERT(VARCHAR,WR.WorkShopRequestDate, 106)WorkshopRequestDate,ISNULL(WR.SpecialRemarks ,'-') SpecialRemarks
                                FROM WorkShopRequest WR WHERE WR.OrganizationId = @org AND WR.isDirectRequest=1
                                ORDER BY WorkShopRequestDate DESC";
                return connection.Query<WorkShopRequest>(query, new { org = organizationId }).ToList();
            }
        }
        public object PendingDirectMaterialRequestforApproval(int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = @"SELECT WR.WorkShopRequestId,WR.WorkShopRequestRefNo,CONVERT(VARCHAR,WR.WorkShopRequestDate, 106)WorkshopRequestDate,ISNULL(WR.SpecialRemarks ,'-') SpecialRemarks
                                FROM WorkShopRequest WR WHERE WR.OrganizationId = @org AND WR.isDirectRequest=1 and isnull(isApproved,0) = 0
                                ORDER BY WorkShopRequestDate DESC";
                return connection.Query<WorkShopRequest>(query, new { org = organizationId }).ToList();
            }
        }
        public WorkShopRequest GetDirectMaterialRequest(int id, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                string query = @"SELECT * from WorkShopRequest  WHERE WorkShopRequestId = @id  AND OrganizationId = @org";
	                                
                               
                               
                WorkShopRequest model = connection.Query<WorkShopRequest>(query, new { org = organizationId, @id = id }, txn).FirstOrDefault();
                string sql = @"SELECT  WI.*,S.WorkShopRequestId,
                                U.UnitName
                                FROM WorkShopRequestItem WI
								INNER JOIN Item I ON WI.ItemId = I.ItemId
								INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
                                LEFT JOIN StoreIssue S ON WI.WorkShopRequestId=S.WorkShopRequestId
                                WHERE WI.WorkShopRequestId = @id";
                model.Items = connection.Query<WorkShopRequestItem>(sql, new { id = id }, txn).ToList();
                //try
                //{
                //    sql = @"SELECT WorkShopRequestId FROM StoreIssue WHERE WorkShopRequestId=@id";
                //    var i = connection.Query<int>(sql, new { id = id }).FirstOrDefault();
                   
                //    if(i>0)
                //    {
                //        model.IsStoreused = true;
                //    }
                  
                //    else
                //    {
                //        model.IsStoreused = false;
                //    }

                //}
                //catch(Exception)
                //{

                //    model.IsStoreused = false;
                //}
            

                return model;
            }
        }
    
        public int UpdateDirectMaterialRequest(WorkShopRequest objWorkShopRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                IDbTransaction txn = connection.BeginTransaction();


                sql = @"UPDATE WorkShopRequest SET
                                WorkShopRequestDate = @WorkShopRequestDate,
                                SpecialRemarks = @SpecialRemarks,
                                CreatedBy = @CreatedBy,
                                CreatedDate = @CreatedDate
                                WHERE WorkShopRequestId = @WorkShopRequestId;
	                               
                        DELETE FROM WorkShopRequestItem WHERE WorkShopRequestId = @WorkShopRequestId;";

                try
                {
                    var id = connection.Execute(sql, objWorkShopRequest, txn);
                  
                    if (id <= 0) throw new Exception();

                    foreach (var item in objWorkShopRequest.Items)
                    {
                        item.WorkShopRequestId = objWorkShopRequest.WorkShopRequestId;
                        id = new WorkShopRequestItemRepository().InsertWorkShopRequestItem(item, connection, txn);
                    }

                    if (id <= 0) throw new Exception();
                    InsertLoginHistory(dataConnection, objWorkShopRequest.CreatedBy, "Update", "Material Request", id.ToString(), objWorkShopRequest.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }
        public void ApproveMaterialRequest(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update WorkShopRequest  SET isApproved=1  OUTPUT INSERTED.WorkShopRequestRefNo WHERE WorkShopRequestId=@id";
                var Refno = connection.Query(sql, new { id = id });
             
            }
        }
    }
}