using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class PurchaseBillRegisterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<PurchaseBillRegister> GetPurchaseBillRegisterData(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"select PurchaseBillRefNo,PurchaseBillDate,PurchaseBillNoDate,S.SupplierName,ItemName,ItemRefNo,G.Quantity,PI.Rate,PI.Amount 
                               from PurchaseBill P,PurchaseBillItem PI,Item I,Supplier S,GRNItem G
                               where P.SupplierId=S.SupplierId AND P.PurchaseBillId=PI.PurchaseBillId AND PI.GRNItemId=G.GRNItemId AND G.ItemId =I.ItemId 
                               and P.isActive=1 AND P.PurchaseBillDate BETWEEN @from AND @to 
                               AND P.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) and S.SupplierId=ISNULL(NULLIF(@id, 0), S.SupplierId) 
                               ORDER BY PurchaseBillDate";



                return connection.Query<PurchaseBillRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }

        public IEnumerable<PurchaseBillRegister> PurchaseBillDetailedData(DateTime? from, DateTime? to, int id, int itmid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @" SELECT ItemName,IC.CategoryName,SUM(G.Quantity)Quantity,U.UnitName,SUM(PI.Amount)Amount
                                FROM PurchaseBill P
                                INNER JOIN PurchaseBillItem PI ON P.PurchaseBillId=PI.PurchaseBillId
                                INNER JOIN GRNItem G ON P.PurchaseBillId=PI.PurchaseBillId
                                INNER JOIN Item I ON G.ItemId =I.ItemId 
                                INNER JOIN ItemCategory IC ON IC.itmCatId=I.ItemCategoryId
                                INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                                WHERE P.isActive=1  AND P.PurchaseBillDate BETWEEN @from AND @to
                                AND P.OrganizationId=@OrganizationId AND  I.ItemId = ISNULL(NULLIF(@itmid, 0), I.ItemId) and IC.itmCatId=ISNULL(NULLIF(@id, 0), IC.itmCatId) 
                                GROUP BY  ItemName,IC.CategoryName,U.UnitName
                                ORDER BY ItemName";

                return connection.Query<PurchaseBillRegister>(qry, new { id = id, itmid = itmid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }

        public IEnumerable<PurchaseBillRegister> PurchaseBillSummaryData(DateTime? from, DateTime? to, int id, int supid, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //              
                string qry = @"SELECT convert(varchar,P.PurchaseBillDate,106)PurchaseBillDate,SUM(PI.Amount)Amount
                                FROM PurchaseBill P
                                INNER JOIN PurchaseBillItem PI ON P.PurchaseBillId=PI.PurchaseBillId
                                INNER JOIN GRNItem G ON P.PurchaseBillId=PI.PurchaseBillId
                                INNER JOIN Item I ON G.ItemId =I.ItemId 
                                INNER JOIN ItemCategory IC ON IC.itmCatId=I.ItemCategoryId
                                WHERE P.isActive=1  AND P.PurchaseBillDate BETWEEN @from AND @to
                                AND P.OrganizationId=@OrganizationId AND  P.SupplierId = ISNULL(NULLIF(@supid, 0), P.SupplierId) and IC.itmCatId=ISNULL(NULLIF(@id, 0), IC.itmCatId) 
                                GROUP BY  PurchaseBillDate
                                ORDER BY PurchaseBillDate";

                return connection.Query<PurchaseBillRegister>(qry, new { id = id, supid = supid, OrganizationId = OrganizationId, from = from, to = to }).ToList();
            }
        }

        public IEnumerable<PurchaseBillRegister> GetPurchaseMonthlyItemWiseData(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                      
                                        DECLARE @FIN_ID int ;
                                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;

                                      
			

                                        CREATE TABLE #PURCHASE_ITEMS_DETAILS
                                        (
                                        Itm_id int,
                                        Itm_Name NVARCHAR(150)
                                        )
				
				
                                        CREATE TABLE #PURCHASE_MONTH_DETAILS
                                        (Itm_id int,
                                        Itm_Name NVARCHAR(150),
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
                                        Dece DECIMAL(18,2),
                                        Total DECIMAL(18,2)
			                            )
               
                                        CREATE TABLE #PURCHASE_DETAILS
                                        (
                                        Itm_id INT,
                                        MONTH_id INTEGER,
                                        Itm_Name NVARCHAR(150),
                                        PB_AMT DECIMAL(18,2)
                                        )
                
                                        INSERT INTO #PURCHASE_DETAILS(MONTH_id,Itm_id,Itm_Name,PB_AMT)
                                        SELECT MONTH(PurchaseBillDate)AS MONTH_CODE,I.ItemId,I.ItemName,
                                        SUM(PI.Amount) as PB_AMT
                                        FROM PurchaseBillItem PI 
                                        INNER JOIN PurchaseBill P  ON P.PurchaseBillId  =PI.PurchaseBillId
                                        INNER JOIN GRNItem GI  ON GI.GRNItemId  =PI.GRNItemId
                                        INNER JOIN Item I ON I.ItemId  =GI.ItemId		
                                        INNER JOIN Supplier S ON S.SupplierId=P.SupplierId 
                                        WHERE PurchaseBillDate>=@FYStartdate  AND PurchaseBillDate <=@FYEnddate AND S.SupplierId=ISNULL(NULLIF(@id, 0),S.SupplierId) AND P.OrganizationId=@OrganizationId
                                        GROUP BY MONTH(PurchaseBillDate),YEAR(PurchaseBillDate),I.ItemId,I.ItemName
			
                                        INSERT INTO #PURCHASE_ITEMS_DETAILS(Itm_id ,Itm_Name )
                                        SELECT DISTINCT I.ItemId,I.ItemName
                                        FROM PurchaseBillItem PI 
                                        INNER JOIN PurchaseBill P  ON P.PurchaseBillId  =PI.PurchaseBillId
                                        INNER JOIN GRNItem GI  ON GI.GRNItemId  =PI.GRNItemId
                                        INNER JOIN Item I ON I.ItemId  =GI.ItemId
                                        INNER JOIN Supplier S ON S.SupplierId=P.SupplierId 	
                                        WHERE PurchaseBillDate>=@FYStartdate  AND PurchaseBillDate <=@FYEnddate AND S.SupplierId=ISNULL(NULLIF(@id, 0),S.SupplierId) AND P.OrganizationId=@OrganizationId		


                                        INSERT INTO #PURCHASE_MONTH_DETAILS(
                                        Itm_id ,Itm_Name ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Total,Jan ,Feb ,Mar )
                                        SELECT Itm_id ,Itm_Name ,0 AS Apr,0 AS May,0 AS Jun,0 AS Jul, 0 AS Aug,0 AS Sep ,0 AS Oct,0 AS Nov,
                                        0 AS Dece,0 AS Total,0 AS Jan,0 AS Feb,0 AS Mar FROM #PURCHASE_ITEMS_DETAILS 

                                        --JAN
                                        Update  #PURCHASE_MONTH_DETAILS SET Jan = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=1

                                        --FEB
                                        Update  #PURCHASE_MONTH_DETAILS SET Feb  = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=2

                                        --MAR
                                        Update  #PURCHASE_MONTH_DETAILS SET Mar  = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=3    

                                        --APR
                                        Update  #PURCHASE_MONTH_DETAILS SET Apr = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS  WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=4

                                        --MAY
                                        Update  #PURCHASE_MONTH_DETAILS SET May  = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE   #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=5

                                        --JUN
                                        Update  #PURCHASE_MONTH_DETAILS SET Jun = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=6

                                        --JUL
                                        Update  #PURCHASE_MONTH_DETAILS SET Jul = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=7

                                        --AUG
                                        Update  #PURCHASE_MONTH_DETAILS SET Aug = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=8

                                        --SEP
                                        Update  #PURCHASE_MONTH_DETAILS SET Sep = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=9

                                        --OCT
                                        Update  #PURCHASE_MONTH_DETAILS SET Oct = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=10

                                        --NOV
                                        Update  #PURCHASE_MONTH_DETAILS SET Nov = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=11

                                        --DEC
                                        Update  #PURCHASE_MONTH_DETAILS SET Dece = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE  #PURCHASE_MONTH_DETAILS.Itm_id= #PURCHASE_DETAILS.Itm_id AND MONTH_id=12

                                        --Total
                                        Update  #PURCHASE_MONTH_DETAILS SET Total = ISNULL(Jan,0)+ISNULL(Feb,0)+ISNULL(Mar,0)+ISNULL(Apr,0)+ISNULL(May,0)
                                        +ISNULL(Jun,0)+ISNULL(Jul,0)+ISNULL(Aug,0)+ISNULL(Sep,0)+ISNULL(Oct,0)+ISNULL(Nov,0)+ISNULL(Dece,0)
                                        FROM PURCHASE_DETAILS WHERE  #PURCHASE_MONTH_DETAILS.Itm_id= PURCHASE_DETAILS.Itm_id 
			                                                    
                                        SELECT Itm_id,Itm_Name ItemName,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Total,Jan ,Feb ,Mar 
                                        FROM  #PURCHASE_MONTH_DETAILS ORDER BY Itm_Name";

                return connection.Query<PurchaseBillRegister>(qry, new { OrganizationId = OrganizationId, id = id, FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }

        public IEnumerable<PurchaseBillRegister> GetPurchaseMonthlySupplieriseData(int OrganizationId, int id, DateTime FYStartdate, DateTime FYEnddate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string qry = @"	
                                       
                                        DECLARE @FIN_ID int ;
                                        SELECT @FIN_ID=FyId from Organization where OrganizationId=@OrganizationId;

                                     
			

                                        CREATE TABLE #PURCHASE_ITEMS_DETAILS
                                        (
                                        Sup_id int,
                                        Sup_Name NVARCHAR(150)
                                        )
				
				
                                        CREATE TABLE #PURCHASE_MONTH_DETAILS
                                        (Sup_id int,
                                        Sup_Name NVARCHAR(150),
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
                                        Dece DECIMAL(18,2),
                                        Total DECIMAL(18,2)
			                            )
               
                                        CREATE TABLE #PURCHASE_DETAILS
                                        (
                                        Sup_id INT,
                                        MONTH_id INTEGER,
                                        Sup_Name NVARCHAR(150),
                                        PB_AMT DECIMAL(18,2)
                                        )
                
                                        INSERT INTO #PURCHASE_DETAILS(MONTH_id,Sup_id,Sup_Name,PB_AMT)
                                        SELECT MONTH(PurchaseBillDate)AS MONTH_CODE,S.SupplierId,S.SupplierName,
                                        SUM(PI.Amount) as PB_AMT
                                        FROM PurchaseBillItem PI 
                                        INNER JOIN PurchaseBill P  ON P.PurchaseBillId  =PI.PurchaseBillId
                                        INNER JOIN GRNItem GI  ON GI.GRNItemId  =PI.GRNItemId
                                        INNER JOIN Item I ON I.ItemId  =GI.ItemId		
                                        INNER JOIN Supplier S ON S.SupplierId=P.SupplierId 
                                        WHERE PurchaseBillDate>=@FYStartdate  AND PurchaseBillDate <=@FYEnddate AND I.ItemId=ISNULL(NULLIF(@id, 0),I.ItemId) AND P.OrganizationId=@OrganizationId
                                        GROUP BY MONTH(PurchaseBillDate),YEAR(PurchaseBillDate),S.SupplierId,S.SupplierName
			
                                        INSERT INTO #PURCHASE_ITEMS_DETAILS(Sup_id ,Sup_Name )
                                        SELECT DISTINCT S.SupplierId,S.SupplierName
                                        FROM PurchaseBillItem PI 
                                        INNER JOIN PurchaseBill P  ON P.PurchaseBillId  =PI.PurchaseBillId
                                        INNER JOIN GRNItem GI  ON GI.GRNItemId  =PI.GRNItemId
                                        INNER JOIN Item I ON I.ItemId  =GI.ItemId
                                        INNER JOIN Supplier S ON S.SupplierId=P.SupplierId 	
                                        WHERE PurchaseBillDate>=@FYStartdate  AND PurchaseBillDate <=@FYEnddate AND I.ItemId=ISNULL(NULLIF(@id, 0),I.ItemId) AND P.OrganizationId=@OrganizationId		


                                        INSERT INTO #PURCHASE_MONTH_DETAILS(
                                        Sup_id ,Sup_Name ,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Total,Jan ,Feb ,Mar )
                                        SELECT Sup_id ,Sup_Name ,0 AS Apr,0 AS May,0 AS Jun,0 AS Jul, 0 AS Aug,0 AS Sep ,0 AS Oct,0 AS Nov,
                                        0 AS Dece,0 AS Total,0 AS Jan,0 AS Feb,0 AS Mar FROM #PURCHASE_ITEMS_DETAILS 

                                        --JAN
                                        Update  #PURCHASE_MONTH_DETAILS SET Jan = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=1

                                        --FEB
                                        Update  #PURCHASE_MONTH_DETAILS SET Feb  = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=2

                                        --MAR
                                        Update  #PURCHASE_MONTH_DETAILS SET Mar  = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=3    

                                        --APR
                                        Update  #PURCHASE_MONTH_DETAILS SET Apr = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS  WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=4

                                        --MAY
                                        Update  #PURCHASE_MONTH_DETAILS SET May  = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE   #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=5

                                        --JUN
                                        Update  #PURCHASE_MONTH_DETAILS SET Jun = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=6

                                        --JUL
                                        Update  #PURCHASE_MONTH_DETAILS SET Jul = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=7

                                        --AUG
                                        Update  #PURCHASE_MONTH_DETAILS SET Aug = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=8

                                        --SEP
                                        Update  #PURCHASE_MONTH_DETAILS SET Sep = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=9

                                        --OCT
                                        Update  #PURCHASE_MONTH_DETAILS SET Oct = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=10

                                        --NOV
                                        Update  #PURCHASE_MONTH_DETAILS SET Nov = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE    #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=11

                                        --DEC
                                        Update  #PURCHASE_MONTH_DETAILS SET Dece = ISNULL(PB_AMT,0)
                                        FROM #PURCHASE_DETAILS WHERE  #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_DETAILS.Sup_id AND MONTH_id=12

                                        --Total
                                        Update  #PURCHASE_MONTH_DETAILS SET Total = ISNULL(Jan,0)+ISNULL(Feb,0)+ISNULL(Mar,0)+ISNULL(Apr,0)+ISNULL(May,0)
                                        +ISNULL(Jun,0)+ISNULL(Jul,0)+ISNULL(Aug,0)+ISNULL(Sep,0)+ISNULL(Oct,0)+ISNULL(Nov,0)+ISNULL(Dece,0)
                                        FROM PURCHASE_DETAILS WHERE  #PURCHASE_MONTH_DETAILS.Sup_id= #PURCHASE_MONTH_DETAILS.Sup_id
			                                                    
                                        SELECT Sup_id,Sup_Name SupplierName,Apr ,May ,Jun ,Jul ,Aug ,Sep ,Oct ,Nov ,Dece,Total,Jan ,Feb ,Mar 
                                        FROM  #PURCHASE_MONTH_DETAILS ORDER BY Sup_Name";

                return connection.Query<PurchaseBillRegister>(qry, new { OrganizationId = OrganizationId, id = id, FYStartdate = FYStartdate, FYEnddate = FYEnddate }).ToList();
            }
        }
    }
}
