using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;
using System.Collections;

namespace ArabErp.DAL
{
    public class SaleOrderRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        /// <summary>
        /// Insert Sale Order Details
        /// </summary>
        /// <param name="model">Object of class SaleOrder</param>
        /// <returns>Primary key of current Transaction</returns>
         public string ConnectionString()
        {
            return dataConnection;
        }
        public string InsertSaleOrder(SaleOrder objSaleOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    var internalId = "";
                    decimal MaterialAmt = 0;
                    if (objSaleOrder.isProjectBased == 0)
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objSaleOrder.OrganizationId ?? 0, 3, true, txn);
                    }
                    else
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objSaleOrder.OrganizationId ?? 0, 4, true, txn);
                    }

                    objSaleOrder.SaleOrderRefNo = internalId;

                    //if (objSaleOrder.isAfterSales == 1)
                    if (objSaleOrder.Materials != null && objSaleOrder.Materials.Count > 0)
                    {
                        MaterialAmt = objSaleOrder.Materials.Sum(m => m.Amount ?? 0);
                    }
                    objSaleOrder.TotalAmount = objSaleOrder.Items.Sum(m => m.Amount) + MaterialAmt;
                    //objSaleOrder.TotalDiscount = objSaleOrder.Items.Sum(m => m.Discount);

                    string sql = @"
                    insert  into SaleOrder(SaleOrderRefNo,SaleOrderDate,CustomerId,CustomerOrderRef,CurrencyId,SpecialRemarks,PaymentTerms,DeliveryTerms,CommissionAgentId,CommissionAmount,CommissionPerc,TotalAmount,TotalDiscount,SalesExecutiveId,EDateArrival,EDateDelivery,CreatedBy,CreatedDate,OrganizationId,SaleOrderApproveStatus,isProjectBased,isAfterSales,SalesQuotationId)
                                   Values (@SaleOrderRefNo,@SaleOrderDate,@CustomerId,@CustomerOrderRef,@CurrencyId,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@CommissionAgentId,@CommissionAmount,@CommissionPerc,@TotalAmount,@TotalDiscount,@SalesExecutiveId,@EDateArrival,@EDateDelivery,@CreatedBy,@CreatedDate,@OrganizationId,1,@isProjectBased,@isAfterSales,@SalesQuotationId);
                    SELECT CAST(SCOPE_IDENTITY() as int) SaleOrderId";

                    var id = connection.Query<int>(sql, objSaleOrder, txn).Single();

                    foreach (SaleOrderItem item in objSaleOrder.Items)
                    {
                        item.SaleOrderId = id;
                        new SaleOrderItemRepository().InsertSaleOrderItem(item, connection, txn);
                    }
                    if (objSaleOrder.Materials != null && objSaleOrder.Materials.Count > 0)
                    {
                        foreach (SalesQuotationMaterial item in objSaleOrder.Materials)
                        {
                            if (item.ItemId == null) continue;
                            item.SaleOrderId = id;
                            new SaleOrderItemRepository().InsertSaleOrderMaterial(item, connection, txn);
                        }
                    }
                    InsertLoginHistory(dataConnection, objSaleOrder.CreatedBy, "Create", "Sale Order", id.ToString(), "0");
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

        public SaleOrder GetSaleOrderFrmQuotation(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 16.1.2017 3.38p
                //string sql = @"select *,isnull(DoorNo,'') +','+ isnull(Street,'')+','+isnull(State,'') CustomerAddress,CONCAT(QuotationRefNo,' - ',CONVERT(VARCHAR(15),QuotationDate,104))QuotationNoDate,GrandTotal TotalAmount, ExpectedDeliveryDate AS EDateDelivery from SalesQuotation S inner join Customer C on S.CustomerId=C.CustomerId  where SalesQuotationId=@Id"; 
                #endregion

                string sql = @"select *,isnull(C.DoorNo,'') +','+ isnull(C.Street,'')+','+isnull(C.State,'') CustomerAddress,
                                CONCAT(QuotationRefNo,' - ',CONVERT(VARCHAR(15),QuotationDate,104))QuotationNoDate,
                                GrandTotal TotalAmount, ExpectedDeliveryDate AS EDateDelivery ,
                                CUR.CurrencyName CurrencyName
                                from SalesQuotation S 
                                inner join Customer C on S.CustomerId=C.CustomerId
                                --INNER JOIN Organization O ON S.OrganizationId = O.OrganizationId
                                INNER JOIN Currency CUR ON C.CurrencyId = CUR.CurrencyId
                                --INNER JOIN Symbol SYM ON CUR.CurrencySymbolId = SYM.SymbolId
                                where SalesQuotationId=@Id";

                var objSaleOrder = connection.Query<SaleOrder>(sql, new
                {
                    Id = Id
                }).First<SaleOrder>();

                return objSaleOrder;
            }
        }



        public List<SaleOrderItem> GetSaleOrderItemFrmQuotation(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select 
	                                S.[SalesQuotationItemId],
	                                S.[SalesQuotationId],
	                                S.[SlNo],
	                                S.[WorkDescriptionId],
	                                S.[Remarks],
	                                S.[PartNo],
	                                S.[Quantity],
	                                S.[UnitId],
	                                S.[Rate],
	                                S.[Discount],
	                                (S.Rate - S.Discount) Amount,
	                                S.[RateType],
	                                S.[OrganizationId],
	                                S.[isActive],
	                                W.*, V.*
                                from SalesQuotationItem S inner join WorkDescription W ON S.WorkDescriptionId=W.WorkDescriptionId
                                   LEFT JOIN VehicleModel V ON  V.VehicleModelId=W.VehicleModelId
                                   where SalesQuotationId=@Id";
                return connection.Query<SaleOrderItem>(sql, new { Id = Id }).ToList();
            }
        }
        public List<SalesQuotationMaterial> GetSaleOrderMaterialFrmQuotation(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SalesQuotationMaterial S inner join Item I ON S.ItemId=I.ItemId LEFT JOIN UNIT U ON I.ItemUnitId=U.UnitId where SalesQuotationId=@Id";

                return connection.Query<SalesQuotationMaterial>(sql, new { Id = Id }).ToList();
            }
        }
        public List<SalesQuotationMaterial> GetSaleOrderMaterial(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SaleOrderMaterial S inner join Item I ON S.ItemId=I.ItemId LEFT JOIN UNIT U ON I.ItemUnitId=U.UnitId where SaleOrderId=@Id";

                return connection.Query<SalesQuotationMaterial>(sql, new { Id = Id }).ToList();
            }
        }

        public SaleOrder GetSaleOrder(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"SELECT 
	                                SaleOrderId,
	                                SaleOrderRefNo,
	                                SaleOrderDate,
	                                CONCAT(QuotationRefNo,' - ',CONVERT (VARCHAR(15),QuotationDate,106))QuotationNoDate,  
                                    C.CustomerId,
	                                CustomerName,
	                                isnull(DoorNo,'') +','+ isnull(Street,'')+','+isnull(State,'') CustomerAddress,
	                                CustomerOrderRef,
	                                S.CurrencyId,
	                                SpecialRemarks,
	                                S.PaymentTerms,
	                                S.DeliveryTerms,
	                                CommissionAgentId,
	                                CommissionAmount,
	                                TotalAmount,
	                                TotalDiscount,
	                                S.SalesExecutiveId,
	                                EDateArrival,
	                                EDateDelivery,
	                                SaleOrderApproveStatus,
	                                SaleOrderHoldStatus,
	                                SaleOrderHoldReason,
	                                SaleOrderHoldDate,
	                                SaleOrderReleaseDate,
	                                S.SalesQuotationId,
	                                SaleOrderClosed,
	                                S.isProjectBased,
	                                CUR.CurrencyName,
	                                S.isAfterSales,
	                                CASE WHEN (SELECT COUNT(JobCardId) FROM JobCard WHERE SaleOrderId = @SaleOrderId) > 0
		                                THEN 1 
	                                WHEN (SELECT COUNT(SaleOrderId) FROM SaleOrder WHERE SaleOrderId = @SaleOrderId AND SaleOrderApproveStatus = 1) > 0
		                                THEN 1
	                                WHEN (SELECT COUNT(SaleOrderId) FROM WorkshopRequest WHERE SaleOrderId = @SaleOrderId) > 0
		                                THEN 1
	                                ELSE 0
	                                END isUsed
                                FROM SaleOrder S 
	                                INNER JOIN Customer C ON S.CustomerId=C.CustomerId  
	                                LEFT JOIN Currency CUR ON S.CurrencyId = CUR.CurrencyId
	                                LEFT JOIN SalesQuotation SQ ON SQ.SalesQuotationId=S.SalesQuotationId
                                WHERE SaleOrderId=@SaleOrderId";

                var objSaleOrder = connection.Query<SaleOrder>(sql, new
                {
                    SaleOrderId = SaleOrderId
                }).First<SaleOrder>();

                return objSaleOrder;
            }
        }


        public List<SaleOrder> GetSaleOrders()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select so.*,c.CustomerName, v.VehicleModelName from SaleOrder so , Customer c ,VehicleModel v  where so.CustomerId=c.CustomerId and so.VehicleModelId=v.VehicleModelId and so.isActive=1";

                var objSaleOrders = connection.Query<SaleOrder>(sql).ToList<SaleOrder>();

                return objSaleOrders;
            }
        }
        /// <summary>
        /// Saleorder Pending List for workshop request 
        /// </summary>
        /// <param name="model">Object of class SaleOrder</param>
        /// <returns>SaleOrders not in WorkshopRequest table</returns>
        public List<SaleOrder> GetSaleOrdersPendingWorkshopRequest(int OrganizationId, int isProjectBased, string saleOrder = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                #region old query 16.12.2016 7.37p
                //                string sql = @"SELECT t.SaleOrderId, t.SaleOrderItemId, SO.CustomerOrderRef,SO.SaleOrderDate,SO.SaleOrderRefNo +' - '+ Convert(varchar,SaleOrderDate,106) SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,WD.WorkDescr WorkDescription,DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing,DATEDIFF(dd,GETDATE (),SO.EDateDelivery)Remaindays 
                //                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                //                             left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId
                //	                        LEFT JOIN WorkDescription WD ON t.WorkDescriptionId = WD.WorkDescriptionId WHERE WR.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1 and SO.SaleOrderHoldStatus IS NULL and SO.OrganizationId = @OrganizationId and SO.isProjectBased=isnull(@isProjectBased,SO.isProjectBased)
                //							 AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                //							 AND ISNULL(SO.isService, 0) = 0
                //                             order by SO.EDateDelivery, SO.SaleOrderDate"; 
                #endregion

                #region old query 23.12.2016 3.36p
                //string sql = @"SELECT t.SaleOrderId, t.SaleOrderItemId,
                //                    SO.CustomerOrderRef,SO.SaleOrderDate,
                //                    SO.SaleOrderRefNo +' - '+ Convert(varchar,SaleOrderDate,106) SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,
                //                    SO.CustomerId,C.CustomerName,

                //                    WD.WorkDescr WorkDescription,

                //                    DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing,DATEDIFF(dd,GETDATE (),SO.EDateDelivery)Remaindays 
                //                    FROM SaleOrderItem t 
                //                     INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId 
                //                     INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                //                     left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId-- OR t.SaleOrderItemId = WR.SaleOrderItemId-- AND WR.SaleOrderItemId <> 0
                //                     LEFT JOIN WorkDescription WD ON t.WorkDescriptionId = WD.WorkDescriptionId
                //                    WHERE ((WR.SaleOrderItemId <> t.SaleOrderItemId AND WR.SaleOrderItemId <> 0) OR WR.SaleOrderId IS NULL)
                //                     --AND WR.SaleOrderItemId <> 0
                //                     AND SO.SaleOrderApproveStatus=1 
                //                     and SO.SaleOrderHoldStatus IS NULL 
                //                     and SO.OrganizationId = @OrganizationId
                //                        and SO.isProjectBased=isnull(@isProjectBased, SO.isProjectBased)
                //                     AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                //                     AND ISNULL(SO.isService, 0) = 0
                //                    order by SO.EDateDelivery, SO.SaleOrderDate"; 
                #endregion
                string sql = "";
                if(isProjectBased==1)
                {
                     sql = @"SELECT * INTO #WORK_REQUEST FROM WorkShopRequest WHERE SaleOrderItemId <> 0;
                                 SELECT * INTO #TEMP FROM(

		                          SELECT  S.SaleOrderRefNo +' - '+ Convert(varchar,SaleOrderDate,106) SaleOrderRefNo, 
                                  C.CustomerName,S.CustomerOrderRef,S.SaleOrderId,
                                  S.isProjectBased,S.EDateArrival,S.EDateDelivery,DATEDIFF(dd,S.SaleOrderDate,GETDATE ()) Ageing,SII.SaleOrderItemId,
	                              DATEDIFF(dd,GETDATE (),S.EDateDelivery)Remaindays,VI.RegistrationNo,VI.ChassisNo,STUFF((SELECT ', '+T2.ItemName + ', '+ T3.ItemName FROM SaleOrderItemUnit T1
								  INNER JOIN SaleOrderItem SI on SI.SaleOrderItemId= T1.SaleOrderItemId
                                  INNER JOIN SaleOrder T4 ON T4.SaleOrderId=SI.SaleOrderId
                                  LEFT JOIN Item T2 ON T1.CondenserUnitId = T2.ItemId
                                  LEFT JOIN Item T3 ON T1.EvaporatorUnitId = T3.ItemId
                                  WHERE T4.SaleOrderId = S.SaleOrderId FOR XML PATH('')), 1, 2, '')WorkDescription  
                                  FROM SaleOrder S 
                                  INNER JOIN SaleOrderItem SII ON SII.SaleOrderId=S.SaleOrderId
								  INNER JOIN Customer C ON S.CustomerId = C.CustomerId
                                  LEFT JOIN VehicleInPass VI ON VI.SaleOrderId=S.SaleOrderId
                                  WHERE S.OrganizationId = @OrganizationId  AND ISNULL(S.isService, 0) = 0 and S.isProjectBased=1
						  
                                   )T1
 
                                  SELECT * FROM #TEMP T LEFT JOIN #WORK_REQUEST WR ON T.SaleOrderItemId = WR.SaleOrderItemId WHERE WR.SaleOrderItemId IS NULL

                                  DROP TABLE #WORK_REQUEST;";
                }
                else
                {
                     sql = @"SELECT * INTO #WORK_REQUEST FROM WorkShopRequest WHERE SaleOrderItemId <> 0;
                                SELECT SOI.SaleOrderId,SOI.SaleOrderItemId,SO.CustomerOrderRef,
	                                SO.SaleOrderDate,SO.SaleOrderRefNo +' - '+ Convert(varchar,SaleOrderDate,106) SaleOrderRefNo,
                                    SO.isProjectBased,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,
	                                C.CustomerName,VI.RegistrationNo,VI.ChassisNo,WD.WorkDescr WorkDescription,
	                                DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing,
	                                DATEDIFF(dd,GETDATE (),SO.EDateDelivery)Remaindays 
                                FROM SaleOrderItem SOI
	                                LEFT JOIN #WORK_REQUEST WR ON SOI.SaleOrderItemId = WR.SaleOrderItemId
	                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
	                                INNER JOIN Customer C ON SO.CustomerId = C.CustomerId
                                    LEFT JOIN VehicleInPass VI ON VI.SaleOrderItemId=SOI.SaleOrderItemId
	                                LEFT JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId
                                WHERE WR.SaleOrderItemId IS NULL
	                                AND SO.SaleOrderApproveStatus = 1
	                                AND SO.SaleOrderHoldStatus IS NULL
	                                AND SO.OrganizationId = @OrganizationId
	                                and SO.isProjectBased=isnull(@isProjectBased, SO.isProjectBased)
	                                AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
	                                AND ISNULL(SO.isService, 0) = 0
	                                AND SO.SaleOrderId NOT IN (SELECT isnull(SaleOrderId,0) FROM WorkShopRequest WHERE SaleOrderItemId = 0 AND ISNULL(JobCardId, 0) = 0)
                                ORDER BY SO.EDateDelivery, SO.SaleOrderDate
                                DROP TABLE #WORK_REQUEST;";
                }




                var objSaleOrders = connection.Query<SaleOrder>(sql, new { OrganizationId = OrganizationId, isProjectBased = isProjectBased, saleOrder = saleOrder }).ToList<SaleOrder>();

                return objSaleOrders;
            }
        }
        /// <summary>
        ///  Saleorder Pending List for  hold stock
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingSO> GetSaleOrdersForHold(int isProjectBased, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT  DISTINCT t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT DISTINCT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,'') WorkDescription,DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing,DATEDIFF(dd,GETDATE (),SO.EDateDelivery)Remaindays,
                             case when ISNULL(JodCardCompleteStatus,0) = 1 then 'Job Card Completed'
                                  when J.SaleOrderItemId is not null then 'Job Card Started'
                                  when SO.SaleOrderHoldStatus='H' then 'Holded'  
                                  when SO.SaleOrderApproveStatus=1 then 'Confirmed' end as Status,SO.TotalAmount
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join SalesInvoice SI on SO.SaleOrderId=SI.SaleOrderId 
                             left join JobCard J on J.SaleOrderItemId = t.SaleOrderItemId
                             WHERE SI.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1 AND SO.OrganizationId=" + OrganizationId + @" AND SO.isProjectBased = " + isProjectBased.ToString() + @" order by SO.SaleOrderDate ASC";

                return connection.Query<PendingSO>(query);
            }
        }


        public SaleOrder GetCombinedWorkDescriptionSaleOrderForWorkshopRequest(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = @"select so.*,c.CustomerName from SaleOrder so left join WorkShopRequest wr on so.SaleOrderId=wr.SaleOrderId , Customer c   where so.CustomerId=c.CustomerId  and wr.SaleOrderId is null and so.isActive=1";
                string sql = @"SELECT t.SaleOrderId,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(10)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId  WHERE SO.SaleOrderId =@SaleOrderId
                             group by t.SaleOrderId";
                var objSaleOrders = connection.Query<SaleOrder>(sql, new { SaleOrderId = SaleOrderId }).Single<SaleOrder>();

                return objSaleOrders;
            }
        }

        public int UpdateSaleOrder(SaleOrder objSaleOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SaleOrder SET SaleOrderDate = @SaleOrderDate, CustomerOrderRef = @CustomerOrderRef,SpecialRemarks = @SpecialRemarks,CommissionAgentId = @CommissionAgentId,CommissionAmount = @CommissionAmount,SalesExecutiveId = @SalesExecutiveId, EDateArrival = @EDateArrival, EDateDelivery = @EDateDelivery, DeliveryTerms = @DeliveryTerms  WHERE SaleOrderId = @SaleOrderId";

                var id = connection.Execute(sql, objSaleOrder);
                InsertLoginHistory(dataConnection, objSaleOrder.CreatedBy, "Update", "Sale Order", id.ToString(), "0");
                return id;
            }
        }
        public int UpdateSaleOrderItemStatus(SaleOrderItem item, string ApprType)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;

                if (ApprType == "WORKSHOP_REQUEST")
                {
                    sql = @"UPDATE SaleOrderItem SET IsPaymentApprovedForWorkshopRequest = @IsPaymentApprovedForWorkshopRequest ,
                        PaymentApprovedForWorkshopRequestReceiptNoAndDate = @PaymentApprovedForWorkshopRequestReceiptNoAndDate,
                        PaymentApprovedForWorkshopRequestCreatedBy = @PaymentApprovedForWorkshopRequestCreatedBy,
                        PaymentApprovedForWorkshopRequestCreatedDate = @PaymentApprovedForWorkshopRequestCreatedDate  WHERE SaleOrderItemId = @SaleOrderItemId";
                }
                else if (ApprType == "JOB_CARD")
                {
                    sql = @"UPDATE SaleOrderItem SET IsPaymentApprovedForJobOrder = @IsPaymentApprovedForJobOrder ,
                        PaymentApprovedForJobOrderReceiptNoAndDate = @PaymentApprovedForJobOrderReceiptNoAndDate,
                        PaymentApprovedForJobOrderCreatedBy = @PaymentApprovedForJobOrderCreatedBy,
                        PaymentApprovedForJobOrderCreatedDate = @PaymentApprovedForJobOrderCreatedDate  WHERE SaleOrderItemId = @SaleOrderItemId";
                }
                else if (ApprType == "DELIVERY_CHALLAN")
                {
                    sql = @"UPDATE SaleOrderItem SET IsPaymentApprovedForDelivery = @IsPaymentApprovedForDelivery ,
                        PaymentApprovedForDeliveryReceiptNoAndDate = @PaymentApprovedForDeliveryReceiptNoAndDate ,
                        PaymentApprovedForDeliveryCreatedBy = @PaymentApprovedForDeliveryCreatedBy,
                        PaymentApprovedForDeliveryCreatedDate = @PaymentApprovedForDeliveryCreatedDate  WHERE SaleOrderItemId = @SaleOrderItemId";
                }

                var id = connection.Execute(sql, item);
                return id;
            }
        }

        public string DeleteSaleOrder(int id, int isAfterSales)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string query = string.Empty;
                try
                {

                    //if (isAfterSales == 1)
                    //{
                    query = "DELETE FROM SaleOrderMaterial  WHERE SaleOrderId = @id;";
                    //}
                    query += @"DELETE FROM SaleOrderItem WHERE SaleOrderId = @id;
                                     DELETE FROM SaleOrder OUTPUT deleted.SaleOrderRefNo WHERE SaleOrderId = @id;";
                    string output = connection.Query<string>(query, new { id = id }, txn).First();
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

        public List<Dropdown> FillCustomer()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CustomerId Id,CustomerName Name from Customer  WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }





        public List<Dropdown> FillQuotationNo()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SalesQuotationId Id,QuotationRefNo Name from SalesQuotation ").ToList();
            }
        }


        public List<Dropdown> FillCommissionAgent()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CommissionAgentId Id,CommissionAgentName Name from CommissionAgent  WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }

        public List<Dropdown> FillCurrency()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CurrencyId Id,CurrencyName Name from Currency  WHERE ISNULL(isActive, 1) = 1").ToList();
            }
        }
        /// <summary>
        /// Get Currency Id from Customer Table with customer Id
        /// </summary>
        /// <param name="cusId">Customer Id</param>
        /// <returns></returns>
        public Dropdown GetCurrencyIdByCustKey(int cusId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "select CurrencyId Id,CurrencyName Name from Currency where CurrencyId=(select CurrencyId from Customer where CustomerId = @cusId)";
                return connection.Query<Dropdown>(query, new { cusId = cusId }).First<Dropdown>();
            }
        }


        public string GetCusomerAddressByKey(int cusId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                Customer customer = connection.Query<Customer>("select * from Customer where CustomerId = " + cusId).FirstOrDefault();

                string address = "";
                if (customer != null)
                {
                    address = customer.DoorNo + ", " + customer.Street + ", " + customer.State;
                }
                return address;
            }
        }

        public string GetContactPerson(int cusId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                Customer customer = connection.Query<Customer>("select * from Customer where CustomerId = " + cusId).FirstOrDefault();

                string ContactPerson = "";
                if (customer.ContactPerson != null)
                {
                    ContactPerson = customer.ContactPerson;
                }
                return ContactPerson;
            }
        }

        public string GetCustomerAddressQuotKey(int quoId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                Customer customer = connection.Query<Customer>("select * from Customer C inner join SalesQuotation S on C.CustomerId=S.CustomerId  where SalesQuotationId = " + quoId).FirstOrDefault();

                string address = "";
                if (customer != null)
                {
                    address = customer.DoorNo + ", " + customer.Street + ", " + customer.State;
                }
                return address;
            }
        }


        public SaleOrder GetSODetailsByQuotKey(int quoId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "SELECT S.CustomerId,S.CurrencyId,PaymentTerms,Remarks SpecialRemarks FROM SalesQuotation S inner join  Customer C on C.CustomerId=S.CustomerId  where SalesQuotationId = @quoId";
                return connection.Query<SaleOrder>(query, new { quoId = quoId }).First<SaleOrder>();
            }
        }
        /// <summary>
        /// Get VehicleModel Id from WorkDescription Table with WorkDescription Id
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public SaleOrderItem GetVehicleModel(int WorkDescriptionId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string query = "SELECT V.VehicleModelId,VehicleModelName FROM WorkDescription W INNER JOIN VehicleModel V ON W.VehicleModelId = V.VehicleModelId AND W.WorkDescriptionId = @WorkDescriptionId";
                return connection.Query<SaleOrderItem>(query, new { WorkDescriptionId = WorkDescriptionId }).First<SaleOrderItem>();
            }
        }
        /// <summary>
        /// Data from sale order table which is not Approved
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingSO> GetSaleOrderPending(int IsProjectBased, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"Select S.SaleOrderId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef,E.EmployeeName,DATEDIFF(DAY,S.SaleOrderDate, GETDATE()) Ageing,DATEDIFF(DAY,GETDATE(), S.EDateDelivery) Remaindays,S.TotalAmount
                                 from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId LEFT JOIN Employee E ON S.CreatedBy = E.EmployeeId
                                 where CommissionAmount>0 And isnull(CommissionAmountApproveStatus,0)=0 AND S.isActive = 1 and S.OrganizationId=@OrganizationId
                                 and  S.IsProjectBased = @IsProjectBased
                                 ORDER BY S.EDateDelivery,S.CreatedDate";
                return connection.Query<PendingSO>(query, new { IsProjectBased = IsProjectBased, OrganizationId = OrganizationId });
            }
        }
        public IEnumerable<PendingSaleOrderForTransactionApproval> GetSaleOrderPendingForTrnApproval(int OrganizationId, string ChassisNo = "", string Customer = "", string JobcardNo = "")
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                #region old query 16.11.2016 4.16p
                //                string query = @"select SI.SaleOrderId, SI.SaleOrderItemId , SH.SaleOrderRefNo, SH.SaleOrderDate, C.CustomerName,i.ItemName freezerUnit,ii.ItemName Box,
                //                               SI.Amount, SI.IsPaymentApprovedForWorkshopRequest, SI.IsPaymentApprovedForJobOrder,
                //                                SI.IsPaymentApprovedForDelivery
                //                                from SaleOrder SH inner join SaleOrderItem SI on SH.SaleOrderId = SI.SaleOrderId
                //                                inner join Customer C on C.CustomerId = SH.CustomerId 
                //                                inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId 
                //								inner join Item I on I.ItemId=W.FreezerUnitId
                //								inner join Item II on II.ItemId=W.BoxId
                //                                order by SH.SaleOrderDate, C.CustomerName "; 
                #endregion

                string query = @"select SI.SaleOrderId, SI.SaleOrderItemId , SH.SaleOrderRefNo, SH.SaleOrderDate, C.CustomerName,i.ItemName freezerUnit,ii.ItemName Box,
                               SI.Amount, SI.IsPaymentApprovedForWorkshopRequest, SI.IsPaymentApprovedForJobOrder,
                                SI.IsPaymentApprovedForDelivery, JC.JobCardNo, CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate, 
                                ISNULL(JC.JodCardCompleteStatus, 0) JodCardCompleteStatus,ISNULL(JQC.IsQCPassed,0)as IsQCPassed,JC.isService, VIP.RegistrationNo,
	                            VIP.ChassisNo
                                from SaleOrder SH inner join SaleOrderItem SI on SH.SaleOrderId = SI.SaleOrderId
								LEFT JOIN JobCard JC ON SI.SaleOrderItemId = JC.SaleOrderItemId
                                LEFT JOIN VehicleInPass VIP ON JC.InPassId = VIP.VehicleInPassId
                                inner join Customer C on C.CustomerId = SH.CustomerId 
                                inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId 
								left join Item I on I.ItemId=W.FreezerUnitId
								left join Item II on II.ItemId=W.BoxId
								left join JobCardQC JQC ON JQC.JobCardId=JC.JobCardId
                                WHERE SH.OrganizationId = @OrganizationId
							    AND Concat(VIP.RegistrationNo,'/',VIP.ChassisNo) LIKE '%'+@ChassisNo+'%'
                                AND isnull(C.CustomerName,'')  LIKE '%'+@Customer+'%'
                                AND isnull( JC.JobCardNo,'')  LIKE '%'+@JobcardNo+'%'
							   order by SH.SaleOrderDate, C.CustomerName";

                return connection.Query<PendingSaleOrderForTransactionApproval>(query, new { OrganizationId = OrganizationId, ChassisNo = ChassisNo, Customer = Customer, JobcardNo = JobcardNo });
            }
        }

        public List<SaleOrderItem> GetSaleOrderItem(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select SOI.*, WD.WorkDescr, VM.VehicleModelName from SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId 
								LEFT JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                                where SaleOrderId=@SaleOrderId";
                return connection.Query<SaleOrderItem>(sql, new { SaleOrderId = SaleOrderId }).ToList();
            }
        }
        /// <summary>
        /// Sale Order Approval-Update SaleOrderApproveStatus=1 in saleorder table
        /// </summary>
        /// <param name="SaleOrderId"></param>
        /// <returns></returns>
        public int UpdateSOApproval(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SaleOrder set CommissionAmountApproveStatus=1 WHERE SaleOrderId=@SaleOrderId";
                return connection.Execute(sql, new { SaleOrderId = SaleOrderId });

            }
        }
        /// <summary>
        /// Sale Order Hold-Update SaleOrderHoldStatus=H in saleorder table
        /// </summary>
        /// <param name="SaleOrderId"></param>
        /// <returns></returns>
        public int UpdateSOHold(int SaleOrderId, string hreason, string HoldDate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SaleOrder set SaleOrderHoldStatus='H',SaleOrderHoldReason=@hreason,SaleOrderHoldDate=@HoldDate  WHERE SaleOrderId=@SaleOrderId";
                return connection.Execute(sql, new { SaleOrderId = SaleOrderId, hreason = hreason, HoldDate = HoldDate });

            }
        }
        /// <summary>
        /// Holded sale order to Release
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingSO> GetSaleOrderHolded(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "Select S.SaleOrderId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef,S.SaleOrderHoldDate,S.SaleOrderHoldReason";
                query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId where S.SaleOrderHoldStatus ='H' and S.isProjectBased = " + isProjectBased.ToString();
                return connection.Query<PendingSO>(query);
            }
        }
        public int UpdateSORelease(int SaleOrderId, string ReleaseDate)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SaleOrder set SaleOrderHoldStatus = null,SaleOrderReleaseDate=@ReleaseDate WHERE SaleOrderId=@SaleOrderId";
                return connection.Execute(sql, new { SaleOrderId = SaleOrderId, ReleaseDate = ReleaseDate });

            }
        }
        /// <summary>
        /// Cancel Sale Order ,no job card started
        /// </summary>
        /// <param name="SaleOrderId"></param>
        /// <param name="ReleaseDate"></param>
        /// <returns></returns>
        public int UpdateSOCancel(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SaleOrder set isActive = 0 WHERE SaleOrderId=@SaleOrderId";
                return connection.Execute(sql, new { SaleOrderId = SaleOrderId });

            }
        }
        public IEnumerable<SaleOrder> GetSaleOrdersForClosing(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select SO.SaleOrderId, SaleOrderRefNo, CONVERT(VARCHAR, SaleOrderDate, 106)SaleOrderDate, CustomerName, SO.SpecialRemarks, CONVERT(VARCHAR, SO.EDateArrival, 106)EDateArrival, CONVERT(VARCHAR, SO.EDateDelivery, 106)EDateDelivery";
                sql += " from SaleOrder SO";
                sql += " inner join SalesInvoice SI on SO.SaleOrderId = SI.SaleOrderId";
                sql += " inner join Customer C on C.CustomerId = SO.CustomerId";
                sql += " where SO.SaleOrderClosed is null";
                sql += " AND SO.isProjectBased = " + isProjectBased.ToString();

                return connection.Query<SaleOrder>(sql);
            }
        }
        public void CloseSaleOrder(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = "exec SaleOrderClosing " + Id.ToString();
                string sql = "update SaleOrder set SaleOrderClosed = 'CLOSED' where SaleOrderId = " + Id.ToString() + ";";
                connection.Query(sql);
            }
        }
        public int IsProjectOrVehicle(int SaleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = "select S.isProjectBased from SaleOrder S inner join SaleOrderItem I on S.SaleOrderId = I.SaleOrderId where I.SaleOrderItemId = " + SaleOrderItemId.ToString();
                return connection.Query<int>(sql).Single();
            }
        }

        public IEnumerable<SaleOrder> GetSaleOrderIncentiveAmount()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @" SELECT SH.SaleOrderId, SH.SaleOrderRefNo, SH.SaleOrderDate,SUM(SI.Amount)TotalAmount,
                                  EmployeeName,CommissionAgentName,CommissionAmount
                                  FROM SaleOrder SH 
                                  INNER JOIN SaleOrderItem SI ON SH.SaleOrderId = SI.SaleOrderId
                                  INNER JOIN CommissionAgent C ON C.CommissionAgentId=SH.CommissionAgentId
                                  INNER JOIN Employee ON EmployeeId =SalesExecutiveId
                                  WHERE SH.CommissionAmountApproveStatus=1
                                  GROUP BY SH.SaleOrderId, SH.SaleOrderRefNo, SH.SaleOrderDate,
                                  EmployeeName,CommissionAgentName,CommissionAmount
                                  ORDER BY SH.SaleOrderDate";
                return connection.Query<SaleOrder>(query).ToList();
            }
        }


        public IEnumerable<SaleOrder> GetSaleOrderIncentiveAmountList(DateTime? FromDate, DateTime? ToDate, int CommissionAgentId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT SH.SaleOrderId, SH.SaleOrderRefNo, SH.SaleOrderDate,SUM(SI.Amount)TotalAmount,
                                  EmployeeName,CommissionAgentName,CommissionAmount
                                  FROM SaleOrder SH 
                                  INNER JOIN SaleOrderItem SI ON SH.SaleOrderId = SI.SaleOrderId
                                  INNER JOIN CommissionAgent C ON C.CommissionAgentId=SH.CommissionAgentId
                                  INNER JOIN Employee ON EmployeeId =SalesExecutiveId
                                  WHERE SH.CommissionAmountApproveStatus=1
								  AND SaleOrderDate Between ISNULL(NULLIF(@FromDate, getDate()), SaleOrderDate) and 
                                  ISNULL(NULLIF(@ToDate, getDate()), SaleOrderDate) AND SH.CommissionAgentId= ISNULL(NULLIF(@CommissionAgentId, 0),SH.CommissionAgentId)
								  GROUP BY SH.SaleOrderId, SH.SaleOrderRefNo, SH.SaleOrderDate,
                                  EmployeeName,CommissionAgentName,CommissionAmount
                                  ORDER BY SH.SaleOrderRefNo;";

                return connection.Query<SaleOrder>(query, new { FromDate = FromDate, ToDate = ToDate, CommissionAgentId = CommissionAgentId }).ToList();
            }
        }

        public IEnumerable<PendingSO> GetPreviousList(int isProjectBased, int id, int cusid, int OrganizationId, DateTime? from, DateTime? to, int service = 0)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "Select S.SaleOrderId,SaleOrderRefNo,SQ.SalesQuotationId,SaleOrderDate,CONCAT(QuotationRefNo,' - ',CONVERT (VARCHAR(15),QuotationDate,106))QuotationNoDate, C.CustomerName, S.CustomerOrderRef";//--, DATEDIFF(DAY, GETDATE(), S.EDateDelivery) DaysLeft, DATEDIFF(DAY, S.SaleOrderDate, GETDATE()) Ageing
                query += " from SaleOrder S";
                query += " inner join Customer C on S.CustomerId = C.CustomerId";
                query += " left join SalesQuotation SQ ON SQ.SalesQuotationId=S.SalesQuotationId";
                query += " where S.SaleOrderId= ISNULL(NULLIF(@id, 0), S.SaleOrderId) AND C.CustomerId = ISNULL(NULLIF(@cusid, 0), C.CustomerId)";
                query += " AND S.isActive = 1 and S.OrganizationId = @OrganizationId and S.SaleOrderDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())";
                query += @" AND S.isProjectBased=@isProjectBased AND ISNULL(S.isService, 0) = @service";


                return connection.Query<PendingSO>(query, new { isProjectBased = isProjectBased, OrganizationId = OrganizationId, id = id, cusid = cusid, to = to, from = from, service = service }).ToList();
            }
        }

        public int isUsed(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT COUNT(JobCardId) FROM JobCard JC
                                INNER JOIN SaleOrderItem SOI ON JC.SaleOrderItemId = SOI.SaleOrderItemId
                                INNER JOIN SaleOrder SO ON SOI.SaleOrderId = SO.SaleOrderId
                                WHERE SO.SaleOrderId = @id
                                AND JC.isActive = 1";

                return connection.Query<int>(query, new { id = id }).First();
            }
        }

        public SaleOrder GetSaleOrderHD(int SaleOrderId, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"  SELECT 
									 OrganizationName,O.DoorNo,O.Street,O.State,O.Phone,O.Fax,O.Email,O.Zip,O.Image1,O.ContactPerson,
	                                SaleOrderId,
	                                CONCAT(SaleOrderRefNo,' - ',CONVERT (VARCHAR(15),SaleOrderDate,106))SaleOrderNoDate, 
	                                CONCAT(QuotationRefNo,' - ',CONVERT (VARCHAR(15),QuotationDate,106))QuotationNoDate,  
                                    C.CustomerId,
	                                CustomerName,
	                                isnull(O.DoorNo,'') +','+ isnull(O.Street,'')+','+isnull(O.State,'') CustomerAddress,
	                                CustomerOrderRef, S.CurrencyId,SpecialRemarks, S.PaymentTerms,S.DeliveryTerms,CA.CommissionAgentName,
									CommissionAmount,TotalAmount,TotalDiscount,EDateArrival, EDateDelivery, SaleOrderApproveStatus,
								    SaleOrderHoldStatus,SaleOrderHoldReason,SaleOrderHoldDate,SaleOrderReleaseDate,S.SalesQuotationId,SaleOrderClosed, 
									S.isProjectBased, CUR.CurrencyName,S.isAfterSales, ORR.CountryName,E.EmployeeName SalesExecutiveName
                                    FROM SaleOrder S 
									LEFT JOIN CommissionAgent CA ON S.CommissionAgentId=CA.CommissionAgentId
									INNER JOIN Organization O ON O.OrganizationId=S.OrganizationId
	                                INNER JOIN Customer C ON S.CustomerId=C.CustomerId  
                                    left  JOIN Country ORR ON ORR.CountryId=O.Country
									LEFT JOIN Employee E ON e.EmployeeId=S.SalesExecutiveId
	                                LEFT JOIN Currency CUR ON S.CurrencyId = CUR.CurrencyId
	                                LEFT JOIN SalesQuotation SQ ON SQ.SalesQuotationId=S.SalesQuotationId
                                    WHERE SaleOrderId=@SaleOrderId";

                var objSaleOrder = connection.Query<SaleOrder>(sql, new
                {
                    SaleOrderId = SaleOrderId,
                    organizationId = organizationId
                }).First<SaleOrder>();

                return objSaleOrder;
            }
        }



        public List<SaleOrderItem> GetSaleOrderItemDT(int SaleOrderId, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select SOI.*, WD.WorkDescr, VM.VehicleModelName from SaleOrderItem SOI
                                INNER JOIN WorkDescription WD ON SOI.WorkDescriptionId = WD.WorkDescriptionId 
								LEFT JOIN VehicleModel VM ON SOI.VehicleModelId = VM.VehicleModelId
                                where SaleOrderId=@SaleOrderId";
                return connection.Query<SaleOrderItem>(sql, new
                {
                    SaleOrderId = SaleOrderId,
                    organizationId = organizationId
                }).ToList();
            }
        }

        public string InsertServiceOrder(ServiceEnquiry model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                
                {
                    if(model.isProjectBased==0)
                    {
                        model.SaleOrderRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 35, true, txn);
                    }
                    else
                    {
                        model.SaleOrderRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 42, true, txn);
                    }
                    //model.TotalAmount = model.Items.Sum(m => m.Amount);
                    //model.TotalDiscount = model.Items.Sum(m => m.Discount);
                    if (model.CustomerOrderRef == null || model.CustomerOrderRef == String.Empty) model.CustomerOrderRef = " ";
                    string query = @"insert  into SaleOrder 
                                    (SaleOrderRefNo, SaleOrderDate, CustomerId, CustomerOrderRef, CurrencyId, SpecialRemarks,
                                    EDateArrival, EDateDelivery, CreatedBy, CreatedDate,
                                    OrganizationId, SaleOrderApproveStatus, isProjectBased, isAfterSales, isActive, isService, ServiceEnquiryId)
                                    Values
                                    (@SaleOrderRefNo, @SaleOrderDate, @CustomerId, @CustomerOrderRef, (SELECT CurrencyId FROM Customer WHERE CustomerId = @CustomerId), @Complaints, 
                                    GETDATE(), GETDATE(), @CreatedBy, GETDATE(),
                                    @OrganizationId, 1, @isProjectBased, @isService, 1, @isService, @ServiceEnquiryId);
                                    SELECT CAST(SCOPE_IDENTITY() as int) SaleOrderId";
                    int id = connection.Query<int>(query, model, txn).FirstOrDefault();
                    if (id > 0)
                    {
                        foreach (SaleOrderItem item in model.Items)
                        {
                            item.SaleOrderId = id;
                            new SaleOrderItemRepository().InsertSaleOrderItem(item, connection, txn);
                        }
                        query = @"UPDATE ServiceEnquiry SET isConfirmed = 1 WHERE ServiceEnquiryId = " + model.ServiceEnquiryId;
                        if (connection.Execute(query, transaction: txn) > 0)
                            txn.Commit();
                        else
                            throw new Exception();
                        return model.SaleOrderRefNo;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public string GetCustomerTelephone(int cusKey)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    var param = new DynamicParameters();
                    return connection.Query<string>("select ISNULL(Phone, '') from Customer where CustomerId = " + cusKey).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return "";
            }
        }



        //        public ServiceEnquiry InsertServiceEnquiry(ServiceEnquiry objServiceEnquiry)
        //        {
        //            using (IDbConnection connection = OpenConnection(dataConnection))
        //            {
        //                var result = new ServiceEnquiry();

        //                IDbTransaction trn = connection.BeginTransaction();

        //                string sql = @"insert into ServiceEnquiry(ServiceEnquiryRefNo,CustomerId,VehicleMake,VehicleRegNo,VehicleChassisNo,VehicleKm,BoxMake,BoxNo,BoxSize
        //			,FreezerMake,FreezerModel,FreezerSerialNo,FreezerHours,TailLiftMake,TailLiftModel,TailLiftSerialNo,OrganizationId,IsConfirmed
        //			,CreatedBy,CreatedDate) values (@ServiceEnquiryRefNo,@CustomerId,@VehicleMake,@VehicleRegNo,@VehicleChassisNo,@VehicleKm,@BoxMake,@BoxNo,@BoxSize
        //			,@FreezerMake,@FreezerModel,@FreezerSerialNo,@FreezerHours,@TailLiftMake,@TailLiftModel,@TailLiftSerialNo,@OrganizationId,@IsConfirmed
        //			,@CreatedBy,@CreatedDate);
        //                               SELECT CAST(SCOPE_IDENTITY() as int)";
        //                try
        //                {
        //                    int internalid = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ServiceEnquiry).Name, "0", 1);
        //                  //  objServiceEnquiry.DesignationRefNo = "D/" + internalid;

        //                    int id = connection.Query<int>(sql, objServiceEnquiry, trn).Single();
        //                    objServiceEnquiry.ServiceEnquiryId = id;
        //                    //connection.Dispose();
        //                    InsertLoginHistory(dataConnection, objServiceEnquiry.CreatedBy.ToString(), "Create", "ServiceEnquiry", id.ToString(), "0");
        //                    trn.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    trn.Rollback();
        //                    objServiceEnquiry.ServiceEnquiryId = 0;
        //                   // objServiceEnquiry.DesignationRefNo = null;

        //                }
        //                return objServiceEnquiry;
        //            }
        //        }

        public string InsertServiceEnquiry(ServiceEnquiry model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    if (model.isProjectBased == 0)
                    {
                        model.ServiceEnquiryRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 33, true, txn);
                    }
                    else
                    {
                        model.ServiceEnquiryRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 41, true, txn);
                    }
                    #region query
                    string query = @"insert into ServiceEnquiry(ServiceEnquiryRefNo,CustomerId,VehicleMake,VehicleRegNo,VehicleChassisNo,VehicleKm,BoxMake,BoxNo,BoxSize
			                        ,FreezerMake,FreezerModel,FreezerSerialNo,FreezerHours,TailLiftMake,TailLiftModel,TailLiftSerialNo,OrganizationId,IsConfirmed
			                        ,CreatedBy,CreatedDate, ServiceEnquiryDate, Complaints,isProjectBased,UnitDetails) 
                                    OUTPUT inserted.ServiceEnquiryRefNo
                                    values
                                    (@ServiceEnquiryRefNo,@CustomerId,@VehicleMake,@VehicleRegNo,@VehicleChassisNo,@VehicleKm,@BoxMake,@BoxNo,@BoxSize
			                       ,@FreezerMake,@FreezerModel,@FreezerSerialNo,@FreezerHours,@TailLiftMake,@TailLiftModel,@TailLiftSerialNo,@OrganizationId,@IsConfirmed
			                       ,@CreatedBy,@CreatedDate, @ServiceEnquiryDate, @Complaints,@isProjectBased,@UnitDetails);";
                    #endregion
                    string output = connection.Query<string>(query, model, txn).FirstOrDefault();
                    txn.Commit();
                    return output;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public IList<ServiceEnquiry> GetPendingServiceEnquiries(int OrganizationId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT ServiceEnquiryId, ServiceEnquiryRefNo, ServiceEnquiryDate, VehicleMake, BoxMake, FreezerMake, TailLiftMake, C.CustomerName 
                                FROM ServiceEnquiry SE INNER JOIN Customer C ON SE.CustomerId = C.CustomerId
                                WHERE SE.OrganizationId = @org AND ISNULL(isCancelled, 0) = 0 AND ISNULL(isConfirmed, 0) = 0 and SE.isProjectBased=@isProjectBased";
                return connection.Query<ServiceEnquiry>(query, new { org = OrganizationId, isProjectBased = isProjectBased }).ToList();
            }
        }

        /// <summary>
        /// Get all details of a Service Enquiry
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServiceEnquiry GetServiceEnquiryDetails(int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"SELECT SE.*,ISNULL(SO.ServiceEnquiryId,0)IsSEUsed  from ServiceEnquiry SE 
                                     Left join SaleOrder SO ON SO.ServiceEnquiryId=SE.ServiceEnquiryId
                                     WHERE SE.ServiceEnquiryId = @id AND SE.OrganizationId = @org ";
                    return connection.Query<ServiceEnquiry>(query, new { id = id, org = OrganizationId }).FirstOrDefault();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public ServiceEnquiry GetJobPrintHD(int ServiceEnquiryId, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {


                string sql = @"  select S.*,O.*,C.CustomerName,C.DoorNo CDoorNo,C.State CState,C.Street CStreet,C.ContactPerson CContactPerson,C.Phone CPhone,C.Zip CZip,
                                  CU.CountryName,U.UserName CreatedUser,U.Signature CreatedUsersig,D.DesignationName CreatedDes from ServiceEnquiry S
                                  left join Customer C ON C.CustomerId=S.CustomerId
                                  left  join Organization O ON O.OrganizationId=S.OrganizationId
                                  left join Country CU ON CU.CountryId=O.Country
								  left join [User] U ON U.UserId=S.CreatedBy
								  left join [Designation] D ON D.DesignationId=U.DesignationId
                                  where ServiceEnquiryId=@ServiceEnquiryId ";

                var objServiceEnquiry = connection.Query<ServiceEnquiry>(sql, new
                {
                    ServiceEnquiryId = ServiceEnquiryId,
                    organizationId = organizationId
                }).First<ServiceEnquiry>();

                return objServiceEnquiry;
            }
        }


        public IList<ServiceEnquiry> GetPendingServiceEnquiryList(int OrganizationId, int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT ServiceEnquiryId, ServiceEnquiryRefNo, ServiceEnquiryDate, VehicleMake, BoxMake, FreezerMake, TailLiftMake, C.CustomerName,
                               ISNULL([VehicleRegNo],'')RegistrationNo ,ISNULL( [VehicleChassisNo],'') ChassisNo,FreezerModel
                                FROM ServiceEnquiry SE INNER JOIN Customer C ON SE.CustomerId = C.CustomerId
                                WHERE SE.OrganizationId= @OrganizationId and SE.isProjectBased=@isProjectBased  order by ServiceEnquiryId desc";
                return connection.Query<ServiceEnquiry>(query, new { OrganizationId = OrganizationId, isProjectBased = isProjectBased }).ToList();
            }
        }


        public int UpdateServiceEnquiry(ServiceEnquiry objServiceEnquiry)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();

                string sql = @"UPDATE
                                ServiceEnquiry SET 
                                CustomerId=@CustomerId,VehicleMake=@VehicleMake,VehicleRegNo=@VehicleRegNo,VehicleChassisNo=@VehicleChassisNo,
                                VehicleKm=@VehicleKm,BoxMake=@BoxMake,BoxNo=@BoxNo,BoxSize=@BoxSize,FreezerMake=@FreezerMake,FreezerModel=@FreezerModel,
                                FreezerSerialNo=@FreezerSerialNo,FreezerHours=@FreezerHours,TailLiftMake=@TailLiftMake,TailLiftModel=@TailLiftModel,
                                TailLiftSerialNo=@TailLiftSerialNo,IsConfirmed=@IsConfirmed,ServiceEnquiryDate=@ServiceEnquiryDate,Complaints=@Complaints, 
                                CreatedBy=@CreatedBy,CreatedDate=@CreatedDate,OrganizationId=@OrganizationId,UnitDetails=@UnitDetails
                                WHERE ServiceEnquiryId = @ServiceEnquiryId;";
                var id = connection.Execute(sql, objServiceEnquiry, txn);

                InsertLoginHistory(dataConnection, objServiceEnquiry.CreatedBy, "Update", "Service Enquiry", id.ToString(), "0");
                txn.Commit();
                return id;

            }

        }


        public string DeleteServiceEnquiry(int ServiceEnquiryId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM SaleOrder WHERE ServiceEnquiryId = @ServiceEnquiryId;
                                    DELETE FROM ServiceEnquiry OUTPUT deleted.ServiceEnquiryRefNo WHERE ServiceEnquiryId = @ServiceEnquiryId";
                    string output = connection.Query<string>(query, new { ServiceEnquiryId = ServiceEnquiryId }, txn).First();
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
        public IList<SaleOrder> GetPendingServiceOrderList(int OrganizationId,int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"
                                      select SaleOrderRefNo,SaleOrderDate,CustomerOrderRef,SE.ServiceEnquiryDate,SE.ServiceEnquiryRefNo,W.WorkDescr,V.VehicleModelName,S.saleorderid from SaleOrder S
								left join ServiceEnquiry SE ON SE.ServiceEnquiryId=S.ServiceEnquiryId
								left join SaleOrderItem SI On SI.SaleOrderId=S.SaleOrderId
								left join VehicleModel V ON V.VehicleModelId=SI.VehicleModelId
								left join WorkDescription W ON W. WorkDescriptionId=SI.WorkDescriptionId
								where S.OrganizationId=@OrganizationId  and isService=1 and SE.isProjectBased=@isProjectBased order by [SaleOrderDate] desc";
                return connection.Query<SaleOrder>(query, new
                {
                    OrganizationId = OrganizationId,
                    isProjectBased = isProjectBased

                }).ToList();
            }
        }
        public ServiceEnquiry GetServiceOrderDetails(int id, int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string query = @"select S.*,SE.ServiceEnquiryRefNo,SE.ServiceEnquiryDate,W.WorkDescr,V.VehicleModelName,SE.Complaints,SE.UnitDetails,JobCardId from SaleOrder S 
						left join ServiceEnquiry SE ON SE.ServiceEnquiryId=S.ServiceEnquiryId 
						left join SaleOrderItem SI On SI.SaleOrderId=S.SaleOrderId
                        left join JobCard J ON J.SaleOrderId=S.SaleOrderId
						left join VehicleModel V ON V.VehicleModelId=SI.VehicleModelId
						left join WorkDescription W ON W. WorkDescriptionId=SI.WorkDescriptionId where S.SaleOrderId=@id AND S.OrganizationId = @org ";
                    return connection.Query<ServiceEnquiry>(query, new { id = id, org = OrganizationId }).FirstOrDefault();

                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public int Count(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select count(SaleOrderId)SaleOrderId from VehicleInPass where  SaleOrderId=@id";

                return connection.Query<int>(query, new { id = id }).First();
            }
        }
        public List<SaleOrderItem> GetSaleOrderItm(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from saleorderitem where saleorderid=@id";
                return connection.Query<SaleOrderItem>(sql, new { id = id }).ToList();
            }
        }
        public int UpdateServiceOrder(SaleOrder model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    if (model.isProjectBased == 0)
                    {
                        model.ServiceEnquiryRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 33, true, txn);
                    }
                    else
                    {
                        model.ServiceEnquiryRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 41, true, txn);
                    }
                   
                    #region query
                    string query = @"update SaleOrder set SaleOrderDate=@SaleOrderDate,CustomerOrderRef=@CustomerOrderRef WHERE SaleOrderId = @SaleOrderId;";
                    #endregion
                    int output = connection.Execute(query, model, txn);

                    // int id = connection.Query<int>(query, model, txn).FirstOrDefault();
                    if (output > 0)
                    {
                        string sql = @"delete from SaleOrderItem where SaleOrderId=@SaleOrderId";
                        connection.Execute(sql, model, txn);
                        foreach (SaleOrderItem item in model.Items)
                        {
                            item.SaleOrderId = model.SaleOrderId;
                            new SaleOrderItemRepository().InsertSaleOrderItem(item, connection, txn);
                        }
                        txn.Commit();
                        return output;
                    }
                    else
                    {
                        throw new Exception();
                    }

                }

                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public string DeletServiceOrder(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"DELETE FROM SaleOrderitem WHERE SaleOrderId = @id;
                                    DELETE FROM SaleOrder OUTPUT deleted.[SaleOrderRefNo] WHERE SaleOrderId = @id";
                    string output = connection.Query<string>(query, new { id = id }, txn).First();
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

        public List<QuerySheetItem> GetRoomDetailsFromQuotation(int salesQuotationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                try
                {
                    string sql = @"DECLARE @QuerySheetId INT = (SELECT
	                                    QuerySheetId
                                    FROM SalesQuotation
                                    WHERE SalesQuotationId = @id)

                                    SELECT
	                                    *
                                    FROM QuerySheetItem
                                    WHERE QuerySheetId = @QuerySheetId

                                    SELECT QuerySheetItemId INTO #QuerySheetItems FROM QuerySheetItem WHERE QuerySheetId = @QuerySheetId

                                    SELECT
	                                    *
                                    FROM QuerySheetItemUnit
                                    WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM #QuerySheetItems)

                                    SELECT
	                                    *
                                    FROM QuerySheetItemDoor
                                    WHERE QuerySheetItemId IN (SELECT QuerySheetItemId FROM #QuerySheetItems)

                                    DROP TABLE #QuerySheetItems";
                    using (var dataset = connection.QueryMultiple(sql, new { id = salesQuotationId }))
                    {
                        SaleOrder model = new SaleOrder();
                        model.ProjectRooms = dataset.Read<QuerySheetItem>().ToList();
                        var units = dataset.Read<QuerySheetUnit>().ToList();
                        var doors = dataset.Read<QuerySheetDoor>().ToList();
                        foreach (var item in model.ProjectRooms)
                        {
                            item.ProjectRoomUnits = units
                                .Where(x => x.QuerySheetItemId == item.QuerySheetItemId)
                                .Select(x => x).ToList();
                            item.ProjectRoomDoors = doors
                                .Where(x => x.QuerySheetItemId == item.QuerySheetItemId)
                                .Select(x => x).ToList();
                        }
                        return model.ProjectRooms;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Insert data into [SaleOrder], [SaleOrderItem], [SaleOrderItemUnit], [SaleOrderItemDoor]
        /// </summary>
        /// <param name="model"></param>
        /// <returns>SaleOrder model with new transaction id</returns>
        public SaleOrder InsertProjectSaleOrder(SaleOrder model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    if (model.isProjectBased == 0)
                    {
                        model.SaleOrderRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 3, true, txn);
                    }
                    else
                    {
                        model.SaleOrderRefNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId ?? 0, 4, true, txn);
                    }
                    #region saving sale order head [SaleOrder]
                    string sql = @"insert  into SaleOrder(SaleOrderRefNo,SaleOrderDate,CustomerId,CustomerOrderRef,CurrencyId,SpecialRemarks,PaymentTerms,DeliveryTerms,CommissionAgentId,CommissionAmount,CommissionPerc,TotalAmount,TotalDiscount,SalesExecutiveId,EDateDelivery,EDateArrival,CreatedBy,CreatedDate,OrganizationId,SaleOrderApproveStatus,isProjectBased,isAfterSales,SalesQuotationId)
                                   Values (@SaleOrderRefNo,@SaleOrderDate,@CustomerId,@CustomerOrderRef,@CurrencyId,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@CommissionAgentId,@CommissionAmount,@CommissionPerc,@TotalAmount,@TotalDiscount,@SalesExecutiveId,@EDateDelivery,@EDateArrival,@CreatedBy,@CreatedDate,@OrganizationId,1,@isProjectBased,@isAfterSales,@SalesQuotationId);
                                    SELECT CAST(SCOPE_IDENTITY() as int) SaleOrderId";
                    model.SaleOrderId = connection.Query<int>(sql, model, txn).First();
                    #endregion
                    #region saving sale order details [SaleOrderItem]
                    sql = @"INSERT INTO SaleOrderItem
                            (
	                            [SaleOrderId],
	                            [SlNo],
	                            [Quantity],
	                            [isActive]
                            )
                            VALUES
                            (
	                            @SaleOrderId,
	                            @SlNo,
	                            @Quantity,
	                            1
                            )
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    model.Items = new List<SaleOrderItem>();
                    model.Items.Add(new SaleOrderItem
                    {
                        SaleOrderId = model.SaleOrderId,
                        SlNo = 1,
                        Quantity = 1
                    });
                    var _SaleOrderItemId = connection.Query<int>(sql, model.Items[0], txn).First();
                    //for (int i = 0; i < model.ProjectRooms.Count; i++)
                    //{
                    //    model.Items.Add(new SaleOrderItem());
                    //    model.Items[i].SaleOrderId = model.SaleOrderId;
                    //    model.Items[i].SlNo = i;
                    //    model.Items[i].Quantity = 1;
                    //    model.Items[i].SaleOrderItemId =
                    //        model.ProjectRooms[i].QuerySheetItemId =
                    //        connection.Query<int>(sql, model.Items[i], txn).First();
                    //}
                    #endregion
                    #region saving room units [SaleOrderItemUnit]
                    foreach (var room in model.ProjectRooms)
                    {
                        foreach (QuerySheetUnit item in room.ProjectRoomUnits)
                        {
                            item.QuerySheetItemId = _SaleOrderItemId;
                            sql = @"insert  into SaleOrderItemUnit(SaleOrderItemId,EvaporatorUnitId,CondenserUnitId,Quantity) 
                                    Values (@QuerySheetItemId,@EvaporatorUnitId,@CondenserUnitId,@Quantity)";
                            connection.Execute(sql, item, txn);
                        }
                        foreach (QuerySheetDoor item in room.ProjectRoomDoors)
                        {
                            item.QuerySheetItemId = _SaleOrderItemId;
                            sql = @"insert  into SaleOrderItemDoor(SaleOrderItemId,DoorId,Quantity) 
                                    Values (@QuerySheetItemId,@DoorId,@Quantity)";
                            connection.Execute(sql, item, txn);
                        }
                    }
                    #endregion
                    txn.Commit();
                    return model;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }
        public string DeleteProjectSaleOrder(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string query = string.Empty;
                try
                {


                    query = @"DELETE FROM SaleOrderItemDoor WHERE SaleOrderItemId IN (SELECT SaleOrderItemId FROM SaleOrderItem WHERE SaleOrderId = @id);
                              DELETE FROM SaleOrderItemUnit WHERE SaleOrderItemId IN (SELECT SaleOrderItemId FROM SaleOrderItem WHERE SaleOrderId = @id);
                   
                               DELETE FROM SaleOrderItem WHERE SaleOrderId = @id;
                               DELETE FROM SaleOrder OUTPUT deleted.SaleOrderRefNo WHERE SaleOrderId = @id;";
                    string output = connection.Query<string>(query, new { id = id }, txn).First();
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