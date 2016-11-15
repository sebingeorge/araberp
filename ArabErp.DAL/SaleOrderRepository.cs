using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

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
        public string InsertSaleOrder(SaleOrder objSaleOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    var internalId = "";
                    decimal MaterialAmt = 0;
                    if (objSaleOrder.isProjectBased == 0 )
                    {
                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objSaleOrder.OrganizationId ?? 0, 3, true,txn);
                    }
                    else
                    {

                        internalId = DatabaseCommonRepository.GetNewDocNo(connection, objSaleOrder.OrganizationId ?? 0, 4, true,txn);
                      
                    }

                    objSaleOrder.SaleOrderRefNo = internalId;

                    if (objSaleOrder.isAfterSales == 1)
                    {

                        MaterialAmt = objSaleOrder.Materials.Sum(m => m.Amount);
                    }
                    objSaleOrder.TotalAmount = objSaleOrder.Items.Sum(m => m.Amount) + MaterialAmt;
                    objSaleOrder.TotalDiscount = objSaleOrder.Items.Sum(m => m.Discount);

               
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
                    if (objSaleOrder.isAfterSales==1)
                    {

                        foreach (SalesQuotationMaterial item in objSaleOrder.Materials)
                        {
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
                string sql = @"select *,isnull(DoorNo,'') +','+ isnull(Street,'')+','+isnull(State,'') CustomerAddress,CONCAT(QuotationRefNo,' - ',CONVERT(VARCHAR(15),QuotationDate,104))QuotationNoDate,GrandTotal TotalAmount, ExpectedDeliveryDate AS EDateDelivery from SalesQuotation S inner join Customer C on S.CustomerId=C.CustomerId  where SalesQuotationId=@Id";

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
	                                DeliveryTerms,
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
         
                string sql = @"SELECT  distinct t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderDate,SO.SaleOrderRefNo +' - '+ Convert(varchar,SaleOrderDate,106) SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT DISTINCT ', ' + CAST(W.WorkDescr AS VARCHAR(MAX)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing,DATEDIFF(dd,GETDATE (),SO.EDateDelivery)Remaindays 
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId WHERE WR.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1 and SO.SaleOrderHoldStatus IS NULL and SO.OrganizationId = @OrganizationId and SO.isProjectBased=isnull(@isProjectBased,SO.isProjectBased)
							 AND SO.SaleOrderRefNo LIKE '%'+@saleOrder+'%'
                             order by SO.EDateDelivery, SO.SaleOrderDate";
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
   
        public string DeleteSaleOrder(int id,int isAfterSales)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                string query = string.Empty;
                try
                {

                    if (isAfterSales == 1)
                    {
                        query = "DELETE FROM SaleOrderMaterial  WHERE SaleOrderId = @id;";
                    }
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
                if (ContactPerson != null)
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
                                 ORDER BY S.EDateDelivery , S.CreatedDate ";
                return connection.Query<PendingSO>(query, new { IsProjectBased = IsProjectBased, OrganizationId = OrganizationId });
            }
        }
        public IEnumerable<PendingSaleOrderForTransactionApproval> GetSaleOrderPendingForTrnApproval()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select SI.SaleOrderId, SI.SaleOrderItemId , SH.SaleOrderRefNo, SH.SaleOrderDate, C.CustomerName,i.ItemName freezerUnit,ii.ItemName Box,
                               SI.Amount, SI.IsPaymentApprovedForWorkshopRequest, SI.IsPaymentApprovedForJobOrder,
                                SI.IsPaymentApprovedForDelivery
                                from SaleOrder SH inner join SaleOrderItem SI on SH.SaleOrderId = SI.SaleOrderId
                                inner join Customer C on C.CustomerId = SH.CustomerId 
                                inner join WorkDescription W on W.WorkDescriptionId = SI.WorkDescriptionId 
								inner join Item I on I.ItemId=W.FreezerUnitId
								inner join Item II on II.ItemId=W.BoxId
                                order by SH.SaleOrderDate, C.CustomerName ";
                return connection.Query<PendingSaleOrderForTransactionApproval>(query);
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
        public int UpdateSORelease(int SaleOrderId,string ReleaseDate)
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

        public IEnumerable<PendingSO> GetPreviousList(int isProjectBased, int id, int cusid, int OrganizationId, DateTime? from, DateTime? to)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "Select S.SaleOrderId,SaleOrderRefNo,SQ.SalesQuotationId,SaleOrderDate,CONCAT(QuotationRefNo,' - ',CONVERT (VARCHAR(15),QuotationDate,106))QuotationNoDate, C.CustomerName, S.CustomerOrderRef";//--, DATEDIFF(DAY, GETDATE(), S.EDateDelivery) DaysLeft, DATEDIFF(DAY, S.SaleOrderDate, GETDATE()) Ageing
                query += " from SaleOrder S";
                query += " inner join Customer C on S.CustomerId = C.CustomerId";
                query += " left join SalesQuotation SQ ON SQ.SalesQuotationId=S.SalesQuotationId";
                query += " where S.SaleOrderId= ISNULL(NULLIF(@id, 0), S.SaleOrderId) AND C.CustomerId = ISNULL(NULLIF(@cusid, 0), C.CustomerId)";
                query += " AND S.isActive = 1 and S.OrganizationId = @OrganizationId and S.SaleOrderDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE())";
                query += " AND S.isProjectBased=@isProjectBased ";


                return connection.Query<PendingSO>(query, new { isProjectBased = isProjectBased, OrganizationId = OrganizationId, id = id, cusid = cusid, to = to, from = from }).ToList();
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

        public SaleOrder GetSaleOrderHD(int SaleOrderId,int organizationId)
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
	                                CustomerOrderRef, S.CurrencyId,SpecialRemarks, S.PaymentTerms,DeliveryTerms,CA.CommissionAgentName,
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
                    organizationId=organizationId
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
                { SaleOrderId = SaleOrderId,
                  organizationId = organizationId
                }).ToList();
            }
        }
        /// 
    }
}