using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PurchaseBillItemRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
    

        public int InsertPurchaseBillItem(PurchaseBillItem model, IDbConnection connection, IDbTransaction trn)
        {
            try
            {
                string sql = @"insert into PurchaseBillItem(PurchaseBillId,GRNItemId,Rate,TaxPercentage,TaxAmount,Discount,Amount, Quantity, TotalAmount) Values (@PurchaseBillId,@GRNItemId,@Rate,@TaxPercentage,@TaxAmount,@Discount,@Amount, @Quantity, @TotAmount);
                SELECT CAST(SCOPE_IDENTITY() as int)";

                var id = connection.Query<int>(sql, model, trn).Single();
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }
//        public PurchaseBillItem GetPurchaseBillItem(int PurchaseBillId)
//        {

//            using (IDbConnection connection = OpenConnection(dataConnection))
//            {
//                string sql = @"select * from PurchaseBillItem
//                        where PurchaseBillId=@PurchaseBillId";

//                var objPurchaseBillItem = connection.Query<PurchaseBillItem>(sql, new
//                {
//                    PurchaseBillId = PurchaseBillId
//                }).First<PurchaseBillItem>();

//                return objPurchaseBillItem;
//            }
//        }
        public List<PurchaseBillItem> GetPurchaseBillItem(int PurchaseBillId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT CONCAT(GRNNo,' - ',CONVERT (VARCHAR(15),GRNDate,106))GRNNoDate,
                                P.PurchaseBillItemId,P.PurchaseBillId,P.GRNItemId,
                                ItemName,P.Quantity,U.UnitName,P.Discount,P.Rate,
                                P.TaxPercentage,
								P.TaxAmount,
								P.Amount-P.Discount Amount,--this is assessable amount
								P.Amount-P.Discount+P.TaxAmount TotAmount,
								P.isActive
                                FROM PurchaseBillItem P
                                INNER JOIN GRNItem GI ON GI.GRNItemId=P.GRNItemId
                                INNER JOIN GRN G ON G.GRNId=GI.GRNId
                                INNER JOIN Item I ON I.ItemId=GI.ItemId
                                INNER JOIN Unit U ON  U.UnitId=I.ItemUnitId
                                WHERE PurchaseBillId= @PurchaseBillId";

                var objPurchaseBillItem = connection.Query<PurchaseBillItem>(sql, new { PurchaseBillId = PurchaseBillId }).ToList<PurchaseBillItem>();

                return objPurchaseBillItem;
            }
        }
        public List<PurchaseBillItem> GetPurchaseBillItems()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseBillItem
                        where isActive=1";

                var objPurchaseBillItems = connection.Query<PurchaseBillItem>(sql).ToList<PurchaseBillItem>();

                return objPurchaseBillItems;
            }
        }

        public int DeletePurchaseBillItem(Unit objPurchaseBillItem)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete PurchaseBillItem  OUTPUT DELETED.PurchaseBillItemId WHERE PurchaseBillItemId=@PurchaseBillItemId";


                var id = connection.Execute(sql, objPurchaseBillItem);
                return id;
            }
        }
   
    }
}