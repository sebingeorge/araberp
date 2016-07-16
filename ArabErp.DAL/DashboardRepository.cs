using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class DashboardRepository: BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<DashboardMonthlySalesQuotations> GetSalesQuotationDetails(int OrganizationId)
        {            
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                            select DATEPART(month,SaleOrderDate)monthcode,DATENAME(month, SaleOrderDate) + ' ' + DATENAME(YEAR, SaleOrderDate) SODate, 
                            SUM(SI.Amount) TotalAmount
                            from SaleOrder SH inner join SaleOrderItem SI on SH.SaleOrderId = SI.SaleOrderId group by SaleOrderDate)
                            select SODate, cast(SUM(TotalAmount)/10000 as decimal(18,2)) TotalAmount from A group by SODate, monthcode";

                return connection.Query<DashboardMonthlySalesQuotations>(sql);
            }
        }
    }
}
