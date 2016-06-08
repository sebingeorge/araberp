using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplyOrderRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public int InsertSupplyOrder(SupplyOrder objSupplyOrder)
        {

            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    string sql = @"insert  into SupplyOrder(SupplyOrderNo,SupplyOrderDate,SupplierId,QuotaionNoAndDate,SpecialRemarks,PaymentTerms,DeliveryTerms,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@SupplyOrderNo,@SupplyOrderDate,@SupplierId,@QuotaionNoAndDate,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, objSupplyOrder, trn).Single<int>();

                    var supplyorderitemrepo = new SupplyOrderItemRepository();
                    foreach (var item in objSupplyOrder.SupplyOrderItems)
                    {
                        item.SupplyOrderId = id;
                        item.OrganizationId = objSupplyOrder.OrganizationId;
                        supplyorderitemrepo.InsertSupplyOrderItem(item, connection, trn);
                    }

                    trn.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);

                    trn.Rollback();

                    throw;
                }
                return id;

            }



        }

        public List<SupplyOrderItem> GetPurchaseRequestItems(List<int> selectedpurchaserequests)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT  CONCAT(PurchaseRequestNo,'/',CONVERT (VARCHAR(15),PurchaseRequestDate,104)) PRNODATE,
                        PurchaseRequestItemId,i.ItemName,i.PartNo,PI.Quantity as BalQty 
                        FROM PurchaseRequest P 
                        INNER JOIN PurchaseRequestItem PI ON P.PurchaseRequestId=PI.PurchaseRequestId
                        INNER JOIN Item i ON PI.ItemId=i.ItemId
                        WHERE P.PurchaseRequestId in @selectedpurchaserequests";

                var objPendingPurchaseRequests = connection.Query<SupplyOrderItem>(sql, new { selectedpurchaserequests = selectedpurchaserequests }).ToList<SupplyOrderItem>();

                return objPendingPurchaseRequests;
            }
        }

        public IEnumerable<PendingPurchaseRequest> GetPendingPurchaseRequest()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseRequest P
WHERE P.isActive=1 and P.PurchaseRequestId not in (
select distinct PR.PurchaseRequestId from [dbo].[SupplyOrderItem] SI join [dbo].[PurchaseRequestItem] 
PRI on SI.PurchaseRequestItemId=PRI.PurchaseRequestItemId join [dbo].[PurchaseRequest] PR on PRI.PurchaseRequestId=PR.PurchaseRequestId)";

                var objPendingPurchaseRequests = connection.Query<PendingPurchaseRequest>(sql).ToList<PendingPurchaseRequest>();

                return objPendingPurchaseRequests;
            }
        }

        public SupplyOrder GetSupplyOrder(int SupplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrder
                        where SupplyOrderId=@SupplyOrderId";

                var objSupplyOrder = connection.Query<SupplyOrder>(sql, new
                {
                    SupplyOrderId = SupplyOrderId
                }).First<SupplyOrder>();

                return objSupplyOrder;
            }
        }

        public List<SupplyOrder> GetSupplyOrders()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrder
                        where isActive=1";

                var objSupplyOrders = connection.Query<SupplyOrder>(sql).ToList<SupplyOrder>();

                return objSupplyOrders;
            }
        }

        public int UpdateSupplyOrder(SupplyOrder objSupplyOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SupplyOrder SET SupplyOrderDate = @SupplyOrderDate ,SupplierId = @SupplierId ,QuotaionNoAndDate = @QuotaionNoAndDate ,SpecialRemarks = @SpecialRemarks,PaymentTerms = @PaymentTerms,DeliveryTerms = @DeliveryTerms,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.SupplyOrderId  WHERE SupplyOrderId = @SupplyOrderId";


                var id = connection.Execute(sql, objSupplyOrder);
                return id;
            }
        }
        public int DeleteSupplyOrder(Unit objSupplyOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplyOrder  OUTPUT DELETED.SupplyOrderId WHERE SupplyOrderId=@SupplyOrderId";


                var id = connection.Execute(sql, objSupplyOrder);
                return id;
            }
        }




    }
}