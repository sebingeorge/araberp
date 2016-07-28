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
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(PurchaseRequest).Name, "0", 1);

                    objPurchaseRequest.PurchaseRequestNo  = "PUR/" + internalId;

                    string sql = @"insert  into PurchaseRequest(PurchaseRequestNo,PurchaseRequestDate,WorkShopRequestId,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@PurchaseRequestNo,@PurchaseRequestDate,@WorkShopRequestId,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    var id = connection.Query<int>(sql, objPurchaseRequest, trn).Single();

                    foreach (PurchaseRequestItem item in objPurchaseRequest.items)
                    {
                        item.PurchaseRequestId = id;
                        new PurchaseRequestItemRepository().InsertPurchaseRequestItem(item, connection, trn);
                    }

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
                try
                {
                    string sql = @" SELECT PurchaseRequestId FROM PurchaseRequestItem P 
                                INNER JOIN SupplyOrderItem S ON S.PurchaseRequestItemId=P.PurchaseRequestItemId 
                                WHERE PurchaseRequestId=@PurchaseRequestId";

                    var id = connection.Query<PurchaseRequest>(sql, new { PurchaseRequestId = PurchaseRequestId }).FirstOrDefault();
                }
                catch (InvalidOperationException)
                {
                    return 0;
                }
                return 1;

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
                return objPurchaseRequest;
            }
        }

        public int DeletePurchaseRequest(int Id)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update PurchaseRequest Set isActive=0 WHERE PurchaseRequestId=@Id";


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
        public IEnumerable<PendingWorkShopRequest> GetWorkShopRequestPending()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"Select WR.WorkShopRequestId,WR.WorkShopRequestRefNo,WR.WorkShopRequestDate,WR.RequiredDate,C.CustomerName,WR.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,DATEDIFF(dd,WR.WorkShopRequestDate,GETDATE ()) Ageing from WorkShopRequest WR INNER JOIN Customer C on C.CustomerId=WR.CustomerId ";
                qry += " INNER JOIN SaleOrder SO on WR.SaleOrderId=SO.SaleOrderId  LEFT JOIN PurchaseRequest PR ON PR.WorkShopRequestId=WR.WorkShopRequestId WHERE PR.PurchaseRequestId is null ";

                return connection.Query<PendingWorkShopRequest>(qry);
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


        public IEnumerable<PurchaseRequest> GetPurchaseRequest()
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
                                    WHERE P.isActive= 1
                                    ORDER BY P.PurchaseRequestDate DESC;
                                    DROP TABLE #SUPPLY;";

                var objPendingPurchaseRequests = connection.Query<PurchaseRequest>(sql).ToList<PurchaseRequest>();

                return objPendingPurchaseRequests;
            }
        }

    }
}
