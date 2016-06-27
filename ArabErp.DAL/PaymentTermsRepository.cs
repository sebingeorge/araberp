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

        public int InsertPaymentTerms(PaymentTerms objPaymentTerms)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"INSERT INTO PaymentTerms (PaymentTermsRefNo,PaymentTermsName,CreatedBy,CreatedDate,OrganizationId,isActive) VALUES(@PaymentTermsRefNo,@PaymentTermsName,@CreatedBy,getDate(),@OrganizationId,1);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, objPaymentTerms).Single();
                return id;
            }
        }

        public IEnumerable<PaymentTerms> FillPaymentTermsList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<PaymentTerms>("SELECT PaymentTermsId,PaymentTermsRefNo,PaymentTermsName FROM PaymentTerms").ToList();
            }
        }

    }
}
