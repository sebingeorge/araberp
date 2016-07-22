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
        public string ConnectionString()
        {
            return dataConnection;
        }

       public JobCardTaskMaster InsertTask(JobCardTaskMaster objTask)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               var result = new JobCardTaskMaster();
               IDbTransaction trn = connection.BeginTransaction();

               string sql = @"INSERT INTO JobCardTaskMaster(JobCardTaskRefNo,JobCardTaskName,MinimumRate,CreatedBy,CreatedDate,OrganizationId) 
                            VALUES(@JobCardTaskRefNo,@JobCardTaskName,@MinimumRate,@CreatedBy,@CreatedDate,@OrganizationId);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
               try
               {
                   int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(JobCardTaskMaster).Name, "0", 1);
                   objTask.JobCardTaskRefNo = "T/" + internalid;

                   int id = connection.Query<int>(sql, objTask, trn).Single();
                   objTask.JobCardTaskMasterId = id;
                   //connection.Dispose();
                   trn.Commit();
               }
               catch (Exception ex)
               {
                   trn.Rollback();
                   objTask.JobCardTaskMasterId = 0;
                   objTask.JobCardTaskRefNo = null;

               }
               return objTask;
           }
       }

       public IEnumerable<JobCardTaskMaster> FillTaskList()
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               return connection.Query<JobCardTaskMaster>("SELECT JobCardTaskMasterId,JobCardTaskRefNo,JobCardTaskName FROM JobCardTaskMaster WHERE isActive=1").ToList();
           }
       }
       public JobCardTaskMaster GetTask(int JobCardTaskMasterId)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {

               string sql = @"select * from JobCardTaskMaster
                        where JobCardTaskMasterId=@JobCardTaskMasterId";

               var objTask = connection.Query<JobCardTaskMaster>(sql, new
               {
                   JobCardTaskMasterId = JobCardTaskMasterId
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

       public JobCardTaskMaster UpdateTask(JobCardTaskMaster objTask)
       {
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               string sql = @"Update JobCardTaskMaster Set JobCardTaskRefNo=@JobCardTaskRefNo,JobCardTaskName=@JobCardTaskName,MinimumRate=@MinimumRate 
                             OUTPUT INSERTED.JobCardTaskMasterId WHERE JobCardTaskMasterId=@JobCardTaskMasterId";


               var id = connection.Execute(sql, objTask);
               return objTask;
           }
       }

       public int DeleteTask(JobCardTaskMaster objTask)
       {
           int result = 0;
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               string sql = @" Update JobCardTaskMaster Set isActive=0 WHERE JobCardTaskMasterId=@JobCardTaskMasterId";
               try
               {

                   var id = connection.Execute(sql, objTask);
                   objTask.JobCardTaskMasterId = id;
                   result = 0;

               }
               catch (SqlException ex)
               {
                   int err = ex.Errors.Count;
                   if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
                   {
                       switch (ex.Errors[0].Number)
                       {
                           case 547: // Foreign Key violation
                               result = 1;
                               break;

                           default:
                               result = 2;
                               break;
                       }
                   }

               }

               return result;
           }
       }


       public string GetRefNo(JobCardTaskMaster objTask)
       {

           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               string RefNo = "";
               var result = new JobCardTaskMaster();

               IDbTransaction trn = connection.BeginTransaction();

               try
               {
                   int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(JobCardTaskMaster).Name, "0", 0);
                   RefNo = "T/" + internalid;
                   trn.Commit();
               }
               catch (Exception ex)
               {
                   trn.Rollback();
               }
               return RefNo;
           }
       }

    }
}

