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
        public List<SalesInvoice> GetSalesInvoiceCustomerList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT DISTINCT C.CustomerName Customer, SO.SaleOrderId SaleOrderId,CONCAT(SO.SaleOrderRefNo,'/',Convert(varchar(15),SO.SaleOrderDate,106 )) as SaleOrderRefNoWithDate
                            FROM SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId
							LEFT JOIN SalesInvoiceItem SII ON SOI.SaleOrderItemId=SII.SaleOrderItemId
							LEFT JOIN JobCard JC ON JC.SaleOrderId=SO.SaleOrderId
							LEFT JOIN Customer C ON C.CustomerId=SO.CustomerId
							WHERE SII.SalesInvoiceId IS NULL AND JC.JodCardCompleteStatus=1 AND SO.isActive=1
							AND SO.isActive=1
							AND JC.isActive=1
							AND C.isActive=1
							AND SOI.isActive=1";

                var objSalesInvoices = connection.Query<SalesInvoice>(sql).ToList<SalesInvoice>();

                return objSalesInvoices;
            }
        }
        public List<SalesInvoiceItem> GetPendingSalesInvoiceList(int SaleOrderId)
        {
          //  int salesOrderId = Convert.ToInt32(SalesOrderId);
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<SalesInvoiceItem>("SELECT * INTO #SaleOrder FROM SaleOrder WHERE SaleOrderId=@SaleOrderId AND isActive=1;SELECT SO.SaleOrderId SaleOrderId,SOI.SaleOrderItemId SaleOrderItemId,SOI.Quantity Quantity,SOI.Rate Rate,SOI.Amount Amount,SOI.VehicleModelId INTO #TEMP_ORDER FROM #SaleOrder SO LEFT JOIN SaleOrderItem SOI ON SO.SaleOrderId=SOI.SaleOrderId;SELECT * INTO #SalesInvoice FROM SalesInvoice WHERE SaleOrderId=@SaleOrderId AND isActive=1;SELECT SI.SaleOrderId,SII.SaleOrderItemId INTO #TEMP_INVOICE FROM #SalesInvoice SI LEFT JOIN SalesInvoiceItem SII ON SI.SalesInvoiceId=SII.SalesInvoiceId;SELECT O.SaleOrderId,O.SaleOrderItemId,O.Quantity,O.Rate,O.Amount,O.VehicleModelId INTO #RESULT FROM #TEMP_ORDER O LEFT JOIN #TEMP_INVOICE I ON O.SaleOrderId=I.SaleOrderId AND O.SaleOrderItemId=I.SaleOrderItemId WHERE I.SaleOrderId IS NULL AND I.SaleOrderItemId IS NULL;SELECT R.SaleOrderId SaleOrderId,R.SaleOrderItemId SaleOrderItemId,R.Quantity Quantity,R.Rate Rate,r.Amount Amount,CONCAT(V.VehicleModelName,'',VehicleModelDescription) VehicleModelName FROM #RESULT R LEFT JOIN VehicleModel V ON R.VehicleModelId=V.VehicleModelId;DROP TABLE #RESULT;DROP TABLE #SaleOrder;DROP TABLE #SalesInvoice;DROP TABLE #TEMP_INVOICE;DROP TABLE #TEMP_ORDER;", new { SaleOrderId = SaleOrderId }).ToList();
            }
        }


    }
}