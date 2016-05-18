using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class SaleOrderItemRepository : BaseRepository
    {

        public int InsertSaleOrderItem(SaleOrderItem objSaleOrderItem)
        {
            string sql = @"insert  into SaleOrderItem(SaleOrderId,SlNo,WorkDescriptionId,Remarks,PartNo,Quantity,Unit,Rate,Discount,Amount,CreatedBy,CreatedDate,OrganizationId) Values (@SaleOrderId,@SlNo,@WorkDescriptionId,@Remarks,@PartNo,@Quantity,@Unit,@Rate,@Discount,@Amount,@CreatedBy,@CreatedDate,OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSaleOrderItem).Single();
            return id;
        }


        public SaleOrderItem GetSaleOrderItem(int SaleOrderItemId)
        {

            string sql = @"select * from SaleOrderItem
                        where SaleOrderItemId=@SaleOrderItemId";

            var objSaleOrderItem = connection.Query<SaleOrderItem>(sql, new
            {
                SaleOrderItemId = SaleOrderItemId
            }).First<SaleOrderItem>();

            return objSaleOrderItem;
        }

        public List<SaleOrderItem> GetSaleOrderItems()
        {
            string sql = @"select * from SaleOrderItem
                        where isActive=1";

            var objSaleOrderItems = connection.Query<SaleOrderItem>(sql).ToList<SaleOrderItem>();

            return objSaleOrderItems;
        }

        public int UpdateSaleOrderItem(SaleOrderItem objSaleOrderItem)
        {
            string sql = @"UPDATE SaleOrderItem SET SlNo = @SlNo ,WorkDescriptionId = @WorkDescriptionId ,Remarks = @Remarks,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,Rate = @Rate,Discount = @Discount  OUTPUT INSERTED.SaleOrderItemId  WHERE SaleOrderItemId = @SaleOrderItemId";


            var id = connection.Execute(sql, objSaleOrderItem);
            return id;
        }

        public int DeleteSaleOrderItem(Unit objSaleOrderItem)
        {
            string sql = @"Delete SaleOrderItem  OUTPUT DELETED.SaleOrderItemId WHERE SaleOrderItemId=@SaleOrderItemId";


            var id = connection.Execute(sql, objSaleOrderItem);
            return id;
        }


    }
}
