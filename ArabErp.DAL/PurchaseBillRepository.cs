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

        public IEnumerable<GRN> PurchaseBillPendingList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = "SELECT GRNId,GRNNo,GRNDate,S.SupplierName Supplier,SupplierDCNoAndDate";
                query += " FROM GRN G INNER JOIN Supplier S ON S.SupplierId=G.SupplierId";
                return connection.Query<GRN>(query);
            }
        }



    }
} 