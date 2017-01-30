using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ProfitabilityReportRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<ProfitabilityReport> GetProfitabilityReport()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ProfitabilityReport>("exec ProfitabilityReport");
            }
        }

        public IEnumerable<ProfitabilityReport> GetPurchaseDetails(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"SELECT SO.SaleOrderId,WRI.ItemId,I.ItemName,(SII.IssuedQuantity)Quantity,0 Rate,0 Amount INTO #TEMP
                                FROM WorkShopRequest WR
                                INNER JOIN SaleOrder SO ON WR.SaleOrderId = SO.SaleOrderId
                                INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                                INNER JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                                INNER JOIN Item I ON I.ItemId=WRI.ItemId
                                GROUP BY SO.SaleOrderId, WRI.ItemId,I.ItemName,SII.IssuedQuantity;
                                SELECT * into #A from 
                                (
                                SELECT MAX(GRNItemId)GRNItemId, ItemId, Rate FROM GRNItem GROUP BY ItemId, Rate
                                UNION
                                SELECT MAX(GRNItemId), ItemId, Rate FROM GRNItem WHERE GRNItemId NOT IN 
                                (SELECT MAX(GRNItemId) FROM GRNItem GROUP BY ItemId) GROUP BY ItemId, Rate
                                UNION
                                SELECT MAX(GRNItemId), ItemId, Rate FROM GRNItem WHERE GRNItemId NOT IN
                                (SELECT MAX(GRNItemId) FROM GRNItem GROUP BY ItemId
                                UNION
                                SELECT MAX(GRNItemId) FROM GRNItem WHERE GRNItemId NOT IN 
                                (SELECT MAX(GRNItemId) FROM GRNItem GROUP BY ItemId) GROUP BY ItemId) GROUP BY ItemId, Rate
                                )AS A;

                                with B as 
                                (
                                select  ItemId,(SUM(Rate)/count(ItemId))AverageRate from #A  group by Rate,ItemId  
                                )
                                update T SET T.Rate=B.AverageRate from B inner join #TEMP T ON T.ItemId=B.ItemId;
                                with C as 
                                (
                                select ItemId,Rate from StandardRate
                                )
                                update T SET T.Rate=C.Rate from C inner join #TEMP T ON T.ItemId=C.ItemId WHERE  T.Rate = 0

                                update #TEMP SET Amount=#TEMP.Quantity*#TEMP.Rate
                                select * from #TEMP where SaleOrderId=@id";

                return connection.Query<ProfitabilityReport>(sql, new { id = id }).ToList();
            }
        }
        public IEnumerable<ProfitabilityReport> GetExpenseDetails(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                 string sql = @"SELECT ExpenseNo,Convert (varchar(20),ExpenseDate,106)ExpenseDate,
                                S.SupplierName Supplier,TotalAmount Amount 
                                FROM ExpenseBill E
                                INNER JOIN Supplier S ON S.SupplierId=E.SupplierId
                                WHERE SaleOrderId=@id";

                return connection.Query<ProfitabilityReport>(sql, new { id = id }).ToList();
            }
        }
        public IEnumerable<ProfitabilityReport> GetLabourDetails(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                 string sql = @"SELECT EMP.EmployeeName,JCTM.JobCardTaskName,
                                JCT.ActualHours,JCTM.MinimumRate Rate,
                                CAST((ISNULL(JCTM.MinimumRate, 0) * ISNULL(JCT.ActualHours, 0)) AS DECIMAL(18,2)) Amount
                                FROM JobCard JC
                                INNER JOIN JobCardTask JCT ON JC.JobCardId = JCT.JobCardId
                                INNER JOIN JobCardTaskMaster JCTM ON JCT.JobCardTaskMasterId = JCTM.JobCardTaskMasterId
                                INNER JOIN Employee EMP ON JCT.EmployeeId = EMP.EmployeeId
                                WHERE  SaleOrderId=@id";

                return connection.Query<ProfitabilityReport>(sql, new { id = id }).ToList();
            }
        }
        public IEnumerable<ProfitabilityReport> GetProfitabilityReportDTPrint()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<ProfitabilityReport>("exec ProfitabilityReport");
            }
        }
    }
}
