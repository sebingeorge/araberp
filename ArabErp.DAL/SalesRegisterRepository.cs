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

                string qry = @"select SalesInvoiceRefNo,SalesInvoiceDate,CustomerName,WorkDescr,SOI.Quantity,SOI.Rate,(SOI.Quantity*SOI.Rate)Amount,SOI.Discount,ISNULL(SO.TotalAmount,0)TotalAmount,UnitName from SalesInvoice S 
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
    }
}
