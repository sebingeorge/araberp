using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PaymentTermsRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public PaymentTerms InsertPaymentTerms(PaymentTerms objPaymentTerms)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new PaymentTerms();

                IDbTransaction trn = connection.BeginTransaction();
                string sql = @"INSERT INTO PaymentTerms (PaymentTermsRefNo,PaymentTermsName,CreatedBy,CreatedDate,OrganizationId,isActive)
                               VALUES(@PaymentTermsRefNo,@PaymentTermsName,@CreatedBy,getDate(),@OrganizationId,1);
                               SELECT CAST(SCOPE_IDENTITY() as int)";


                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(PaymentTerms).Name, "0", 1);
                    objPaymentTerms.PaymentTermsRefNo = "PT/" + internalid;

                    int id = connection.Query<int>(sql, objPaymentTerms, trn).Single();
                    objPaymentTerms.PaymentTermsId = id;
                    //connection.Dispose();
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    objPaymentTerms.PaymentTermsId = 0;
                    objPaymentTerms.PaymentTermsRefNo = null;

                }
                return objPaymentTerms;
            }
        }


        public IEnumerable<PaymentTerms> FillPaymentTermsList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<PaymentTerms>("SELECT PaymentTermsId,PaymentTermsRefNo,PaymentTermsName FROM PaymentTerms WHERE isActive=1").ToList();
            }
        }

        public PaymentTerms GetPaymentTerms(int PaymentTermsId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PaymentTerms
                        where PaymentTermsId=@PaymentTermsId";

                var objPaymentTerms = connection.Query<PaymentTerms>(sql, new
                {
                    PaymentTermsId = PaymentTermsId
                }).First<PaymentTerms>();

                return objPaymentTerms;
            }
        }


        public PaymentTerms UpdatePaymentTerms(PaymentTerms objPaymentTerms)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update PaymentTerms Set PaymentTermsRefNo=@PaymentTermsRefNo,PaymentTermsName=@PaymentTermsName OUTPUT INSERTED.PaymentTermsId WHERE PaymentTermsId=@PaymentTermsId";


                var id = connection.Execute(sql, objPaymentTerms);
                return objPaymentTerms;
            }
        }

        public int DeletePaymentTerms(PaymentTerms objPaymentTerms)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" Update PaymentTerms Set isActive=0 WHERE PaymentTermsId=@PaymentTermsId";
                try
                {

                    var id = connection.Execute(sql, objPaymentTerms);
                    objPaymentTerms.PaymentTermsId = id;
                    result = 0;

                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
                    {
                        switch (ex.Errors[0].Number)
                        {
                            case 547: // Foreign Key violation
                                result = 1;
                                break;

                            default:
                                result = 2;
                                break;
                        }
                    }

                }

                return result;
            }
        }


        public string GetRefNo(PaymentTerms objPaymentTerms)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string RefNo = "";
                var result = new PaymentTerms();

                IDbTransaction trn = connection.BeginTransaction();

                try
                {
                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(PaymentTerms).Name, "0", 0);
                    RefNo = "PT/" + internalid;
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }
                return RefNo;
            }
        }
    }
}
