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
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(SaleOrder).Name, "0", 1);

                    objSaleOrder.SaleOrderRefNo = "SAL/" + internalId;
                    objSaleOrder.TotalAmount = objSaleOrder.Items.Sum(m => m.Amount);
                    objSaleOrder.TotalDiscount = objSaleOrder.Items.Sum(m => m.Discount);
                    string sql = @"
                    insert  into SaleOrder(SaleOrderRefNo,SaleOrderDate,CustomerId,CustomerOrderRef,CurrencyId,SpecialRemarks,PaymentTerms,DeliveryTerms,CommissionAgentId,CommissionAmount,CommissionPerc,TotalAmount,TotalDiscount,SalesExecutiveId,EDateArrival,EDateDelivery,CreatedBy,CreatedDate,OrganizationId,SaleOrderApproveStatus,isProjectBased,SalesQuotationId)
                                   Values (@SaleOrderRefNo,@SaleOrderDate,@CustomerId,@CustomerOrderRef,@CurrencyId,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@CommissionAgentId,@CommissionAmount,@CommissionPerc,@TotalAmount,@TotalDiscount,@SalesExecutiveId,@EDateArrival,@EDateDelivery,@CreatedBy,@CreatedDate,@OrganizationId,1,@isProjectBased,@SalesQuotationId);
                    SELECT CAST(SCOPE_IDENTITY() as int) SaleOrderId";



                    var id = connection.Query<int>(sql, objSaleOrder, txn).Single();

                    foreach (SaleOrderItem item in objSaleOrder.Items)
                    {
                        item.SaleOrderId = id;
                        new SaleOrderItemRepository().InsertSaleOrderItem(item, connection, txn);
                    }

                    txn.Commit();

                    return id + "|SAL/" + internalId;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return "0";
                }
            }
        }





        public SaleOrder GetSaleOrder(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select *,DoorNo +','+ Street+','+State CustomerAddress from SaleOrder S inner join Customer C on S.CustomerId=C.CustomerId  where SaleOrderId=@SaleOrderId";

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
        public List<SaleOrder> GetSaleOrdersPendingWorkshopRequest()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = @"select so.*,c.CustomerName from SaleOrder so left join WorkShopRequest wr on so.SaleOrderId=wr.SaleOrderId , Customer c   where so.CustomerId=c.CustomerId  and wr.SaleOrderId is null and so.isActive=1";
                string sql = @"SELECT  t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo +','+ Replace(Convert(varchar,SaleOrderDate,106),' ','/') SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(10)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing 
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId WHERE WR.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1 and SO.SaleOrderHoldStatus IS NULL
                             GROUP BY t.SaleOrderId,SO.CustomerOrderRef,C.CustomerName,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,SO.SaleOrderDate";
                var objSaleOrders = connection.Query<SaleOrder>(sql).ToList<SaleOrder>();

                return objSaleOrders;
            }
        }
        /// <summary>
        ///  Saleorder Pending List for  hold stock
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingSO> GetSaleOrdersForHold(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT  t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(10)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription,DATEDIFF(dd,SO.SaleOrderDate,GETDATE ()) Ageing 
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId 
                             WHERE WR.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1 and SO.SaleOrderHoldStatus IS NULL AND isProjectBased = "+ isProjectBased.ToString() + @"
                             GROUP BY t.SaleOrderId,SO.CustomerOrderRef,C.CustomerName,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,SO.SaleOrderDate";
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
                string sql = @"UPDATE SaleOrder SET SaleOrderDate = @SaleOrderDate ,CustomerId = @CustomerId ,CustomerOrderRef = @CustomerOrderRef ,VehicleModelId = @VehicleModelId,SpecialRemarks = @SpecialRemarks,PaymentTermsRemarks = @PaymentTermsRemarks,DeliveryTerms = @DeliveryTerms,CommissionAgentId = @CommissionAgentId,CommisionAmount = @CommisionAmount,SalesExecutiveId = @SalesExecutiveId   OUTPUT INSERTED.SaleOrderId  WHERE SaleOrderId = @SaleOrderId";


                var id = connection.Execute(sql, objSaleOrder);
                return id;
            }
        }
        public int DeleteSaleOrder(Unit objSaleOrder)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SaleOrder  OUTPUT DELETED.SaleOrderId WHERE SaleOrderId=@SaleOrderId";
                var id = connection.Execute(sql, objSaleOrder);
                return id;
            }
        }
        public List<Dropdown> FillCustomer()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CustomerId Id,CustomerName Name from Customer").ToList();
            }
        }

        public List<Dropdown> FillCommissionAgent()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CommissionAgentId Id,CommissionAgentName Name from CommissionAgent").ToList();
            }
        }
      
        public List<Dropdown> FillCurrency()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CurrencyId Id,CurrencyName Name from Currency").ToList();
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
        public IEnumerable<PendingSO> GetSaleOrderPending()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "Select S.SaleOrderId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef";
                query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId where isnull(SaleOrderApproveStatus,0)=0";
                return connection.Query<PendingSO>(query);
            }
        }

        public List<SaleOrderItem> GetSaleOrderItem(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SaleOrderItem where SaleOrderId=@SaleOrderId";
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
                string sql = @"Update SaleOrder set SaleOrderApproveStatus=1 WHERE SaleOrderId=@SaleOrderId";
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
        public IEnumerable<SaleOrder> GetSaleOrdersForClosing(int isProjectBased)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select SO.SaleOrderId, SaleOrderRefNo, SaleOrderDate, CustomerName, SO.SpecialRemarks, SO.EDateArrival, SO.EDateDelivery";
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
                string sql = "exec SaleOrderClosing " + Id.ToString();
                connection.Query(sql);
            }
        }
    }
}