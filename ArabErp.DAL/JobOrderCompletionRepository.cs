using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using ArabErp.DAL;
using DapperExtensions;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class JobOrderCompletionRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<JobOrderPending> GetPendingJobOrder(int? isProjectBased, int OrganizationId, int id, int cusid, string RegNo = "")
     {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if ((isProjectBased ?? 0) == 0)
                {
                    query = @"select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, V.VehicleModelName,isnull(RegistrationNo,'')RegistrationNo,isnull(ChassisNo,'')ChassisNo, J.isOnHold
                     from JobCard J
                    inner join SaleOrderItem SI on SI.SaleOrderItemId = J.SaleOrderItemId
                    inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId
                    inner join Customer C on S.CustomerId = C.CustomerId 
                    inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId
                    inner join VehicleInPass VI ON VI.VehicleInPassId=J.InPassId
                    left join VehicleModel V on V.VehicleModelId = W.VehicleModelId 
                    where (ISNULL(J.JodCardCompleteStatus,0) <> 1 OR ISNULL(J.isOnHold, 0) = 1) and J.isProjectBased = 0 AND J.OrganizationId = @OrganizationId
                    and  J.JobCardId = ISNULL(NULLIF(@id, 0), J.JobCardId) and S.CustomerId = ISNULL(NULLIF(@cusid, 0), S.CustomerId)
                    AND (ISNULL(RegistrationNo, '') LIKE '%'+@RegNo+'%' OR ISNULL(ChassisNo, '') LIKE '%'+@RegNo+'%')";
                }
                else
                {
                    query = @"select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, '' VehicleModelName,isnull(RegistrationNo,'')RegistrationNo,isnull(ChassisNo,'')ChassisNo, J.isOnHold
                    from JobCard J
                    inner join SaleOrderItem SI on SI.SaleOrderItemId = J.SaleOrderItemId
                    inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId
                    inner join Customer C on S.CustomerId = C.CustomerId 
                    LEFT join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId
                    LEFT join VehicleInPass VI ON VI.VehicleInPassId=J.InPassId
                    where ISNULL(J.JodCardCompleteStatus,0) <> 1 and J.isProjectBased = 1 AND J.OrganizationId = @OrganizationId
                    and J.JobCardId = ISNULL(NULLIF(@id, 0), J.JobCardId) and S.CustomerId = ISNULL(NULLIF(@cusid, 0), S.CustomerId)";
                }

                return connection.Query<JobOrderPending>(query, new { isProjectBased = isProjectBased, OrganizationId = OrganizationId, id = id, cusid = cusid, RegNo = RegNo });
            }
        }

        public JobCardCompletion GetJobCardCompletion(int JobCardId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                string sql = string.Empty;
                if (isProjectBased == 0)
                {
                    query = @"SELECT distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,V.VehicleModelId, V.VehicleModelName,
                            W.WorkDescr, W.WorkDescriptionId, J.SpecialRemarks,(ISNULL(SS.WorkShopRequestId,0))StoreIssued
                            FROM JobCard J INNER JOIN SaleOrder S on S.SaleOrderId = J.SaleOrderId
                            INNER JOIN SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId
                            INNER JOIN Customer C on S.CustomerId = C.CustomerId
                            INNER JOIN WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId
                            LEFT JOIN VehicleModel V on V.VehicleModelId = W.VehicleModelId
                            LEFT JOIN WorkShopRequest WR ON WR.SaleOrderId=J.SaleOrderId
                            LEFT JOIN StoreIssue SS ON SS.WorkShopRequestId=WR.WorkShopRequestId
                            where J.JobCardId = " + JobCardId.ToString() + "  and J.isProjectBased = 0";
                }
                else
                {
                    query = "SELECT distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,0 VehicleModelId, '' VehicleModelName,";
                    query += " WorkDescr=(case when J.isService=0 then STUFF((SELECT ', '+T2.ItemName + ', '+ T3.ItemName FROM SaleOrderItemUnit T1";
                    query += " LEFT JOIN Item T2 ON T1.CondenserUnitId = T2.ItemId";
                    query += " LEFT JOIN Item T3 ON T1.EvaporatorUnitId = T3.ItemId";
                    query += " WHERE T1.SaleOrderItemId = SI.SaleOrderItemId FOR XML PATH('')), 1, 2, '')else W.WorkDescr end), W.WorkDescriptionId, J.SpecialRemarks, (ISNULL(SS.WorkShopRequestId,0))StoreIssued,(ISNULL(SS.WorkShopRequestId,0))StoreIssued";
                    query += " FROM JobCard J INNER JOIN SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                    query += " INNER JOIN SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                    query += " INNER JOIN Customer C on S.CustomerId = C.CustomerId";
                    query += " LEFT JOIN WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " LEFT JOIN WorkShopRequest WR ON WR.SaleOrderId=J.SaleOrderId";
                    query += " LEFT JOIN StoreIssue SS ON SS.WorkShopRequestId=WR.WorkShopRequestId";
                    query += " where J.JobCardId = " + JobCardId.ToString() + " and J.isProjectBased = 1";
                }

                var jobcard = connection.Query<JobCardCompletion>(query).FirstOrDefault();

                //query = string.Empty;
                //query = "select J.SlNo, JT.JobCardTaskMasterId, JT.JobCardTaskName, E.EmployeeId, E.EmployeeName, J.TaskDate, 0 ActualHours, 0 Existing";
                //query += " from JobCardTask J inner join JobCardTaskMaster JT on J.JobCardTaskMasterId = JT.JobCardTaskMasterId";
                //query += " inner join Employee E on E.EmployeeId = J.EmployeeId";
                //query += " where J.JobCardId = " + JobCardId.ToString();

                #region old query 19.12.2016 3.33p
                //                query = @"SELECT 
                //	                        T1.EmployeeId,
                //							T1.JobCardTaskId,
                //	                        SUM(T1.ActualHours) TotalHours
                //                            INTO #TOTAL
                //                            FROM JobCardDailyActivityTask T1
                //	                        INNER JOIN JobCardDailyActivity T2 ON T1.JobCardDailyActivityId = T2.JobCardDailyActivityId
                //                            WHERE T2.JobCardId = @JobCardId
                //                            GROUP BY T1.EmployeeId, T1.JobCardTaskId
                //
                //                        SELECT DISTINCT
                //	                        M.JobCardTaskName,
                //							JT.SlNo,
                //	                        EMP.EmployeeName,
                //	                        DAT.JobCardTaskId,
                //	                        ISNULL(T.TotalHours, 0) ActualHours,
                //							JT.Hours,
                //	                        (SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity WHERE JobCardId = @JobCardId 
                //                            ORDER BY JobCardDailyActivityDate) StartDate,
                //	                        (SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity WHERE JobCardId = @JobCardId 
                //                            ORDER BY JobCardDailyActivityDate DESC) EndDate
                //                            FROM JobCardTask JT
                //	                        INNER JOIN JobCardTaskMaster M ON JT.JobCardTaskMasterId = M.JobCardTaskMasterId
                //	                        LEFT JOIN JobCardDailyActivity DA ON JT.JobCardId = DA.JobCardId
                //	                        LEFT JOIN JobCardDailyActivityTask DAT ON DA.JobCardDailyActivityId = DAT.JobCardDailyActivityId AND M.JobCardTaskMasterId = DAT.JobCardTaskId
                //	                        LEFT JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
                //	                        LEFT JOIN #TOTAL T ON EMP.EmployeeId = T.EmployeeId AND DAT.JobCardTaskId = T.JobCardTaskId
                //                            WHERE JT.JobCardId = @JobCardId;
                //
                //                            DROP TABLE #TOTAL;"; 
                #endregion

                query = @"SELECT 
	                        T1.EmployeeId,
							T1.JobCardTaskId,
	                        SUM(T1.ActualHours) TotalHours
                            INTO #TOTAL
                            FROM JobCardDailyActivityTask T1
	                        INNER JOIN JobCardDailyActivity T2 ON T1.JobCardDailyActivityId = T2.JobCardDailyActivityId
                            WHERE T2.JobCardId = @JobCardId
                            GROUP BY T1.EmployeeId, T1.JobCardTaskId

                        SELECT DISTINCT
	                        M.JobCardTaskName,
							JT.SlNo,
	                        EMP.EmployeeName,
	                        DAT.JobCardTaskId,
	                        ISNULL(T.TotalHours, 0) ActualHours,
							JT.Hours,
	                        (SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity WHERE JobCardId = @JobCardId 
                            ORDER BY JobCardDailyActivityDate) StartDate,
	                        (SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity WHERE JobCardId = @JobCardId 
                            ORDER BY JobCardDailyActivityDate DESC) EndDate
                            FROM JobCardTask JT
	                        INNER JOIN JobCardTaskMaster M ON JT.JobCardTaskMasterId = M.JobCardTaskMasterId
	                        LEFT JOIN JobCardDailyActivity DA ON JT.JobCardId = DA.JobCardId
	                        LEFT JOIN JobCardDailyActivityTask DAT ON DA.JobCardDailyActivityId = DAT.JobCardDailyActivityId AND JT.JobCardTaskId = DAT.JobCardTaskId
	                        LEFT JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
	                        LEFT JOIN #TOTAL T ON EMP.EmployeeId = T.EmployeeId AND DAT.JobCardTaskId = T.JobCardTaskId
                            WHERE JT.JobCardId = @JobCardId;

                            DROP TABLE #TOTAL;";
                jobcard.JobCardTask = connection.Query<JobCardCompletionTask>(query, new { JobCardId = JobCardId }).ToList();

                #region old query 29.12.2016 2.45p
                //                sql = @"select COUNT(WI.WorkShopRequestItemId) StoreIssued
                //                        from jobcard J
                //                        Left join WorkShopRequest W ON W.JobCardId=J.JobCardId OR j.SaleOrderId=W.SaleOrderId
                //                        left join WorkShopRequestItem WI ON WI.WorkShopRequestId=W.WorkShopRequestId
                //                        left join Item I  ON I.ItemId=WI.ItemId
                //                        LEFT JOIN StoreIssueItem SII ON WI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                //                        where ISNULL(I.isConsumable,0)=0 and J.jobcardid=@JobCardId AND SII.WorkShopRequestItemId IS  NULL"; 
                #endregion
                sql = @"SELECT
	                        WRI.ItemId,
	                        SUM(WRI.Quantity) Quantity
                        INTO #WORK
                        FROM WorkShopRequest WR
                        INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                        INNER JOIN Item I ON WRI.ItemId = I.ItemId
                        WHERE ISNULL(I.isConsumable, 0) = 0 AND (WR.JobCardId = @JobCardId OR 
                        WR.SaleOrderItemId = (SELECT SaleOrderItemId FROM JobCard WHERE JobCardId = @JobCardId)
                        OR (WR.SaleOrderItemId = 0 AND ISNULL(WR.JobCardId, 0) = 0 AND WR.SaleOrderId = (SELECT SaleOrderId FROM JobCard WHERE JobCardId = @JobCardId)))
                        GROUP BY WRI.ItemId

                        SELECT
	                        WRI.ItemId,
	                        SUM(SII.IssuedQuantity) Quantity
                        INTO #ISSUE
                        FROM WorkShopRequest WR
                        INNER JOIN WorkShopRequestItem WRI ON WR.WorkShopRequestId = WRI.WorkShopRequestId
                        INNER JOIN Item I ON WRI.ItemId = I.ItemId
                        LEFT JOIN StoreIssueItem SII ON WRI.WorkShopRequestItemId = SII.WorkShopRequestItemId
                        WHERE ISNULL(I.isConsumable, 0) = 0 AND (WR.JobCardId = @JobCardId OR 
                        WR.SaleOrderItemId = (SELECT SaleOrderItemId FROM JobCard WHERE JobCardId = @JobCardId)
                        OR (WR.SaleOrderItemId = 0 AND ISNULL(WR.JobCardId, 0) = 0 AND WR.SaleOrderId = (SELECT SaleOrderId FROM JobCard WHERE JobCardId = @JobCardId)))
                        GROUP BY WRI.ItemId

                        SELECT
	                        COUNT(#WORK.ItemId)
                        FROM #WORK
	                        LEFT JOIN #ISSUE ON #WORK.ItemId = #ISSUE.ItemId
                        WHERE #WORK.Quantity > ISNULL(#ISSUE.Quantity, 0)

                        DROP TABLE #ISSUE;
                        DROP TABLE #WORK;";
                int val = connection.Query<int>(sql, new { JobCardId = JobCardId }).First();
                jobcard.StoreIssued = val == 0 ? true : false;
                //jobcard.JobCardTask = new List<JobCardCompletionTask>();

                //foreach (JobCardCompletionTask item in tasks)
                //{
                //    jobcard.JobCardTask.Add(item);
                //}
                return jobcard;
            }
        }

        public int UpdateJobCardCompletion(JobCardCompletion jobcard, string CreatedBy)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = "update JobCard set JodCardCompleteStatus = 1, JodCardCompletedDate='" +
                        jobcard.JobCardCompletedDate.ToString("dd-MMM-yyyy") +
                        "', WarrentyPeriod = '" + jobcard.WarrentyPeriod.ToString("dd/MMM/yyyy") + "'" +
                        (jobcard.isOnHold ? ", isOnHold = 1, HeldBy = " + CreatedBy + ", HeldOn = GETDATE()" : "") +
                        " where jobCardId=" + jobcard.JobCardId.ToString();
                    var count = connection.Query(query, transaction: txn);
                    if (jobcard.isOnHold) goto commit;
                    //query = @"UPDATE JobCardQC JQ SET JQ.IsQCPassed =1 inner join JobCard J on J.JobCardId=JQ.JobCardId   WHERE J.isService=1 AND JQ.JobCardId = " + jobcard.JobCardId + "";
                    //connection.Execute(sql: query, transaction: txn);
                    //                    query = @"UPDATE SaleOrderItem SET IsPaymentApprovedForDelivery = (SELECT isService FROM JobCard WHERE JobCardId = " + jobcard.JobCardId + @")
                    //                                WHERE SaleOrderItemId = (SELECT SaleOrderItemId FROM JobCard WHERE JobCardId = " + jobcard.JobCardId + ")";
                    //                    connection.Execute(sql: query, transaction: txn);
                    int i = 0;
                    foreach (var item in jobcard.JobCardTask)
                    {
                        if (item.Existing == 1)
                        {
                            query = string.Empty;
                            query = @"insert  into JobCardTask(JobCardId,JobCardTaskMasterId,SlNo,EmployeeId,TaskDate,StartTime,EndTime,
                                      Hours,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values 
                            (" + jobcard.JobCardId.ToString() + "," + item.JobCardTaskMasterId + "," + i.ToString() + "," + item.EmployeeId.ToString() + "," + item.TaskDate.ToString("dd/MMM/yyyy") + "," + item.StartTime + "," + item.EndTime + "," + item.ActualHours + "," + item.ActualHours + ",NULL,GETDATE(),NULL); SELECT CAST(SCOPE_IDENTITY() as int)";
                            connection.Query(query, transaction: txn);
                            i++;
                        }
                        else
                        {
                            query = string.Empty;
                            //query = "update JobCardTask set ActualHours = " + item.ActualHours.ToString() + " where JobCardId = " + jobcard.JobCardId.ToString() + ";";
                            //query = "update JobCardTask set ActualHours = " + item.ActualHours.ToString() + ",StartTime = " + item.StartTime.ToString() + ",EndTime = " + item.EndTime.ToString() + "  where SlNo = " + item.SlNo.ToString() + " and JobCardId = " + jobcard.JobCardId.ToString() + ";";
                            query = "update JobCardTask set ActualHours = " + item.ActualHours.ToString() + " where SlNo = " + item.SlNo.ToString() + " and JobCardId = " + jobcard.JobCardId.ToString() + ";";
                            connection.Query(query, transaction: txn);
                        }
                    }
                commit:
                    InsertLoginHistory(dataConnection, CreatedBy, "Update", "Job Card Completion", id.ToString(), "0");
                    txn.Commit();
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
            return id;
        }
    }
}
