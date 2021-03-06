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
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<PendingSO> GetSaleOrdersForPerforma(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT  DISTINCT t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT DISTINCT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()]
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
                string sql = @"SELECT 
	                                SO.SaleOrderId,
	                                SO.CustomerOrderRef,
	                                SO.SaleOrderRefNo,
	                                SO.EDateArrival,
	                                SO.EDateDelivery,
	                                SO.CustomerId,
	                                C.CustomerName,
	                                ISNULL(DoorNo,'') +', '+ ISNULL(Street,'')+', '+ISNULL([State],'') CustomerAddress,
	                                Convert(varchar,SaleOrderDate,106) SaleOrderDate,
	                                SO.SaleOrderRefNo,
	                                SO.isProjectBased,
	                                SO.PaymentTerms,
									SYM.SymbolName
                                FROM  SaleOrder SO 
									INNER JOIN Customer C  ON SO.CustomerId = C.CustomerId
									INNER JOIN Currency CUR ON C.CurrencyId = CUR.CurrencyId
									INNER JOIN Symbol SYM ON CUR.CurrencySymbolId = SYM.SymbolId
                                WHERE SO.SaleOrderId = @SaleOrderId";
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

                    if (objProInvoice.isProjectBased==1)
                    {
                        #region delete and inserting print description
                        try
                        {
                            sql = @"DELETE FROM PrintDescription WHERE SaleOrderId = " + objProInvoice.SaleOrderId;
                            connection.Execute(sql, transaction: txn);
                            sql = @"INSERT INTO PrintDescription (SaleOrderId, Description, UoM, Quantity, PriceEach, Amount, CreatedBy, CreatedDate, OrganizationId)
                                    VALUES (@SaleOrderId, @Description, @UoM, @Quantity, @PriceEach, @Amount, @CreatedBy, GETDATE(), @OrganizationId)";
                            foreach (var item in objProInvoice.PrintDescriptions)
                            {
                                if (item.Description == null) continue;
                                item.SaleOrderId = objProInvoice.SaleOrderId;
                                item.CreatedBy = int.Parse(objProInvoice.CreatedBy);
                                item.OrganizationId = objProInvoice.OrganizationId;
                                item.Amount = (item.Quantity ?? 0) * item.PriceEach;
                                if (connection.Execute(sql, item, txn) <= 0) throw new Exception();
                            }
                        }
                        catch (Exception) { throw new Exception(); }
                        #endregion
                    }

                    InsertLoginHistory(dataConnection, objProInvoice.CreatedBy, "Create", "Proforma Invoice", id.ToString(), "0");
                    txn.Commit();

                    return id + "|" + internalId;
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
                               ProformaInvoiceDate,P.SaleOrderId,P.SpecialRemarks,P.PaymentTerms,P.isProjectBased,
							   S.SaleOrderRefNo, CONVERT(VARCHAR, S.SaleOrderDate, 106)SaleOrderDate, SYM.SymbolName from  ProformaInvoice P
                               inner join SaleOrder S on S.SaleOrderId=P.SaleOrderId
                               inner join Customer C ON C.CustomerId=S.CustomerId
								INNER JOIN Currency CUR ON C.CurrencyId = CUR.CurrencyId
								INNER JOIN Symbol SYM ON CUR.CurrencySymbolId = SYM.SymbolId where ProformaInvoiceId = @Id";

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

        public int DeleteProformaInvoiceDT(int Id)
        {
            int result2 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM ProformaInvoiceItem WHERE ProformaInvoiceId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

        public string UpdateProformaInvoiceHD(ProformaInvoice objProInvoice)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
         
            {
                IDbTransaction txn = connection.BeginTransaction();
                string sql = @" UPDATE ProformaInvoice SET ProformaInvoiceRefNo=@ProformaInvoiceRefNo,ProformaInvoiceDate = @ProformaInvoiceDate ,
                                SaleOrderId =@SaleOrderId ,SpecialRemarks = @SpecialRemarks ,PaymentTerms=@PaymentTerms,CreatedBy = @CreatedBy,
                                CreatedDate = @CreatedDate,OrganizationId=@OrganizationId,isProjectBased=@isProjectBased
                                WHERE ProformaInvoiceId = @ProformaInvoiceId";

                var id = connection.Execute(sql, objProInvoice, txn);


                if (objProInvoice.isProjectBased == 1)
                {
                    #region inserting print description
                    try
                    {
                        sql = @"DELETE FROM PrintDescription WHERE SaleOrderId = " + objProInvoice.SaleOrderId;
                        connection.Execute(sql, transaction: txn);
                        sql = @"INSERT INTO PrintDescription (SaleOrderId, Description, UoM, Quantity, PriceEach, Amount, CreatedBy, CreatedDate, OrganizationId)
                                    VALUES (@SaleOrderId, @Description, @UoM, @Quantity, @PriceEach, @Amount, @CreatedBy, GETDATE(), @OrganizationId)";
                        foreach (var item in objProInvoice.PrintDescriptions)
                        {
                            if (item.Description == null) continue;
                            item.SaleOrderId = objProInvoice.SaleOrderId;
                            item.CreatedBy = int.Parse(objProInvoice.CreatedBy);
                            item.OrganizationId = objProInvoice.OrganizationId;
                            item.Amount = (item.Quantity ?? 0) * item.PriceEach;
                            if (connection.Execute(sql, item, txn) <= 0) throw new Exception();
                        }
                    }
                    catch (Exception) { throw new Exception(); }
                    #endregion
                }
                //InsertLoginHistory(dataConnection, objSupplyOrder.CreatedBy, "Update", "LPO", id.ToString(), "0");
                txn.Commit();
                //return id;
                return objProInvoice.ProformaInvoiceRefNo;
            }
        }

        public string InsertProformaInvoiceDT(ProformaInvoice objProInvoice)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var ProformaInvoiceItemrepo = new ProformaInvoiceItemRepository();
                    foreach (var item in objProInvoice.Items)
                    //foreach (ProformaInvoiceItem item in objProInvoice.Items)
                    {
                        item.ProformaInvoiceId = objProInvoice.ProformaInvoiceId;
                        ProformaInvoiceItemrepo.InsertProformaInvoiceItem(item, connection, trn);
                    }
                    InsertLoginHistory(dataConnection, objProInvoice.CreatedBy, "Update", "Proforma Invoice", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    trn.Rollback();
                    throw;
                }
                return objProInvoice.ProformaInvoiceRefNo;
            }
        }

        public ProformaInvoice GetProformaInvoiceHD(int ProformaInvoiceId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"SELECT O.*,C.CustomerName,Concat(C.DoorNo,',',C.Street,',',C.Phone)CustomerAddress,S.CustomerOrderRef,Concat(C.DoorNo,',',C.Street,',',C.State,',',C.Country,',',C.Zip)CustomerAddress,ProformaInvoiceId,ProformaInvoiceRefNo,
                               ProformaInvoiceDate,P.SaleOrderId,P.SpecialRemarks,P.PaymentTerms,P.isProjectBased,S.SaleOrderRefNo,U.UserName CreateUser,U.Signature CreateSig,DS.DesignationName CreatedDes,CU.CurrencyName,ORR.CountryName
                               FROM  ProformaInvoice P
                               INNER JOIN SaleOrder S ON S.SaleOrderId=P.SaleOrderId
                               INNER JOIN Customer C ON C.CustomerId=S.CustomerId
							   left join Organization O ON P.OrganizationId=o.OrganizationId 
							   left join [User] U ON U.UserId=P.CreatedBy
							   	left JOIN Currency CU ON CU.CurrencyId=O.CurrencyId 
								left  JOIN Country ORR ON ORR.CountryId=O.Country
							   left join Designation DS ON DS.DesignationId=U.DesignationId where ProformaInvoiceId =@ProformaInvoiceId";

                var objProInvoice = connection.Query<ProformaInvoice>(sql, new
                {
                    ProformaInvoiceId = ProformaInvoiceId
                }).First<ProformaInvoice>();

                return objProInvoice;
            }
        }
        public List<PrintDescription> GetPrintDescriptions(int? SaleOrderItemIds)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"	SELECT
	                            PD.*
                                FROM PrintDescription PD 
								inner join ProformaInvoice PF on PD.SaleOrderId=@ids";
                    return connection.Query<PrintDescription>(sql, new { ids = SaleOrderItemIds }).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<PrintDescription> GetPrintDescription(int? SaleOrderItemIds)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"	select * from PrintDescription PD
                                    inner join  ProformaInvoice PI ON PI.SaleOrderId=PD.SaleOrderId
                                    where ProformaInvoiceId=@ids";
                    return connection.Query<PrintDescription>(sql, new { ids = SaleOrderItemIds }).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
