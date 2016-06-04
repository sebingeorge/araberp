using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PurchaseRequestItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


       
        /// <summary>
        /// Insert PurchaseRequestItem
        /// </summary>
        /// <param name="PurchaseRequestItemId"></param>
        /// <returns></returns>
        public int InsertPurchaseRequestItem(PurchaseRequestItem model, IDbConnection connection, IDbTransaction trn)
        {
            try
            {

                string sql = @"insert  into PurchaseRequestItem(PurchaseRequestId,SlNo,ItemId,Remarks,Quantity,isActive) Values (@PurchaseRequestId,@SlNo,@ItemId,@Remarks,@Quantity,1);
                SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, model, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }

        }


        public PurchaseRequestItem GetPurchaseRequestItem(int PurchaseRequestItemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"select * from PurchaseRequestItem
                        where PurchaseRequestItemId=@PurchaseRequestItemId";

                var objPurchaseRequestItem = connection.Query<PurchaseRequestItem>(sql, new
                {
                    PurchaseRequestItemId = PurchaseRequestItemId
                }).First<PurchaseRequestItem>();

                return objPurchaseRequestItem;
            }
        }
        public List<PurchaseRequestItem> GetPurchaseRequestItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseRequestItem
                        where isActive=1";

                var objPurchaseRequestItems = connection.Query<PurchaseRequestItem>(sql).ToList<PurchaseRequestItem>();

                return objPurchaseRequestItems;
            }
        }
        public int UpdatePurchaseRequestItem(PurchaseRequestItem objPurchaseRequestItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"UPDATE PurchaseRequestItem SET PurchaseRequestId = @PurchaseRequestId ,SlNo = @SlNo ,ItemId = @ItemId ,ItemDescription = @ItemDescription,PartNo = @PartNo,Quantity = @Quantity,Unit = @Unit,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate  OUTPUT INSERTED.PurchaseRequestItemId  WHERE PurchaseRequestItemId = @PurchaseRequestItemId";


                var id = connection.Execute(sql, objPurchaseRequestItem);
                return id;
            }
        }

        public int DeletePurchaseRequestItem(Unit objPurchaseRequestItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete PurchaseRequestItem  OUTPUT DELETED.PurchaseRequestItemId WHERE PurchaseRequestItemId=@PurchaseRequestItemId";


                var id = connection.Execute(sql, objPurchaseRequestItem);
                return id;
            }
        }


    }
}
