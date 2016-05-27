using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
   public  class TaskRepository:BaseRepository
    {

       static string dataConnection = GetConnectionString("arab");

       public int InsertTask(Task objTask)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               string sql = @"INSERT INTO Task(TaskRefNo,TaskName,CreatedBy,CreatedDate,OrganizationId) VALUES(@TaskRefNo,@TaskName,@CreatedBy,getDate(),@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";
               var id = connection.Query<int>(sql, objTask).Single();
               return id;
           }
       }

       public IEnumerable<Task> FillTaskList()
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               return connection.Query<Task>("SELECT TaskRefNo,TaskName FROM Task").ToList();
           }
       }
       public Task GetTask(int TaskId)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {

               string sql = @"select * from Task
                        where TaskId=@TaskId";

               var objTask = connection.Query<Task>(sql, new
               {
                   TaskId = TaskId
               }).First<Task>();

               return objTask;
           }
        }

       public List<Task> GetTask()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from Task
                        where OrganizationId>0";

                var objTask = connection.Query<Task>(sql).ToList<Task>();

                return objTask;
            }
        }

    }
}

