using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SalesInvoiceRepository : BaseRepository
    {

        public int InsertSalesInvoice(SalesInvoice objSalesInvoice)
        {
            string sql = @"insert  into SalesInvoice(SalesInvoiceDate,JobCardId,SpecialRemarks,PaymentTerms,CreatedBy,CreatedDate) Values (@SalesInvoiceDate,@JobCardId,@SpecialRemarks,@PaymentTerms,@CreatedBy,@CreatedDate);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSalesInvoice).Single();
            return id;
        }


        public SalesInvoice GetSalesInvoice(int SalesInvoiceId)
        {

            string sql = @"select * from SalesInvoice
                        where SalesInvoiceId=@SalesInvoiceId";

            var objSalesInvoice = connection.Query<SalesInvoice>(sql, new
            {
                SalesInvoiceId = SalesInvoiceId
            }).First<SalesInvoice>();

            return objSalesInvoice;
        }

        public List<SalesInvoice> GetSalesInvoices()
        {
            string sql = @"select * from SalesInvoice
                        where isActive=1";

            var objSalesInvoices = connection.Query<SalesInvoice>(sql).ToList<SalesInvoice>();

            return objSalesInvoices;
        }

        public int UpdateSalesInvoice(SalesInvoice objSalesInvoice)
        {
            string sql = @"UPDATE SalesInvoice SET SalesInvoiceDate = @SalesInvoiceDate ,JobCardId = @JobCardId ,SpecialRemarks = @SpecialRemarks ,PaymentTerms = @PaymentTerms,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,OrganizationId = @OrganizationId  OUTPUT INSERTED.SalesInvoiceId  WHERE SalesInvoiceId = @SalesInvoiceId";


            var id = connection.Execute(sql, objSalesInvoice);
            return id;
        }

        public int DeleteSalesInvoice(Unit objSalesInvoice)
        {
            string sql = @"Delete SalesInvoice  OUTPUT DELETED.SalesInvoiceId WHERE SalesInvoiceId=@SalesInvoiceId";


            var id = connection.Execute(sql, objSalesInvoice);
            return id;
        }


    }
}