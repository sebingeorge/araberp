using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class JobCardQCParamRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public int InsertSaleOrderItem(JobCardQCParam objJobCardQCParam, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"INSERT INTO JobCardQCParam(QCParamId,JobCardQCId,QCParamValue,OrganizationId) VALUES(@QCParamId,@JobCardQCId,@QCParamValue,@OrganizationId);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objJobCardQCParam, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public JobCardQCParam GetJobCardQCParam(int JobCardQCParamId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardQCParam
                        where JobCardQCParamId=@JobCardQCParamId";

                var objJobCardQCParam = connection.Query<JobCardQCParam>(sql, new
                {
                    JobCardQCParamId = JobCardQCParamId
                }).First<JobCardQCParam>();

                return objJobCardQCParam;
            }
        }

        public List<JobCardQCParam> GetJobCardQCParams()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from JobCardQCParam
                        where isActive=1";

                var objJobCardQCParams = connection.Query<JobCardQCParam>(sql).ToList<JobCardQCParam>();

                return objJobCardQCParams;
            }
        }



        public int DeleteJobCardQCParam(Unit objJobCardQCParam)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete JobCardQCParam  OUTPUT DELETED.JobCardQCParamId WHERE JobCardQCParamId=@JobCardQCParamId";


                var id = connection.Execute(sql, objJobCardQCParam);
                return id;
            }
        }
        public List<JobCardQCParam> GetJobCardQCParamList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select QCParamId,QCParamName from QCParam";

                var objSalesInvoices = connection.Query<JobCardQCParam>(sql).ToList<JobCardQCParam>();

                return objSalesInvoices;
            }
        }


    }
}