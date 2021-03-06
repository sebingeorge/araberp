﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class WorkVsTaskRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertWorkVsTask(WorkVsTask objWorkVsTask, IDbConnection connection, IDbTransaction trn)
        {
               try
            {
           
                string sql = @"insert  into WorkVsTask(WorkDescriptionId,JobCardTaskMasterId,Hours,CreatedBy,CreatedDate,OrganizationId) Values (@WorkDescriptionId,@JobCardTaskMasterId,@Hours,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkVsTask,trn).Single();
                return id;
            }
               catch (Exception)
               {
                   throw;
               }
            
        }


        public WorkVsTask GetWorkVsTask(int WorkVsTaskId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkVsTask
                        where WorkVsTaskId=@WorkVsTaskId";

                var objWorkVsTask = connection.Query<WorkVsTask>(sql, new
                {
                    WorkVsTaskId = WorkVsTaskId
                }).First<WorkVsTask>();

                return objWorkVsTask;
            }
        }

        public List<WorkVsTask> GetWorkVsTasks()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkVsTask
                        where isActive=1";

                var objWorkVsTasks = connection.Query<WorkVsTask>(sql).ToList<WorkVsTask>();

                return objWorkVsTasks;
            }
        }
        public List<WorkVsTask> GetWorkDescriptionWorkVsTasks(int Id)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from WorkVsTask where isActive=1 and WorkDescriptionId=@Id";

                var objWorkVsTasks = connection.Query<WorkVsTask>(sql, new
                {
                    Id = Id
                }).ToList<WorkVsTask>();

                return objWorkVsTasks;
            }
        }
        public int UpdateWorkVsTask(WorkVsTask objWorkVsTask)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE WorkVsTask SET WorkDescriptionId = @WorkDescriptionId ,JobCardTaskId = @JobCardTaskId ,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkVsTaskId  WHERE WorkVsTaskId = @WorkVsTaskId";


                var id = connection.Execute(sql, objWorkVsTask);
                return id;
            }
        }

        public int DeleteWorkVsTask(Unit objWorkVsTask)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete WorkVsTask  OUTPUT DELETED.WorkVsTaskId WHERE WorkVsTaskId=@WorkVsTaskId";


                var id = connection.Execute(sql, objWorkVsTask);
                return id;
            }
        }


    }
}