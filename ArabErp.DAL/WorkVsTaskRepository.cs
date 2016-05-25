using System;
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


        public int InsertWorkVsTask(WorkVsTask objWorkVsTask)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into WorkVsTask(WorkDescriptionId,JobCardTaskId,CreatedBy,CreatedDate,OrganizationId) Values (@WorkDescriptionId,@JobCardTaskId,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objWorkVsTask).Single();
                return id;
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