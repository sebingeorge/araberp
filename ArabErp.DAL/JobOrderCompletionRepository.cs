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
    public class JobOrderCompletionRepository:BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public IEnumerable<JobOrderPending> GetPendingJobOrder(int? isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if ((isProjectBased ?? 0) == 0)
                {
                    query += " select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, V.VehicleModelName";
                    query += " from JobCard J";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderItemId = J.SaleOrderItemId";
                    query += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId ";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " inner join VehicleModel V on V.VehicleModelId = W.VehicleModelId ";
                    query += " where ISNULL(J.JodCardCompleteStatus,0) <> 1 and J.isProjectBased = 0";
                }
                else
                {
                    query += " select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerName, '' VehicleModelName";
                    query += " from JobCard J";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderItemId = J.SaleOrderItemId";
                    query += " inner join SaleOrder S on S.SaleOrderId = SI.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId ";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " where ISNULL(J.JodCardCompleteStatus,0) <> 1 and J.isProjectBased = 1";
                }
                
                
                return connection.Query<JobOrderPending>(query);
            }
        }

        public JobCardCompletion GetJobCardCompletion(int JobCardId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = string.Empty;
                if (isProjectBased == 0)
                {
                    query = "select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,";
                    query += " V.VehicleModelId, V.VehicleModelName, W.WorkDescr, W.WorkDescriptionId, J.SpecialRemarks";
                    query += " from JobCard J inner join SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " inner join VehicleModel V on V.VehicleModelId = W.VehicleModelId";
                    query += " where J.JobCardId = " + JobCardId.ToString() + "  and J.isProjectBased = 0";
                }
                else
                {
                    query = "select distinct J.JobCardId, J.JobCardNo, J.JobCardDate, C.CustomerId, C.CustomerName,";
                    query += " 0 VehicleModelId, '' VehicleModelName, W.WorkDescr, W.WorkDescriptionId, J.SpecialRemarks";
                    query += " from JobCard J inner join SaleOrder S on S.SaleOrderId = J.SaleOrderId";
                    query += " inner join SaleOrderItem SI on SI.SaleOrderId = S.SaleOrderId";
                    query += " inner join Customer C on S.CustomerId = C.CustomerId";
                    query += " inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId";
                    query += " where J.JobCardId = " + JobCardId.ToString() + " and J.isProjectBased = 1";
                }
                
                var jobcard = connection.Query<JobCardCompletion>(query).FirstOrDefault();

                //query = string.Empty;
                //query = "select J.SlNo, JT.JobCardTaskMasterId, JT.JobCardTaskName, E.EmployeeId, E.EmployeeName, J.TaskDate, 0 ActualHours, 0 Existing";
                //query += " from JobCardTask J inner join JobCardTaskMaster JT on J.JobCardTaskMasterId = JT.JobCardTaskMasterId";
                //query += " inner join Employee E on E.EmployeeId = J.EmployeeId";
                //query += " where J.JobCardId = " + JobCardId.ToString();

                query = @"SELECT 
	                        T1.EmployeeId,
	                        SUM(T1.ActualHours) TotalHours
                        INTO #TOTAL
                        FROM JobCardDailyActivityTask T1
	                        INNER JOIN JobCardDailyActivity T2 ON T1.JobCardDailyActivityId = T2.JobCardDailyActivityId
                        WHERE T2.JobCardId = @JobCardId
                        GROUP BY T1.EmployeeId

                        SELECT DISTINCT
	                        M.JobCardTaskName,
							JT.SlNo,
	                        EMP.EmployeeName,
	                        DAT.JobCardTaskId,
	                        ISNULL(T.TotalHours, 0) ActualHours,
							JT.Hours,
	                        (SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity WHERE JobCardId = @JobCardId ORDER BY JobCardDailyActivityDate) StartDate,
	                        (SELECT TOP 1 CONVERT(VARCHAR, JobCardDailyActivityDate, 106) FROM JobCardDailyActivity WHERE JobCardId = @JobCardId ORDER BY JobCardDailyActivityDate DESC) EndDate
                        FROM JobCardTask JT
	                        INNER JOIN JobCardTaskMaster M ON JT.JobCardTaskMasterId = M.JobCardTaskMasterId
	                        LEFT JOIN JobCardDailyActivity DA ON JT.JobCardId = DA.JobCardId
	                        LEFT JOIN JobCardDailyActivityTask DAT ON DA.JobCardDailyActivityId = DAT.JobCardDailyActivityId AND M.JobCardTaskMasterId = DAT.JobCardTaskId
	                        LEFT JOIN Employee EMP ON JT.EmployeeId = EMP.EmployeeId
	                        LEFT JOIN #TOTAL T ON EMP.EmployeeId = T.EmployeeId
                        WHERE JT.JobCardId = @JobCardId;

                        DROP TABLE #TOTAL;";

                jobcard.JobCardTask = connection.Query<JobCardCompletionTask>(query, new { JobCardId = JobCardId }).ToList();

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
                    string query = "update JobCard set JodCardCompleteStatus = 1, JodCardCompletedDate='" + jobcard.JobCardCompletedDate.ToString("dd-MMM-yyyy") + "', WarrentyPeriod = '" + jobcard.WarrentyPeriod.ToString("dd/MMM/yyyy") + "' where jobCardId=" + jobcard.JobCardId.ToString();
                    var count = connection.Query(query, transaction: txn);
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
                            query = "update JobCardTask set ActualHours = " + item.ActualHours.ToString() + ",StartTime = " + item.StartTime.ToString() + ",EndTime = " + item.EndTime.ToString() + "  where SlNo = " + item.SlNo.ToString() + " and JobCardId = " + jobcard.JobCardId.ToString() + ";";
                            connection.Query(query, transaction: txn);
                        }
                    }
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
