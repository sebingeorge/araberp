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

        public int InsertSupplyOrder(SupplyOrder objSupplyOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into SupplyOrder(SupplyOrderDate,SupplierId,QuotaionNoAndDate,SpecialRemarks,PaymentTerms,DeliveryTerms,RequiredDate,CreatedBy,CreatedDate,OrganizationId,CreatedDate,OrganizationId) Values (@SupplyOrderDate,@SupplierId,@QuotaionNoAndDate,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSupplyOrder).Single();
                return id;
            }
        }

        public List<SupplyOrderItem> GetPurchaseRequestItems(List<int> selectedpurchaserequests)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select PurchaseRequestItemId,i.ItemName,i.PartNo,p.Quantity as BalQty from PurchaseRequestItem p join Item i on p.ItemId=i.ItemId
                        where p.PurchaseRequestId in @selectedpurchaserequests";

                var objPendingPurchaseRequests = connection.Query<SupplyOrderItem>(sql, new { selectedpurchaserequests = selectedpurchaserequests }).ToList<SupplyOrderItem>();

                return objPendingPurchaseRequests;
            }
        }

        public IEnumerable<PendingPurchaseRequest> GetPendingPurchaseRequest()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseRequest
                        where isActive=1";

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