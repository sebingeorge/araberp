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
        public int InsertPurchaseRequest(PurchaseRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"insert  into PurchaseRequest(PurchaseRequestNo,PurchaseRequestDate,WorkShopRequestId,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@PurchaseRequestNo,@PurchaseRequestDate,@WorkShopRequestId,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, model, trn).Single();
                    var saleorderitemrepo = new PurchaseRequestItemRepository();
                    foreach (var item in model.items)
                    {
                        item.PurchaseRequestId = id;
                        new PurchaseRequestItemRepository().InsertPurchaseRequestItem(item, connection, trn);

                    }

                    trn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return 0;
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
        public int UpdatePurchaseRequest(PurchaseRequest objPurchaseRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE PurchaseRequest SET PurchaseRequestNo = @PurchaseRequestNo ,PurchaseRequestDate = @PurchaseRequestDate ,WorkShopRequestId = @WorkShopRequestId ,SpecialRemarks = @SpecialRemarks,RequiredDate = @RequiredDate  OUTPUT INSERTED.PurchaseRequestId  WHERE PurchaseRequestId = @PurchaseRequestId";
                var id = connection.Execute(sql, objPurchaseRequest);
                return id;
            }
        }

        public int DeletePurchaseRequest(Unit objPurchaseRequest)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete PurchaseRequest  OUTPUT DELETED.PurchaseRequestId WHERE PurchaseRequestId=@PurchaseRequestId";
                var id = connection.Execute(sql, objPurchaseRequest);
                return id;
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
                string qry = @"Select WR.WorkShopRequestId,WR.WorkShopRequestNo,WR.WorkShopRequestDate,WR.RequiredDate,C.CustomerName,WR.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,DATEDIFF(dd,WR.WorkShopRequestDate,GETDATE ()) Ageing from WorkShopRequest WR INNER JOIN Customer C on C.CustomerId=WR.CustomerId ";
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
                qry += " GETDATE() PurchaseRequestDate,WR.WorkShopRequestNo +','+ Replace(Convert(varchar,WorkShopRequestDate,106),' ','/') WorkShopRequestNo";
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
        public List<PurchaseRequestItem> GetPurchaseRequestItem(int WorkShopRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "select  I.PartNo,I.ItemId,I.ItemName,WI.Quantity WRRequestQty,UnitName,I.MinLevel, ISNULL(( select (Quantity-IssuedQuantity) from  StoreIssueItem S WHERE WI.WorkShopRequestItemId=S.WorkShopRequestItemId),0) WRIssueQty,(I.MinLevel + WI.Quantity)TotalQty,ISNULL((select sum(Quantity) from StockUpdate SU where ItemId = I.ItemId ),0)CurrentStock,0 Quantity  from WorkShopRequest W ";
                query += " INNER JOIN WorkShopRequestItem WI ON W.WorkShopRequestId=WI.WorkShopRequestId  INNER JOIN";
                query += " Item I ON WI.ItemId=I.ItemId  INNER JOIN Unit U on U.UnitId =I.ItemUnitId WHERE W.WorkShopRequestId=@WorkShopRequestId";
                return connection.Query<PurchaseRequestItem>(query, new { WorkShopRequestId = WorkShopRequestId }).ToList();
            }
        }

    }
}
