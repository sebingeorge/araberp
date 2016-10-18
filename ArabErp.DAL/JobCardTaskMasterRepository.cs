using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardTaskMasterRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertJobCardTaskMaster(JobCardTaskMaster objJobCardTaskMaster)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into JobCardTaskMaster(JobCardTaskName,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardTaskName,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardTaskMaster).Single();
                InsertLoginHistory(dataConnection, objJobCardTaskMaster.CreatedBy, "Create", "Job Card Task Master", id.ToString(), "0");
                return id;
            }
        }


        public JobCardTaskMaster GetJobCardTaskMaster(int JobCardTaskMasterId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from JobCardTaskMaster
                        where JobCardTaskMasterId=@JobCardTaskMasterId";

                var objJobCardTaskMaster = connection.Query<JobCardTaskMaster>(sql, new
                {
                    JobCardTaskMasterId = JobCardTaskMasterId
                }).First<JobCardTaskMaster>();

                return objJobCardTaskMaster;
            }
        }

        public List<Dropdown> FillJobCardTaskMaster()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select JobCardTaskMasterId Id,JobCardTaskName Name from JobCardTaskMaster WHERE isActive=1").ToList();
            }
        }

        public List<JobCardTaskMaster> GetJobCardTaskMasters()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardTaskMaster
                        where isActive=1";

                var objJobCardTaskMasters = connection.Query<JobCardTaskMaster>(sql).ToList<JobCardTaskMaster>();

                return objJobCardTaskMasters;
            }
        }

        public int UpdateJobCardTaskMaster(JobCardTaskMaster objJobCardTaskMaster)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE JobCardTaskMaster SET JobCardTaskName = @JobCardTaskName ,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate  OUTPUT INSERTED.JobCardTaskMasterId  WHERE JobCardTaskMasterId = @JobCardTaskMasterId";


                var id = connection.Execute(sql, objJobCardTaskMaster);
                InsertLoginHistory(dataConnection, objJobCardTaskMaster.CreatedBy, "Update", "Job Card Task Master", id.ToString(), "0");
                return id;
            }
        }

        public int DeleteJobCardTaskMaster(Unit objJobCardTaskMaster)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardTaskMaster  OUTPUT DELETED.JobCardTaskMasterId WHERE JobCardTaskMasterId=@JobCardTaskMasterId";


                var id = connection.Execute(sql, objJobCardTaskMaster);
                InsertLoginHistory(dataConnection, objJobCardTaskMaster.CreatedBy, "Delete", "Job Card Task Master", id.ToString(), "0");
                return id;
            }
        }


    }
}