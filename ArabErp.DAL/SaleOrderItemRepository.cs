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
            string sql = @"insert  into SaleOrderItem(SaleOrderId,SlNo,WorkDescriptionId,Remarks,PartNo,Quantity,UnitId,Rate,Discount,Amount) Values (@SaleOrderId,@SlNo,@WorkDescriptionId,@Remarks,@PartNo,@Quantity,@UnitId,@Rate,@Discount,@Amount);
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
            string sql = @"UPDATE SaleOrderItem SET SlNo = @SlNo ,WorkDescriptionId = @WorkDescriptionId ,Remarks = @Remarks,PartNo = @PartNo,Quantity = @Quantity,UnitId = @UnitId,Rate = @Rate,Discount = @Discount  OUTPUT INSERTED.SaleOrderItemId  WHERE SaleOrderItemId = @SaleOrderItemId";


            var id = connection.Execute(sql, objSaleOrderItem);
            return id;
        }

        public int DeleteSaleOrderItem(Unit objSaleOrderItem)
        {
            string sql = @"Delete SaleOrderItem  OUTPUT DELETED.SaleOrderItemId WHERE SaleOrderItemId=@SaleOrderItemId";


            var id = connection.Execute(sql, objSaleOrderItem);
            return id;
        }

        public List <Dropdown> FillWorkDesc()
        {
            var param = new DynamicParameters();
            return connection.Query<Dropdown>("select WorkDescriptionId Id ,WorkDescr Name from WorkDescription").ToList();
        }
        public List<Dropdown> FillUnit()
        {
            var param = new DynamicParameters();
            return connection.Query<Dropdown>("select UnitId Id,UnitName Name from Unit").ToList();
        }
    }
}
