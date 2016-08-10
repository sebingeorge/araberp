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
                //sql += " select distinct S.SaleOrderId, C.CustomerName, S.SaleOrderRefNo, S.SaleOrderDate,S.EDateDelivery,";
                //sql += " '' JobCardApproval,";
                //sql += " JobCard = case when J.SaleOrderItemId is null then  '-' else JobCardNo +','+ CONVERT (VARCHAR(15),JobCardDate,106) end,";
                //sql += " JobCardComplete = case when ISNULL(JodCardCompleteStatus,0) = 1 then 'Yes' else 'End' end,";
                //sql += " WorkShopRequest = case when W.WorkShopRequestId is null then '-' else case when SIS.StoreIssueId is null then  WorkShopRequestRefNo +','+ CONVERT (VARCHAR(15),W.RequiredDate,106) else '-'  end end,";
                //sql += " PurchaseRequest = case when P.PurchaseRequestId is null then '-' else  PurchaseRequestNo +','+ CONVERT (VARCHAR(15),PurchaseRequestDate,106) end,";
                //sql += " SuppyOrder =  case when GI.SupplyOrderItemId is null then '-' else case when G.GRNId is null then  SupplyOrderNo +','+ CONVERT (VARCHAR(15),SO.RequiredDate,106) else '-'  end end,";
                //sql += " GRN = case when G.GRNId is null then '-' else GRNNo +','+ CONVERT (VARCHAR(15),GRNDate,106)  end,";
                //sql += " SalesInvoice = case when SLI.SalesInvoiceId is null then 'No' else 'Yes' end,";
                //sql += " VehicleInpass =  case when J.InPassId is null then '-' else  VehicleInPassNo +','+ CONVERT (VARCHAR(15),VehicleInPassDate,106)  end,";
                //sql += " VI.RegistrationNo, V.VehicleModelName";
                //sql += " from SaleOrderItem SI";
                //sql += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                //sql += " inner join Customer C on C.CustomerId = S.CustomerId";
                //sql += " inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId";
                //sql += " left join WorkShopRequest W on W.SaleOrderId = S.SaleOrderId";
                //sql += " left join PurchaseRequest P on P.WorkShopRequestId = W.WorkShopRequestId";
                //sql += " left join PurchaseRequestItem PRI on PRI.PurchaseRequestId	= P.PurchaseRequestId";
                //sql += " left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId";
                //sql += " left join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId";
                //sql += " left join SupplyOrder SO on SO.SupplyOrderId = SUI.SupplyOrderId";
                //sql += " left join GRNItem GI on GI.SupplyOrderItemId = SUI.SupplyOrderItemId";
                //sql += " left join GRN G on G.GRNId = G.GRNId";
                //sql += " left join SalesInvoiceItem SLI on SLI.SaleOrderItemId = SI.SaleOrderItemId";
                //sql += " left join StoreIssue SIS on SIS.WorkShopRequestId=W.WorkShopRequestId";
                //sql += " left join VehicleInPass VI on VI.VehicleInPassId = J.InPassId";
                //sql += " order by S.SaleOrderDate, S.SaleOrderId";

                sql = @"with A as (
                select distinct S.SaleOrderId, C.CustomerName, S.SaleOrderRefNo, S.SaleOrderDate,S.EDateDelivery, '' JobCardApproval, 
                JobCard = case when J.SaleOrderItemId is null then  '-' else JobCardNo +','+ CONVERT (VARCHAR(15),JobCardDate,106) end, 
                JobCardComplete = case when ISNULL(JodCardCompleteStatus,0) = 1 then JobCardNo +',' + CONVERT (VARCHAR(15),JodCardCompletedDate,106) else JobCardNo +','+'-' end, 
                WorkShopRequest = case when W.WorkShopRequestId is null then NULL else case when SIS.StoreIssueId is null then  WorkShopRequestRefNo +','+ CONVERT (VARCHAR(15),W.RequiredDate,106) else NULL end end, 
                PurchaseRequest = case when P.PurchaseRequestId is null then NULL else  PurchaseRequestNo +','+ CONVERT (VARCHAR(15),PurchaseRequestDate,106) end, 
                SuppyOrder =  case when GI.SupplyOrderItemId is null then NULL else case when G.GRNId is null then  SupplyOrderNo +','+ CONVERT (VARCHAR(15),SO.RequiredDate,106) else NULL end end, 
                GRN = case when G.GRNId is null then NULL else GRNNo +','+ CONVERT (VARCHAR(15),GRNDate,106)  end, 
                SalesInvoice = case when SLI.SalesInvoiceId is null then 'No' else 'Yes' end, 
                VehicleInpass =  case when J.InPassId is null then NULL else  VehicleInPassNo +','+ CONVERT (VARCHAR(15),VehicleInPassDate,106)  end, 
                VI.RegistrationNo, V.VehicleModelName,
                Allocation = case when IB.SerialNo is null then NULL else IB.SerialNo end
                from SaleOrderItem SI inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId 
                inner join Customer C on C.CustomerId = S.CustomerId 
                inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId 
                left join WorkShopRequest W on W.SaleOrderId = S.SaleOrderId 
                left join PurchaseRequest P on P.WorkShopRequestId = W.WorkShopRequestId 
                left join PurchaseRequestItem PRI on PRI.PurchaseRequestId	= P.PurchaseRequestId 
                left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId 
                left join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId 
                left join SupplyOrder SO on SO.SupplyOrderId = SUI.SupplyOrderId 
                left join GRNItem GI on GI.SupplyOrderItemId = SUI.SupplyOrderItemId 
                left join GRN G on G.GRNId = G.GRNId 
                left join SalesInvoiceItem SLI on SLI.SaleOrderItemId = SI.SaleOrderItemId 
                left join StoreIssue SIS on SIS.WorkShopRequestId=W.WorkShopRequestId 
                left join VehicleInPass VI on VI.VehicleInPassId = J.InPassId 
                left join ItemBatch IB on IB.SaleOrderItemId = SI.SaleOrderItemId)
                select 
                distinct SaleOrderId, CustomerName, SaleOrderRefNo, SaleOrderDate, EDateDelivery,VehicleModelName,
                RegistrationNo, VehicleInPass, 
                JobCard = (STUFF((SELECT ',' + CAST(T.JobCard AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                JobCardComplete = (STUFF((SELECT ',' + CAST(T.JobCardComplete AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                WorkShopRequest = (STUFF((SELECT ',' + CAST(T.WorkShopRequest AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                PurchaseRequest = (STUFF((SELECT ',' + CAST(T.PurchaseRequest AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                SuppyOrder = (STUFF((SELECT ',' + CAST(T.SuppyOrder AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                GRN = (STUFF((SELECT ',' + CAST(T.GRN AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                Allocation = (STUFF((SELECT ',' + CAST(T.Allocation AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,'')),

                SalesInvoice = (STUFF((SELECT ',' + CAST(T.SalesInvoice AS VARCHAR(MAX))
                FROM A T where T.SaleOrderId = A1.SaleOrderId and T.VehicleModelName = A1.VehicleModelName and T.RegistrationNo = A1.RegistrationNo
                FOR XML PATH('')),1,1,''))

                from A A1
                where SalesInvoice = 'No'
                order by SaleOrderDate, SaleOrderId";

                return connection.Query<SaleOrderStatus>(sql);
            }
        }
    }
}
