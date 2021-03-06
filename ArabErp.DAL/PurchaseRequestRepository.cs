﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PurchaseRequestRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        /// <summary>
        /// Insert PurchaseRequest
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string InsertPurchaseRequest(PurchaseRequest objPurchaseRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objPurchaseRequest.OrganizationId, 8, true, trn);

                    objPurchaseRequest.PurchaseRequestNo = internalId;

                    var id = Insert(objPurchaseRequest, connection, trn);
                    
                    InsertLoginHistory(dataConnection, objPurchaseRequest.CreatedBy, "Create", "Purchase Request", id.ToString(), "0");
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

        private object Insert(PurchaseRequest objPurchaseRequest, IDbConnection connection, IDbTransaction trn)
        {
            string sql = @"insert  into PurchaseRequest(PurchaseRequestNo,PurchaseRequestDate,WorkShopRequestId,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@PurchaseRequestNo,@PurchaseRequestDate,@WorkShopRequestId,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = connection.Query<int>(sql, objPurchaseRequest, trn).Single();

            foreach (PurchaseRequestItem item in objPurchaseRequest.items)
            {
                item.PurchaseRequestId = id;
                if (item.Quantity == null || item.Quantity == 0) continue;
                new PurchaseRequestItemRepository().InsertPurchaseRequestItem(item, connection, trn);
            }
            return id;
        }


        public PurchaseRequest GetPurchaseRequest(int PurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseRequest  where PurchaseRequestId=@PurchaseRequestId";


                var objPurchaseRequest = connection.Query<PurchaseRequest>(sql, new
                {
                    PurchaseRequestId = PurchaseRequestId
                }).First<PurchaseRequest>();

                return objPurchaseRequest;
            }
        }

        public List<PurchaseRequest> GetPurchaseRequests()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseRequest
                        where isActive=1";

                var objPurchaseRequests = connection.Query<PurchaseRequest>(sql).ToList<PurchaseRequest>();

                return objPurchaseRequests;
            }
        }

        public int CHECK(int PurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT count(PurchaseRequestId)count FROM PurchaseRequestItem P 
                                INNER JOIN SupplyOrderItem S ON S.PurchaseRequestItemId=P.PurchaseRequestItemId 
                                WHERE PurchaseRequestId=@PurchaseRequestId";

                var id = connection.Query<int>(sql, new { PurchaseRequestId = PurchaseRequestId }).FirstOrDefault();

                return id;

            }

        }

        public string UpdatePurchaseRequest(PurchaseRequest objPurchaseRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    string sql = @" UPDATE PurchaseRequest SET PurchaseRequestDate = @PurchaseRequestDate ,
                                WorkShopRequestId = @WorkShopRequestId ,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate  
                                OUTPUT INSERTED.PurchaseRequestId  WHERE PurchaseRequestId = @PurchaseRequestId";
                    var id = connection.Execute(sql, objPurchaseRequest, trn);

                    sql = @" DELETE FROM PurchaseRequestItem WHERE PurchaseRequestId=@Id";
                    connection.Execute(sql, new { Id = objPurchaseRequest.PurchaseRequestId }, transaction: trn);

                    foreach (PurchaseRequestItem item in objPurchaseRequest.items)
                    {
                        item.PurchaseRequestId = objPurchaseRequest.PurchaseRequestId;
                        if (item.Quantity == null || item.Quantity == 0) continue;
                        new PurchaseRequestItemRepository().InsertPurchaseRequestItem(item, connection, trn);
                    }

                    InsertLoginHistory(dataConnection, objPurchaseRequest.CreatedBy, "Update", "Purchase Request", id.ToString(), "0");
                    trn.Commit();
                    return objPurchaseRequest.PurchaseRequestNo;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    throw ex;
                }
            }
        }
        /// <summary>
        /// Delete Purchase Request HD Details
        /// </summary>
        /// <returns></returns>
        public int DeletePurchaseRequestHD(int Id, string CreatedBy)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = @" Update PurchaseRequest Set isActive=0 WHERE PurchaseRequestId=@Id";
                string sql = @" DELETE FROM PurchaseRequest WHERE PurchaseRequestId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    InsertLoginHistory(dataConnection, CreatedBy, "Delete", "Purchase Request", id.ToString(), "0");
                    return id;

                }

            }
        }
        /// <summary>
        /// Delete Purchase Request DT Details
        /// </summary>
        /// <returns></returns>
        public int DeletePurchaseRequestDT(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM PurchaseRequestItem WHERE PurchaseRequestId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }
        /// <summary>
        /// Pending Workshop Request For Purchase Request
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingWorkShopRequest> GetWorkShopRequestPending(int OrganizationId, int cusid, string WRNo = "", string Type = "all")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 8.11.2016 11.13a
                //string qry = @"Select WR.WorkShopRequestId,WR.WorkShopRequestRefNo,WR.WorkShopRequestDate,WR.RequiredDate,C.CustomerName,WR.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,DATEDIFF(dd,WR.WorkShopRequestDate,GETDATE ()) Ageing, DATEDIFF(DAY, GETDATE(), WR.RequiredDate) DaysLeft from WorkShopRequest WR INNER JOIN Customer C on C.CustomerId=WR.CustomerId ";
                //qry += " INNER JOIN SaleOrder SO on WR.SaleOrderId=SO.SaleOrderId  LEFT JOIN PurchaseRequest PR ON PR.WorkShopRequestId=WR.WorkShopRequestId WHERE PR.PurchaseRequestId is null and WR.OrganizationId=@OrganizationId ORDER BY WR.RequiredDate, WR.WorkShopRequestDate"; 
                #endregion

                #region old query 16.11.2016 12.59p
                //                string query = @"Select 
                //	                                WR.WorkShopRequestId,
                //	                                WR.WorkShopRequestRefNo,
                //	                                WR.WorkShopRequestDate,
                //	                                WR.RequiredDate,
                //	                                C.CustomerName,
                //	                                WR.CustomerOrderRef,
                //	                                SO.SaleOrderRefNo,
                //	                                SO.SaleOrderDate,
                //	                                DATEDIFF(dd,WR.WorkShopRequestDate,GETDATE ()) Ageing, 
                //	                                DATEDIFF(DAY, GETDATE(), WR.RequiredDate) DaysLeft
                //                                from WorkShopRequest WR 
                //                                    INNER JOIN Customer C on C.CustomerId=WR.CustomerId
                //                                    INNER JOIN SaleOrder SO on WR.SaleOrderId=SO.SaleOrderId  
                //                                    LEFT JOIN PurchaseRequest PR ON PR.WorkShopRequestId=WR.WorkShopRequestId 
                //                                    LEFT JOIN PurchaseRequestItem PRI ON PR.PurchaseRequestId = PRI.PurchaseRequestId
                //                                    LEFT JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                //                                WHERE /*PR.PurchaseRequestId is null and*/ WR.OrganizationId=@OrganizationId
                //                                    GROUP BY WR.WorkShopRequestId,
                //	                                WR.WorkShopRequestRefNo,
                //	                                WR.WorkShopRequestDate,
                //	                                WR.RequiredDate,
                //	                                C.CustomerName,
                //	                                WR.CustomerOrderRef,
                //	                                SO.SaleOrderRefNo,
                //	                                SO.SaleOrderDate
                //                                HAVING SUM(ISNULL(PRI.Quantity, 0)) < SUM(WRI.Quantity)
                //                                ORDER BY WR.RequiredDate, WR.WorkShopRequestDate"; 
                #endregion

                string query = @"SELECT
	                                --PRI.PurchaseRequestId,
	                                WRI.WorkShopRequestId,
	                                PRI.ItemId
	                                --SUM(PRI.Quantity) PR,
	                                --WRI.Quantity WR
                                INTO #TEMP1
                                FROM PurchaseRequestItem PRI
	                                INNER JOIN PurchaseRequest PR ON PRI.PurchaseRequestId = PRI.PurchaseRequestId
	                                RIGHT JOIN WorkShopRequest WR ON PR.WorkShopRequestId = WR.WorkShopRequestId
	                                RIGHT JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId AND WRI.ItemId = PRI.ItemId
                                GROUP BY WRI.WorkShopRequestId, PRI.ItemId, WRI.Quantity
                                HAVING SUM(ISNULL(PRI.Quantity, 0))<WRI.Quantity
                                ORDER BY WRI.WorkShopRequestId;

                                Select 
	                                WR.WorkShopRequestId,
	                                WR.WorkShopRequestRefNo,
	                                WR.WorkShopRequestDate,
	                                WR.RequiredDate,
	                                C.CustomerName,
	                                WR.CustomerOrderRef,
	                                SO.SaleOrderRefNo,
	                                SO.SaleOrderDate,
	                                DATEDIFF(dd,WR.WorkShopRequestDate,GETDATE ()) Ageing, 
	                                DATEDIFF(DAY, GETDATE(), WR.RequiredDate) DaysLeft
                                from #TEMP1 T1 
									INNER JOIN WorkShopRequest WR ON T1.WorkShopRequestId = WR.WorkShopRequestId
                                    INNER JOIN Customer C on C.CustomerId=WR.CustomerId
                                    INNER JOIN SaleOrder SO on WR.SaleOrderId=SO.SaleOrderId  
                                    LEFT JOIN PurchaseRequest PR ON PR.WorkShopRequestId=WR.WorkShopRequestId 
                                    LEFT JOIN PurchaseRequestItem PRI ON PR.PurchaseRequestId = PRI.PurchaseRequestId
                                    LEFT JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                                    
                                    --modified by on 1.2.2017 3.43p
                                    LEFT JOIN JobCard JC ON WR.SaleOrderId = JC.SaleOrderId

                                    WHERE /*PR.PurchaseRequestId is null and*/ WR.OrganizationId = @OrganizationId
                                    AND
								    ISNULL(SO.isProjectBased, 0) = CASE @Type WHEN 'project' THEN 1 WHEN 'transport' THEN 0 WHEN 'all' THEN ISNULL(SO.isProjectBased, 0) END
                                    AND  WR.CustomerId = ISNULL(NULLIF(@cusid, 0),  WR.CustomerId)
                                    AND (ISNULL(WR.WorkShopRequestRefNo, '') LIKE '%'+@WRNo+'%')
                                    AND ISNULL(JC.JodCardCompleteStatus, 0) = 0
                                    GROUP BY WR.WorkShopRequestId,
	                                WR.WorkShopRequestRefNo,
	                                WR.WorkShopRequestDate,
	                                WR.RequiredDate,
	                                C.CustomerName,
	                                WR.CustomerOrderRef,
	                                SO.SaleOrderRefNo,
	                                SO.SaleOrderDate
                                --HAVING SUM(ISNULL(PRI.Quantity, 0)) < SUM(WRI.Quantity)
                                ORDER BY WR.RequiredDate, WR.WorkShopRequestDate;

								DROP TABLE #TEMP1;";

                 return connection.Query<PendingWorkShopRequest>(query, new { OrganizationId = OrganizationId, cusid = cusid, WRNo = WRNo,Type=Type });
            }
        }
        /// <summary>
        /// Purchase Request Transaction Head Part Details
        /// </summary>
        /// <param name="WorkShopRequestId"></param>
        /// <returns></returns>
        public PurchaseRequest GetPurchaseRequestDetails(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = "Select WR.WorkShopRequestId ,WR.CustomerOrderRef, C.CustomerName,";
                qry += " GETDATE() PurchaseRequestDate,WR.WorkShopRequestRefNo +' - '+ Convert(varchar,WorkShopRequestDate,106) WorkShopRequestRefNo";
                qry += " from WorkShopRequest WR inner join Customer C on WR.CustomerId = C.CustomerId";
                qry += " where WR.WorkShopRequestId = " + WorkShopRequestId.ToString();

                PurchaseRequest workshoprequest = connection.Query<PurchaseRequest>(qry).FirstOrDefault();
                return workshoprequest;
            }
        }

        /// <summary>
        /// Purchase Request Transaction Item Part Details
        /// </summary>
        /// <param name="WorkShopRequestId"></param>
        /// <returns></returns>
        //public void GetPurchaseRequestItem(int Id)
        public List<PurchaseRequestItem> GetPurchaseRequestItem(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "exec PurchaseRequestData " + Id.ToString();
                return connection.Query<PurchaseRequestItem>(sql, new { WorkShopRequestId = Id }).ToList();
            }
            //            using (IDbConnection connection = OpenConnection(dataConnection))
            //            {
            //                string sql = @"select ItemId from WorkShopRequestItem
            //                        where WorkShopRequestId=@WorkShopRequestId";

            //                var OBJITEMS = connection.Query<PurchaseRequestItem>(sql, new { WorkShopRequestId = WorkShopRequestId }).ToList();

            //                string sql1 = @" SELECT  SUM(Quantity) -( SELECT SUM(IssuedQuantity)  FROM StoreIssueItem S INNER JOIN WorkShopRequestItem W ON W.WorkShopRequestItemId=S.WorkShopRequestItemId WHERE  W.ItemId= in @OBJITEMS)WRIssueQty
            //                            FROM WorkShopRequestItem W WHERE  W.ItemId in ";


            //                string query = "select  I.PartNo,I.ItemId,I.ItemName,WI.Quantity WRRequestQty,UnitName,I.MinLevel,(I.MinLevel + WI.Quantity)TotalQty,ISNULL((select sum(Quantity) from StockUpdate SU where ItemId = I.ItemId ),0)CurrentStock,0 Quantity  from WorkShopRequest W ";
            //                query += " INNER JOIN WorkShopRequestItem WI ON W.WorkShopRequestId=WI.WorkShopRequestId  INNER JOIN";
            //                query += " Item I ON WI.ItemId=I.ItemId  INNER JOIN Unit U on U.UnitId =I.ItemUnitId WHERE W.WorkShopRequestId=@WorkShopRequestId";

            //                //string query = "select  I.PartNo,I.ItemId,I.ItemName,WI.Quantity WRRequestQty,UnitName,I.MinLevel, ISNULL(( select (Quantity-IssuedQuantity) from  StoreIssueItem S WHERE WI.WorkShopRequestItemId=S.WorkShopRequestItemId),0) WRIssueQty,(I.MinLevel + WI.Quantity)TotalQty,ISNULL((select sum(Quantity) from StockUpdate SU where ItemId = I.ItemId ),0)CurrentStock,0 Quantity  from WorkShopRequest W ";
            //                //query += " INNER JOIN WorkShopRequestItem WI ON W.WorkShopRequestId=WI.WorkShopRequestId  INNER JOIN";
            //                //query += " Item I ON WI.ItemId=I.ItemId  INNER JOIN Unit U on U.UnitId =I.ItemUnitId WHERE W.WorkShopRequestId=@WorkShopRequestId";
            //                return connection.Query<PurchaseRequestItem>(query, new { WorkShopRequestId = WorkShopRequestId }).ToList();
            //            }
        }


        /// <summary>
        /// Purchase Request Transaction Head Part Details
        /// </summary>
        /// <param name="PurchaseRequestId"></param>
        /// <returns></returns>
        public PurchaseRequest GetPurchaseRequestHDDetails(int PurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"DECLARE @isUsed BIT = 0;
                                IF EXISTS(SELECT SupplyOrderId FROM SupplyOrderItem SOI
			                                INNER JOIN PurchaseRequestItem PRI
				                                ON SOI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
			                                WHERE PRI.PurchaseRequestId = @PurchaseRequestId)
	                                SET @isUsed = 1

                                SELECT PurchaseRequestId,PurchaseRequestNo,PurchaseRequestDate,C.CustomerName,WR.CustomerOrderRef,PR.WorkShopRequestId,
                                WR.WorkShopRequestRefNo +','+ Replace(Convert(varchar,WorkShopRequestDate,106),' ','/') WorkShopRequestRefNo,
                                PR.SpecialRemarks,PR.RequiredDate, @isUsed AS isUsed
                                FROM PurchaseRequest PR
                                INNER JOIN WorkShopRequest WR ON WR.WorkShopRequestId=PR.WorkShopRequestId
                                INNER JOIN Customer C ON WR.CustomerId = C.CustomerId
                                WHERE PR.PurchaseRequestId = @PurchaseRequestId";

                PurchaseRequest PurchaseRequest = connection.Query<PurchaseRequest>(qry, new { PurchaseRequestId = PurchaseRequestId }).FirstOrDefault();
                return PurchaseRequest;
            }
        }

        /// <summary>
        /// Purchase Request Transaction DT Part Details
        /// </summary>
        /// <param name="WorkShopRequestId"></param>
        /// <returns></returns>

        public List<PurchaseRequestItem> GetPurchaseRequestDTDetails(int PurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "exec PurchaseRequestItemDetails " + PurchaseRequestId.ToString();
                return connection.Query<PurchaseRequestItem>(sql, new { PurchaseRequestId = PurchaseRequestId }).ToList();
            }
        }


        public IList<PurchaseRequest> GetPurchaseRequest(int OrganizationId, int id, int cusid, DateTime? from, DateTime? to,int WR,int MR)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select distinct SI.PurchaseRequestItemId, SUM(SI.OrderedQty) SuppliedQuantity 
                                INTO #SUPPLY
                                from [dbo].[SupplyOrderItem] SI
                                GROUP BY SI.PurchaseRequestItemId;
                                select DISTINCT
	                                P.PurchaseRequestId,
	                                PurchaseRequestNo,
	                                PurchaseRequestDate,
	                                C.CustomerName,
                                    P.RequiredDate,
									P.CreatedDate,
                                    ISNULL(P.SpecialRemarks, '-') SpecialRemarks, 
                                    ISNULL(WRK.WorkShopRequestRefNo, '')+' - '+CONVERT(VARCHAR, WRK.WorkShopRequestDate, 106) WorkShopRequestRefNo
                                    from PurchaseRequest P
                                    INNER JOIN PurchaseRequestItem PRI ON P.PurchaseRequestId = PRI.PurchaseRequestId
                                    INNER JOIN WorkShopRequest WRK ON P.WorkShopRequestId = WRK.WorkShopRequestId
                                    INNER JOIN Customer C ON C.CustomerId=WRK.CustomerId
                                    LEFT JOIN #SUPPLY SUP ON PRI.PurchaseRequestItemId = SUP.PurchaseRequestItemId
                                    WHERE P.isActive= 1 and P.OrganizationId=@OrganizationId
                                    and P.PurchaseRequestId = ISNULL(NULLIF(@id, 0), P.PurchaseRequestId )
                                    and C.CustomerId = ISNULL(NULLIF(@cusid, 0), C.CustomerId)
                                    and WRK.WorkShopRequestId = ISNULL(NULLIF(@WR, 0),WRK.WorkShopRequestId)
                                    and WRK.WorkShopRequestId = ISNULL(NULLIF(@MR, 0),WRK.WorkShopRequestId)
                                    and P.PurchaseRequestDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())  
                                    ORDER BY P.PurchaseRequestDate DESC, P.CreatedDate DESC;
                                    DROP TABLE #SUPPLY;";

                return connection.Query<PurchaseRequest>(sql, new { OrganizationId = OrganizationId, id = id, cusid = cusid, to = to, from = from,WR=WR,MR=MR }).ToList<PurchaseRequest>();
            }
        }



        public IEnumerable<PurchaseRequestRegister> GetPendingPARregisterData(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT distinct PurchaseRequestNo,PurchaseRequestDate,I.ItemName,isnull(PI.Remarks,'-')Remarks,
                                PI.Quantity ReqQty,Sum(isnull (GI.Quantity,0))GRNQTY,'0' SettledQty,(PI.Quantity)-Sum(isnull (GI.Quantity,0))BALQTY
                                FROM PurchaseRequest P
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestId=P.PurchaseRequestId
                                INNER JOIN Item I ON I.ItemId=PI.ItemId
                                LEFT JOIN SupplyOrderItem SI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                                LEFT JOIN GRNItem GI ON GI.SupplyOrderItemId=SI.SupplyOrderItemId
                                WHERE P.OrganizationId=@OrganizationId
                                GROUP BY PurchaseRequestNo,PurchaseRequestDate,I.ItemName,PI.Remarks,PI.Quantity";
                return connection.Query<PurchaseRequestRegister>(qry, new { OrganizationId = OrganizationId }).ToList();
            }
        }

        public IEnumerable<PurchaseRequestRegister> GetPendingPARregisterDT(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT distinct PurchaseRequestNo,PurchaseRequestDate,I.ItemName,isnull(PI.Remarks,'-')Remarks,
                                PI.Quantity ReqQty,Sum(isnull (GI.Quantity,0))GRNQTY,'0' SettledQty,(PI.Quantity)-Sum(isnull (GI.Quantity,0))BALQTY
                                FROM PurchaseRequest P
                                INNER JOIN PurchaseRequestItem PI ON PI.PurchaseRequestId=P.PurchaseRequestId
                                INNER JOIN Item I ON I.ItemId=PI.ItemId
                                LEFT JOIN SupplyOrderItem SI ON SI.PurchaseRequestItemId=PI.PurchaseRequestItemId
                                LEFT JOIN GRNItem GI ON GI.SupplyOrderItemId=SI.SupplyOrderItemId
                                WHERE P.OrganizationId=@OrganizationId
                                GROUP BY PurchaseRequestNo,PurchaseRequestDate,I.ItemName,PI.Remarks,PI.Quantity";
                return connection.Query<PurchaseRequestRegister>(qry, new { OrganizationId = OrganizationId }).ToList();
            }
        }

        public PurchaseRequestRegister PurchaseRequestRegisterHD(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"	select O.Image1,O.OrganizationName from Organization O where OrganizationId=@OrganizationId";

                var objPurchaseRequestRegisterId = connection.Query<PurchaseRequestRegister>(qry, new
                {

                    OrganizationId = OrganizationId,

                }).First<PurchaseRequestRegister>();

                return objPurchaseRequestRegisterId;
            }
        }


        public PurchaseRequest GetPurchaseRequestHDDetailsPrint(int PurchaseRequestId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = "SELECT O.*,PurchaseRequestId,PurchaseRequestNo,PurchaseRequestDate,C.CustomerName,WR.CustomerOrderRef,PR.WorkShopRequestId, ORR.CountryName,";
                qry += "  WR.WorkShopRequestRefNo +','+ Replace(Convert(varchar,WorkShopRequestDate,106),' ','/') WorkShopRequestRefNo,";
                qry += "  PR.SpecialRemarks,PR.RequiredDate,U.UserName CreatedUser,U.Signature CreatedUsersig FROM PurchaseRequest PR";
                qry += "  INNER JOIN WorkShopRequest WR ON WR.WorkShopRequestId=PR.WorkShopRequestId";
                qry += "   INNER JOIN Customer C ON WR.CustomerId = C.CustomerId";
                qry += "  INNER JOIN Organization O ON O.OrganizationId=PR.OrganizationId";
                qry += "  left  JOIN Country ORR ON ORR.CountryId=O.Country left join [User] U ON U.UserId=PR.CreatedBy";
                qry += "  WHERE PR.PurchaseRequestId=" + PurchaseRequestId.ToString();

                PurchaseRequest PurchaseRequest = connection.Query<PurchaseRequest>(qry, new { OrganizationId = OrganizationId }).First();
                return PurchaseRequest;
            }
        }

        public List<PurchaseRequestItem> PurchaseRequestItemDetailsPrint(int PurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "exec PurchaseRequestItemDetails " + PurchaseRequestId.ToString();
                return connection.Query<PurchaseRequestItem>(sql, new { PurchaseRequestId = PurchaseRequestId }).ToList();
            }
        }

        public IEnumerable<PendingForGRN> GetLastPurchaseRate(int itemId, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"SELECT TOP 3
	                                S.SupplierId,
	                                S.SupplierName,
	                                GI.ItemId,
	                                I.ItemName,
	                                GI.Quantity,
	                                GI.Rate
                                FROM GRNItem GI
	                                INNER JOIN GRN G ON GI.GRNId = G.GRNId
	                                LEFT JOIN SupplyOrderItem SOI ON GI.SupplyOrderItemId = SOI.SupplyOrderItemId
	                                LEFT JOIN SupplyOrder SO ON SOI.SupplyOrderId = SO.SupplyOrderId
	                                LEFT JOIN Supplier S ON SO.SupplierId = S.SupplierId
	                                INNER JOIN Item I ON GI.ItemId = I.ItemId
                                WHERE GI.ItemId = @itemId
	                                AND G.OrganizationId = @org
								ORDER BY G.GRNDate DESC";
                    var list = connection.Query<PendingForGRN>(sql, new { itemId = itemId, org = OrganizationId }).ToList();
                    if (list == null || list.Count == 0)
                    {
                        sql = @"SELECT
	                            SR.ItemId, I.ItemName, SR.Rate
                            FROM StandardRate SR
	                            INNER JOIN Item I ON SR.ItemId = I.ItemId
                            WHERE SR.ItemId = @itemId";
                        list = connection.Query<PendingForGRN>(sql, new { itemId = itemId }).ToList();
                    }
                    return list;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}
