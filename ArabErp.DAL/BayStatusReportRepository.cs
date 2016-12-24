using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class BayStatusReportRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<BayStatus> GetBayStatusReport(string type, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += @"with Job as (select distinct JobCardId, JobCardDate, JobCardNo, BayId, SaleOrderId,SaleOrderItemId, 
                ISNULL(JodCardCompleteStatus,0) Complete, WorkDescriptionId, InPassId, isService from JobCard where ISNULL(JodCardCompleteStatus,0) = 0) 
                select distinct B.BayId, B.BayName, Occupied = case when Job.Complete IS NULL then 'No' else 'Yes' end, Job.JobCardDate, 
                Job.JobCardNo, Job.JobCardId, I.RegistrationNo ChasisNoRegNo, V.VehicleModelName, U.ItemName UnitName,W.WorkDescr WorkDescription, SO.EDateDelivery, 
                C.CustomerName, SO.EDateArrival,
                CASE WHEN DA.JobCardId IS NULL THEN 0 ELSE 1 END [Status],
                STUFF((SELECT ', '+T2.JobCardTaskName /*+' ('+CAST(T1.Hours AS VARCHAR)+' hrs)'*/ FROM JobCardTask T1 
                INNER JOIN JobCardTaskMaster T2 ON T1.JobCardTaskMasterId = T2.JobCardTaskMasterId
                WHERE T1.JobCardId = Job.JobCardId FOR XML PATH('')), 1, 2, '') TaskNames
                --STUFF((SELECT ', '+T4.JobCardTaskName+' ('+CAST(SUM(T2.ActualHours) AS VARCHAR)+' hrs)' FROM JobCardDailyActivity T1
                --INNER JOIN JobCardDailyActivityTask T2 ON T1.JobCardDailyActivityId = T2.JobCardDailyActivityId
                --INNER JOIN JobCardTask T3 ON T2.JobCardTaskId = T3.JobCardTaskId
                --INNER JOIN JobCardTaskMaster T4 ON T3.JobCardTaskMasterId = T4.JobCardTaskMasterId
                --WHERE T1.JobCardId = Job.JobCardId 
                --GROUP BY T1.JobCardId, T2.JobCardTaskId, T4.JobCardTaskName FOR XML PATH('')), 1, 2, '') TaskProgress
                from Bay B left join Job on B.BayId = Job.BayId 
                left join JobCardTask JT on JT.JobCardId = Job.JobCardId 
                left join SaleOrderItem SI on SI.SaleOrderItemId = Job.SaleOrderItemId 
                left join SaleOrder SO on SO.SaleOrderId = SI.SaleOrderId 
                left join VehicleModel V on V.VehicleModelId = SI.VehicleModelId 
                left join Customer C on C.CustomerId = SO.CustomerId
                left join VehicleInPass I on I.VehicleInPassId = Job.InPassId
                left join WorkDescription W on W.WorkDescriptionId = Job.WorkDescriptionId
                left join Item U on U.ItemId = W.FreezerUnitId 
                LEFT JOIN JobCardDailyActivity DA ON Job.JobCardId = DA.JobCardId
                WHERE ISNULL(Job.isService, 0) = CASE @type WHEN 'service' THEN 1 WHEN 'new' THEN 0 WHEN 'all' THEN ISNULL(Job.isService, 0) END
                AND B.OrganizationId = @org
                ORDER BY B.BayName, Occupied";

                return connection.Query<BayStatus>(sql, new { type = type.ToLower(), @org = OrganizationId });
            }
        }

        public List<JobCardDailyActivityTask> GetJobCardDetails(int? jobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = string.Empty;
                    sql += @"SELECT T4.JobCardTaskName, T5.EmployeeName, CONVERT(VARCHAR, T3.TaskDate, 106) TaskStartDate, 
                            T3.Hours, SUM(T2.ActualHours) ActualHours FROM JobCardDailyActivity T1
                            INNER JOIN JobCardDailyActivityTask T2 ON T1.JobCardDailyActivityId = T2.JobCardDailyActivityId
                            INNER JOIN JobCardTask T3 ON T2.JobCardTaskId = T3.JobCardTaskId
                            INNER JOIN JobCardTaskMaster T4 ON T3.JobCardTaskMasterId = T4.JobCardTaskMasterId
                            INNER JOIN Employee T5 ON T3.EmployeeId = T5.EmployeeId
                            WHERE T1.JobCardId = ISNULL(@id, 0)
                            GROUP BY T1.JobCardId, T2.JobCardTaskId, T4.JobCardTaskName, T5.EmployeeName, T3.Hours, T3.TaskDate
                            ORDER BY T3.TaskDate, T5.EmployeeName";
                    return connection.Query<JobCardDailyActivityTask>(sql, new { @id = jobCardId }).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
