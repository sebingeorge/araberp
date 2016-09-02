using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardDailyActivityRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertJobCardDailyActivity(JobCardDailyActivity objJobCardDailyActivity)
        {
            JobCardDailyActivityTaskRepository task = new JobCardDailyActivityTaskRepository();
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                int id = 0;
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objJobCardDailyActivity.OrganizationId, 27, true,trn);
                    objJobCardDailyActivity.JobCardDailyActivityRefNo = internalId.ToString();
                    string sql = @"insert  into JobCardDailyActivity (JobCardDailyActivityDate,JobCardId,JobCardDailyActivityRefNo,Remarks,EmployeeId,CreatedBy,CreatedDate,OrganizationId) 
                                                            Values (@JobCardDailyActivityDate,@JobCardId,@JobCardDailyActivityRefNo,@Remarks,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId);
                                SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, objJobCardDailyActivity, trn).Single();

                    foreach (var item in objJobCardDailyActivity.JobCardDailyActivityTask)
                    {
                        item.JobCardDailyActivityId = id;
                        item.CreatedDate = DateTime.Now;
                        sql = @"insert  into JobCardDailyActivityTask (JobCardDailyActivityId,JobCardTaskId,TaskStartDate,TaskEndDate,ActualHours,CreatedBy,CreatedDate,OrganizationId, EmployeeId, StartTime, EndTime) Values 
                                                                      (@JobCardDailyActivityId,@JobCardTaskMasterId,@TaskStartDate,@TaskEndDate,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId, @EmployeeId, @StartTime, @EndTime);
                        SELECT CAST(SCOPE_IDENTITY() as int)";


                        var taskid = connection.Query<int>(sql, item, trn).Single();
                    }
                    InsertLoginHistory(dataConnection, objJobCardDailyActivity.CreatedBy, "Create", "Job Card", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    throw ex;
                }
                return id;
            }
        }


        public JobCardDailyActivity GetJobCardDailyActivity(int JobCardDailyActivityId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                JobCardDailyActivityId,
	                                JobCardDailyActivityRefNo,
	                                CONVERT(VARCHAR, JobCardDailyActivityDate, 106) JobCardDailyActivityDate,
	                                EMP.EmployeeName,
	                                JC.JobCardNo,
	                                SpecialRemarks Remarks,
	                                Image1,Image2,Image3,Image4
                                FROM JobCardDailyActivity DA
                                INNER JOIN JobCard JC ON DA.JobCardId = JC.JobCardId
                                INNER JOIN Employee EMP ON JC.EmployeeId = EMP.EmployeeId
                                WHERE DA.JobCardDailyActivityId = @JobCardDailyActivityId
                                AND DA.isActive = 1";

                var objJobCardDailyActivity = connection.Query<JobCardDailyActivity>(sql, new
                {
                    JobCardDailyActivityId = JobCardDailyActivityId
                }).First<JobCardDailyActivity>();

                objJobCardDailyActivity.JobCardDailyActivityTask = new JobCardDailyActivityTaskRepository().GetJobCardDailyActivityTasks(JobCardDailyActivityId);

                return objJobCardDailyActivity;
            }
        }

        /// <summary>
        /// Get all items in Daily Activity
        /// </summary>
        /// <returns></returns>
        public List<JobCardDailyActivity> GetJobCardDailyActivitys()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                JobCardDailyActivityId,
	                                SUM(ActualHours) ActualHours,
	                                STUFF((SELECT ', ' + CAST(M.JobCardTaskName AS VARCHAR(MAX)) [text()]
	                                FROM JobCardDailyActivityTask T inner join JobCardTaskMaster M on T.JobCardTaskId = M.JobCardTaskMasterId
	                                WHERE T.JobCardDailyActivityId = TASK.JobCardDailyActivityId
	                                FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') Tasks
	                                INTO #TASKS
                                FROM JobCardDailyActivityTask TASK
                                WHERE isActive = 1
                                GROUP BY JobCardDailyActivityId

                                SELECT
	                                DA.JobCardDailyActivityId,
	                                DA.JobCardDailyActivityRefNo,
	                                CONVERT(VARCHAR, DA.JobCardDailyActivityDate, 106) JobCardDailyActivityDate,
	                                Remarks,
	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                JC.JobCardNo,
	                                EMP.EmployeeName,
	                                T.ActualHours,
	                                T.Tasks
                                FROM JobCardDailyActivity DA
                                INNER JOIN JobCard JC ON DA.JobCardId = JC.JobCardId
                                INNER JOIN Employee EMP ON DA.EmployeeId = EMP.EmployeeId
                                INNER JOIN #TASKS T ON DA.JobCardDailyActivityId = T.JobCardDailyActivityId
                                WHERE DA.isActive = 1
                                ORDER BY DA.JobCardDailyActivityDate DESC, DA.CreatedDate DESC;

                                DROP TABLE #TASKS;";

                var objJobCardDailyActivitys = connection.Query<JobCardDailyActivity>(sql).ToList<JobCardDailyActivity>();

                return objJobCardDailyActivitys;
            }
        }



        public int DeleteJobCardDailyActivity(Unit objJobCardDailyActivity)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardDailyActivity  OUTPUT DELETED.JobCardDailyActivityId WHERE JobCardDailyActivityId=@JobCardDailyActivityId";


                var id = connection.Execute(sql, objJobCardDailyActivity);
                return id;
            }
        }

        public IEnumerable<JobCardForDailyActivity> PendingJobcardTasks(int type, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select J.JobCardId,J.JobCardNo, J.JobCardDate, E.EmployeeName,W.WorkDescr,J.RequiredDate
                            from JobCard J
                            inner join Employee E on E.EmployeeId = J.EmployeeId
                            inner join WorkDescription W on W.WorkDescriptionId = J.WorkDescriptionId
                            where J.JodCardCompleteStatus is null
							AND J.OrganizationId = @OrganizationId
							AND	J.isProjectBased = @type
							AND J.isActive = 1";
                return connection.Query<JobCardForDailyActivity>(sql, new { OrganizationId = OrganizationId, type = type });
            }
        }

        /// <summary>
        /// Update the image name in table, according to the given index (Image1, Image2, Image3, Image4)
        /// </summary>
        /// <param name="fileName">Name of file with extension</param>
        /// <param name="id">JobCardDailyActivityId</param>
        /// <param name="index">Index of column</param>
        public void UpdateImageName(string fileName, int? id, int? index)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"UPDATE JobCardDailyActivity SET Image" + index + " = @fileName WHERE JobCardDailyActivityId = @id";
                connection.Execute(query, new { fileName = fileName, id = id });
            }
        }

        public List<JobCardDailyActivityTask> GetJobCardTasksForDailyActivity(int Id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                T.EmployeeId,
	                                E.EmployeeName,
	                                T.JobCardTaskMasterId,
	                                M.JobCardTaskName,
									CONVERT(VARCHAR, GETDATE(), 106) TaskStartDate,
									CONVERT(VARCHAR, GETDATE(), 106) TaskEndDate
                                FROM JobCardTask T
									INNER JOIN JobCard J ON T.JobCardId = J.JobCardId
	                                INNER JOIN Employee E ON T.EmployeeId = E.EmployeeId
	                                INNER JOIN JobCardTaskMaster M ON T.JobCardTaskMasterId = M.JobCardTaskMasterId
                                WHERE T.isActive = 1 AND T.JobCardId = @id AND J.OrganizationId = @OrganizationId";
                return connection.Query<JobCardDailyActivityTask>(query, new { id = Id, OrganizationId = OrganizationId }).ToList();
            }
        }
    }
}