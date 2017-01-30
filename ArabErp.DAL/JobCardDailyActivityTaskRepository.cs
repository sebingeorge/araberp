using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardDailyActivityTaskRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertJobCardDailyActivityTask(JobCardDailyActivityTask objJobCardDailyActivityTask)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into JobCardDailyActivityTask (JobCardDailyActivityId,JobCardTaskId,TaskStartDate,TaskEndDate,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardDailyActivityId,@JobCardTaskId,@TaskStartDate,@TaskEndDate,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardDailyActivityTask).Single();
                return id;
            }
        }


        public JobCardDailyActivityTask GetJobCardDailyActivityTask(int JobCardDailyActivityTaskId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardDailyActivityTask
                        where JobCardDailyActivityTaskId=@JobCardDailyActivityTaskId";

                var objJobCardDailyActivityTask = connection.Query<JobCardDailyActivityTask>(sql, new
                {
                    JobCardDailyActivityTaskId = JobCardDailyActivityTaskId
                }).First<JobCardDailyActivityTask>();

                return objJobCardDailyActivityTask;
            }
        }

        /// <summary>
        /// Get JobCard Daily Activity Tasks by JobCardDailyActivityId (0 for all Tasks)
        /// </summary>
        /// <param name="JobCardDailyActivityId">0 for all tasks</param>
        /// <returns></returns>
        public List<JobCardDailyActivityTask> GetJobCardDailyActivityTasks(int JobCardDailyActivityId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                            T.JobCardDailyActivityId,T.EmployeeId,E.EmployeeName,JTA.JobCardTaskMasterId,
                                JTA.JobCardTaskName, T.StartTime, T.EndTime,T.ActualHours,
							    CONVERT(VARCHAR, GETDATE(), 106) TaskStartDate,
							    CONVERT(VARCHAR, GETDATE(), 106) TaskEndDate,T.OverTime
                                FROM JobCardDailyActivityTask T
                                INNER JOIN JobCardDailyActivity JDA ON JDA.JobCardDailyActivityId=T.JobCardDailyActivityId
                                INNER JOIN JobCardTask JT ON JT.JobCardTaskId=T.JobCardTaskId
                                INNER JOIN JobCardTaskMaster JTA ON JTA.JobCardTaskMasterId = JT.JobCardTaskMasterId
                                INNER JOIN Employee E ON E.EmployeeId=T.EmployeeId
                                WHERE T.JobCardDailyActivityId = @JobCardDailyActivityId
                                AND T.isActive = 1";

                var objJobCardDailyActivityTasks = connection.Query<JobCardDailyActivityTask>(sql, new { JobCardDailyActivityId = JobCardDailyActivityId }).ToList<JobCardDailyActivityTask>();

                return objJobCardDailyActivityTasks;
            }
        }

        public string DeleteJobCardDailyActivityTask(int JobCardDailyActivityId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"Delete JobCardDailyActivityTask  OUTPUT DELETED.JobCardDailyActivityId WHERE JobCardDailyActivityId=@JobCardDailyActivityId;";
                    string output = connection.Query<string>(query, new { JobCardDailyActivityId = JobCardDailyActivityId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }
        
    }
}
