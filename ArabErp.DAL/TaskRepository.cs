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

       public int InsertTask(JobCardTaskMaster objTask)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               string sql = @"INSERT INTO JobCardTaskMaster(JobCardTaskRefNo,JobCardTaskName,CreatedBy,CreatedDate,OrganizationId) VALUES(@JobCardTaskRefNo,@JobCardTaskName,@CreatedBy,getDate(),@OrganizationId);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
               var id = connection.Query<int>(sql, objTask).Single();
               return id;
           }
       }

       public IEnumerable<JobCardTaskMaster> FillTaskList()
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               return connection.Query<JobCardTaskMaster>("SELECT JobCardTaskRefNo,JobCardTaskName FROM JobCardTaskMaster").ToList();
           }
       }
       public JobCardTaskMaster GetTask(int TaskId)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {

               string sql = @"select * from JobCardTaskMaster
                        where JobCardTaskMasterId=@JobCardTaskMasterId";

               var objTask = connection.Query<JobCardTaskMaster>(sql, new
               {
                   TaskId = TaskId
               }).First<JobCardTaskMaster>();

               return objTask;
           }
        }

       public List<JobCardTaskMaster> GetTask()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardTaskMaster
                        where OrganizationId>0";

                var objTask = connection.Query<JobCardTaskMaster>(sql).ToList<JobCardTaskMaster>();

                return objTask;
            }
        }

    }
}

