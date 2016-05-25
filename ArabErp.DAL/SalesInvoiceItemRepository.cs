using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SalesInvoiceItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");



        public int InsertSalesInvoiceItem(SalesInvoiceItem objSalesInvoiceItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into SalesInvoiceItem(SalesInvoiceId,WorkDescription,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,Rate,Discount,Amount,CreatedBy,CreatedDate,OrganizationId) Values (@SalesInvoiceId,@WorkDescription,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@Rate,@Discount,@Amount,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objSalesInvoiceItem).Single();
                return id;
            }
        }

        public SalesInvoiceItem GetSalesInvoiceItem(int SalesInvoiceItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesInvoiceItem
                        where SalesInvoiceItemId=@SalesInvoiceItemId";

                var objSalesInvoiceItem = connection.Query<SalesInvoiceItem>(sql, new
                {
                    SalesInvoiceItemId = SalesInvoiceItemId
                }).First<SalesInvoiceItem>();

                return objSalesInvoiceItem;
            }
        }

        public List<SalesInvoiceItem> GetSalesInvoiceItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesInvoiceItem
                        where isActive=1";

                var objSalesInvoiceItems = connection.Query<SalesInvoiceItem>(sql).ToList<SalesInvoiceItem>();

                return objSalesInvoiceItems;
            }
        }

        public int UpdateSalesInvoiceItem(SalesInvoiceItem objSalesInvoiceItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SalesInvoiceItem SET SalesInvoiceId = @SalesInvoiceId ,WorkDescription = @WorkDescription ,SlNo = @SlNo ,ItemId = @ItemId,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,Rate = @Rate  OUTPUT INSERTED.SalesInvoiceItemId  WHERE SalesInvoiceItemId = @SalesInvoiceItemId";


                var id = connection.Execute(sql, objSalesInvoiceItem);
                return id;
            }
        }

        public int DeleteSalesInvoiceItem(Unit objSalesInvoiceItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SalesInvoiceItem  OUTPUT DELETED.SalesInvoiceItemId WHERE SalesInvoiceItemId=@SalesInvoiceItemId";


                var id = connection.Execute(sql, objSalesInvoiceItem);
                return id;
            }
        }


    }
}
