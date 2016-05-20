using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class JobCardTaskMasterRepository : BaseRepository
    {

        public int InsertJobCardTaskMaster(JobCardTaskMaster objJobCardTaskMaster)
        {
            string sql = @"insert  into JobCardTaskMaster(JobCardTaskName,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardTaskName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objJobCardTaskMaster).Single();
            return id;
        }


        public JobCardTaskMaster GetJobCardTaskMaster(int JobCardTaskMasterId)
        {

            string sql = @"select * from JobCardTaskMaster
                        where JobCardTaskMasterId=@JobCardTaskMasterId";

            var objJobCardTaskMaster = connection.Query<JobCardTaskMaster>(sql, new
            {
                JobCardTaskMasterId = JobCardTaskMasterId
            }).First<JobCardTaskMaster>();

            return objJobCardTaskMaster;
        }

        public List<JobCardTaskMaster> GetJobCardTaskMasters()
        {
            string sql = @"select * from JobCardTaskMaster
                        where isActive=1";

            var objJobCardTaskMasters = connection.Query<JobCardTaskMaster>(sql).ToList<JobCardTaskMaster>();

            return objJobCardTaskMasters;
        }

        public int UpdateJobCardTaskMaster(JobCardTaskMaster objJobCardTaskMaster)
        {
            string sql = @"UPDATE JobCardTaskMaster SET JobCardTaskName = @JobCardTaskName ,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate  OUTPUT INSERTED.JobCardTaskMasterId  WHERE JobCardTaskMasterId = @JobCardTaskMasterId";


            var id = connection.Execute(sql, objJobCardTaskMaster);
            return id;
        }

        public int DeleteJobCardTaskMaster(Unit objJobCardTaskMaster)
        {
            string sql = @"Delete JobCardTaskMaster  OUTPUT DELETED.JobCardTaskMasterId WHERE JobCardTaskMasterId=@JobCardTaskMasterId";


            var id = connection.Execute(sql, objJobCardTaskMaster);
            return id;
        }


    }
}