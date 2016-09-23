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
                string sql = @"SELECT QCParamId,QCParamName,QCParaName ParaName 
                               FROM QCParam 
                               INNER JOIN QCParaType on QCParam.QCParaId=QCParaType.QCParaId
                               ORDER BY QCParamId ";

                var objSalesInvoices = connection.Query<JobCardQCParam>(sql).ToList<JobCardQCParam>();

                return objSalesInvoices;
            }
        }

        public List<JobCardQCParam> GetJobCardQCParamDt(int JobCardQCId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT QP.QCParamId,QT.QCParaName ParaName,QP.QCParamName,QCParamValue FROM JobCardQCParam JQC
                                INNER JOIN QCParam QP ON QP.QCParamId=JQC.QCParamId
                                INNER JOIN QCParaType QT ON QT.QCParaId=QP.QCParaId
                                WHERE JQC.JobCardQCId=@JobCardQCId
                                ORDER BY QP.QCParamId";

                var objSalesInvoices = connection.Query<JobCardQCParam>(sql, new { JobCardQCId = JobCardQCId }).ToList<JobCardQCParam>();

                return objSalesInvoices;
            }
        }

    }
}