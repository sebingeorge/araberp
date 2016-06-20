using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;

namespace ArabErp.DAL
{
    public class PurchaseBillRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");


        public List<PurchaseBillItem> GetGRNItems(List<int> selectedgrn)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT  CONCAT(GRNNo,'/',CONVERT (VARCHAR(15),GRNDate,104))
                               GRNNoDate,GI.GRNItemId,ItemName,GI.Quantity,U.UnitName,GI.Discount,GI.Rate,0 taxperc,0 tax,GI.Amount,GI.Amount TotAmount FROM GRN G 
                               INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                               INNER JOIN Item I ON I.ItemId=GI.ItemId
                               INNER JOIN Unit U ON  U.UnitId=I.ItemUnitId
                               WHERE G .GRNId in @selectedgrn";

                var objPendingGRN = connection.Query<PurchaseBillItem>(sql, new { selectedgrn = selectedgrn }).ToList<PurchaseBillItem>();

                return objPendingGRN;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int InsertPurchaseBill(PurchaseBill model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    int id = 0;

                    string sql = @"insert  into PurchaseBill(PurchaseBillRefNo,SupplierId,PurchaseBillDate,Remarks,PurchaseBillAmount
                             ,AdditionRemarks,DeductionRemarks,Deduction,Addition,CreatedBy,CreatedDate,OrganizationId)
                              Values (@PurchaseBillRefNo,@SupplierId,@PurchaseBillDate,@Remarks,@PurchaseBillAmount
                             ,@AdditionRemarks,@DeductionRemarks,@Deduction,@Addition,@CreatedBy,@CreatedDate,@OrganizationId);
                             SELECT CAST(SCOPE_IDENTITY() as int)";

                    id = connection.Query<int>(sql, model, trn).Single();
                    var purchasebillitemrepo = new PurchaseBillItemRepository();
                    foreach (var item in model.Items)
                    {
                        item.PurchaseBillId = id;
                        new PurchaseBillItemRepository().InsertPurchaseBillItem(item, connection, trn);

                    }

                    trn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return 0;
                }


            }
        }
        public PurchaseBill GetPurchaseBill(int PurchaseBillId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseBill
                        where PurchaseBillId=@PurchaseBillId";

                var objPurchaseBill = connection.Query<PurchaseBill>(sql, new
                {
                    PurchaseBillId = PurchaseBillId
                }).First<PurchaseBill>();

                return objPurchaseBill;
            }
        }

        public List<PurchaseBill> GetPurchaseBills()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"select * from PurchaseBill
                        where isActive=1";

                var objPurchaseBills = connection.Query<PurchaseBill>(sql).ToList<PurchaseBill>();

                return objPurchaseBills;
            }
        }



        public int DeletePurchaseBill(Unit objPurchaseBill)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"Delete PurchaseBill  OUTPUT DELETED.PurchaseBillId WHERE PurchaseBillId=@PurchaseBillId";


                var id = connection.Execute(sql, objPurchaseBill);
                return id;
            }
        }
        /// <summary>
        /// Pending GRN For Purchase Bill
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PendingGRN> GetGRNPending(int supplierId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string qry = @"Select G.GRNId,G.GRNNo,G.GRNDate,S.SupplierName,S.SupplierId,DATEDIFF(dd,G.GRNDate,GETDATE ()) Ageing 
                               from GRN G 
                               INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                               INNER JOIN Supplier S ON G.SupplierId=S.SupplierId
                               LEFT JOIN PurchaseBillItem P ON P.GRNItemId=GI.GRNItemId 
                               WHERE P.PurchaseBillId is null AND S.SupplierId=@supplierId 
                               GROUP BY G.GRNId,G.GRNNo,G.GRNDate,S.SupplierName,S.SupplierId";
                return connection.Query<PendingGRN>(qry, new { SupplierId = supplierId }).ToList();
            }
        }

    }
} 