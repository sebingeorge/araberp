﻿using System;
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
        /// 
        public PurchaseBill GetBillDueDate(int supplierId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @"SELECT (getdate()+CreditPeriod)PurchaseBillDueDate  FROM Supplier S WHERE S.SupplierId=@SupplierId";
                var objPurchaseBill = connection.Query<PurchaseBill>(sql, new { supplierId = supplierId }).Single<PurchaseBill>();
                return objPurchaseBill;
            }
        }


        public string InsertPurchaseBill(PurchaseBill objPurchaseBill)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, objPurchaseBill.OrganizationId, 13, true, trn);

                    objPurchaseBill.PurchaseBillRefNo = internalId;

                    string sql = @"insert  into PurchaseBill(PurchaseBillRefNo,SupplierId,PurchaseBillDate,PurchaseBillNoDate,PurchaseBillDueDate,
                                   CurrencyId,Remarks,PurchaseBillAmount,AdditionId,DeductionId,Deduction,Addition,CreatedBy,CreatedDate,OrganizationId)
                                   Values (@PurchaseBillRefNo,@SupplierId,@PurchaseBillDate,@PurchaseBillNoDate,@PurchaseBillDueDate,@CurrencyId,@Remarks,
                                   @PurchaseBillAmount,@AdditionId,@DeductionId,@Deduction,@Addition,@CreatedBy,@CreatedDate,@OrganizationId);
                                   SELECT CAST(SCOPE_IDENTITY() as int)";


                    var id = connection.Query<int>(sql, objPurchaseBill, trn).Single();

                    foreach (PurchaseBillItem item in objPurchaseBill.Items)
                    {
                        item.PurchaseBillId = id;
                        new PurchaseBillItemRepository().InsertPurchaseBillItem(item, connection, trn);
                    }
                    InsertLoginHistory(dataConnection, objPurchaseBill.CreatedBy, "Create", "Purchase Bill", id.ToString(), "0");
                    trn.Commit();

                    return id + "|PRB/" + internalId;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return "0";
                }
            }
        }
        public PurchaseBill GetPurchaseBill(int PurchaseBillId)
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT S.SupplierName Supplier, * FROM PurchaseBill P
                                INNER JOIN Supplier S ON P.SupplierId=S.SupplierId
                                WHERE PurchaseBillId=@PurchaseBillId";

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


        /// <summary>
        /// Delete SO HD Details
        /// </summary>
        /// <returns></returns>
        public int DeletePuchaseBillHD(int Id, string CreatedBy)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM PurchaseBill WHERE PurchaseBillId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    InsertLoginHistory(dataConnection, CreatedBy, "Delete", "Purchase Bill", id.ToString(), "0");
                    return id;

                }

            }
        }
        /// <summary>
        /// Delete SO DT Details
        /// </summary>
        /// <returns></returns>
        public int DeletePuchaseBillDT(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM PurchaseBillItem WHERE PurchaseBillId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

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
                string qry = @"Select distinct G.GRNId,G.GRNNo,G.GRNDate,S.SupplierName,S.SupplierId,DATEDIFF(dd,G.GRNDate,GETDATE ()) Ageing,ST.StockPointName, ISNULL(G.GrandTotal, 0.00) GrandTotal 
                               from GRN G 
                               INNER JOIN GRNItem GI ON G.GRNId=GI.GRNId
                               INNER JOIN Supplier S ON G.SupplierId=S.SupplierId
                               INNER JOIN Stockpoint ST ON G.WareHouseId = ST.StockPointId
                               LEFT JOIN PurchaseBillItem P ON P.GRNItemId=GI.GRNItemId 
                               WHERE P.PurchaseBillId is null AND S.SupplierId=ISNULL(NULLIF(@supplierId, 0), S.SupplierId)";
                              
                return connection.Query<PendingGRN>(qry, new { SupplierId = supplierId }).ToList();
            }
        }

        public IList<PurchaseBill> GetPurchaseBillPreviousList(int id, int supid, DateTime? from, DateTime? to,int OrganizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
	                                PurchaseBillId,PurchaseBillRefNo,CONVERT(VARCHAR(15),PurchaseBillDate, 104)PurchaseBillDate,PurchaseBillNoDate,
	                                ISNULL(S.SupplierName, '-') Supplier,ISNULL(P.PurchaseBillAmount, 0.00) PurchaseBillAmount
	                                FROM PurchaseBill P INNER JOIN Supplier S ON S.SupplierId=P.SupplierId
                                    WHERE P.PurchaseBillId = ISNULL(NULLIF(@id, 0), P.PurchaseBillId) 
                                    and P.SupplierId = ISNULL(NULLIF(@supid, 0), P.SupplierId)
                                    and P.PurchaseBillDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                                    and ISNULL(P.isActive, 1) = 1 and P.OrganizationId = @OrganizationId 
                                    ORDER BY PurchaseBillDate DESC, P.CreatedDate DESC;";
                return connection.Query<PurchaseBill>(query, new { OrganizationId = OrganizationId, id = id, supid = supid, to = to, from = from }).ToList();
            }
        }

        public DateTime GetDueDate(DateTime d,int sup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                //PurchaseBill PurchaseBill = connection.Query<PurchaseBill>("select (d+CreditPeriod) PurchaseBillDueDate FROM Supplier WHERE SupplierId= " + sup).FirstOrDefault();
                PurchaseBill PurchaseBill = connection.Query<PurchaseBill>(
                    "select DATEADD(day,CreditPeriod,@date) PurchaseBillDueDate FROM Supplier WHERE SupplierId= " + sup,
                    new { date = d }).Single<PurchaseBill>();
                DateTime duedate = System.DateTime.Today; 
                if (PurchaseBill != null)
                {
                    duedate = PurchaseBill.PurchaseBillDueDate;
                }
                return duedate;
            }
        }

        public List<Dropdown> FillCurrency()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select CurrencyId Id,CurrencyName Name from Currency").ToList();
            }
        }
    }
} 