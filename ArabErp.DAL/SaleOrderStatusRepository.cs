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
        public IEnumerable<SaleOrderStatus> GetSaleOrderStatus(DateTime? from, DateTime? to, string customer = "", string sono = "", string lpoNo = "", string ChassisNo = "", string InstallType = "")
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

                sql = @"select distinct SI.SaleOrderItemId, S.SaleOrderId, C.CustomerName, S.SaleOrderRefNo, S.SaleOrderDate,S.EDateDelivery,S.CustomerOrderRef,S.isService, 
                        isnull(VI.RegistrationNo,isnull(VI.ChassisNo,''))RegistrationNo,
                        V.VehicleModelName,
                        VehicleInpass =  case when VI.VehicleInPassId is null then NULL else  VehicleInPassNo +','+ CONVERT (VARCHAR(15),VehicleInPassDate,106)  end, 
                        JobCard = cast('' as varchar(max)), 
                        JobCardComplete = cast('' as varchar(max)), 
                        DeliveryChellan = cast('' as varchar(max)), 
                        WorkShopRequest = cast('' as varchar(max)), 
                        PurchaseRequest = cast('' as varchar(max)), 
                        SuppyOrder =  cast('' as varchar(max)), 
                        GRN = cast('' as varchar(max)), 
                        SalesInvoice = case when SLI.SalesInvoiceId is null then NULL else SLIH.SalesInvoiceRefNo + ',' + CONVERT (VARCHAR(15),SLIH.SalesInvoiceDate,106) end, 
                        Allocation = case when IB.SerialNo is null then NULL else IB.SerialNo end
                        into #RESULT
                        from SaleOrderItem SI inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId 
                        inner join Customer C on C.CustomerId = S.CustomerId 
                        inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId 
                        --left join WorkShopRequest W on W.SaleOrderId = S.SaleOrderId 
                        --left join PurchaseRequest P on P.WorkShopRequestId = W.WorkShopRequestId 
                        --left join PurchaseRequestItem PRI on PRI.PurchaseRequestId	= P.PurchaseRequestId 
                        --left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId 
                        --left join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId 
                        --left join SupplyOrder SO on SO.SupplyOrderId = SUI.SupplyOrderId 
                        --left join GRNItem GI on GI.SupplyOrderItemId = SUI.SupplyOrderItemId 
                        --left join GRN G on G.GRNId = G.GRNId 
                        left join SalesInvoiceItem SLI on SLI.SaleOrderItemId = SI.SaleOrderItemId 
                        left Join SalesInvoice SLIH on SLIH.SalesInvoiceId = SLI.SalesInvoiceId
                        --left join StoreIssue SIS on SIS.WorkShopRequestId=W.WorkShopRequestId 
                        left join VehicleInPass VI on VI.SaleOrderItemId = SI.SaleOrderItemId
                        left join ItemBatch IB on IB.SaleOrderItemId = SI.SaleOrderItemId;

                        --select A.SaleOrderItemId, J.JobCardNo, J.JobCardDate from #RESULT A left join JobCard J on J.SaleOrderItemId = A.SaleOrderItemId
                        --where J.SaleOrderItemId is not null

                        update R set R.JobCard = J.JobCardNo +', '+ CONVERT (VARCHAR(15),J.JobCardDate,106) 
                        from JobCard J, #RESULT R
                        where J.SaleOrderId = R.SaleOrderId and R.SaleOrderItemId = J.SaleOrderItemId;

                        update R set R.JobCardComplete = J.JobCardNo +', '+ CONVERT (VARCHAR(15),J.JodCardCompletedDate,106) 
                        from JobCard J, #RESULT R
                        where J.SaleOrderId = R.SaleOrderId and R.SaleOrderItemId = J.SaleOrderItemId
                        and J.JodCardCompleteStatus = 1;

                        update R set R.DeliveryChellan = D.DeliveryChallanRefNo +', '+ CONVERT (VARCHAR(15),D.DeliveryChallanDate,106) 
                        from DeliveryChallan D,JobCard J, #RESULT R
                        where D.JobCardId = J.JobCardId and  J.SaleOrderId = R.SaleOrderId and R.SaleOrderItemId = J.SaleOrderItemId;

                        update #RESULT set WorkShopRequest = (STUFF((SELECT ', ' + CAST(T.WorkShopRequestRefNo AS VARCHAR(MAX))
                        FROM WorkShopRequest T where T.SaleOrderId = #RESULT.SaleOrderId
                        FOR XML PATH('')),1,1,''))

                        update #RESULT set PurchaseRequest = (STUFF((SELECT ', ' + CAST(P.PurchaseRequestNo AS VARCHAR(MAX))
                        FROM PurchaseRequest P inner join WorkShopRequest W on P.WorkShopRequestId = W.WorkShopRequestId where W.SaleOrderId = #RESULT.SaleOrderId
                        FOR XML PATH('')),1,1,''))

                        update #RESULT set SuppyOrder = (STUFF((SELECT distinct ', ' + CAST(SI.SupplyOrderNo AS VARCHAR(MAX))
                        FROM PurchaseRequest P inner join WorkShopRequest W on P.WorkShopRequestId = W.WorkShopRequestId 
                        inner join PurchaseRequestItem PRI on PRI.PurchaseRequestId = P.PurchaseRequestId
                        inner join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                        inner join SupplyOrder SI on SI.SupplyOrderId = SUI.SupplyOrderId
                        where W.SaleOrderId = #RESULT.SaleOrderId
                        FOR XML PATH('')),1,1,''))

                          select * from #RESULT where  CustomerName LIKE '%'+@customer+'%'
                        AND SaleOrderRefNo  LIKE '%'+@sono+'%'
                        AND isnull(CustomerOrderRef,'')  LIKE '%'+@lpoNo+'%'
                        AND isnull(RegistrationNo,'')  LIKE '%'+@ChassisNo+'%'
                        AND  ISNULL(isService, 0) = CASE @InstallType WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(isService, 0) END
                        AND SaleOrderDate >= @from AND SaleOrderDate <= @to 
                        drop table #RESULT;";

                   return connection.Query<SaleOrderStatus>(sql, new {from=from,to=to, customer = customer, sono = sono, lpoNo = lpoNo, ChassisNo = ChassisNo, InstallType = InstallType });
            }
        }
        public IEnumerable<SaleOrderStatus> GetSaleOrderStatusDTPrint(string customer = "", string sono = "", string lpoNo = "", string ChassisNo = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;


                sql = @"select distinct SI.SaleOrderItemId, S.SaleOrderId, C.CustomerName, S.SaleOrderRefNo, S.SaleOrderDate,S.CustomerOrderRef, 
                        --VI.RegistrationNo, V.VehicleModelName,
						CONCAT(VehicleModelName,' - ',CONVERT (VARCHAR(15),RegistrationNo,106))VehicleMdlNameReg,
						DATEDIFF(DAY,SaleOrderDate,GETDATE()) AS SOAgeDays,
						DATEDIFF(DAY,EDateDelivery,GETDATE()) AS EDD,
                        isnull(VI.RegistrationNo,isnull(VI.ChassisNo,''))RegistrationNo,
                        VehicleInpass =  case when VI.VehicleInPassId is null then NULL else  VehicleInPassNo +','+ CONVERT (VARCHAR(15),VehicleInPassDate,106)  end, 
                        JobCard = cast('' as varchar(max)), 
                        JobCardComplete = cast('' as varchar(max)), 
                        DeliveryChellan = cast('' as varchar(max)), 
                        WorkShopRequest = cast('' as varchar(max)), 
                        PurchaseRequest = cast('' as varchar(max)), 
                        SuppyOrder =  cast('' as varchar(max)), 
                        GRN = cast('' as varchar(max)), 
                        SalesInvoice = case when SLI.SalesInvoiceId is null then NULL else SLIH.SalesInvoiceRefNo + ',' + CONVERT (VARCHAR(15),SLIH.SalesInvoiceDate,106) end, 
                        Allocation = case when IB.SerialNo is null then NULL else IB.SerialNo end
                        into #RESULT
                        from SaleOrderItem SI inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId 
                        inner join Customer C on C.CustomerId = S.CustomerId 
                        inner join VehicleModel V on V.VehicleModelId = SI.VehicleModelId 
                        --left join WorkShopRequest W on W.SaleOrderId = S.SaleOrderId 
                        --left join PurchaseRequest P on P.WorkShopRequestId = W.WorkShopRequestId 
                        --left join PurchaseRequestItem PRI on PRI.PurchaseRequestId	= P.PurchaseRequestId 
                        --left join JobCard J on J.SaleOrderItemId = SI.SaleOrderItemId 
                        --left join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId 
                        --left join SupplyOrder SO on SO.SupplyOrderId = SUI.SupplyOrderId 
                        --left join GRNItem GI on GI.SupplyOrderItemId = SUI.SupplyOrderItemId 
                        --left join GRN G on G.GRNId = G.GRNId 
                        left join SalesInvoiceItem SLI on SLI.SaleOrderItemId = SI.SaleOrderItemId 
                        left Join SalesInvoice SLIH on SLIH.SalesInvoiceId = SLI.SalesInvoiceId
                        --left join StoreIssue SIS on SIS.WorkShopRequestId=W.WorkShopRequestId 
                        left join VehicleInPass VI on VI.SaleOrderItemId = SI.SaleOrderItemId
                        left join ItemBatch IB on IB.SaleOrderItemId = SI.SaleOrderItemId;

                        --select A.SaleOrderItemId, J.JobCardNo, J.JobCardDate from #RESULT A left join JobCard J on J.SaleOrderItemId = A.SaleOrderItemId
                        --where J.SaleOrderItemId is not null

                        update R set R.JobCard = J.JobCardNo +', '+ CONVERT (VARCHAR(15),J.JobCardDate,106) 
                        from JobCard J, #RESULT R
                        where J.SaleOrderId = R.SaleOrderId and R.SaleOrderItemId = J.SaleOrderItemId;

                        update R set R.JobCardComplete = J.JobCardNo +', '+ CONVERT (VARCHAR(15),J.JodCardCompletedDate,106) 
                        from JobCard J, #RESULT R
                        where J.SaleOrderId = R.SaleOrderId and R.SaleOrderItemId = J.SaleOrderItemId
                        and J.JodCardCompleteStatus = 1;

                        update R set R.DeliveryChellan = D.DeliveryChallanRefNo +', '+ CONVERT (VARCHAR(15),D.DeliveryChallanDate,106) 
                        from DeliveryChallan D,JobCard J, #RESULT R
                        where D.JobCardId = J.JobCardId and  J.SaleOrderId = R.SaleOrderId and R.SaleOrderItemId = J.SaleOrderItemId;

                        update #RESULT set WorkShopRequest = (STUFF((SELECT ', ' + CAST(T.WorkShopRequestRefNo AS VARCHAR(MAX))
                        FROM WorkShopRequest T where T.SaleOrderId = #RESULT.SaleOrderId
                        FOR XML PATH('')),1,1,''))

                        update #RESULT set PurchaseRequest = (STUFF((SELECT ', ' + CAST(P.PurchaseRequestNo AS VARCHAR(MAX))
                        FROM PurchaseRequest P inner join WorkShopRequest W on P.WorkShopRequestId = W.WorkShopRequestId where W.SaleOrderId = #RESULT.SaleOrderId
                        FOR XML PATH('')),1,1,''))

                        update #RESULT set SuppyOrder = (STUFF((SELECT distinct ', ' + CAST(SI.SupplyOrderNo AS VARCHAR(MAX))
                        FROM PurchaseRequest P inner join WorkShopRequest W on P.WorkShopRequestId = W.WorkShopRequestId 
                        inner join PurchaseRequestItem PRI on PRI.PurchaseRequestId = P.PurchaseRequestId
                        inner join SupplyOrderItem SUI on SUI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                        inner join SupplyOrder SI on SI.SupplyOrderId = SUI.SupplyOrderId
                        where W.SaleOrderId = #RESULT.SaleOrderId
                        FOR XML PATH('')),1,1,''))

                        select * from #RESULT where  CustomerName LIKE '%'+@customer+'%'
                        AND SaleOrderRefNo  LIKE '%'+@sono+'%'
                        AND isnull(CustomerOrderRef,'')  LIKE '%'+@lpoNo+'%'
                        AND isnull(RegistrationNo,'')  LIKE '%'+@ChassisNo+'%';

                        drop table #RESULT;";

                return connection.Query<SaleOrderStatus>(sql, new { customer = customer, sono = sono, lpoNo = lpoNo, ChassisNo = ChassisNo });
            }
        }
    }
}
