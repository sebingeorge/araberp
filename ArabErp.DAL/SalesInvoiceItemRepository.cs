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
        public List<SalesInvoiceItem> GetSalesInvoiceItemforPrint(int SalesInvoiceId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" select W.WorkDescriptionRefNo,W.WorkDescr WorkDescription,SI.Amount,SI.Rate,SI.Quantity,UnitName Unit from SalesInvoiceItem SI inner join SalesInvoice S ON SI.SalesInvoiceId=S.SalesInvoiceId
                                inner join SaleOrderItem SII ON SII.SaleOrderItemId=SI.SaleOrderItemId
                                LEFT JOIN WorkDescription W ON W.WorkDescriptionId=SII.WorkDescriptionId
                                LEFT JOIN UNIT U ON U.UnitId=SII.UnitId
                                where SI.SalesInvoiceId=@SalesInvoiceId";
                               
                return connection.Query<SalesInvoiceItem>(sql, new { SalesInvoiceId = SalesInvoiceId }).ToList();

               
            }
        }

        public List<SalesInvoiceItem> GetSelectedSalesInvoiceDT(List<int> salesorderitemid,int saleorderid)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var objSalesInvoiceDT = new List<SalesInvoiceItem>();
                try
                {
                    string sql = @"SELECT * INTO #SaleOrderItem FROM SaleOrderItem WHERE SaleOrderId=@SaleOrderId AND SaleOrderItemId IN @SaleOrderItemIdList
                               SELECT s.SaleOrderId SaleOrderId,S.SaleOrderItemId SaleOrderItemId, W.WorkDescr WorkDescription, V.VehicleModelName, S.Quantity QuantityTxt,U.UnitName Unit,S.Rate Rate,S.Discount Discount,S.Amount Amount,j.JobCardId JobCardId
                               FROM #SaleOrderItem S LEFT JOIN Unit U on S.UnitId=U.UnitId
                               LEFT JOIN WorkDescription W on S.WorkDescriptionId=W.WorkDescriptionId
                               Left JOIN VehicleModel V on S.VehicleModelId=V.VehicleModelId
							   left join JobCard J on J.SaleOrderItemId=S.SaleOrderItemId
                               DROP TABLE #SaleOrderItem";

                     objSalesInvoiceDT = connection.Query<SalesInvoiceItem>(sql, new { SaleOrderItemIdList = salesorderitemid, SaleOrderId = saleorderid }).ToList<SalesInvoiceItem>();
                }
                catch (Exception ex)
                    {

                    }

                return objSalesInvoiceDT;
            }
        }
        public int InsertSalesInvoiceItem(SalesInvoiceItem objSalesInvoiceItem, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"INSERT INTO SalesInvoiceItem(SalesInvoiceId,SaleOrderItemId,JobCardId,Quantity,Rate,Discount,Amount) VALUES (@SalesInvoiceId,@SaleOrderItemId,@JobCardId,@QuantityTxt,@Rate,@Discount,@Amount);
                                    SELECT CAST(SCOPE_IDENTITY() as int) ";


                var id = connection.Query<int>(sql, objSalesInvoiceItem, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
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


        public string DeleteSalesInvoiceItem(int SalesInvoiceId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"Delete SalesInvoiceItem  OUTPUT DELETED.SalesInvoiceId WHERE SalesInvoiceId=@SalesInvoiceId;";
                    string output = connection.Query<string>(query, new { SalesInvoiceId = SalesInvoiceId }, txn).First();
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

    }
}
