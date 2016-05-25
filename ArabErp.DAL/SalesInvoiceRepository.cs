using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SalesInvoiceRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public int InsertSalesInvoice(SalesInvoice objSalesInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into SalesInvoice(SalesInvoiceDate,JobCardId,SpecialRemarks,PaymentTerms,CreatedBy,CreatedDate) Values (@SalesInvoiceDate,@JobCardId,@SpecialRemarks,@PaymentTerms,@CreatedBy,@CreatedDate);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSalesInvoice).Single();
                return id;
            }
        }

        public SalesInvoice GetSalesInvoice(int SalesInvoiceId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from SalesInvoice
                        where SalesInvoiceId=@SalesInvoiceId";

                var objSalesInvoice = connection.Query<SalesInvoice>(sql, new
                {
                    SalesInvoiceId = SalesInvoiceId
                }).First<SalesInvoice>();

                return objSalesInvoice;
            }
        }

        public List<SalesInvoice> GetSalesInvoices()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesInvoice
                        where isActive=1";

                var objSalesInvoices = connection.Query<SalesInvoice>(sql).ToList<SalesInvoice>();

                return objSalesInvoices;
            }
        }

        public int UpdateSalesInvoice(SalesInvoice objSalesInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SalesInvoice SET SalesInvoiceDate = @SalesInvoiceDate ,JobCardId = @JobCardId ,SpecialRemarks = @SpecialRemarks ,PaymentTerms = @PaymentTerms,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,OrganizationId = @OrganizationId  OUTPUT INSERTED.SalesInvoiceId  WHERE SalesInvoiceId = @SalesInvoiceId";


                var id = connection.Execute(sql, objSalesInvoice);
                return id;
            }
        }

        public int DeleteSalesInvoice(Unit objSalesInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SalesInvoice  OUTPUT DELETED.SalesInvoiceId WHERE SalesInvoiceId=@SalesInvoiceId";


                var id = connection.Execute(sql, objSalesInvoice);
                return id;
            }
        }


    }
}