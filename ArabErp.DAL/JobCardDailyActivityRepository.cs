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
                string internalId = "";
                try
                {

                    if (objJobCardDailyActivity.isProjectBased == 1)
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objJobCardDailyActivity.OrganizationId, 38, true, trn);
                    }
                    else
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objJobCardDailyActivity.OrganizationId, 27, true, trn);
                    }

                    objJobCardDailyActivity.JobCardDailyActivityRefNo = internalId.ToString();
                    string sql = @"insert  into JobCardDailyActivity (JobCardDailyActivityDate,JobCardId,JobCardDailyActivityRefNo,Remarks,EmployeeId,CreatedBy,CreatedDate,OrganizationId) 
                                                            Values (@JobCardDailyActivityDate,@JobCardId,@JobCardDailyActivityRefNo,@Remarks,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId);
                                SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, objJobCardDailyActivity, trn).Single();

                    foreach (var item in objJobCardDailyActivity.JobCardDailyActivityTask)
                    {
                        if (item.ActualHours == null || item.ActualHours == 0) continue;
                        item.JobCardDailyActivityId = id;
                        item.CreatedDate = DateTime.Now;
                        sql = @"insert  into JobCardDailyActivityTask (JobCardDailyActivityId,JobCardTaskId,TaskStartDate,TaskEndDate,OverTime,ActualHours,CreatedBy,CreatedDate,OrganizationId, EmployeeId, StartTime, EndTime) Values 
                                (@JobCardDailyActivityId,@JobCardTaskId,@TaskStartDate,@TaskEndDate,@OverTime,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId, NULLIF(@EmployeeId, 0), @StartTime, @EndTime);
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
                string sql = @"SELECT JobCardDailyActivityId,JobCardDailyActivityRefNo,CONVERT(VARCHAR, JobCardDailyActivityDate, 106) JobCardDailyActivityDate,
	                           EMP.EmployeeName,JC.JobCardId,JC.JobCardNo,JC.EmployeeId,DA.Remarks,Image1,Image2,Image3,Image4,
                               JC.isProjectBased,ISNULL(JC.JodCardCompleteStatus,0)IsUsed
                               FROM JobCardDailyActivity DA
                               INNER JOIN JobCard JC ON DA.JobCardId = JC.JobCardId
                               LEFT JOIN Employee EMP ON JC.EmployeeId = EMP.EmployeeId
                               WHERE DA.JobCardDailyActivityId = @JobCardDailyActivityId
                               AND DA.isActive = 1";

                var objJobCardDailyActivity = connection.Query<JobCardDailyActivity>(sql, new
                {
                    JobCardDailyActivityId = JobCardDailyActivityId
                }).FirstOrDefault<JobCardDailyActivity>();

                if (objJobCardDailyActivity != null)
                    objJobCardDailyActivity.JobCardDailyActivityTask = new JobCardDailyActivityTaskRepository().GetJobCardDailyActivityTasks(JobCardDailyActivityId);

                return objJobCardDailyActivity;
            }
        }

        /// <summary>
        /// Get all items in Daily Activity
        /// </summary>
        /// <returns></returns>
        public List<JobCardDailyActivity> GetJobCardDailyActivitys(int OrganizationId, int type)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"

								SELECT
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
	                                DA.Remarks,
	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                JC.JobCardNo,
	                                EMP.EmployeeName,
	                                T.ActualHours,
	                                T.Tasks,
                                    JC.isProjectBased,
									V.ChassisNo,V.RegistrationNo
                                FROM JobCardDailyActivity DA
                                INNER JOIN JobCard JC ON DA.JobCardId = JC.JobCardId
                                INNER JOIN Employee EMP ON DA.EmployeeId = EMP.EmployeeId
                                INNER JOIN #TASKS T ON DA.JobCardDailyActivityId = T.JobCardDailyActivityId
							    Left join VehicleInPass V ON V.VehicleInPassId=JC.InPassId
                                WHERE DA.isActive = 1
                                    AND DA.OrganizationId = @OrganizationId
									AND JC.isProjectBased = @type
                                ORDER BY DA.JobCardDailyActivityDate DESC, DA.CreatedDate DESC;

                                DROP TABLE #TASKS;";

                var objJobCardDailyActivitys = connection.Query<JobCardDailyActivity>(sql, new { OrganizationId = OrganizationId, type = type }).ToList<JobCardDailyActivity>();

                return objJobCardDailyActivitys;
            }
        }

        public JobCardDailyActivity DeleteJobCardDailyActivity(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM JobCardDailyActivityTask WHERE JobCardDailyActivityId=@Id;
                                     DELETE FROM JobCardDailyActivity 
	                                        OUTPUT deleted.JobCardDailyActivityRefNo, 
	                                        deleted.Image1,
	                                        deleted.Image2,
	                                        deleted.Image3,
	                                        deleted.Image4
                                        WHERE JobCardDailyActivityId=@Id;";
                    JobCardDailyActivity output = connection.Query<JobCardDailyActivity>(query, new { Id = Id }, txn).FirstOrDefault();
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

        public IEnumerable<JobCardForDailyActivity> PendingJobcardTasks(int type, int OrganizationId, string RegNo = "", string jcno = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                if(type==0)
                {
                sql = @" select J.JobCardId,J.JobCardNo, J.JobCardDate, E.EmployeeName,W.WorkDescr,J.RequiredDate,
                                CustomerName,RegistrationNo,ChassisNo
                                from JobCard J
                                inner join SaleOrder S on S.SaleOrderId=J.SaleOrderId
                                inner join Employee E on E.EmployeeId = J.EmployeeId
                                LEFT join WorkDescription W on W.WorkDescriptionId = J.WorkDescriptionId
                                LEFT join VehicleInPass on VehicleInPassId=InPassId
                                inner join Customer C on C.CustomerId=S.CustomerId
                                where J.JodCardCompleteStatus is null
						        AND J.OrganizationId = @OrganizationId
						        AND	J.isProjectBased = @type
                                AND (ISNULL(RegistrationNo, '') LIKE '%'+@RegNo+'%'
			                    OR ISNULL(ChassisNo, '') LIKE '%'+@RegNo+'%')
	                            AND ISNULL(J.JobCardNo,'') LIKE '%'+@jcno+'%'
						        AND J.isActive = 1";
            }
                else
                {

                    sql = @" select J.JobCardId,J.JobCardNo, J.JobCardDate, E.EmployeeName,J.RequiredDate,
											CustomerName,
											STUFF((SELECT ', ' + CAST(JTM.JobCardTaskName AS VARCHAR(MAX)) [text()]
											FROM JobCard JOB
											inner join JobCardTask M on M.JobCardId = JOB.JobCardId
											inner join JobCardTaskMaster JTM ON jtm.JobCardTaskMasterId=M.JobCardTaskMasterId
											WHERE JOB.JobCardId=J.JobCardId
											FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescr
											from JobCard J
											inner join SaleOrder S on S.SaleOrderId=J.SaleOrderId
											inner join Employee E on E.EmployeeId = J.EmployeeId
											inner join Customer C on C.CustomerId=S.CustomerId
											where J.JodCardCompleteStatus is null
											AND J.OrganizationId = @OrganizationId
											AND	J.isProjectBased = @type
--										AND (ISNULL(RegistrationNo, '') LIKE '%'+@RegNo+'%'
--										OR ISNULL(ChassisNo, '') LIKE '%'+@RegNo+'%')
											AND ISNULL(J.JobCardNo,'') LIKE '%'+@jcno+'%'
											AND J.isActive = 1";
                }
                return connection.Query<JobCardForDailyActivity>(sql, new { OrganizationId = OrganizationId, type = type, RegNo = RegNo, jcno = jcno });
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
                                    T.JobCardTaskId,
	                                T.JobCardTaskMasterId,
	                                M.JobCardTaskName,
									CONVERT(VARCHAR, T.TaskDate, 106) TaskStartDate,
									CONVERT(VARCHAR, T.TaskDate, 106) TaskEndDate
                                FROM JobCardTask T
									INNER JOIN JobCard J ON T.JobCardId = J.JobCardId
	                                LEFT JOIN Employee E ON T.EmployeeId = E.EmployeeId
	                                INNER JOIN JobCardTaskMaster M ON T.JobCardTaskMasterId = M.JobCardTaskMasterId
                                WHERE T.isActive = 1 AND T.JobCardId = @id AND J.OrganizationId = @OrganizationId";
                return connection.Query<JobCardDailyActivityTask>(query, new { id = Id, OrganizationId = OrganizationId }).ToList();
            }
        }

        public int UpdateJobCardDailyActivity(JobCardDailyActivity objJobCardDailyActivity)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                string query = @"Delete JobCardDailyActivityTask  OUTPUT DELETED.JobCardDailyActivityId WHERE JobCardDailyActivityId=@JobCardDailyActivityId;";
                string output = connection.Query<string>(query, new { JobCardDailyActivityId = objJobCardDailyActivity.JobCardDailyActivityId }, txn).FirstOrDefault();

                string sql = @"UPDATE JobCardDailyActivity SET JobCardDailyActivityDate=@JobCardDailyActivityDate,JobCardId = @JobCardId ,
                               JobCardDailyActivityRefNo=@JobCardDailyActivityRefNo,Remarks=@Remarks,EmployeeId=@EmployeeId,
                               CreatedBy = @CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId  
                               OUTPUT INSERTED.JobCardDailyActivityRefNo  WHERE JobCardDailyActivityId = @JobCardDailyActivityId";
                try
                {
                    var id = connection.Execute(sql, objJobCardDailyActivity, txn);

                    foreach (var item in objJobCardDailyActivity.JobCardDailyActivityTask)
                    {
                        item.JobCardDailyActivityId = objJobCardDailyActivity.JobCardDailyActivityId;
                        item.CreatedDate = DateTime.Now;
                        sql = @"insert  into JobCardDailyActivityTask (JobCardDailyActivityId,JobCardTaskId,TaskStartDate,TaskEndDate,OverTime,ActualHours,CreatedBy,CreatedDate,OrganizationId, EmployeeId, StartTime, EndTime) Values 
                        (@JobCardDailyActivityId,@JobCardTaskMasterId,@TaskStartDate,@TaskEndDate,@OverTime,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId, @EmployeeId, @StartTime, @EndTime);
                        SELECT CAST(SCOPE_IDENTITY() as int)";


                        var taskid = connection.Query<int>(sql, item, txn).Single();
                    }

                    InsertLoginHistory(dataConnection, objJobCardDailyActivity.CreatedBy, "Update", "Job Card", id.ToString(), objJobCardDailyActivity.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public int InsertProjectDailyActivity(JobCardDailyActivity model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    //int _DailyActivityId = 0;
                    model.JobCardDailyActivityRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 38, true, txn);
                    string sql1 = @"SELECT JobCardTaskId FROM JobCardTask WHERE 
                                    JobCardTaskMasterId = @JobCardTaskMasterId AND 
                                    TaskDate = CONVERT(VARCHAR, @TaskStartDate, 106) AND 
                                    JobCardId = @id AND EmployeeId IS NULL",
                    sql2 = @"DELETE FROM JobCardTask WHERE JobCardTaskId IN @id",
                    sql3 = @"INSERT INTO JobCardTask (JobCardId, JobCardTaskMasterId, SlNo, EmployeeId, TaskDate, [Hours], CreatedBy, OrganizationId, isActive)
                           OUTPUT inserted.JobCardTaskId VALUES (@JobCardId, @JobCardTaskMasterId, @SlNo, @EmployeeId, @TaskDate, @Hours, @CreatedBy, @OrganizationId, 1)",
                    sql4 = @"SELECT JobCardTaskId FROM JobCardTask WHERE 
                                    JobCardTaskMasterId = @JobCardTaskMasterId AND 
                                    TaskDate = CONVERT(VARCHAR, @TaskStartDate, 106) AND 
                                    JobCardId = @id AND EmployeeId = @EmployeeId";
                    model.JobCardDailyActivityId = InsertDailyActivityHead(model, connection, txn);
                    foreach (var item in model.JobCardDailyActivityTask)
                    {
                        item.JobCardDailyActivityId = model.JobCardDailyActivityId;
                        List<int> existingTasks = connection.Query<int>(sql1, new
                        {
                            JobCardTaskMasterId = item.JobCardTaskMasterId,
                            TaskStartDate = item.TaskStartDate,
                            id = model.JobCardId
                        }, txn).ToList();

                        if (existingTasks.Count > 0)
                            connection.Execute(sql2, new { id = existingTasks }, txn);

                        item.JobCardTaskId = connection.Query<int>(sql4, new
                                            {
                                                JobCardTaskMasterId = item.JobCardTaskMasterId,
                                                TaskStartDate = item.TaskStartDate,
                                                id = model.JobCardId,
                                                EmployeeId = item.EmployeeId
                                            }, txn).FirstOrDefault();
                        if (item.JobCardTaskId == 0)
                        {
                            item.JobCardTaskId = connection.Query<int>(sql3, new
                            {
                                JobCardId = model.JobCardId,
                                JobCardTaskMasterId = item.JobCardTaskMasterId,
                                SlNo = 0, //SlNo should be given here
                                EmployeeId = item.EmployeeId,
                                TaskDate = item.TaskStartDate,
                                Hours = item.ActualHours,
                                CreatedBy = model.CreatedBy,
                                OrganizationId = model.OrganizationId
                            }, txn).FirstOrDefault();
                            if (item.JobCardTaskId <= 0) throw new Exception("Insertion of task failed.");
                            InsertDailyActivityTasks(item, connection, txn);
                        }
                        else
                            InsertDailyActivityTasks(item, connection, txn);
                        //}
                        //else
                        //{
                        //    InsertDailyActivityTasks(item, connection, txn);
                        //}
                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "DailyActivity", model.JobCardDailyActivityId.ToString(), model.OrganizationId.ToString());
                    txn.Commit();
                    return model.JobCardDailyActivityId;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        private int InsertDailyActivityHead(JobCardDailyActivity model, IDbConnection connection, IDbTransaction txn)
        {
            string sql = @"insert  into JobCardDailyActivity (JobCardDailyActivityDate,JobCardId,JobCardDailyActivityRefNo,Remarks,EmployeeId,CreatedBy,CreatedDate,OrganizationId) 
                           Values (@JobCardDailyActivityDate,@JobCardId,@JobCardDailyActivityRefNo,@Remarks,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

            return connection.Query<int>(sql, model, txn).Single();
        }

        private int InsertDailyActivityTasks(JobCardDailyActivityTask item, IDbConnection connection, IDbTransaction txn)
        {
            //foreach (var item in model.JobCardDailyActivityTask)
            //{
            if (item.ActualHours == null || item.ActualHours == 0) return 0;
            item.JobCardDailyActivityId = item.JobCardDailyActivityId;
            item.CreatedDate = DateTime.Now;
            item.TaskEndDate = item.TaskStartDate;
            string sql = @"insert  into JobCardDailyActivityTask (JobCardDailyActivityId,JobCardTaskId,TaskStartDate,TaskEndDate,OverTime,ActualHours,CreatedBy,CreatedDate,OrganizationId, EmployeeId, StartTime, EndTime) Values 
                        (@JobCardDailyActivityId,@JobCardTaskId,@TaskStartDate,@TaskEndDate,@OverTime,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId, NULLIF(@EmployeeId, 0), @StartTime, @EndTime);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            var taskid = connection.Query<int>(sql, item, txn).Single();
            //}
            return 1;
        }
    }
}