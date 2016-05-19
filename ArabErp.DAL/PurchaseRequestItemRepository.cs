using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;

namespace ArabErp.DAL
{
    public class PurchaseRequestItemRepository : BaseRepository
    {

        public int InsertPurchaseRequestItem(PurchaseRequestItem objPurchaseRequestItem)
        {
            string sql = @"insert  into PurchaseRequestItem(PurchaseRequestId,SlNo,ItemId,ItemDescription,PartNo,Quantity,Unit,CreatedBy,CreatedDate,OrganizationId,CreatedDate,OrganizationId) Values (@PurchaseRequestId,@SlNo,@ItemId,@ItemDescription,@PartNo,@Quantity,@Unit,@CreatedBy,@CreatedDate,@OrganizationId,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


            var id = connection.Query<int>(sql, objPurchaseRequestItem).Single();
            return id;
        }


        public PurchaseRequestItem GetPurchaseRequestItem(int PurchaseRequestItemId)
        {

            string sql = @"select * from PurchaseRequestItem
                        where PurchaseRequestItemId=@PurchaseRequestItemId";

            var objPurchaseRequestItem = connection.Query<PurchaseRequestItem>(sql, new
            {
                PurchaseRequestItemId = PurchaseRequestItemId
            }).First<PurchaseRequestItem>();

            return objPurchaseRequestItem;
        }

        public List<PurchaseRequestItem> GetPurchaseRequestItems()
        {
            string sql = @"select * from PurchaseRequestItem
                        where isActive=1";

            var objPurchaseRequestItems = connection.Query<PurchaseRequestItem>(sql).ToList<PurchaseRequestItem>();

            return objPurchaseRequestItems;
        }

        public int UpdatePurchaseRequestItem(PurchaseRequestItem objPurchaseRequestItem)
        {
            string sql = @"UPDATE PurchaseRequestItem SET PurchaseRequestId = @PurchaseRequestId ,SlNo = @SlNo ,ItemId = @ItemId ,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.PurchaseRequestItemId  WHERE PurchaseRequestItemId = @PurchaseRequestItemId";


            var id = connection.Execute(sql, objPurchaseRequestItem);
            return id;
        }

        public int DeletePurchaseRequestItem(Unit objPurchaseRequestItem)
        {
            string sql = @"Delete PurchaseRequestItem  OUTPUT DELETED.PurchaseRequestItemId WHERE PurchaseRequestItemId=@PurchaseRequestItemId";


            var id = connection.Execute(sql, objPurchaseRequestItem);
            return id;
        }


    }
}
