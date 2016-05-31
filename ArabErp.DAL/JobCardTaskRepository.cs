using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardTaskRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertJobCardTask(JobCardTask objJobCardTask)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into StockReturnItem(JobCardId,JobCardTaskMasterId,SlNo,EmployeeId,TaskDate,Hours,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardId,@JobCardTaskMasterId,@SlNo,@EmployeeId,@TaskDate,@Hours,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardTask).Single();
                return id;
            }
        }


        public JobCardTask GetJobCardTask(int JobCardTaskId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardTask
                        where JobCardTaskId=@JobCardTaskId";

                var objJobCardTask = connection.Query<JobCardTask>(sql, new
                {
                    JobCardTaskId = JobCardTaskId
                }).First<JobCardTask>();

                return objJobCardTask;
            }
        }

        public List<JobCardTask> GetJobCardTasks()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardTask
                        where isActive=1";

                var objJobCardTasks = connection.Query<JobCardTask>(sql).ToList<JobCardTask>();

                return objJobCardTasks;
            }
        }

        public int UpdateJobCardTask(JobCardTask objJobCardTask)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE JobCardTask SET JobCardId = @JobCardId ,JobCardTaskMasterId = @JobCardTaskMasterId ,SlNo = @SlNo ,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.JobCardTaskId  WHERE JobCardTaskId = @JobCardTaskId";


                var id = connection.Execute(sql, objJobCardTask);
                return id;
            }
        }

        public int DeleteJobCardTask(Unit objJobCardTask)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardTask  OUTPUT DELETED.JobCardTaskId WHERE JobCardTaskId=@JobCardTaskId";


                var id = connection.Execute(sql, objJobCardTask);
                return id;
            }

        }
    }

}
