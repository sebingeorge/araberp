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
                sql += " select distinct S.SaleOrderId, C.CustomerName, S.SaleOrderRefNo, S.SaleOrderDate,S.EDateDelivery,";
                sql += " JobCard = case when (select count(*) from JobCard J right join SaleOrderItem SI1 on J.SaleOrderItemId = SI1.SaleOrderItemId";
                sql += " where SI1.SaleOrderId = SI.SaleOrderId and JobCardId is not null) = 0 then 'No' else 'Yes' end ,";
                sql += " JobCardComplete = case when (select count(*) from JobCard J right join SaleOrderItem SI1 on J.SaleOrderItemId = SI1.SaleOrderItemId";
                sql += " where SI1.SaleOrderId = SI.SaleOrderId and J.JobCardId is not null) = 0 then 'No'";
                sql += " when (select count(*) from JobCard J right join SaleOrderItem SI on J.SaleOrderItemId = SI.SaleOrderItemId";
                sql += " where SI.SaleOrderId = SI.SaleOrderId and JobCardId is not null and JodCardCompleteStatus is null) <> 0 then 'No' else 'Yes' end,";
                sql += " WorkShopRequest = case when W.WorkShopRequestId is null then 'No' else 'Yes' end,";
                sql += " PurchaseRequest = case when P.PurchaseRequestId is null then 'No' else 'Yes' end";
                sql += " from SaleOrderItem SI";
                sql += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                sql += " inner join Customer C on C.CustomerId = S.CustomerId";
                sql += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";
                sql += " left join WorkShopRequest W on W.SaleOrderId = S.SaleOrderId";
                sql += " left join PurchaseRequest P on P.WorkShopRequestId = W.WorkShopRequestId";
                sql += " order by S.SaleOrderDate, S.SaleOrderId";

                return connection.Query<SaleOrderStatus>(sql);
            }
        }
    }
}
