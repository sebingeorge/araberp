using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SalesRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<SalesRegister> GetSalesRegister(DateTime? from, DateTime? to,int id ,int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"select SalesInvoiceRefNo,SalesInvoiceDate,CustomerName,WorkDescr,SOI.Quantity,SOI.Rate,(SOI.Quantity*SOI.Rate)Amount,SOI.Discount,ISNULL(SOI.Amount,0)TotalAmount,UnitName from SalesInvoice S 
                               inner join SalesInvoiceItem SI on S.SalesInvoiceId=SI.SalesInvoiceId
                               inner join SaleOrder SO ON SO.SaleOrderId=S.SaleOrderId
                               inner join SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
                               inner join Customer C ON C.CustomerId=SO.CustomerId
                               inner join WorkDescription W ON W.WorkDescriptionId=SOI.WorkDescriptionId
                               left join Unit U ON U.UnitId=SOI.UnitId
                               where SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to and S.OrganizationId=@OrganizationId and SO.CustomerId=ISNULL(NULLIF(@id, 0),SO.CustomerId)";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id=id }).ToList();
            }
        }


        public IEnumerable<SalesRegister> GetSalesRegisterSummary(DateTime? from, DateTime? to, int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"select SalesInvoiceRefNo,SalesInvoiceDate,CustomerName,ISNULL(SO.TotalAmount,0)TotalAmount from SalesInvoice S 
                               inner join SalesInvoiceItem SI on S.SalesInvoiceId=SI.SalesInvoiceId
                               inner join SaleOrder SO ON SO.SaleOrderId=S.SaleOrderId
                               inner join Customer C ON C.CustomerId=SO.CustomerId

            where SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to and S.OrganizationId=@OrganizationId and SO.CustomerId=ISNULL(NULLIF(@id, 0),SO.CustomerId)";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id = id }).ToList();
            }
        }

        public IEnumerable<SalesRegister> GetPendingSO(DateTime? from, DateTime? to, int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	select SaleOrderRefNo,SaleOrderDate,CustomerName,WorkDescr,SI.Quantity Quantity,isnull(SN.Quantity,0) INVQTY,(SI.Quantity-isnull(SN.Quantity,0))BALQTY, 
                                case when (SI.Quantity-isnull(SN.Quantity,0)) < 0 then 'Excess'
                                when (isnull(SN.Quantity,0)-SI.Quantity) > 0 then 'Shortage'
                                when isnull(SN.Quantity,0) = 0 then 'Pending'  end as Status
                                from SaleOrder S
				                inner join SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
				                inner join Customer C ON C.CustomerId=S.CustomerId
				                inner join WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
				                left join SalesInvoiceItem SN on SN.SaleOrderItemId=SI.SaleOrderItemId
                                WHERE SaleOrderDate >= @from AND SaleOrderDate <= @to and S.OrganizationId=@OrganizationId and S.CustomerId=ISNULL(NULLIF(@id, 0),S.CustomerId)
				                GROUP BY SaleOrderRefNo,SaleOrderDate,CustomerName,WorkDescr,SI.Quantity ,SN.Quantity,SI.SaleOrderItemId
				                having (SI.Quantity-isnull(SN.Quantity,0)) > 0";
                                 
                            

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id = id }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetSalesAnalysisProductWise(DateTime? from, DateTime? to, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	select SaleOrderRefNo,SalesInvoiceDate,WorkDescr,isnull(SN.Quantity,0) Quantity,isnull(SN.Amount,0) Amount,
                                round((sum(SN.Amount)/(select sum(SN.Amount) From
                                SaleOrder S,SaleOrderItem SI,WorkDescription W,SalesInvoiceItem SN,SalesInvoice SNI
                                Where  S.SaleOrderId=SI.SaleOrderId And  W.WorkDescriptionId=SI.WorkDescriptionId And SN.SaleOrderItemId=SI.SaleOrderItemId
                                And SNI.SalesInvoiceId=SN.SalesInvoiceId and SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to and S.OrganizationId=@OrganizationId)*100),2) as Perc
                                from SaleOrder S
	                            inner join SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
	                            inner join WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
	                            inner join SalesInvoiceItem SN on SN.SaleOrderItemId=SI.SaleOrderItemId
                                inner join SalesInvoice SNI ON  SNI.SalesInvoiceId=SN.SalesInvoiceId
                                where SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to and S.OrganizationId=@OrganizationId
                                GROUP BY SaleOrderRefNo,SalesInvoiceDate,WorkDescr,SN.Quantity,SN.Amount";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetSalesAnalysisCustomerWise(DateTime? from, DateTime? to, int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	select SalesInvoiceRefNo,CustomerName,SUM(isnull(SN.Amount,0)) Amount from SaleOrder S
	                            inner join SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
	                            inner join Customer C ON C.CustomerId=S.CustomerId
	                            inner join SalesInvoiceItem SN on SN.SaleOrderItemId=SI.SaleOrderItemId
                                inner join SalesInvoice SNI ON  SNI.SalesInvoiceId=SN.SalesInvoiceId
                                where SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to and S.OrganizationId=@OrganizationId and S.CustomerId=ISNULL(NULLIF(@id, 0),S.CustomerId)
                                group by SalesInvoiceRefNo,CustomerName";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id = id }).ToList();
            }
        }

    }
}
