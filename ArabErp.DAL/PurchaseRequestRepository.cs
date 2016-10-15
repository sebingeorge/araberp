using System;
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

                    objPurchaseRequest.PurchaseRequestNo  = internalId;

                    string sql = @"insert  into PurchaseRequest(PurchaseRequestNo,PurchaseRequestDate,WorkShopRequestId,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@PurchaseRequestNo,@PurchaseRequestDate,@WorkShopRequestId,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objPurchaseRequest, trn).Single();

                    foreach (PurchaseRequestItem item in objPurchaseRequest.items)
                    {
                        item.PurchaseRequestId = id;
                        new PurchaseRequestItemRepository().InsertPurchaseRequestItem(item, connection, trn);
                    }
                    InsertLoginHistory(dataConnection, objPurchaseRequest.CreatedBy, "Create", "Purchase Request", id.ToString(), "0");
                    trn.Commit();

                    return id + "|PUR/" + internalId;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return "0";
                }
            }
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

        public PurchaseRequest UpdatePurchaseRequest(PurchaseRequest objPurchaseRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE PurchaseRequest SET PurchaseRequestNo = @PurchaseRequestNo ,PurchaseRequestDate = @PurchaseRequestDate ,
                                WorkShopRequestId = @WorkShopRequestId ,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate  
                                OUTPUT INSERTED.PurchaseRequestId  WHERE PurchaseRequestId = @PurchaseRequestId";
                var id = connection.Execute(sql, objPurchaseRequest);
                InsertLoginHistory(dataConnection, objPurchaseRequest.CreatedBy, "Update", "Purchase Request", id.ToString(), "0");
                return objPurchaseRequest;
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
            int result3 = 0;
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
        public IEnumerable<PendingWorkShopRequest> GetWorkShopRequestPending(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"Select WR.WorkShopRequestId,WR.WorkShopRequestRefNo,WR.WorkShopRequestDate,WR.RequiredDate,C.CustomerName,WR.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,DATEDIFF(dd,WR.WorkShopRequestDate,GETDATE ()) Ageing, DATEDIFF(DAY, GETDATE(), WR.RequiredDate) DaysLeft from WorkShopRequest WR INNER JOIN Customer C on C.CustomerId=WR.CustomerId ";
                qry += " INNER JOIN SaleOrder SO on WR.SaleOrderId=SO.SaleOrderId  LEFT JOIN PurchaseRequest PR ON PR.WorkShopRequestId=WR.WorkShopRequestId WHERE PR.PurchaseRequestId is null and WR.OrganizationId=@OrganizationId ORDER BY WR.RequiredDate DESC, WR.WorkShopRequestDate DESC";

                return connection.Query<PendingWorkShopRequest>(qry, new { OrganizationId = OrganizationId });
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
                qry += " GETDATE() PurchaseRequestDate,WR.WorkShopRequestRefNo +','+ Replace(Convert(varchar,WorkShopRequestDate,106),' ','/') WorkShopRequestRefNo";
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
                string qry = " SELECT PurchaseRequestId,PurchaseRequestNo,PurchaseRequestDate,C.CustomerName,WR.CustomerOrderRef,PR.WorkShopRequestId,";
                      qry += " WR.WorkShopRequestRefNo +','+ Replace(Convert(varchar,WorkShopRequestDate,106),' ','/') WorkShopRequestRefNo,";
                      qry += " PR.SpecialRemarks,PR.RequiredDate";
                      qry += " FROM PurchaseRequest PR";
                      qry += " INNER JOIN WorkShopRequest WR ON WR.WorkShopRequestId=PR.WorkShopRequestId";
                      qry += " INNER JOIN Customer C ON WR.CustomerId = C.CustomerId";
                      qry += " WHERE PR.PurchaseRequestId = " + PurchaseRequestId.ToString();

                PurchaseRequest PurchaseRequest = connection.Query<PurchaseRequest>(qry).FirstOrDefault();
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


        public IList<PurchaseRequest> GetPurchaseRequest(int OrganizationId, int id, int cusid, DateTime? from, DateTime? to)
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
                                    and P.PurchaseRequestDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())  
                                    ORDER BY P.RequiredDate DESC, P.PurchaseRequestDate DESC;
                                    DROP TABLE #SUPPLY;";

                return connection.Query<PurchaseRequest>(sql, new { OrganizationId = OrganizationId, id = id, cusid = cusid, to = to, from = from }).ToList<PurchaseRequest>();
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

    }
}
