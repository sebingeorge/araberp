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

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"INSERT INTO JobCardQC(JobCardId,EmployeeId,JobCardQCDate,IsQCPassed,CreatedBy,CreatedDate,OrganizationId) VALUES (@JobCardId,@EmployeeId,GETDATE(),@IsQCPassed,@CreatedBy,GETDATE(),@OrganizationId);
           

                        SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, objJobCardQC, trn).Single();
                    var JobCardQCParamRepo = new JobCardQCParamRepository();
                    
                    foreach (var item in objJobCardQC.JobCardQCParams)
                    {
                        item.JobCardQCId = id;
                        JobCardQCParamRepo.InsertSaleOrderItem(item, connection, trn);
                    }

                    trn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    throw;
                    return 0;

                }


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
        public List<Dropdown> FillJobCard()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                //return connection.Query<Dropdown>("x",
                // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
                return connection.Query<Dropdown>("select JobCardNo Name,JobCardId Id from JobCard where JobCardId NOT IN (SELECT JobCardId FROM JobCardQC)").ToList();

            }
        }
        public List<Dropdown> FillEmployee()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                //return connection.Query<Dropdown>("x",
                // return connection.Query<Dropdown>("dbo.usp_MvcGetDayClosingDetails", param, commandType: CommandType.StoredProcedure).ToList();
                return connection.Query<Dropdown>("SELECT EmployeeName Name,EmployeeId Id FROM employee").ToList();

            }
        }


    }
}