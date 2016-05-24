using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SaleOrderRepository : BaseRepository
    {

        public int InsertSaleOrder(SaleOrder model)
        {
                         string sql = @"insert  into SaleOrder(SaleOrderRefNo,SaleOrderDate,CustomerId,CustomerOrderRef,VehicleModelId,SpecialRemarks,PaymentTerms,DeliveryTerms,CommissionAgentId,CommisionAmount,SalesExecutiveId,CreatedBy,CreatedDate,OrganizationId) Values (@SaleOrderRefNo,@SaleOrderDate,@CustomerId,@CustomerOrderRef,@VehicleModelId,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@CommissionAgentId,@CommisionAmount,@SalesExecutiveId,@CreatedBy,@CreatedDate,@OrganizationId);
           

                        SELECT CAST(SCOPE_IDENTITY() as int)";


                        var id = connection.Query<int>(sql, model).Single();
            var saleorderitemrepo=new SaleOrderItemRepository();
            foreach (var item in model.Items)
            {
                item.SaleOrderId = id;
                saleorderitemrepo.InsertSaleOrderItem(item);
            }

                        return id;
        }


        public SaleOrder GetSaleOrder(int SaleOrderId)
        {

            string sql = @"select * from SaleOrder
                        where SaleOrderId=@SaleOrderId";

            var objSaleOrder = connection.Query<SaleOrder>(sql, new
            {
                SaleOrderId = SaleOrderId
            }).First<SaleOrder>();

            return objSaleOrder;
        }
        public List<SaleOrder> GetSaleOrders()
        {
            string sql = @"select * from SaleOrder
                        where isActive=1";

            var objSaleOrders = connection.Query<SaleOrder>(sql).ToList<SaleOrder>();

            return objSaleOrders;
        }
        public int UpdateSaleOrder(SaleOrder objSaleOrder)
        {
            string sql = @"UPDATE SaleOrder SET SaleOrderDate = @SaleOrderDate ,CustomerId = @CustomerId ,CustomerOrderRef = @CustomerOrderRef ,VehicleModelId = @VehicleModelId,SpecialRemarks = @SpecialRemarks,PaymentTerms = @PaymentTerms,DeliveryTerms = @DeliveryTerms,CommissionAgentId = @CommissionAgentId,CommisionAmount = @CommisionAmount,SalesExecutiveId = @SalesExecutiveId   OUTPUT INSERTED.SaleOrderId  WHERE SaleOrderId = @SaleOrderId";


            var id = connection.Execute(sql, objSaleOrder);
            return id;
        }
        public int DeleteSaleOrder(Unit objSaleOrder)
        {
            string sql = @"Delete SaleOrder  OUTPUT DELETED.SaleOrderId WHERE SaleOrderId=@SaleOrderId";
            var id = connection.Execute(sql, objSaleOrder);
            return id;
        }
        public List<Dropdown> FillCustomer()
        {
            var param = new DynamicParameters();
            return connection.Query<Dropdown>("select CustomerId Id,CustomerName Name from Customer").ToList();
        }
        public List<Dropdown> FillVehicle()
        {
            var param = new DynamicParameters();
            return connection.Query<Dropdown>("select VehicleModelId Id,VehicleModelName Name from VehicleModel").ToList();
        }
        public List<Dropdown> FillCommissionAgent()
        {
            var param = new DynamicParameters();
            return connection.Query<Dropdown>("select CommissionAgentId Id,CommissionAgentName Name from CommissionAgent").ToList();
        }
        public List<Dropdown> FillEmployee()
        {
            var param = new DynamicParameters();
            return connection.Query<Dropdown>("select EmployeeId Id,EmployeeName Name from Employee").ToList();
        }
    }
}