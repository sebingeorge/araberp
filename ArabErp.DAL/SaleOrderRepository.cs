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
        public SaleOrder InsertSaleOrder(SaleOrder model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var result = new SaleOrder();
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                   

                    string sql = @"DECLARE	@return_value int,
	            	@INTERNALID bigint,
	            	@ERRORCODE nvarchar(100)
                    EXEC	@return_value = [dbo].[GET_NEXT_SYSTEM_INTERNALID]
	            	@UNIQUEID = N'0',
		            @DOCUMENTTYPEID = N'SALE ORDER',
	            	@DOUPDATE = 1,
	              	@INTERNALID = @INTERNALID OUTPUT,
	            	@ERRORCODE = @ERRORCODE OUTPUT
                    insert  into SaleOrder(SaleOrderRefNo,SaleOrderDate,CustomerId,CustomerOrderRef,CurrencyId,SpecialRemarks,PaymentTerms,DeliveryTerms,CommissionAgentId,CommissionAmount,CommissionPerc,SalesExecutiveId,EDateArrival,EDateDelivery,CreatedBy,CreatedDate,OrganizationId)
                    Values (CONCAT('SO/',@INTERNALID),@SaleOrderDate,@CustomerId,@CustomerOrderRef,@CurrencyId,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@CommissionAgentId,@CommissionAmount,@CommissionPerc,@SalesExecutiveId,@EDateArrival,@EDateDelivery,@CreatedBy,@CreatedDate,@OrganizationId);
                    SELECT CAST(SCOPE_IDENTITY() as int) SaleOrderId,CONCAT('SO/',@INTERNALID) SaleOrderRefNo";
                    
                     result = connection.Query<SaleOrder>(sql, model, trn).First<SaleOrder>();

                    var saleorderitemrepo = new SaleOrderItemRepository();
                    foreach (var item in model.Items)
                    {
                        item.SaleOrderId = result.SaleOrderId;
                        saleorderitemrepo.InsertSaleOrderItem(item, connection, trn);
                    }
                    trn.Commit();
                    
                }
                catch (Exception)
                {
                    trn.Rollback();
                    result.SaleOrderId = 0;
                    result.SaleOrderRefNo = null;
                    
                }
                return result;

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
        /// Saleorder Pending List for workshop request and hold stock
        /// </summary>
        /// <param name="model">Object of class SaleOrder</param>
        /// <returns>SaleOrders not in WorkshopRequest table</returns>
        public List<SaleOrder> GetSaleOrdersPendingWorkshopRequest()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = @"select so.*,c.CustomerName from SaleOrder so left join WorkShopRequest wr on so.SaleOrderId=wr.SaleOrderId , Customer c   where so.CustomerId=c.CustomerId  and wr.SaleOrderId is null and so.isActive=1";
                string sql = @"SELECT  t.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.SaleOrderDate,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(10)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId WHERE WR.SaleOrderId is null and SO.isActive=1 and SO.SaleOrderApproveStatus=1 and SO.SaleOrderHoldStatus IS NULL
                             GROUP BY t.SaleOrderId,SO.CustomerOrderRef,C.CustomerName,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,SO.SaleOrderDate";
                var objSaleOrders = connection.Query<SaleOrder>(sql).ToList<SaleOrder>();

                return objSaleOrders;
            }
        }

        public SaleOrder GetSaleOrderForWorkshopRequest(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = @"select so.*,c.CustomerName from SaleOrder so left join WorkShopRequest wr on so.SaleOrderId=wr.SaleOrderId , Customer c   where so.CustomerId=c.CustomerId  and wr.SaleOrderId is null and so.isActive=1";
                string sql = @"SELECT  SO.SaleOrderId,SO.CustomerOrderRef,SO.SaleOrderRefNo,SO.EDateArrival,SO.EDateDelivery,SO.CustomerId,C.CustomerName,SO.SaleOrderDate SaleOrderDate
                                FROM  SaleOrder SO  INNER JOIN Customer C  ON SO.CustomerId =C.CustomerId
                                WHERE SO.SaleOrderId =@SaleOrderId";
                var objSaleOrders = connection.Query<SaleOrder>(sql, new { SaleOrderId = SaleOrderId }).Single<SaleOrder>();

                return objSaleOrders;
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
                string sql = @"UPDATE SaleOrder SET SaleOrderDate = @SaleOrderDate ,CustomerId = @CustomerId ,CustomerOrderRef = @CustomerOrderRef ,VehicleModelId = @VehicleModelId,SpecialRemarks = @SpecialRemarks,PaymentTerms = @PaymentTerms,DeliveryTerms = @DeliveryTerms,CommissionAgentId = @CommissionAgentId,CommisionAmount = @CommisionAmount,SalesExecutiveId = @SalesExecutiveId   OUTPUT INSERTED.SaleOrderId  WHERE SaleOrderId = @SaleOrderId";


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
        public List<Dropdown> FillEmployee()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select EmployeeId Id,EmployeeName Name from Employee").ToList();
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
        /// <summary>
        /// To Show SaleOrder Details in WorkshopRequest Transaction 
        /// </summary>
        /// <returns></returns>
        public List<SaleOrder> GetSaleOrderData()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT  SO.*,t.SaleOrderId,C.CustomerName,STUFF((SELECT ', ' + CAST(W.WorkDescr AS VARCHAR(10)) [text()]
                             FROM SaleOrderItem SI inner join WorkDescription W on W.WorkDescriptionId=SI.WorkDescriptionId
                             WHERE SI.SaleOrderId = t.SaleOrderId
                             FOR XML PATH(''), TYPE)
                            .value('.','NVARCHAR(MAX)'),1,2,' ') WorkDescription
                             FROM SaleOrderItem t INNER JOIN SaleOrder SO on t.SaleOrderId=SO.SaleOrderId INNER JOIN Customer C ON SO.CustomerId =C.CustomerId
                             left join WorkShopRequest WR on SO.SaleOrderId=WR.SaleOrderId WHERE WR.SaleOrderId is null and SO.isActive=1
                             GROUP BY t.SaleOrderId,SO.CustomerOrderRef,C.CustomerName,SO.SaleOrderRefNo";

                var objSaleOrderData = connection.Query<SaleOrder>(sql).ToList<SaleOrder>();

                return objSaleOrderData;
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
        public int UpdateSOHold(int SaleOrderId, string hreason)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SaleOrder set SaleOrderHoldStatus='H',SaleOrderHoldReason=@hreason  WHERE SaleOrderId=@SaleOrderId";
                return connection.Execute(sql, new { SaleOrderId = SaleOrderId, hreason = hreason });

            }
        }
        /// <summary>
        /// Holded sale order to Release
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingSO> GetSaleOrderHolded()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "Select S.SaleOrderId,SaleOrderRefNo, SaleOrderDate, C.CustomerName, S.CustomerOrderRef";
                query += " from SaleOrder S inner join Customer C on S.CustomerId = C.CustomerId where S.SaleOrderHoldStatus ='H'";
                return connection.Query<PendingSO>(query);
            }
        }
        public int UpdateSORelease(int SaleOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Update SaleOrder set SaleOrderHoldStatus = null WHERE SaleOrderId=@SaleOrderId";
                return connection.Execute(sql, new { SaleOrderId = SaleOrderId });

            }
        }
    }
}