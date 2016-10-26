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
                    string internalId = DatabaseCommonRepository.GetNewDocNo(connection, objJobCardQC.OrganizationId, 17, true,trn);

                    objJobCardQC.JobCardQCRefNo = internalId;
                    string sql = @"INSERT INTO JobCardQC(JobCardId,JobCardQCRefNo,EmployeeId,JobCardQCDate,IsQCPassed,CreatedBy,CreatedDate,OrganizationId) VALUES (@JobCardId,@JobCardQCRefNo,@EmployeeId,GETDATE(),@IsQCPassed,@CreatedBy,GETDATE(),@OrganizationId);
           

                        SELECT CAST(SCOPE_IDENTITY() as int)";


                    id = connection.Query<int>(sql, objJobCardQC, trn).Single();
                    var JobCardQCParamRepo = new JobCardQCParamRepository();
                    
                    foreach (var item in objJobCardQC.JobCardQCParams)
                    {
                        item.JobCardQCId = id;
                        JobCardQCParamRepo.InsertSaleOrderItem(item, connection, trn);
                    }
                    InsertLoginHistory(dataConnection, objJobCardQC.CreatedBy, "Create", "Job Card QC", id.ToString(), "0");
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
        public IEnumerable<PendingJobCardQC> GetPendingJobCardQC()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT * INTO #JobCard FROM JobCard WHERE JodCardCompleteStatus=1;
                               SELECT JC.JobCardId JobCardId,JC.JobCardNo JobCardNo,JC.JobCardDate JobCardDate,JC.SaleOrderId SaleOrderId,JC.SaleOrderItemId SaleOrderItemId INTO #JOBCARDvsQC 
                               FROM #JobCard JC WHERE JC.JobCardId NOT IN (SELECT JobCardId FROM JobCardQC);
                               SELECT JCQ.JobCardId JobCardId,JCQ.JobCardNo JobCardNo,JCQ.JobCardDate JobCardDate,JCQ.SaleOrderId SaleOrderId,JCQ.SaleOrderItemId SaleOrderItemId,SO.CustomerId CustomerId,SOI.VehicleModelId VehicleModelId INTO #JOBCARD_vs_SALERORDER
                               FROM #JOBCARDvsQC JCQ LEFT JOIN SaleOrder SO ON JCQ.SaleOrderId=SO.SaleOrderId
							   LEFT JOIN SaleOrderItem SOI ON JCQ.SaleOrderItemId=SOI.SaleOrderItemId;
                               SELECT JVS.JobCardId JobCardId,JVS.JobCardNo JobCardNo,JVS.JobCardDate JobCardDate,JVS.SaleOrderId SaleOrderId,JVS.SaleOrderItemId SaleOrderItemId,JVS.CustomerId CustomerId,JVS.VehicleModelId VehicleModelId,C.CustomerName,/*CONCAT(VM.VehicleModelName,'  ',VM.VehicleModelDescription)*/ VM.VehicleModelDescription VehicleModelName
                               FROM #JOBCARD_vs_SALERORDER JVS LEFT JOIN Customer C ON JVS.CustomerId=C.CustomerId
						       LEFT JOIN VehicleModel VM ON JVS.VehicleModelId=VM.VehicleModelId;
                               DROP TABLE #JobCard;
                               DROP TABLE #JOBCARDvsQC;
                               DROP TABLE #JOBCARD_vs_SALERORDER;";
                return connection.Query<PendingJobCardQC>(sql);
            }
        }
        public JobCardQC GetJobCardQC(int JobCardQCId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT JobCardQCId,JobCardQCRefNo,J.JobCardNo,J.JobCardDate JcDate,JQ.JobCardId,
                                C.CustomerName Customer,V.VehicleModelName VehicleModel,
                                JQ.EmployeeId,JobCardQCDate,IsQCPassed,ISNULL(DC.JobCardId,0)IsUsed 
                                FROM JobCardQC JQ
                                INNER JOIN JobCard J ON J.JobCardId=JQ.JobCardId
                                INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                INNER JOIN SaleOrderItem SI ON SI.SaleOrderId=S.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
                                LEFT JOIN VehicleModel V ON V.VehicleModelId=SI.VehicleModelId
                                LEFT JOIN DeliveryChallan DC ON DC.JobCardId=JQ.JobCardId
                                WHERE JobCardQCId=@JobCardQCId";

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

        public int UpdateJobCardQC(JobCardQC objJobCardQC)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string sql = @"UPDATE
                                JobCardQC SET JobCardQCRefNo=@JobCardQCRefNo,EmployeeId=@EmployeeId,
                                JobCardQCDate=@JobCardQCDate,IsQCPassed=@IsQCPassed,CreatedBy=@CreatedBy,
                                CreatedDate=@CreatedDate,OrganizationId=@OrganizationId
                                WHERE JobCardId = @JobCardId;";
                try
                {
                    var id = connection.Execute(sql, objJobCardQC, txn);

                    int i = 0;

                    var JobCardQCParamRepo = new JobCardQCParamRepository();

                    foreach (var item in objJobCardQC.JobCardQCParams)
                    {
                        item.JobCardQCId = objJobCardQC.JobCardQCId;
                        JobCardQCParamRepo.InsertSaleOrderItem(item, connection, txn);
                    }

                    InsertLoginHistory(dataConnection, objJobCardQC.CreatedBy, "Update", "Project Completion", id.ToString(), objJobCardQC.OrganizationId.ToString());
                    txn.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }

        public string DeleteJobCardQC(int JobCardQCId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM JobCardQCParam WHERE JobCardQCId = @JobCardQCId;
                                     DELETE FROM JobCardQC OUTPUT deleted.JobCardQCRefNo WHERE JobCardQCId = @JobCardQCId;";
                    string output = connection.Query<string>(query, new { JobCardQCId = JobCardQCId }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
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
                return connection.Query<Dropdown>("SELECT EmployeeName Name,EmployeeId Id FROM employee where isActive=1").ToList();

            }
        }
        public IEnumerable<JobCardQC> GetPreviousList( int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)

        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @" select JobCardQCId,JobCardQCRefNo,JobCardQCDate ,JobCardNo,JobCardDate,
                                CASE WHEN IsQCPassed=1 THEN 'PASSED' ELSE 'FAIL' END QCStatus  
                                from JobCardQC J 
                                inner join JobCard JC ON J.JobCardId=JC.JobCardId
                                where J.isActive=1 and J.OrganizationId = @OrganizationId 
                                and  J.JobCardQCId = ISNULL(NULLIF(@id, 0), J.JobCardQCId)  
                                AND Convert(date,J.JobCardQCDate,106) BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                ORDER BY J.JobCardQCDate Desc";
                return connection.Query<JobCardQC>(qry, new { id = id, cusid = cusid, from = from, to = to, OrganizationId = OrganizationId}).ToList();

            }
        }


        public JobCardQC GetJobCardQCHDPrint(int JobCardQCId,int organizationId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT O.*, JobCardQCId,JobCardQCRefNo,J.JobCardNo,J.JobCardDate JcDate,JQ.JobCardId,
                                C.CustomerName Customer,V.VehicleModelName VehicleModel,Employeename,
                                JobCardQCDate,IsQCPassed,ISNULL(DC.JobCardId,0)IsUsed 
                                FROM JobCardQC JQ
						        Left JOIN Employee E ON E.EmployeeId=JQ.EmployeeId
                                INNER JOIN JobCard J ON J.JobCardId=JQ.JobCardId
                                INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                INNER JOIN SaleOrderItem SI ON SI.SaleOrderId=S.SaleOrderId
                                INNER JOIN Customer C ON C.CustomerId=S.CustomerId
								INNER JOIN Organization O ON O.OrganizationId=JQ.OrganizationId
                                LEFT JOIN VehicleModel V ON V.VehicleModelId=SI.VehicleModelId
                                LEFT JOIN DeliveryChallan DC ON DC.JobCardId=JQ.JobCardId
                                WHERE JobCardQCId=@JobCardQCId";

                var objJobCardQC = connection.Query<JobCardQC>(sql, new
                {
                    JobCardQCId = JobCardQCId,
                    organizationId = organizationId
                }).First<JobCardQC>();

                return objJobCardQC;
            }
        }

    }
}