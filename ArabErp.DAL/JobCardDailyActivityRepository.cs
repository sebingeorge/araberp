using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardDailyActivityRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertJobCardDailyActivity(JobCardDailyActivity objJobCardDailyActivity)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into JobCardDailyActivity (JobCardDailyActivityDate,JobCardId,Remarks,EmployeeId,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardDailyActivityDate,@JobCardId,@Remarks,@EmployeeId,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardDailyActivity).Single();
                return id;
            }
        }


        public JobCardDailyActivity GetJobCardDailyActivity(int JobCardDailyActivityId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardDailyActivity
                        where JobCardDailyActivityId=@JobCardDailyActivityId";

                var objJobCardDailyActivity = connection.Query<JobCardDailyActivity>(sql, new
                {
                    JobCardDailyActivityId = JobCardDailyActivityId
                }).First<JobCardDailyActivity>();

                return objJobCardDailyActivity;
            }
        }

        public List<JobCardDailyActivity> GetJobCardDailyActivitys()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardDailyActivity
                        where isActive=1";

                var objJobCardDailyActivitys = connection.Query<JobCardDailyActivity>(sql).ToList<JobCardDailyActivity>();

                return objJobCardDailyActivitys;
            }
        }



        public int DeleteJobCardDailyActivity(Unit objJobCardDailyActivity)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardDailyActivity  OUTPUT DELETED.JobCardDailyActivityId WHERE JobCardDailyActivityId=@JobCardDailyActivityId";


                var id = connection.Execute(sql, objJobCardDailyActivity);
                return id;
            }
        }


    }
}