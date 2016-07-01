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
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(JobCardDailyActivity).Name, "0", 1);
                    objJobCardDailyActivity.JobCardDailyActivityRefNo = "DA/" + internalId.ToString();
                    string sql = @"insert  into JobCardDailyActivity (JobCardDailyActivityDate,JobCardId,JobCardDailyActivityRefNo,Remarks,EmployeeId,CreatedBy,CreatedDate,OrganizationId) 
                                                            Values (@JobCardDailyActivityDate,@JobCardId,@JobCardDailyActivityRefNo,@Remarks,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId);
                                SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, objJobCardDailyActivity, trn).Single();

                    foreach (var item in objJobCardDailyActivity.JobCardDailyActivityTask)
                    {
                        item.JobCardDailyActivityId = id;
                        item.CreatedDate = DateTime.Now;
                        sql = @"insert  into JobCardDailyActivityTask (JobCardDailyActivityId,JobCardTaskId,TaskStartDate,TaskEndDate,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values 
                                                                      (@JobCardDailyActivityId,@JobCardTaskId,@TaskStartDate,@TaskEndDate,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId);
                        SELECT CAST(SCOPE_IDENTITY() as int)";


                        var taskid = connection.Query<int>(sql, item, trn).Single();
                    }

                    trn.Commit();
                }
                catch(Exception ex)
                {
                    trn.Rollback();
                    return 0;
                }
                return id;
            }
        }


        public JobCardDailyActivity GetJobCardDailyActivity(int JobCardDailyActivityId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardDailyActivity
                        where JobCardDailyActivityId=@JobCardDailyActivityId";

                var objJobCardDailyActivity = connection.Query<JobCardDailyActivity>(sql, new
                {
                    JobCardDailyActivityId = JobCardDailyActivityId
                }).First<JobCardDailyActivity>();

                return objJobCardDailyActivity;
            }
        }

        public List<JobCardDailyActivity> GetJobCardDailyActivitys()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardDailyActivity
                        where isActive=1";

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
        
        public IEnumerable<JobCardForDailyActivity> PendingJobcardTasks()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select J.JobCardId,J.JobCardNo, J.JobCardDate, E.EmployeeName,W.WorkDescr,J.RequiredDate
                            from JobCard J
                            inner join Employee E on E.EmployeeId = J.EmployeeId
                            inner join WorkDescription W on W.WorkDescriptionId = J.WorkDescriptionId
                            where J.isProjectBased = 1 and J.JodCardCompleteStatus is null";
                return connection.Query<JobCardForDailyActivity>(sql);
            }
        }

    }
}