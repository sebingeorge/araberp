using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using NUnit.Framework;
using System.Text;
using System.Collections;

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

        public SalesInvoice GetSalesInvoiceItems(List<SalesInvoiceItem> objSalesInvoiceItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                StringBuilder sb = new StringBuilder("<DATA>");

                foreach (var items in objSalesInvoiceItem)
                {
                    sb.AppendFormat("<Rows SaleOrderId={0}{1}{0}", (char)34, items.SaleOrderId);
                    sb.AppendFormat(" SaleOrderItemId={0}{1}{0}", (char)34, items.SaleOrderItemId);
                    sb.AppendFormat(" SelectStatus={0}{1}{0}></Rows>", (char)34, Convert.ToInt32(items.SelectStatus));
                }
                sb.Append("</DATA>");

                string x = sb.ToString();
                var param = new DynamicParameters();
                param.Add("@XML_DATA", sb.ToString(), dbType: DbType.Xml);
               SalesInvoice Sales_invoice=new SalesInvoice();

                Sales_invoice.SaleInvoiceItems=new List<SalesInvoiceItem>();

                Sales_invoice = connection.Query<SalesInvoice>("dbo.USP_GET_PENDING_INVOICE_HD", param, commandType: CommandType.StoredProcedure).Single();
                Sales_invoice.SaleInvoiceItems = connection.Query<SalesInvoiceItem>("dbo.[USP_GET_PENDING_INVOICE_DT]", param, commandType: CommandType.StoredProcedure).ToList();
               return Sales_invoice;
            }
        }
        public ArrayList SalesInvoice(SalesInvoice objSalesInvoiceItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                StringBuilder InvoiceItemsDT = new StringBuilder("<InvoiceItemsDT_DATA>");

                foreach (var items in objSalesInvoiceItem.SaleInvoiceItems)
                {
                    InvoiceItemsDT.AppendFormat("<InvoiceItemsDT_Rows SaleOrderId={0}{1}{0}", (char)34, items.SaleOrderId);
                    InvoiceItemsDT.AppendFormat(" SaleOrderItemId={0}{1}{0}></InvoiceItemsDT_Rows>", (char)34, items.SaleOrderItemId);
                  
                }
                InvoiceItemsDT.Append("</InvoiceItemsDT_DATA>");

                StringBuilder InvoiceItemsHD = new StringBuilder("<InvoiceItemsHD_DATA>");
                InvoiceItemsHD.AppendFormat("<InvoiceItemsHD_Rows SaleOrderId={0}{1}{0}", (char)34, objSalesInvoiceItem.SaleOrderId);
                InvoiceItemsHD.AppendFormat(" Addition={0}{1}{0}", (char)34, objSalesInvoiceItem.Addition);
                InvoiceItemsHD.AppendFormat(" AdditionRemarks={0}{1}{0}", (char)34, objSalesInvoiceItem.AdditionRemarks);
                InvoiceItemsHD.AppendFormat(" Deduction={0}{1}{0}", (char)34, objSalesInvoiceItem.Deduction);
                InvoiceItemsHD.AppendFormat(" DeductionRemarks={0}{1}{0}></InvoiceItemsHD_Rows>", (char)34, objSalesInvoiceItem.DeductionRemarks);
                InvoiceItemsHD.Append("</InvoiceItemsHD_DATA>");



                int i = 1;


                string x = InvoiceItemsHD.ToString();
                string y = InvoiceItemsDT.ToString();
                var param = new DynamicParameters();
                param.Add("@XML_DATA_InvoiceItemsHD", InvoiceItemsHD.ToString(), dbType: DbType.Xml);
                param.Add("@XML_DATA_InvoiceItemsDT", InvoiceItemsDT.ToString(), dbType: DbType.Xml);
                param.Add("@RESULT", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                param.Add("@RID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Query<int?>("dbo.[USP_SAVE_PENDING_INVOICE]", param, commandType: CommandType.StoredProcedure);

                int? rid = param.Get<int?>("@RID");
                string res = param.Get<string>("@RESULT");

                ArrayList resList = new ArrayList();
                resList.Add(rid);
                resList.Add(res);
                return resList;

                //SalesInvoice Sales_invoice = new SalesInvoice();

                //Sales_invoice.SaleInvoiceItems = new List<SalesInvoiceItem>();

                //Sales_invoice = connection.Query<SalesInvoice>("dbo.USP_GET_PENDING_INVOICE_HD", param, commandType: CommandType.StoredProcedure).Single();
                //Sales_invoice.SaleInvoiceItems = connection.Query<SalesInvoiceItem>("dbo.[USP_GET_PENDING_INVOICE_DT]", param, commandType: CommandType.StoredProcedure).ToList();
               // return i ;
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
