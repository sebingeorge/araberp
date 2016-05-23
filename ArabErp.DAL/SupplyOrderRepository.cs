﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SupplyOrderRepository : BaseRepository
    {

        public int InsertSupplyOrder(SupplyOrder objSupplyOrder)
        {
            string sql = @"insert  into SupplyOrder(SupplyOrderDate,SupplierId,QuotaionNoAndDate,SpecialRemarks,PaymentTerms,DeliveryTerms,RequiredDate,CreatedBy,CreatedDate,OrganizationId,CreatedDate,OrganizationId) Values (@SupplyOrderDate,@SupplierId,@QuotaionNoAndDate,@SpecialRemarks,@PaymentTerms,@DeliveryTerms,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSupplyOrder).Single();
            return id;
        }


        public SupplyOrder GetSupplyOrder(int SupplyOrderId)
        {

            string sql = @"select * from SupplyOrder
                        where SupplyOrderId=@SupplyOrderId";

            var objSupplyOrder = connection.Query<SupplyOrder>(sql, new
            {
                SupplyOrderId = SupplyOrderId
            }).First<SupplyOrder>();

            return objSupplyOrder;
        }

        public List<SupplyOrder> GetSupplyOrders()
        {
            string sql = @"select * from SupplyOrder
                        where isActive=1";

            var objSupplyOrders = connection.Query<SupplyOrder>(sql).ToList<SupplyOrder>();

            return objSupplyOrders;
        }

        public int UpdateSupplyOrder(SupplyOrder objSupplyOrder)
        {
            string sql = @"UPDATE SupplyOrder SET SupplyOrderDate = @SupplyOrderDate ,SupplierId = @SupplierId ,QuotaionNoAndDate = @QuotaionNoAndDate ,SpecialRemarks = @SpecialRemarks,PaymentTerms = @PaymentTerms,DeliveryTerms = @DeliveryTerms,RequiredDate = @RequiredDate,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.SupplyOrderId  WHERE SupplyOrderId = @SupplyOrderId";


            var id = connection.Execute(sql, objSupplyOrder);
            return id;
        }

        public int DeleteSupplyOrder(Unit objSupplyOrder)
        {
            string sql = @"Delete SupplyOrder  OUTPUT DELETED.SupplyOrderId WHERE SupplyOrderId=@SupplyOrderId";


            var id = connection.Execute(sql, objSupplyOrder);
            return id;
        }


    }
}