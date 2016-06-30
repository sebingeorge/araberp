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

        public List<JobCardDailyActivityTask> GetJobCardDailyActivityTasks()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardDailyActivityTask
                        where isActive=1";

                var objJobCardDailyActivityTasks = connection.Query<JobCardDailyActivityTask>(sql).ToList<JobCardDailyActivityTask>();

                return objJobCardDailyActivityTasks;
            }
        }



        public int DeleteJobCardDailyActivityTask(Unit objJobCardDailyActivityTask)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardDailyActivityTask  OUTPUT DELETED.JobCardDailyActivityTaskId WHERE JobCardDailyActivityTaskId=@JobCardDailyActivityTaskId";


                var id = connection.Execute(sql, objJobCardDailyActivityTask);
                return id;
            }
        }


    }
}
