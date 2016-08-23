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
        public IEnumerable<SalesRegister> GetTargetVsAchieved(int OrganizationId, int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                        DECLARE @FIN_START_DATE DATETIME;
                        DECLARE @FIN_LAST_DATE DATETIME; 
                        DECLARE @FIN_ID INT;
                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;

                        SELECT @FIN_START_DATE=FyStartDate,@FIN_LAST_DATE=FyEndDate FROM FinancialYear WHERE FyId = @FIN_ID;
                        SELECT MonthId,[MonthName],MonthFromDate [YEAR] INTO #MONTH_DETAILS FROM Month;
                        SELECT  W.WorkDescriptionId,W.WorkDescrShortName WorkDescr,M.MonthId,[MonthName],
                        MAX(Target)AS [Target],
                        ISNULL(ROUND(SUM (ROUND((TotalAmount),2)),2),0) AS [Achieved],ROUND(ISNULL(MAX(Target),0) - ISNULL(SUM (ROUND((TotalAmount),2)),0),2) AS Varience,
                        ROUND((((ISNULL(MAX(Target),0)-ISNULL(SUM (ROUND((TotalAmount),2)),0))/NULLIF(ISNULL(MAX(Target),0),0))*100),2)AS [Varperc]
                        FROM  WorkDescription W 
                        LEFT JOIN SaleOrderItem SOI ON SOI.WorkDescriptionId =W.WorkDescriptionId
                        LEFT JOIN SalesInvoiceItem SI ON SOI.SaleOrderItemId =SI.SaleOrderItemId  
                        INNER JOIN SalesInvoice S ON SI.SalesInvoiceId =S.SalesInvoiceId
                        AND SalesInvoiceDate >= @FIN_START_DATE  AND SalesInvoiceDate <=@FIN_LAST_DATE
                        LEFT JOIN SalesTarget T ON T.WorkDescriptionId  = W.WorkDescriptionId AND T.FyId=@FIN_ID 
                        INNER JOIN #MONTH_DETAILS M ON M.MonthId=T.MonthId
                        where M.MonthId=ISNULL(NULLIF(@id, 0),M.MonthId)
                        GROUP BY  W.WorkDescriptionId,W.WorkDescrShortName,M.MonthId,[MonthName]
                        ORDER BY [Varperc] DESC
                        DROP TABLE #MONTH_DETAILS";	
              return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId,id=id}).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetProductWiseSalesRegister(int OrganizationId, int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                        DECLARE @FIN_START_DATE DATETIME;
                                        DECLARE @FIN_LAST_DATE DATETIME;
                                        DECLARE @FIN_ID int ;
                                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;

                                        SELECT @FIN_START_DATE=FyStartDate,@FIN_LAST_DATE=FyEndDate FROM FinancialYear WHERE FyId = @FIN_ID;
			

                                        CREATE TABLE #SALES_WORKDESC_DETAILS
                                        (
                                        Wrk_id int,
                                        Wrk_Name NVARCHAR(150)
                                        )
				
				
                                        CREATE TABLE #SALES_MONTH_DETAILS
                                        (Wrk_id int,
                                        Wrk_Name NVARCHAR(150),
                                        Jan DECIMAL(18,2),
                                        Feb DECIMAL(18,2),
                                        Mar DECIMAL(18,2),
                                        Apr DECIMAL(18,2),
                                        May DECIMAL(18,2),
                                        Jun DECIMAL(18,2),
                                        Jul DECIMAL(18,2),
                                        Aug DECIMAL(18,2),
                                        Sep DECIMAL(18,2),
                                        Oct DECIMAL(18,2),
                                        Nov DECIMAL(18,2),
                                        Dece DECIMAL(18,2)
			
                                        )
               
                                        CREATE TABLE #SALES_DETAILS
                                        (
                                        Wrk_id INT,
                                        MONTH_id INTEGER,
                                        Wrk_Name NVARCHAR(150),
                                        SO_AMT DECIMAL(18,2)
                                        )
                
                                        INSERT INTO #SALES_DETAILS(MONTH_id,Wrk_id,Wrk_Name,SO_AMT)

                                        SELECT MONTH(SalesInvoiceDate)AS MONTH_CODE,W.WorkDescriptionId,W.WorkDescrShortName WorkDescr,
                                        SUM(SI.Amount) as SO_AMT
                                        FROM SalesInvoiceItem SI 
                                        INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
                                        INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
                                        INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
                                        LEFT JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId		
                                        INNER JOIN Customer C ON C.CustomerId=SO.CustomerId AND C.CustomerId=8
                                        WHERE SalesInvoiceDate>=@FIN_START_DATE  AND SalesInvoiceDate <=@FIN_LAST_DATE AND S.OrganizationId=@OrganizationId
                                        GROUP BY MONTH(SalesInvoiceDate),YEAR(SalesInvoiceDate),W.WorkDescriptionId,W.WorkDescrShortName
			


                                        INSERT INTO #SALES_WORKDESC_DETAILS(Wrk_id ,Wrk_Name )
                                        SELECT DISTINCT W.WorkDescriptionId,W.WorkDescrShortName FROM SalesInvoiceItem SI 
                                        INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
                                        INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
                                        INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
                                        LEFT JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId	
                                        WHERE S.SalesInvoiceDate>=@FIN_START_DATE  AND S.SalesInvoiceDate <=@FIN_LAST_DATE 
                                        AND  S.OrganizationId=@OrganizationId		


                                        INSERT INTO #SALES_MONTH_DETAILS(
                                        Wrk_id ,Wrk_Name ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar )
                                        SELECT Wrk_id ,Wrk_Name ,0 AS Apr,0 AS May,0 AS Jun,0 AS Jul, 0 AS Aug,0 AS Sep ,0 AS Oct,0 AS Nov,
                                        0 AS Dece,0 AS Jan,0 AS Feb,0 AS Mar FROM #SALES_WORKDESC_DETAILS 

                                        --JAN
                                        Update  #SALES_MONTH_DETAILS SET Jan = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=1

                                        --FEB
                                        Update  #SALES_MONTH_DETAILS SET Feb  = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=2

                                        --MAR
                                        Update  #SALES_MONTH_DETAILS SET Mar  = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=3    

                                        --APR
                                        Update  #SALES_MONTH_DETAILS SET Apr = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS  WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=4

                                        --MAY
                                        Update  #SALES_MONTH_DETAILS SET May  = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE   #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=5

                                        --JUN
                                        Update  #SALES_MONTH_DETAILS SET Jun = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=6

                                        --JUL
                                        Update  #SALES_MONTH_DETAILS SET Jul = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=7

                                        --AUG
                                        Update  #SALES_MONTH_DETAILS SET Aug = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=8

                                        --SEP
                                        Update  #SALES_MONTH_DETAILS SET Sep = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=9

                                        --OCT
                                        Update  #SALES_MONTH_DETAILS SET Oct = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=10

                                        --NOV
                                        Update  #SALES_MONTH_DETAILS SET Nov = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=11

                                        --DEC
                                        Update  #SALES_MONTH_DETAILS SET Dece = ISNULL(SO_AMT,0)
                                        FROM #SALES_DETAILS WHERE    #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=12

			                                                    
                                        SELECT Wrk_id ,Wrk_Name WorkDescr  ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar 
                                        FROM  #SALES_MONTH_DETAILS"; 





	
              return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId,id=id}).ToList();
            }
        }
        
    }
}
