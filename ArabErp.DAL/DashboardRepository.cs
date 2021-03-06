﻿using System;
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
        public IEnumerable<DashboardMonthlySalesOrders> GetSalesOrderDetails(int OrganizationId)
        {            
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                            select DATEPART(month,SaleOrderDate)monthcode,DATENAME(month, SaleOrderDate) + ' ' + DATENAME(YEAR, SaleOrderDate) SODate, 
                            SUM(SI.Amount) TotalAmount
                            from SaleOrder SH inner join SaleOrderItem SI on SH.SaleOrderId = SI.SaleOrderId group by SaleOrderDate)
                            select SODate, cast(SUM(TotalAmount)/10000 as decimal(18,2)) TotalAmount from A group by SODate, monthcode";

                return connection.Query<DashboardMonthlySalesOrders>(sql);
            }
        }
        public IEnumerable<DashboardTotalSalesQuotations> GetSalesQuotationDetails(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                            select DATEPART(month,QuotationDate)monthcode,DATENAME(month, QuotationDate) + ' ' + DATENAME(YEAR, QuotationDate) SODate from SalesQuotation)
                            select monthcode,A.SODate, count(*) Quotations from A group by SODate,monthcode";

                return connection.Query<DashboardTotalSalesQuotations>(sql);
            }
        }
        public IEnumerable<DashboardTotalSalesQuotations> GetAccesptedSalesQuotationDetails(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                select DATEPART(month,QuotationDate)monthcode,DATENAME(month, QuotationDate) + ' ' + DATENAME(YEAR, QuotationDate) SODate 
                from SalesQuotation SQ inner join SaleOrder SO on SQ.SalesQuotationId = SO.SalesQuotationId)
                select monthcode,A.SODate, count(*) Quotations from A group by SODate,monthcode";

                return connection.Query<DashboardTotalSalesQuotations>(sql);
            }
        }
        public IEnumerable<DashboardTotalSalesQuotations> GetAccesptedProjectSalesQuotationDetails(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                select DATEPART(month,QuotationDate)monthcode,DATENAME(month, QuotationDate) + ' ' + DATENAME(YEAR, QuotationDate) SODate 
                from SalesQuotation SQ inner join SaleOrder SO on SQ.SalesQuotationId = SO.SalesQuotationId where SQ.isProjectBased = 1)
                select monthcode,A.SODate, count(*) Quotations from A group by SODate,monthcode";

                return connection.Query<DashboardTotalSalesQuotations>(sql);
            }
        }
        public IEnumerable<DashboardTotalSalesQuotations> GetAccesptedTransportationSalesQuotationDetails(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                select DATEPART(month,QuotationDate)monthcode,DATENAME(month, QuotationDate) + ' ' + DATENAME(YEAR, QuotationDate) SODate 
                from SalesQuotation SQ inner join SaleOrder SO on SQ.SalesQuotationId = SO.SalesQuotationId where SQ.isProjectBased = 0)
                select monthcode,A.SODate, count(*) Quotations from A group by SODate,monthcode";

                return connection.Query<DashboardTotalSalesQuotations>(sql);
            }
        }
        public IEnumerable<DashboardPurchaseSales> GetSalesDetails(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                select DATEPART(month,SalesInvoiceDate)monthcode,DATENAME(month, SalesInvoiceDate) + ' ' + DATENAME(YEAR, SalesInvoiceDate) InvoiceDate,
                SOI.Amount
                from SalesInvoice SH
                inner join SalesInvoiceItem SI on SH.SalesInvoiceId = SI.SalesInvoiceId
                inner join SaleOrderItem SOI on SOI.SaleOrderItemId = SI.SaleOrderItemId)
                select monthcode, InvoiceDate, sum(Amount)Amount from A group by monthcode, InvoiceDate;";

                return connection.Query<DashboardPurchaseSales>(sql);
            }
        }
        public IEnumerable<DashboardPurchaseSales> GetPurchaseDetails(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"with A as(
                select DATEPART(month,PurchaseBillDate)monthcode,DATENAME(month, PurchaseBillDate) + ' ' + DATENAME(YEAR, PurchaseBillDate) InvoiceDate,
                PurchaseBillAmount
                from PurchaseBill)
                select monthcode, InvoiceDate, sum(PurchaseBillAmount)Amount from A group by monthcode, InvoiceDate;";

                return connection.Query<DashboardPurchaseSales>(sql);
            }
        }
        public DashboardFGAllocated GetFGAllocated(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"declare @TotalFG decimal(18,2);
                                declare @AllocatedFG decimal(18,2);
                                select @TotalFG = count(*) from ItemBatch;
                                select @AllocatedFG = count(*) from ItemBatch where SaleOrderItemId is not null;
                                select TotalFG = @TotalFG, AllocatedFG = @AllocatedFG;";

                return connection.Query<DashboardFGAllocated>(sql).Single();
            }
        }
        public DashboardSaleOrderAllocated GetFGAllocatedSaleOrder(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"declare @TotalFG decimal(18,2);
                declare @AllocatedSaleOrders decimal(18,2);
                declare @TotalSaleOrders decimal(18,2);
                select @TotalFG = count(*) from ItemBatch;
                select @TotalSaleOrders = count(*) from SaleOrderItem where SaleOrderItemId not in (select SaleOrderItemId from ItemBatch where SaleOrderItemId is not null);
                select @AllocatedSaleOrders = count(*) from SaleOrderItem where SaleOrderItemId in (select SaleOrderItemId from ItemBatch where SaleOrderItemId is not null);
                select TotalFG = @TotalFG, AllocatedSaleOrders = @AllocatedSaleOrders, TotalSaleOrders = @TotalSaleOrders;";

                return connection.Query<DashboardSaleOrderAllocated>(sql).Single();
            }
        }
        public IEnumerable<DashboardJobCardCompletedDaily> GetJobCardCompletedDaily(int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 7.1.2017 3.00p
                //                string sql = @"select top 7 left(convert(varchar(50),JodCardCompletedDate,106),6) JobcardDate, COUNT(JobCardId) JobCardCount 
                //				                from JobCard where JodCardCompletedDate is not null
                //				                GROUP BY JobCard.JodCardCompletedDate
                //				                order by JodCardCompletedDate desc"; 
                #endregion

                string sql = @"SELECT
                                JodCardCompletedDate,

                                COUNT(isProjectBased) JobcardCount,0 JobcardCount_Transport,0JobcardCount_Project 
                                INTO #TEMP
                                FROM JobCard
                                WHERE JobCard.JodCardCompleteStatus = 1 
                                GROUP BY JodCardCompletedDate 
                                ORDER BY JobCard.JodCardCompletedDate DESC;

                                WITH A AS
                                (
	                                SELECT JodCardCompletedDate,count(Jobcardid)JobcardCount_Transport FROM JobCard WHERE JobCard.JodCardCompleteStatus = 1 AND JobCard.isProjectBased = 0
	                                GROUP BY JobCard.JodCardCompleteStatus,JodCardCompletedDate
                                )
                                update T SET T.JobcardCount_Transport=A.JobcardCount_Transport FROM A INNER JOIN  #TEMP T ON T.JodCardCompletedDate=A.JodCardCompletedDate;

                                WITH B AS
                                (
	                                SELECT JodCardCompletedDate,count(Jobcardid)JobcardCount_Project FROM JobCard WHERE JobCard.JodCardCompleteStatus = 1 AND JobCard.isProjectBased = 1
	                                GROUP BY JobCard.JodCardCompleteStatus,JodCardCompletedDate
                                )
                                update T SET T.JobcardCount_Project=B.JobcardCount_Project FROM B INNER JOIN #TEMP T ON T.JodCardCompletedDate=B.JodCardCompletedDate;
                                select top 7 left(convert(varchar(50),JodCardCompletedDate,106),6) JobcardDate, JobcardCount_Transport, JobcardCount_Project from #TEMP order by #TEMP.JodCardCompletedDate DESC
                                drop table #TEMP;";

                return connection.Query<DashboardJobCardCompletedDaily>(sql);
            }
        }
    }
}
