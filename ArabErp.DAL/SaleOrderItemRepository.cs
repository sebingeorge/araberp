using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SaleOrderItemRepository : BaseRepository
    {

        static string dataConnection = GetConnectionString("arab");

        public int InsertSaleOrderItem(SaleOrderItem objSaleOrderItem, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"insert  into SaleOrderItem(SaleOrderId, WorkDescriptionId, VehicleModelId) 
                                                    Values (@SaleOrderId, @WorkDescriptionId, @VehicleModelId);
                       
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = 0;
                //for (int i = 0; i < objSaleOrderItem.Quantity; i++)
                //{
                    id = connection.Query<int>(sql, objSaleOrderItem, trn).Single();
                //}
                return id;
            }
            catch (Exception)
            {
                throw;
            }
            
        }


        public SaleOrderItem GetSaleOrderItem(int SaleOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SaleOrderItem
                        where SaleOrderItemId=@SaleOrderItemId";

                var objSaleOrderItem = connection.Query<SaleOrderItem>(sql, new
                {
                    SaleOrderItemId = SaleOrderItemId
                }).First<SaleOrderItem>();

                return objSaleOrderItem;
            }
        }

        public List<SaleOrderItem> GetSaleOrderItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SaleOrderItem
                        where isActive=1";

                var objSaleOrderItems = connection.Query<SaleOrderItem>(sql).ToList<SaleOrderItem>();

                return objSaleOrderItems;
            }
        }

        public int UpdateSaleOrderItem(SaleOrderItem objSaleOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SaleOrderItem SET SlNo = @SlNo ,WorkDescriptionId = @WorkDescriptionId ,Remarks = @Remarks,PartNo = @PartNo,Quantity = @Quantity,UnitId = @UnitId,Rate = @Rate,Discount = @Discount  OUTPUT INSERTED.SaleOrderItemId  WHERE SaleOrderItemId = @SaleOrderItemId";


                var id = connection.Execute(sql, objSaleOrderItem);
                return id;
            }
        }

        public int DeleteSaleOrderItem(Unit objSaleOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SaleOrderItem  OUTPUT DELETED.SaleOrderItemId WHERE SaleOrderItemId=@SaleOrderItemId";


                var id = connection.Execute(sql, objSaleOrderItem);
                return id;
            }
        }
        public int InsertSaleOrderMaterial(SalesQuotationMaterial objSalesQuotationMaterial, IDbConnection connection, IDbTransaction trn)
        {


            string sql = @"insert  into SaleOrderMaterial(SaleOrderId,ItemId,Quantity,Rate,Amount,Discount,isActive) Values (@SaleOrderId,@ItemId,@Quantity,@Rate,@Amount,@Discount,1);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objSalesQuotationMaterial, trn).Single();
            return id;

        }
      
    }
}
