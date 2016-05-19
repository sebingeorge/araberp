﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SaleOrderRepository : BaseRepository
    {

        public int InsertSaleOrder(SaleOrder objSaleOrder)
        {
            string sql = @"insert  into SaleOrder(SaleOrderDate,CustomerId,CustomerOrderRef,VehicleModelId,SpecialRemarks,PaymentTerms,DeliveryTerms,CommissionAgentId,CommisionAmount,SalesExecutiveId,CreatedBy,CreatedDate) Values (@SaleOrderDate,@CustomerId,@CustomerOrderRef,@VehicleModelId,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@CommissionAgentId,@CommisionAmount,@SalesExecutiveId,@CreatedBy,@CreatedDate));
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSaleOrder).Single();
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
       
    }
}