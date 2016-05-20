using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class WorkVsTaskRepository : BaseRepository
    {

        public int InsertWorkVsTask(WorkVsTask objWorkVsTask)
        {
            string sql = @"insert  into WorkVsTask(WorkDescriptionId,JobCardTaskId,CreatedBy,CreatedDate,OrganizationId) Values (@WorkDescriptionId,@JobCardTaskId,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objWorkVsTask).Single();
            return id;
        }


        public WorkVsTask GetWorkVsTask(int WorkVsTaskId)
        {

            string sql = @"select * from WorkVsTask
                        where WorkVsTaskId=@WorkVsTaskId";

            var objWorkVsTask = connection.Query<WorkVsTask>(sql, new
            {
                WorkVsTaskId = WorkVsTaskId
            }).First<WorkVsTask>();

            return objWorkVsTask;
        }

        public List<WorkVsTask> GetWorkVsTasks()
        {
            string sql = @"select * from WorkVsTask
                        where isActive=1";

            var objWorkVsTasks = connection.Query<WorkVsTask>(sql).ToList<WorkVsTask>();

            return objWorkVsTasks;
        }

        public int UpdateWorkVsTask(WorkVsTask objWorkVsTask)
        {
            string sql = @"UPDATE WorkVsTask SET WorkDescriptionId = @WorkDescriptionId ,JobCardTaskId = @JobCardTaskId ,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate  OUTPUT INSERTED.WorkVsTaskId  WHERE WorkVsTaskId = @WorkVsTaskId";


            var id = connection.Execute(sql, objWorkVsTask);
            return id;
        }

        public int DeleteWorkVsTask(Unit objWorkVsTask)
        {
            string sql = @"Delete WorkVsTask  OUTPUT DELETED.WorkVsTaskId WHERE WorkVsTaskId=@WorkVsTaskId";


            var id = connection.Execute(sql, objWorkVsTask);
            return id;
        }


    }
}