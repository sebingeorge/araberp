using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class ProformaInvoiceRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public IEnumerable<PendingSO> GetSaleOrdersForPerforma(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT  DISTINCT t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(10)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing,DATEDIFF(dd,GETDATE (),SO.EDateDelivery)Remaindays,
                             SO.TotalAmount
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join SalesInvoice SI on SO.SaleOrderId=SI.SaleOrderId 
                             WHERE SI.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1  AND SO.isProjectBased = " + isProjectBased.ToString() + @" order by SO.SaleOrderDate ASC";

                return connection.Query<PendingSO>(query);
            }
        }

        public ProformaInvoice GetSaleOrderForPorforma(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT  SO.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,DoorNo +','+ Street+','+State CustomerAddress,SO.SaleOrderDate SaleOrderDate,
                                SO.SaleOrderRefNo +','+ Replace(Convert(varchar,SaleOrderDate,106),' ','/') SaleOrderRefNo,SO.isProjectBased
                                FROM  SaleOrder SO  INNER JOIN Customer C  ON SO.CustomerId =C.CustomerId
                                WHERE SO.SaleOrderId =@SaleOrderId";
                var objSaleOrders = connection.Query<ProformaInvoice>(sql, new { SaleOrderId = SaleOrderId }).Single<ProformaInvoice>();
                return objSaleOrders;
            }
        }
        public List<ProformaInvoiceItem> GetPorformaInvoiceData(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = @"SELECT s.SaleOrderId ,S.SaleOrderItemId, W.WorkDescr WorkDescription,v.VehicleModelName,S.Quantity,U.UnitName,S.Rate,S.Discount,S.Amount
                            FROM SaleOrderItem S LEFT JOIN Unit U on S.UnitId=U.UnitId
                            LEFT JOIN WorkDescription W on S.WorkDescriptionId=W.WorkDescriptionId
                            Left JOIN VehicleModel V on S.VehicleModelId=V.VehicleModelId
                            WHERE S.SaleOrderId=@SaleOrderId";
                return connection.Query<ProformaInvoiceItem>(query,
                new { SaleOrderId = SaleOrderId }).ToList();
            }
        }
        public string InsertProformaInvoice(ProformaInvoice objProInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(ProformaInvoice).Name, "0", 1);

                    objProInvoice.ProformaInvoiceRefNo = "PINV/" + internalId;

                    string sql = @"
                    insert  into ProformaInvoice(ProformaInvoiceRefNo,ProformaInvoiceDate,SaleOrderId,SpecialRemarks,PaymentTerms,CreatedBy,CreatedDate,OrganizationId,isProjectBased)
                                   Values (@ProformaInvoiceRefNo,@ProformaInvoiceDate,@SaleOrderId,@SpecialRemarks,@PaymentTerms,@CreatedBy,@CreatedDate,@OrganizationId,@isProjectBased);
                    SELECT CAST(SCOPE_IDENTITY() as int) ProformaInvoiceId";



                    var id = connection.Query<int>(sql, objProInvoice, txn).Single();

                    foreach (ProformaInvoiceItem item in objProInvoice.Items)
                    {
                        item.ProformaInvoiceId = id;
                        new ProformaInvoiceItemRepository().InsertProformaInvoiceItem(item, connection, txn);
                    }
                    InsertLoginHistory(dataConnection, objProInvoice.CreatedBy, "Create", "Proforma Invoice", id.ToString(), "0");
                    txn.Commit();

                    return id + "|PINV/" + internalId;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "0";
                }
            }
        }

        public IList<ProformaInvoice> PreviousList(int OrganizationId, int type, DateTime? from, DateTime? to, int customer, int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = @"SELECT
	                                PRO.ProformaInvoiceId,
                                    PRO.[ProformaInvoiceRefNo],
	                                CONVERT(DATETIME, PRO.ProformaInvoiceDate, 106) ProformaInvoiceDate,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                CUS.CustomerName,
	                                PRO.SpecialRemarks
                                FROM ProformaInvoice PRO
	                                INNER JOIN SaleOrder SO ON PRO.SaleOrderId = SO.SaleOrderId
	                                INNER JOIN Customer CUS ON SO.CustomerId = CUS.CustomerId
                                WHERE PRO.OrganizationId = @OrganizationId
                                    AND PRO.isActive = 1
                                    AND PRO.isProjectBased = @type
                                    AND CONVERT(DATE, PRO.ProformaInvoiceDate, 106) BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())
                                    AND PRO.ProformaInvoiceId = ISNULL(NULLIF(CAST(@id AS INT), 0), PRO.ProformaInvoiceId)
                                    AND SO.CustomerId = ISNULL(NULLIF(CAST(@customer AS INT), 0), SO.CustomerId)
                                    ORDER BY PRO.ProformaInvoiceDate DESC, PRO.CreatedDate DESC";
                return connection.Query<ProformaInvoice>(query, new
                {
                    OrganizationId = OrganizationId,
                    type = type,
                    from = from,
                    to = to,
                    customer = customer,
                    id = id
                }).ToList();
            }
        }

    }
}
