using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardQCRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertJobCardQC(JobCardQC objJobCardQC)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into JobCardQC(JobCardId,EmployeeId,JobCardQCDate,IsQCPassed,CreatedBy,CreatedDate,OrganizationId,isActive) Values (@JobCardId,@EmployeeId,@JobCardQCDate,@IsQCPassed,@CreatedBy,@CreatedDate,@OrganizationId,@isActive);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardQC).Single();
                return id;
            }
        }


        public JobCardQC GetJobCardQC(int JobCardQCId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardQC
                        where JobCardQCId=@JobCardQCId";

                var objJobCardQC = connection.Query<JobCardQC>(sql, new
                {
                    JobCardQCId = JobCardQCId
                }).First<JobCardQC>();

                return objJobCardQC;
            }
        }

        public List<JobCardQC> GetJobCardQCs()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardQC
                        where isActive=1";

                var objJobCardQCs = connection.Query<JobCardQC>(sql).ToList<JobCardQC>();

                return objJobCardQCs;
            }
        }



        public int DeleteJobCardQC(Unit objJobCardQC)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardQC  OUTPUT DELETED.JobCardQCId WHERE JobCardQCId=@JobCardQCId";


                var id = connection.Execute(sql, objJobCardQC);
                return id;
            }
        }


    }
}