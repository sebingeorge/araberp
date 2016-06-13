using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SaleOrderStatusRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<SaleOrderStatus> GetSaleOrderStatus()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select S.SaleOrderId, C.CustomerName, S.SaleOrderRefNo, S.SaleOrderDate,S.EDateDelivery,";
                sql += " '' JobCardApproval,";
                sql += " JobCard = case when J.SaleOrderItemId is null then 'No' else 'Yes' end,";
                sql += " JobCardComplete = case when ISNULL(JodCardCompleteStatus,0) = 1 then 'Yes' else 'End' end,";
                sql += " WorkShopRequest = case when W.WorkShopRequestId is null then 'No' else 'Yes' end,";
                sql += " PurchaseRequest = case when P.PurchaseRequestId is null then 'No' else 'Yes' end,";
                sql += " SuppyOrder = case when SupplyOrderItemId is null then 'No' else 'Yes' end,";
                sql += " GRN = case when G.GRNId is null then 'No' else 'Yes' end,";
                sql += " SalesInvoice = case when SLI.SalesInvoiceId is null then 'No' else 'Yes' end,";
                sql += " VehicleInpass =  case when J.InPassId is null then 'No' else 'Yes' end,";
                sql += " VI.RegistrationNo, V.VehicleModelName";
                sql += " from SaleOrderItem SI";
                sql += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                sql += " inner join Customer C on C.CustomerId = S.CustomerId";
                sql += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";
                sql += " left join WorkShopRequest W on W.SaleOrderId = S.SaleOrderId";
                sql += " left join PurchaseRequest P on P.WorkShopRequestId = W.WorkShopRequestId";
                sql += " left join PurchaseRequestItem PRI on PRI.PurchaseRequestId	= P.PurchaseRequestId";
                sql += " left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId";
                sql += " left join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId";
                sql += " left join SupplyOrder SO on SO.SupplyOrderId = SUI.SupplyOrderId";
                sql += " left join GRN G on G.SupplyOrderId = SO.SupplyOrderId";
                sql += " left join SalesInvoiceItem SLI on SLI.SaleOrderItemId = SI.SaleOrderItemId";
                sql += " left join VehicleInPass VI on VI.VehicleInPassId = J.InPassId";
                sql += " order by S.SaleOrderDate, S.SaleOrderId";
                return connection.Query<SaleOrderStatus>(sql);
            }
        }
    }
}
