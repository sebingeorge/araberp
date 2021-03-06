﻿using System;
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


        public int InsertJobCardTask(JobCardTask objJobCardTask, IDbConnection connection, IDbTransaction trn)
        {
            try
            {
                string sql = @"insert  into JobCardTask(JobCardId,JobCardTaskMasterId,SlNo,EmployeeId,TaskDate,Hours,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values 
                (@JobCardId,@JobCardTaskId,@SlNo,NULLIF(@EmployeeId, 0),@TaskDate,@Hours,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId);
                SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardTask,trn).Single();
                return id;
            }
            catch(Exception ex)
            {
                return 0;
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


        public List<JobCardTask> GetJobCardDT(int JobCardId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT distinct JT.TaskDate TaskDate,JM.JobCardTaskName,E.EmployeeName Employee,JT.[Hours]
                                FROM JobCard J
                                INNER JOIN JobCardTask JT ON J.JobCardId=JT.JobCardId
                                LEFT JOIN Employee E ON E.EmployeeId=JT.EmployeeId
                                INNER JOIN JobCardTaskMaster JM ON JM.JobCardTaskMasterId=JT.JobCardTaskMasterId
                                WHERE J.JobCardId=@JobCardId";

                return connection.Query<JobCardTask>(sql, new { JobCardId = JobCardId }).ToList();


            }
        }

    }

}
