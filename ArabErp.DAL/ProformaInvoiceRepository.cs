﻿using System;
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
                             WHERE SI.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1  AND SO.isProjectBased = " + isProjectBased.ToString() + @" AND SO.SaleOrderId NOT IN (SELECT SaleOrderId FROM ProformaInvoice) order by SO.SaleOrderDate ASC";

                return connection.Query<PendingSO>(query);
            }
        }

        public ProformaInvoice GetSaleOrderForPorforma(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT  SO.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,DoorNo +','+ Street+','+State CustomerAddress,SO.SaleOrderDate SaleOrderDate,
                                SO.SaleOrderRefNo +','+ Replace(Convert(varchar,SaleOrderDate,106),' ','/') SaleOrderRefNo,SO.isProjectBased,SO.PaymentTerms
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
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objProInvoice.OrganizationId, 6, true, txn);

                    objProInvoice.ProformaInvoiceRefNo = internalId;

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

        public ProformaInvoice GetProformaInvoiceHdDetails(int Id)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select C.CustomerName,S.CustomerOrderRef,Concat(C.DoorNo,',',C.Street,',',C.State,',',C.Country,',',C.Zip)CustomerAddress,ProformaInvoiceId,ProformaInvoiceRefNo,
                               ProformaInvoiceDate,P.SaleOrderId,P.SpecialRemarks,P.PaymentTerms,P.isProjectBased from  ProformaInvoice P
                               inner join SaleOrder S on S.SaleOrderId=P.SaleOrderId
                               inner join Customer C ON C.CustomerId=S.CustomerId where ProformaInvoiceId =@Id";

                var objProformaInvoice = connection.Query<ProformaInvoice>(sql, new
                {
                    Id = Id

                }).First<ProformaInvoice>();

                return objProformaInvoice;
            }
        }

        public List<ProformaInvoiceItem> GetProformaInvoiceItemDetails(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT V.VehicleModelName,W.WorkDescr WorkDescription,ProformaInvoiceItemId,ProformaInvoiceId,P.SaleOrderItemId,S.Quantity,P.Rate,P.Discount,P.Amount,u.UnitName
                                FROM ProformaInvoiceItem P
                                inner join SaleOrderItem S ON P.SaleOrderItemId=S.SaleOrderItemId
                                left join WorkDescription W on S.WorkDescriptionId=W.WorkDescriptionId
                                left join VehicleModel V on V.VehicleModelId=S.VehicleModelId
                                left join Unit u on u.UnitId=s.UnitId where ProformaInvoiceId =@Id";



                var objProInvoiceItem = connection.Query<ProformaInvoiceItem>(sql, new { Id = Id }).ToList<ProformaInvoiceItem>();

                return objProInvoiceItem;
            }
        }
    }
}
