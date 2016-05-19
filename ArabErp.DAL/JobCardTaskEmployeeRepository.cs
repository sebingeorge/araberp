using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class JobCardTaskEmployeeRepository : BaseRepository
    {

        public int InsertJobCardTaskEmployee(JobCardTaskEmployee objJobCardTaskEmployee)
        {
            string sql = @"insert  into JobCardTaskEmployee(JobCardTaskId,EmployeeId,TaskDate,Hours,ActualHours,CreatedBy,CreatedDate,OrganizationId) Values (@JobCardTaskId,@EmployeeId,@TaskDate,@Hours,@ActualHours,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objJobCardTaskEmployee).Single();
            return id;
        }


        public JobCardTaskEmployee GetJobCardTaskEmployee(int JobCardTaskEmployeeId)
        {

            string sql = @"select * from JobCardTaskEmployee
                        where JobCardTaskEmployeeId=@JobCardTaskEmployeeId";

            var objJobCardTaskEmployee = connection.Query<JobCardTaskEmployee>(sql, new
            {
                JobCardTaskEmployeeId = JobCardTaskEmployeeId
            }).First<JobCardTaskEmployee>();

            return objJobCardTaskEmployee;
        }

        public List<JobCardTaskEmployee> GetJobCardTaskEmployees()
        {
            string sql = @"select * from JobCardTaskEmployee
                        where isActive=1";

            var objJobCardTaskEmployees = connection.Query<JobCardTaskEmployee>(sql).ToList<JobCardTaskEmployee>();

            return objJobCardTaskEmployees;
        }

        public int UpdateJobCardTaskEmployee(JobCardTaskEmployee objJobCardTaskEmployee)
        {
            string sql = @"UPDATE JobCardTaskEmployee SET JobCardTaskId = @JobCardTaskId ,EmployeeId = @EmployeeId ,TaskDate = @TaskDate ,Hours = @Hours,ActualHours = @ActualHours,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.JobCardTaskEmployeeId  WHERE JobCardTaskEmployeeId = @JobCardTaskEmployeeId";


            var id = connection.Execute(sql, objJobCardTaskEmployee);
            return id;
        }

        public int DeleteJobCardTaskEmployee(Unit objJobCardTaskEmployee)
        {
            string sql = @"Delete JobCardTaskEmployee  OUTPUT DELETED.JobCardTaskEmployeeId WHERE JobCardTaskEmployeeId=@JobCardTaskEmployeeId";


            var id = connection.Execute(sql, objJobCardTaskEmployee);
            return id;
        }


    }
}