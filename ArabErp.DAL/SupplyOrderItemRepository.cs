using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class SupplyOrderItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertSupplyOrderItem(SupplyOrderItem objSupplyOrderItem, IDbConnection connection, IDbTransaction trn)
        {
           
                string sql = @"insert  into SupplyOrderItem(SupplyOrderId,PurchaseRequestItemId,SlNo,BalQty,OrderedQty,Rate,Discount,Amount,OrganizationId) Values (@SupplyOrderId,@PurchaseRequestItemId,@SlNo,@BalQty,@OrderedQty,@Rate,@Discount,@Amount,@OrganizationId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, objSupplyOrderItem,trn).Single();
                return id;
            
        }


        public SupplyOrderItem GetSupplyOrderItem(int SupplyOrderItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from SupplyOrderItem
                        where SupplyOrderItemId=@SupplyOrderItemId";

                var objSupplyOrderItem = connection.Query<SupplyOrderItem>(sql, new
                {
                    SupplyOrderItemId = SupplyOrderItemId
                }).First<SupplyOrderItem>();

                return objSupplyOrderItem;
            }
        }
        public List<SupplyOrderItem> GetSupplyOrderItems(int supplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                SOI.SupplyOrderItemId,
	                                SOI.BalQty,
	                                SOI.OrderedQty,
	                                SOI.Rate,SOI.Rate FixedRate,
	                                SOI.Discount,
	                                SOI.Amount,
                                    I.ItemId,
	                                I.ItemName,SOI.PurchaseRequestItemId,
	                                ISNULL(I.PartNo, '-') PartNo,
	                                ISNULL(PR.PurchaseRequestNo, '') + ' - ' + CONVERT(VARCHAR, PR.PurchaseRequestDate, 106) PRNODATE
                                FROM SupplyOrderItem SOI
                                INNER JOIN PurchaseRequestItem PRI ON SOI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                                INNER JOIN PurchaseRequest PR ON PRI.PurchaseRequestId = PR.PurchaseRequestId
                                INNER JOIN Item I ON PRI.ItemId = I.ItemId
                                WHERE SOI.SupplyOrderId = @supplyOrderId
                                AND ISNULL(SOI.isActive, 1) = 1;";

                var objSupplyOrderItems = connection.Query<SupplyOrderItem>(sql, new { supplyOrderId = supplyOrderId }).ToList<SupplyOrderItem>();

                return objSupplyOrderItems;
            }
        }

        public int UpdateSupplyOrderItem(SupplyOrderItem objSupplyOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE SupplyOrderItem SET SupplyOrderId = @SupplyOrderId ,PurchaseRequestItemId = @PurchaseRequestItemId ,SlNo = @SlNo ,BalQty = @BalQty,OrderedQty = @OrderedQty,Rate = @Rate,Discount = @Discount,Amount = @Amount,CreatedBy = @CreatedBy ,CreatedDate = @CreatedDate   OUTPUT INSERTED.SupplyOrderItemId  WHERE SupplyOrderItemId = @SupplyOrderItemId";


                var id = connection.Execute(sql, objSupplyOrderItem);
                return id;
            }
        }
        public int DeleteSupplyOrderItem(Unit objSupplyOrderItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete SupplyOrderItem  OUTPUT DELETED.SupplyOrderItemId WHERE SupplyOrderItemId=@SupplyOrderItemId";


                var id = connection.Execute(sql, objSupplyOrderItem);
                return id;
            }
        }

        public List<SupplyOrderItem> GetSupplyOrderItemsDTPrint(int supplyOrderId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT
	                                SOI.SupplyOrderItemId,
	                                SOI.BalQty,
	                                SOI.OrderedQty,
	                                SOI.Rate,SOI.Rate FixedRate,
	                                SOI.Discount,
	                                SOI.Amount,
                                    I.ItemId,
									I.ItemRefNo,
									UnitName,
									SOI.Discount,
	                                I.ItemName,SOI.PurchaseRequestItemId,
	                                ISNULL(I.PartNo, '-') PartNo,
	                                ISNULL(PR.PurchaseRequestNo, '') + ' - ' + CONVERT(VARCHAR, PR.PurchaseRequestDate, 106) PRNODATE
                                FROM SupplyOrderItem SOI
                                INNER JOIN PurchaseRequestItem PRI ON SOI.PurchaseRequestItemId = PRI.PurchaseRequestItemId
                                INNER JOIN PurchaseRequest PR ON PRI.PurchaseRequestId = PR.PurchaseRequestId
                                INNER JOIN Item I ON PRI.ItemId = I.ItemId
								 INNER JOIN Unit U ON U.UnitId = I.ItemUnitId

                                WHERE SOI.SupplyOrderId = @SupplyOrderId
                                AND ISNULL(SOI.isActive, 1) = 1
";

                var objSupplyOrderItems = connection.Query<SupplyOrderItem>(sql, new { supplyOrderId = supplyOrderId }).ToList<SupplyOrderItem>();

                return objSupplyOrderItems;
            }
        }
    }
}
