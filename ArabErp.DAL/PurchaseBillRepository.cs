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
                               GRNNoDate,ItemName,GI.Quantity,GI.Unit,GI.Discount,GI.Rate,0 taxperc,0 tax,GI.Amount FROM GRN G 
                               INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                               INNER JOIN Item I ON I.ItemId=GI.ItemId
                               WHERE G .GRNId in @selectedgrn";

                var objPendingGRN = connection.Query<PurchaseBillItem>(sql, new { selectedgrn = selectedgrn }).ToList<PurchaseBillItem>();

                return objPendingGRN;
            }
        }






        public int InsertPurchaseBill(PurchaseBill objPurchaseBill)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"insert  into PurchaseBill(SupplierId,PurchaseBillDate,Remarks,EmployeeId,PurchaseBillAmount,AdditionRemarks,DeductionRemarks,Deduction,Addition,CurrencyId,CreatedBy,CreatedDate,OrganizationId) Values (@SupplierId,@PurchaseBillDate,@Remarks,@EmployeeId,@PurchaseBillAmount,@AdditionRemarks,@DeductionRemarks,@Deduction,@Addition,@CurrencyId,@CreatedBy,@CreatedDate,@OrganizationId);
            SELECT CAST(SCOPE_IDENTITY() as int)";


                var id = connection.Query<int>(sql, objPurchaseBill).Single();
                return id;
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
                string qry = @"Select G.GRNId,G.GRNNo,G.GRNDate,SO.SupplyOrderDate,SO.SupplyOrderNo,S.SupplierName,DATEDIFF(dd,G.GRNDate,GETDATE ()) Ageing from GRN G 
                             INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                             INNER JOIN  SupplyOrderItem SI ON SI.SupplyOrderItemId=GI.SupplyOrderItemId
                             INNER JOIN SupplyOrder SO ON SO.SupplyOrderId=SI.SupplyOrderId
                             INNER JOIN Supplier S ON G.SupplierId=S.SupplierId
                             WHERE S.SupplierId=@supplierId
                             GROUP BY G.GRNId,G.GRNNo,G.GRNDate,SO.SupplyOrderDate,SO.SupplyOrderNo,S.SupplierName";
                return connection.Query<PendingGRN>(qry, new { SupplierId = supplierId }).ToList();
            }
        }

    }
} 