using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class ProformaInvoiceItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertProformaInvoiceItem(ProformaInvoiceItem objProInvoiceItem, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"insert  into ProformaInvoiceItem(ProformaInvoiceId,SaleOrderItemId,Rate,Discount,Amount) 
                                                    Values (@ProformaInvoiceId,@SaleOrderItemId,@Rate,@Discount,@Amount);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objProInvoiceItem, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}


