using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class JobCardTaskRepository : BaseRepository
    {

        public int InsertJobCardTask(JobCardTask objJobCardTask)
        {
            string sql = @"insert  into JobCardTask(JobCardId,JobCardTaskMasterId,SlNo,CreatedBy,CreatedDate) Values (@JobCardId,@JobCardTaskMasterId,@SlNo,@CreatedBy,@CreatedDate);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objJobCardTask).Single();
            return id;
        }


        public JobCardTask GetJobCardTask(int JobCardTaskId)
        {

            string sql = @"select * from JobCardTask
                        where JobCardTaskId=@JobCardTaskId";

            var objJobCardTask = connection.Query<JobCardTask>(sql, new
            {
                JobCardTaskId = JobCardTaskId
            }).First<JobCardTask>();

            return objJobCardTask;
        }

        public List<JobCardTask> GetJobCardTasks()
        {
            string sql = @"select * from JobCardTask
                        where isActive=1";

            var objJobCardTasks = connection.Query<JobCardTask>(sql).ToList<JobCardTask>();

            return objJobCardTasks;
        }

        public int UpdateJobCardTask(JobCardTask objJobCardTask)
        {
            string sql = @"UPDATE JobCardTask SET JobCardId = @JobCardId ,JobCardTaskMasterId = @JobCardTaskMasterId ,SlNo = @SlNo ,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.JobCardTaskId  WHERE JobCardTaskId = @JobCardTaskId";


            var id = connection.Execute(sql, objJobCardTask);
            return id;
        }

        public int DeleteJobCardTask(Unit objJobCardTask)
        {
            string sql = @"Delete JobCardTask  OUTPUT DELETED.JobCardTaskId WHERE JobCardTaskId=@JobCardTaskId";


            var id = connection.Execute(sql, objJobCardTask);
            return id;
        }


    }
}
