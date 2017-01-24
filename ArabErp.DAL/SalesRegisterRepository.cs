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
        public IEnumerable<SalesRegister> GetSalesRegister(DateTime? from, DateTime? to,int id ,int OrganizationId, int? project)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 19.1.2016 1.06p
                //                string qry = @"select SalesInvoiceRefNo,SalesInvoiceDate,CustomerName,WorkDescr,SOI.Quantity,SOI.Rate,(SOI.Quantity*SOI.Rate)Amount,SOI.Discount,ISNULL(SOI.Amount,0)TotalAmount,UnitName from SalesInvoice S 
                //                               inner join SalesInvoiceItem SI on S.SalesInvoiceId=SI.SalesInvoiceId
                //                               inner join SaleOrder SO ON SO.SaleOrderId=S.SaleOrderId
                //                               inner join SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
                //                               inner join Customer C ON C.CustomerId=SO.CustomerId
                //                               inner join WorkDescription W ON W.WorkDescriptionId=SOI.WorkDescriptionId
                //                               left join Unit U ON U.UnitId=SOI.UnitId
                //                               where SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to and S.OrganizationId=@OrganizationId and SO.CustomerId=ISNULL(NULLIF(@id, 0),SO.CustomerId)"; 
                #endregion

                string qry = @"select 
	                                SalesInvoiceRefNo,
	                                SalesInvoiceDate,
	                                CustomerName,
	                                --WorkDescr,
	                                --SOI.Quantity,
	                                --SOI.Rate,
	                                --(SI.Quantity*SI.Rate)Amount,
	                                COUNT(SI.Quantity) Quantity,
	                                SUM(ISNULL(ISNULL(SI.Rate, 0)*ISNULL(SI.Quantity, 1), 0)) Amount,
	                                SUM(ISNULL(SI.Discount, 0)) Discount,
	                                ISNULL(S.TotalAmount,0)TotalAmount,
	                                --UnitName
	                                REPLACE(STUFF((SELECT DISTINCT ', ' + CAST(T3.WorkDescr AS VARCHAR(MAX)) FROM SaleOrderItem T1
			                            INNER JOIN SalesInvoiceItem T2 ON T1.SaleOrderItemId = T2.SaleOrderItemId
			                            INNER JOIN WorkDescription T3 ON T1.WorkDescriptionId = T3.WorkDescriptionId
			                            WHERE T2.SalesInvoiceId = S.SalesInvoiceId FOR XML PATH('')), 1, 2, ''), 'amp;', '') WorkDescr
                                from SalesInvoice S 
                                inner join SalesInvoiceItem SI on S.SalesInvoiceId=SI.SalesInvoiceId
                                inner join SaleOrder SO ON SO.SaleOrderId=S.SaleOrderId
                                --inner join SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
                                inner join Customer C ON C.CustomerId=SO.CustomerId
                                --inner join WorkDescription W ON W.WorkDescriptionId=SOI.WorkDescriptionId
                                --left join Unit U ON U.UnitId=SOI.UnitId
                                where SalesInvoiceDate >= @from AND SalesInvoiceDate <= @to 
                                    and S.OrganizationId=@OrganizationId 
                                    and SO.CustomerId=ISNULL(NULLIF(@id, 0),SO.CustomerId)
                                    AND SO.isProjectBased = ISNULL(@project, SO.isProjectBased)
                                GROUP BY S.SalesInvoiceId, SalesInvoiceRefNo, SalesInvoiceDate, CustomerName, SI.Discount, S.TotalAmount
                                ORDER BY S.SalesInvoiceDate, S.SalesInvoiceId";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id=id, project = project }).ToList();
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

                string qry = @"	select SaleOrderRefNo,SaleOrderDate,CustomerName,CONCAT(W.WorkDescrShortName,'/',FU.ItemName,'/',B.ItemName)WorkDescr,
                                SI.Quantity Quantity,isnull(SN.Quantity,0) INVQTY,(SI.Quantity-isnull(SN.Quantity,0))BALQTY, 
                                case when (SI.Quantity-isnull(SN.Quantity,0)) < 0 then 'Excess'
                                when (isnull(SN.Quantity,0)-SI.Quantity) > 0 then 'Shortage'
                                when isnull(SN.Quantity,0) = 0 then 'Pending'  end as Status
                                from SaleOrder S
				                inner join SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
				                inner join Customer C ON C.CustomerId=S.CustomerId
				                inner join WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                                left join Item FU ON FU.ItemId=W.FreezerUnitId
                                left join Item B ON B.ItemId =W.BoxId
				                left join SalesInvoiceItem SN on SN.SaleOrderItemId=SI.SaleOrderItemId
                                WHERE SaleOrderDate >= @from AND SaleOrderDate <= @to and S.OrganizationId=@OrganizationId and S.CustomerId=ISNULL(NULLIF(@id, 0),S.CustomerId)
				                GROUP BY SaleOrderRefNo,SaleOrderDate,CustomerName,W.WorkDescrShortName,FU.ItemName,B.ItemName,SI.Quantity ,SN.Quantity,SI.SaleOrderItemId
				                having (SI.Quantity-isnull(SN.Quantity,0)) > 0";
                                 
                            

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id = id }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetSalesAnalysisProductWise(DateTime? from, DateTime? to, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	select SaleOrderRefNo,SalesInvoiceDate,WorkDescrShortName,isnull(SN.Quantity,0) Quantity,isnull(SN.Amount,0) Amount,
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
                                GROUP BY SaleOrderRefNo,SalesInvoiceDate,WorkDescrShortName,SN.Quantity,SN.Amount";

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
        public IEnumerable<SalesRegister> GetTargetVsAchieved(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
               
                        DECLARE @FIN_ID INT;
                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;

                        SELECT MonthId,[MonthName],MonthFromDate [YEAR] INTO #MONTH_DETAILS FROM Month;
                        SELECT  W.WorkDescriptionId,W.WorkDescrShortName WorkDescr,M.MonthId,[MonthName],
                        MAX(Target)AS [Target],
                        ISNULL(ROUND(SUM (ROUND((TotalAmount),2)),2),0) AS [Achieved],ROUND(ISNULL(MAX(Target),0) - ISNULL(SUM (ROUND((TotalAmount),2)),0),2) AS Varience,
                        ROUND((((ISNULL(MAX(Target),0)-ISNULL(SUM (ROUND((TotalAmount),2)),0))/NULLIF(ISNULL(MAX(Target),0),0))*100),2)AS [Varperc]
                        FROM  WorkDescription W 
                        LEFT JOIN SaleOrderItem SOI ON SOI.WorkDescriptionId =W.WorkDescriptionId
                        LEFT JOIN SalesInvoiceItem SI ON SOI.SaleOrderItemId =SI.SaleOrderItemId  
                        INNER JOIN SalesInvoice S ON SI.SalesInvoiceId =S.SalesInvoiceId
                        AND SalesInvoiceDate >= @FYStartdate  AND SalesInvoiceDate <=@FYEnddate
                        LEFT JOIN SalesTarget T ON T.WorkDescriptionId  = W.WorkDescriptionId AND T.FyId=@FIN_ID 
                        INNER JOIN #MONTH_DETAILS M ON M.MonthId=T.MonthId
                        where M.MonthId=ISNULL(NULLIF(@id, 0),M.MonthId)
                        GROUP BY  W.WorkDescriptionId,W.WorkDescrShortName,M.MonthId,[MonthName]
                        ORDER BY [Varperc] DESC
                        DROP TABLE #MONTH_DETAILS";
                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, id = id, FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetProductWiseSalesRegister(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                      
                                        DECLARE @FIN_ID int;
                                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;
                                       
			
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

                                        SELECT DISTINCT MONTH(SalesInvoiceDate)AS MONTH_CODE,W.WorkDescriptionId,W.WorkDescrShortName WorkDescr,
                                        S.TotalAmount as SO_AMT
                                        FROM SalesInvoiceItem SI 
                                        INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
                                        INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
                                        INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
                                        LEFT JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId		
                                        INNER JOIN Customer C ON C.CustomerId=SO.CustomerId AND C.CustomerId=ISNULL(NULLIF(@id, 0),C.CustomerId)
                                        WHERE SalesInvoiceDate>=@FYStartdate  AND SalesInvoiceDate <=@FYEnddate AND S.OrganizationId=@OrganizationId
                                      
			
                                        INSERT INTO #SALES_WORKDESC_DETAILS(Wrk_id ,Wrk_Name )
                                        SELECT DISTINCT W.WorkDescriptionId,W.WorkDescrShortName FROM SalesInvoiceItem SI 
                                        INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
                                        INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
                                        INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
                                        LEFT JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId	
                                        WHERE S.SalesInvoiceDate>=@FYStartdate  AND S.SalesInvoiceDate <=@FYEnddate 
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
                                        FROM #SALES_DETAILS WHERE  #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=12

                                                   
                                        SELECT Wrk_id,Wrk_Name WorkDescr,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar 
                                        FROM  #SALES_MONTH_DETAILS"; 

              return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId,id=id,FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }


        public IEnumerable<SalesRegister> GetCustomerWiseSalesRegister(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                 
                                    DECLARE @FIN_ID int;
                                    SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;
                                            

		                                    CREATE TABLE #SALES_CUS_DETAILS
		                                    (
		                                    Cus_Id int,
		                                    Cus_Name NVARCHAR(150)
		                                    )
		
		                                    CREATE TABLE #SALES_MONTH_DETAILS
		                                    (Cus_Id int,
		                                    Cusname_Name NVARCHAR(150),
		                                    Apr DECIMAL(18,2),
		                                    May DECIMAL(18,2),
		                                    Jun DECIMAL(18,2),
		                                    Jul DECIMAL(18,2),
		                                    Aug DECIMAL(18,2),
		                                    Sep DECIMAL(18,2),
		                                    Oct DECIMAL(18,2),
		                                    Nov DECIMAL(18,2),
		                                    Dece DECIMAL(18,2),
		                                    Jan DECIMAL(18,2),
		                                    Feb DECIMAL(18,2),
		                                    Mar DECIMAL(18,2)  
		                                    )
               
		                                    CREATE TABLE #SALES_DETAILS
		                                    (
		                                    MCus_CODE INT,
		                                    MONTH_CODE INTEGER,
		                                    MCus_NAME NVARCHAR(150),
		                                    SO_AMT DECIMAL(18,2)
		                                    )


		                                    INSERT INTO #SALES_DETAILS(MONTH_CODE,MCus_CODE,MCus_NAME,SO_AMT )					
		                                    SELECT DISTINCT MONTH(SalesInvoiceDate)AS MONTH_CODE,C.CustomerId,CustomerName,
	                                        S.TotalAmount as SO_AMT  FROM SalesInvoiceItem SI 
		                                    INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
		                                    INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
		                                    INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
		                                    INNER JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId	AND W.WorkDescriptionId =ISNULL(NULLIF(@id, 0),W.WorkDescriptionId)	
		                                    INNER JOIN Customer C ON C.CustomerId=SO.CustomerId AND C.CustomerId=C.CustomerId
		                                    WHERE SalesInvoiceDate>=@FYStartdate AND SalesInvoiceDate <=@FYEnddate AND S.OrganizationId=@OrganizationId
		                                    ORDER BY CustomerName		


		                                    INSERT INTO #SALES_CUS_DETAILS(Cus_Id ,Cus_Name)
		                                    SELECT DISTINCT C.CustomerId ,C.CustomerName  FROM SalesInvoice S,Customer C,SaleOrder SO
		                                    WHERE SO.SaleOrderId =S.SaleOrderId AND C.CustomerId=SO.CustomerId AND SalesInvoiceDate>=@FYStartdate  
		                                    AND SalesInvoiceDate <=@FYEnddate AND S.OrganizationId=@OrganizationId
		
		
		                                    INSERT INTO #SALES_MONTH_DETAILS(
		                                    Cus_Id ,Cusname_Name ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar )				   
		                                    SELECT Cus_Id,Cus_Name ,0 AS Apr,0 AS May,0 AS Jun,0 AS Jul, 0 AS Aug,0 AS Sep ,0 AS Oct,0 AS Nov,
		                                    0 AS Dece,0 AS Jan,0 AS Feb,0 AS Mar FROM #SALES_CUS_DETAILS 


		                                    --JAN
			                                    Update  #SALES_MONTH_DETAILS SET Jan = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE  Cus_Id= MCus_CODE  AND MONTH_CODE=1

			                                    --FEB
			                                    Update  #SALES_MONTH_DETAILS SET Feb  = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=2

			                                    --MAR
			                                    Update  #SALES_MONTH_DETAILS SET Mar  = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=3		
			                                    --APR
			                                    Update  #SALES_MONTH_DETAILS SET Apr = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS  WHERE    Cus_Id= MCus_CODE AND MONTH_CODE=4

			                                    --MAY
			                                    Update  #SALES_MONTH_DETAILS SET May  = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=5

			                                    --JUN
			                                    Update  #SALES_MONTH_DETAILS SET Jun = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=6

			                                    --JUL
			                                    Update  #SALES_MONTH_DETAILS SET Jul = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=7

			                                    --AUG
			                                    Update  #SALES_MONTH_DETAILS SET Aug = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE   Cus_Id= MCus_CODE  AND MONTH_CODE=8

			                                    --SEP
			                                    Update  #SALES_MONTH_DETAILS SET Sep = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=9

			                                    --OCT
			                                    Update  #SALES_MONTH_DETAILS SET Oct = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=10

			                                    --NOV
			                                    Update  #SALES_MONTH_DETAILS SET Nov = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE   Cus_Id= MCus_CODE  AND MONTH_CODE=11

			                                    --DEC
			                                    Update  #SALES_MONTH_DETAILS SET Dece = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=12
     
                                                          
		                                    SELECT Cus_Id,Cusname_Name CustomerName ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar 
		                                    FROM  #SALES_MONTH_DETAILS ";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, id = id,FYStartdate = FYStartdate, FYEnddate = FYEnddate  }).ToList();
            }
        }

        public IEnumerable<GINRegister> GetGINRegisterData(string id, int OrganizationId,string partno)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @" select I.ItemRefNo,I.ItemName,ISNULL(I.PartNo,'-')PartNo,SO.SaleOrderRefNo,J.JobCardNo,W.WorkShopRequestRefNo,
                                WI.Quantity,SUM(ISNULL(SI.IssuedQuantity,0))ISSQTY,WI.Quantity-SUM(ISNULL(SI.IssuedQuantity,0))BALQTY,U.UnitName,
                                (select sum(Quantity) from StockUpdate S where S.ItemId=I.ItemId)STOCK
                                from WorkShopRequest W
		                        INNER JOIN WorkShopRequestItem WI  ON W.WorkShopRequestId=WI.WorkShopRequestId
		                        INNER JOIN Item I ON I.ItemId=WI.ItemId
		                        INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
		                        LEFT JOIN  SaleOrder SO ON SO.SaleOrderId=W.SaleOrderId
                                LEFT JOIN  SaleOrderItem SOI ON SOI.SaleOrderId=SO.SaleOrderId
                                LEFT JOIN  WorkDescription WD ON WD.WorkDescriptionId=SOI.WorkDescriptionId
                                LEFT JOIN  StoreIssueItem SI ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
                                LEFT JOIN  Item FU ON FU.ItemId=WD.FreezerUnitId
                                LEFT JOIN  Item B ON B.ItemId=WD.BoxId
                                LEFT JOIN  JobCard J ON J.JobCardId=W.JobCardId 
                                WHERE I.ItemName LIKE '%'+@id+'%' and isnull(I.PartNo,'') like '%'+@partno+'%' AND  W.OrganizationId=@OrganizationId	
		                        group by I.ItemRefNo,I.ItemName,I.PartNo,SO.SaleOrderRefNo,J.JobCardNo,WI.Quantity,U.UnitName,W.WorkShopRequestRefNo,I.ItemId";

                return connection.Query<GINRegister>(qry, new { OrganizationId = OrganizationId, id = id,partno=partno}).ToList();
            }
        }

        public IEnumerable<SalesRegister> GetSalesRegisterDTPrint(DateTime? from, DateTime? to, int id, int OrganizationId)
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

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id = id }).ToList();
            }
        }

        public IEnumerable<SalesRegister> GetSalesRegisterSummaryDTPrint(DateTime? from, DateTime? to, int id, int OrganizationId)
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


        public IEnumerable<SalesRegister> GetPendingSODTPrint(DateTime? from, DateTime? to, int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	select SaleOrderRefNo,SaleOrderDate,CustomerName,CONCAT(W.WorkDescrShortName,'/',FU.ItemName,'/',B.ItemName)WorkDescr,
                                SI.Quantity Quantity,isnull(SN.Quantity,0) INVQTY,(SI.Quantity-isnull(SN.Quantity,0))BALQTY, 
                                case when (SI.Quantity-isnull(SN.Quantity,0)) < 0 then 'Excess'
                                when (isnull(SN.Quantity,0)-SI.Quantity) > 0 then 'Shortage'
                                when isnull(SN.Quantity,0) = 0 then 'Pending'  end as Status
                                from SaleOrder S
				                inner join SaleOrderItem SI ON S.SaleOrderId=SI.SaleOrderId
				                inner join Customer C ON C.CustomerId=S.CustomerId
				                inner join WorkDescription W ON W.WorkDescriptionId=SI.WorkDescriptionId
                                left join Item FU ON FU.ItemId=W.FreezerUnitId
                                left join Item B ON B.ItemId =W.BoxId
				                left join SalesInvoiceItem SN on SN.SaleOrderItemId=SI.SaleOrderItemId
                                WHERE SaleOrderDate >= @from AND SaleOrderDate <= @to and S.OrganizationId=@OrganizationId and S.CustomerId=ISNULL(NULLIF(@id, 0),S.CustomerId)
				                GROUP BY SaleOrderRefNo,SaleOrderDate,CustomerName,W.WorkDescrShortName,FU.ItemName,B.ItemName,SI.Quantity ,SN.Quantity,SI.SaleOrderItemId
				                having (SI.Quantity-isnull(SN.Quantity,0)) > 0";



                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to, id = id }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetSalesAnalysisProductWiseDTPrint(DateTime? from, DateTime? to, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	select SaleOrderRefNo,SalesInvoiceDate,WorkDescrShortName,isnull(SN.Quantity,0) Quantity,isnull(SN.Amount,0) Amount,
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
                                GROUP BY SaleOrderRefNo,SalesInvoiceDate,WorkDescrShortName,SN.Quantity,SN.Amount";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }


        public IEnumerable<SalesRegister> GetSalesAnalysisCustomerWiseDTPrint(DateTime? from, DateTime? to, int id, int OrganizationId)
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

        public IEnumerable<SalesRegister> GetTargetVsAchievedDTPrint(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
               
                        DECLARE @FIN_ID INT;
                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;

                        SELECT MonthId,[MonthName],MonthFromDate [YEAR] INTO #MONTH_DETAILS FROM Month;
                        SELECT  W.WorkDescriptionId,W.WorkDescrShortName WorkDescr,M.MonthId,[MonthName],
                        MAX(Target)AS [Target],
                        ISNULL(ROUND(SUM (ROUND((TotalAmount),2)),2),0) AS [Achieved],ROUND(ISNULL(MAX(Target),0) - ISNULL(SUM (ROUND((TotalAmount),2)),0),2) AS Varience,
                        ROUND((((ISNULL(MAX(Target),0)-ISNULL(SUM (ROUND((TotalAmount),2)),0))/NULLIF(ISNULL(MAX(Target),0),0))*100),2)AS [Varperc]
                        FROM  WorkDescription W 
                        LEFT JOIN SaleOrderItem SOI ON SOI.WorkDescriptionId =W.WorkDescriptionId
                        LEFT JOIN SalesInvoiceItem SI ON SOI.SaleOrderItemId =SI.SaleOrderItemId  
                        INNER JOIN SalesInvoice S ON SI.SalesInvoiceId =S.SalesInvoiceId
                        AND SalesInvoiceDate >= @FYStartdate  AND SalesInvoiceDate <=@FYEnddate
                        LEFT JOIN SalesTarget T ON T.WorkDescriptionId  = W.WorkDescriptionId AND T.FyId=@FIN_ID 
                        INNER JOIN #MONTH_DETAILS M ON M.MonthId=T.MonthId
                        where M.MonthId=ISNULL(NULLIF(@id, 0),M.MonthId)
                        GROUP BY  W.WorkDescriptionId,W.WorkDescrShortName,M.MonthId,[MonthName]
                        ORDER BY [Varperc] DESC
                        DROP TABLE #MONTH_DETAILS";
                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, id = id, FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetProductWiseSalesRegisterDTPrint(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                      
                                        DECLARE @FIN_ID int;
                                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;
                                       
			
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

                                        SELECT DISTINCT MONTH(SalesInvoiceDate)AS MONTH_CODE,W.WorkDescriptionId,W.WorkDescrShortName WorkDescr,
                                        S.TotalAmount as SO_AMT
                                        FROM SalesInvoiceItem SI 
                                        INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
                                        INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
                                        INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
                                        LEFT JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId		
                                        INNER JOIN Customer C ON C.CustomerId=SO.CustomerId AND C.CustomerId=ISNULL(NULLIF(@id, 0),C.CustomerId)
                                        WHERE SalesInvoiceDate>=@FYStartdate  AND SalesInvoiceDate <=@FYEnddate AND S.OrganizationId=@OrganizationId
                                      
			
                                        INSERT INTO #SALES_WORKDESC_DETAILS(Wrk_id ,Wrk_Name )
                                        SELECT DISTINCT W.WorkDescriptionId,W.WorkDescrShortName FROM SalesInvoiceItem SI 
                                        INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
                                        INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
                                        INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
                                        LEFT JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId	
                                        WHERE S.SalesInvoiceDate>=@FYStartdate  AND S.SalesInvoiceDate <=@FYEnddate 
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
                                        FROM #SALES_DETAILS WHERE  #SALES_MONTH_DETAILS.Wrk_id= #SALES_DETAILS.Wrk_id AND MONTH_id=12

                                                   
                                        SELECT Wrk_id,Wrk_Name WorkDescr,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar 
                                        FROM  #SALES_MONTH_DETAILS";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, id = id, FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }
        public IEnumerable<SalesRegister> GetCustomerWiseSalesRegisterDTPrint(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                 
                                    DECLARE @FIN_ID int;
                                    SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;
                                            

		                                    CREATE TABLE #SALES_CUS_DETAILS
		                                    (
		                                    Cus_Id int,
		                                    Cus_Name NVARCHAR(150)
		                                    )
		
		                                    CREATE TABLE #SALES_MONTH_DETAILS
		                                    (Cus_Id int,
		                                    Cusname_Name NVARCHAR(150),
		                                    Apr DECIMAL(18,2),
		                                    May DECIMAL(18,2),
		                                    Jun DECIMAL(18,2),
		                                    Jul DECIMAL(18,2),
		                                    Aug DECIMAL(18,2),
		                                    Sep DECIMAL(18,2),
		                                    Oct DECIMAL(18,2),
		                                    Nov DECIMAL(18,2),
		                                    Dece DECIMAL(18,2),
		                                    Jan DECIMAL(18,2),
		                                    Feb DECIMAL(18,2),
		                                    Mar DECIMAL(18,2)  
		                                    )
               
		                                    CREATE TABLE #SALES_DETAILS
		                                    (
		                                    MCus_CODE INT,
		                                    MONTH_CODE INTEGER,
		                                    MCus_NAME NVARCHAR(150),
		                                    SO_AMT DECIMAL(18,2)
		                                    )


		                                    INSERT INTO #SALES_DETAILS(MONTH_CODE,MCus_CODE,MCus_NAME,SO_AMT )					
		                                    SELECT DISTINCT MONTH(SalesInvoiceDate)AS MONTH_CODE,C.CustomerId,CustomerName,
	                                        S.TotalAmount as SO_AMT  FROM SalesInvoiceItem SI 
		                                    INNER JOIN SalesInvoice S  ON SI.SalesInvoiceId  =S.SalesInvoiceId  
		                                    INNER JOIN SaleOrderItem SOI  ON SI.SaleOrderItemId  =SOI.SaleOrderItemId 
		                                    INNER JOIN SaleOrder SO ON SOI.SaleOrderId=SO.SaleOrderId
		                                    INNER JOIN WorkDescription W ON W.WorkDescriptionId  =SOI.WorkDescriptionId	AND W.WorkDescriptionId =ISNULL(NULLIF(@id, 0),W.WorkDescriptionId)	
		                                    INNER JOIN Customer C ON C.CustomerId=SO.CustomerId AND C.CustomerId=C.CustomerId
		                                    WHERE SalesInvoiceDate>=@FYStartdate AND SalesInvoiceDate <=@FYEnddate AND S.OrganizationId=@OrganizationId
		                                    ORDER BY CustomerName		


		                                    INSERT INTO #SALES_CUS_DETAILS(Cus_Id ,Cus_Name)
		                                    SELECT DISTINCT C.CustomerId ,C.CustomerName  FROM SalesInvoice S,Customer C,SaleOrder SO
		                                    WHERE SO.SaleOrderId =S.SaleOrderId AND C.CustomerId=SO.CustomerId AND SalesInvoiceDate>=@FYStartdate  
		                                    AND SalesInvoiceDate <=@FYEnddate AND S.OrganizationId=@OrganizationId
		
		
		                                    INSERT INTO #SALES_MONTH_DETAILS(
		                                    Cus_Id ,Cusname_Name ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar )				   
		                                    SELECT Cus_Id,Cus_Name ,0 AS Apr,0 AS May,0 AS Jun,0 AS Jul, 0 AS Aug,0 AS Sep ,0 AS Oct,0 AS Nov,
		                                    0 AS Dece,0 AS Jan,0 AS Feb,0 AS Mar FROM #SALES_CUS_DETAILS 


		                                    --JAN
			                                    Update  #SALES_MONTH_DETAILS SET Jan = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE  Cus_Id= MCus_CODE  AND MONTH_CODE=1

			                                    --FEB
			                                    Update  #SALES_MONTH_DETAILS SET Feb  = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=2

			                                    --MAR
			                                    Update  #SALES_MONTH_DETAILS SET Mar  = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=3		
			                                    --APR
			                                    Update  #SALES_MONTH_DETAILS SET Apr = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS  WHERE    Cus_Id= MCus_CODE AND MONTH_CODE=4

			                                    --MAY
			                                    Update  #SALES_MONTH_DETAILS SET May  = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=5

			                                    --JUN
			                                    Update  #SALES_MONTH_DETAILS SET Jun = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=6

			                                    --JUL
			                                    Update  #SALES_MONTH_DETAILS SET Jul = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=7

			                                    --AUG
			                                    Update  #SALES_MONTH_DETAILS SET Aug = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE   Cus_Id= MCus_CODE  AND MONTH_CODE=8

			                                    --SEP
			                                    Update  #SALES_MONTH_DETAILS SET Sep = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=9

			                                    --OCT
			                                    Update  #SALES_MONTH_DETAILS SET Oct = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=10

			                                    --NOV
			                                    Update  #SALES_MONTH_DETAILS SET Nov = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE   Cus_Id= MCus_CODE  AND MONTH_CODE=11

			                                    --DEC
			                                    Update  #SALES_MONTH_DETAILS SET Dece = ISNULL(SO_AMT,0)
			                                    FROM #SALES_DETAILS WHERE    Cus_Id= MCus_CODE  AND MONTH_CODE=12
     
                                                          
		                                    SELECT Cus_Id,Cusname_Name CustomerName ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Jan ,Feb ,Mar 
		                                    FROM  #SALES_MONTH_DETAILS ";

                return connection.Query<SalesRegister>(qry, new { OrganizationId = OrganizationId, id = id, FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }
        public IEnumerable<GINRegister> GetGINRegisterDataDetailsPrint(string id, int OrganizationId,string partno)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @" select I.ItemRefNo,I.ItemName,ISNULL(PartNo,'-')PartNo,W.WorkShopRequestRefNo,WI.Quantity,SUM(ISNULL(SI.IssuedQuantity,0))ISSQTY,WI.Quantity-SUM(ISNULL(SI.IssuedQuantity,0))BALQTY,U.UnitName,
                                (select sum(Quantity) from StockUpdate S where S.ItemId=I.ItemId)STOCK
                                from WorkShopRequest W
		                        INNER JOIN WorkShopRequestItem WI  ON W.WorkShopRequestId=WI.WorkShopRequestId
		                        INNER JOIN Item I ON I.ItemId=WI.ItemId
		                        INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
		                        LEFT JOIN StoreIssueItem SI ON SI.WorkShopRequestItemId=WI.WorkShopRequestItemId
                                WHERE I.ItemName LIKE '%'+@id+'%' and W.OrganizationId=@OrganizationId	
                                and isnull(I.PartNo,'') like '%'+@partno+'%'
		                        group by I.ItemRefNo,I.ItemName,I.PartNo,WI.Quantity,U.UnitName,W.WorkShopRequestRefNo,I.ItemId";

                return connection.Query<GINRegister>(qry, new { OrganizationId = OrganizationId, id = id ,partno=partno}).ToList();
            }
        }
    }

}
