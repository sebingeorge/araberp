using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;


namespace ArabErp.DAL
{
   public  class TaskRepository:BaseRepository
    {


       public int InsertTask(Task objTask)
        {
            string sql = @"INSERT INTO Task(TaskRefNo,TaskName,CreatedBy,CreatedDate,OrganizationId) VALUES(@TaskRefNo,@TaskName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
            var id = connection.Query<int>(sql, objTask).Single();
            return id;
        }

       public Task GetTask(int TaskId)
        {

            string sql = @"select * from Task
                        where TaskId=@TaskId";

            var objTask = connection.Query<Task>(sql, new
            {
                TaskId = TaskId
            }).First<Task>();

            return objTask;
        }

       public List<Task> GetTask()
        {
            string sql = @"select * from Task
                        where OrganizationId>0";

            var objTask = connection.Query<Task>(sql).ToList<Task>();

            return objTask;
        }

    }
}

