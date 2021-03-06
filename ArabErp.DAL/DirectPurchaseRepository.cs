﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using ArabErp.Domain;
using System.Data.SqlClient;

namespace ArabErp.DAL
{
    public class DirectPurchaseRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string InsertDirectPurchaseRequest(DirectPurchaseRequest model)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {

                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 10, true, txn);
                    model.PurchaseRequestNo = internalId;


                    if (model.SoOrJc == "JC")
                    {
                        model.SaleOrderId = null;
                    }
                    else if (model.SoOrJc == "SO")
                    {
                        model.JobCardId = null;
                    }
                    else model.isStockOrder = true;
                    model.TotalAmount = model.items.Sum(m => (m.Quantity * m.Rate));
                    string sql = @"INSERT INTO DirectPurchaseRequest
                                (
	                                [PurchaseRequestNo],
	                                [PurchaseRequestDate],
	                                [SpecialRemarks],
	                                [RequiredDate],
	                                [TotalAmount],
	                                [CreatedBy],
	                                [CreatedDate],
	                                [OrganizationId],
	                                [isActive],
	                                [isApproved],
                                    [SaleOrderId],
                                    [JobCardId],
                                    [isStockOrder]
                                )
                                VALUES
                                (
	                                @PurchaseRequestNo,
                                    @PurchaseRequestDate,
                                    @SpecialRemarks,
                                    @RequiredDate,
                                    @TotalAmount,
                                    @CreatedBy,
                                    @CreatedDate,
                                    @OrganizationId,
                                    1,
                                    0,
                                    @SaleOrderId,
                                    @JobCardId,
                                    @isStockOrder
                                )
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    id = connection.Query<int>(sql, model, txn).Single<int>();

                    var supplyorderitemrepo = new DirectPurchaseItemRepository();
                    foreach (var item in model.items)
                    {
                        item.DirectPurchaseRequestId = id;
                        new DirectPurchaseItemRepository().InsertDirectPurchaseRequestItem(item, connection, txn);

                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Direct Purchase", id.ToString(), "0");
                    txn.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    txn.Rollback();
                    throw;
                }
                return model.PurchaseRequestNo;
            }
        }

        //            foreach (var item in model.items)
        //            {
        //                item.DirectPurchaseRequestId = id;
        //                new DirectPurchaseItemRepository().InsertDirectPurchaseRequestItem(item, connection, txn);
        //            }
        //            txn.Commit();
        //            return id;
        //        }
        //        catch (Exception)
        //        {
        //            txn.Rollback();
        //            return 0;
        //        }
        //    }
        //}
        /// <summary>
        /// Returns the purchase limit based on organization Id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public string GetPurchaseLimit(int organizationId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    return connection.Query<string>(@"SELECT S.SymbolName INTO #SYM FROM Organization O INNER JOIN Currency C ON O.CurrencyId = C.CurrencyId INNER JOIN Symbol S ON C.CurrencySymbolId = S.SymbolId WHERE O.OrganizationId = @organizationId;
                        SELECT TOP 1 CONVERT(VARCHAR, EffectiveDate, 106)+'|'+/*ISNULL((SELECT SymbolName FROM #SYM), '')+' '+*/CAST(Limit AS VARCHAR) FROM DirectPurchaseRequestLimit WHERE EffectiveDate <= GETDATE() AND OrganizationId = @organizationId ORDER BY EffectiveDate DESC;
                        DROP TABLE #SYM;",
                        new { organizationId = organizationId }).First();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Checks if request no exists
        /// </summary>
        /// <param name="requestNo"></param>
        /// <returns></returns>
        public int isNotExist(string requestNo)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<int>(@"IF NOT EXISTS(SELECT PurchaseRequestNo FROM DirectPurchaseRequest WHERE PurchaseRequestNo = @requestNo AND ISNULL(isActive, 1) = 1) SELECT 1; ELSE SELECT 0;",
                    new { requestNo = requestNo }).First();
            }
        }

        public int validateTotal(int total)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<int>(@"IF((SELECT TOP 1 Limit FROM DirectPurchaseRequestLimit WHERE EffectiveDate <= GETDATE() AND OrganizationId = 1 ORDER BY EffectiveDate DESC) >= @total) SELECT 1;ELSE SELECT 0;",
                    new { total = total }).First();
            }
        }
        /// <summary>
        /// Return all un-approved direct purchase requests
        /// </summary>
        /// <returns></returns>
        public List<DirectPurchaseRequest> GetUnApprovedRequests(int UserID)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<DirectPurchaseRequest>(@"SELECT
                                                DirectPurchaseRequestId,PurchaseRequestDate,CreatedDate,
	                                            ISNULL(PurchaseRequestNo, '') + ' - ' + CONVERT(VARCHAR, PurchaseRequestDate, 106) PurchaseRequestNo,
	                                            ISNULL(SpecialRemarks, '-')SpecialRemarks,
	                                            TotalAmount,UserAmount=(SELECT Isnull(Amount,0) FROM [user] WHERE UserId=1015) 
                                            FROM DirectPurchaseRequest D
                                            WHERE ISNULL(D.isActive, 1) = 1
                                            AND ISNULL(isApproved, 0) = 0
											ORDER BY PurchaseRequestDate DESC, CreatedDate DESC;").ToList();
            }
        }

        public int ApproveRequest(int id)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    connection.Execute(@"UPDATE DirectPurchaseRequest SET isApproved = 1 WHERE DirectPurchaseRequestId = @id",
                        new { id = id });
                    return 1;
                }
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IList<DirectPurchaseRequest> GetPreviousList(int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT
	                                DirectPurchaseRequestId,
	                                PurchaseRequestNo,
	                                CONVERT(VARCHAR, PurchaseRequestDate, 106) PurchaseRequestDate,
	                                DP.SpecialRemarks,
	                                DP.RequiredDate,
	                                DATEDIFF(DAY, PurchaseRequestDate, GETDATE()) Ageing,
	                                DATEDIFF(DAY, GETDATE(), DP.RequiredDate) DaysLeft,
	                                DP.TotalAmount,
	                                ISNULL(SO.SaleOrderId, 0) SaleOrderId,
	                                SO.SaleOrderRefNo,
	                                CONVERT(VARCHAR, SO.SaleOrderDate, 106) SaleOrderDate,
	                                JC.JobCardNo,
	                                CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                                ISNULL(JC.JobCardId, 0) JobCardId,
	                                DP.CreatedBy
                                FROM DirectPurchaseRequest DP
                                LEFT JOIN SaleOrder SO ON DP.SaleOrderId = SO.SaleOrderId
                                LEFT JOIN JobCard JC ON DP.JobCardId = JC.JobCardId
                                WHERE DP.OrganizationId = @OrganizationId
                                AND DP.isActive = 1
                                ORDER BY DP.PurchaseRequestDate DESC, DP.CreatedDate DESC";
                return connection.Query<DirectPurchaseRequest>(query, new { OrganizationId = organizationId }).ToList();
            }
        }

        public DirectPurchaseRequest GetDirectPurchaseRequest(int DirectPurchaseRequestId)
        {
            try
            {
                using (IDbConnection connection = OpenConnection(dataConnection))
                {
                    string query = @"SELECT *, CASE WHEN SaleOrderId IS NOT NULL THEN 'SO' WHEN JobCardId IS NOT NULL THEN 'JC' END AS SoOrJc FROM DirectPurchaseRequest
                                    WHERE DirectPurchaseRequestId = @DirectPurchaseRequestId
	                                AND ISNULL(isActive, 1) = 1";

                    var objDirectPurchaseRequest = connection.Query<DirectPurchaseRequest>(query, new
                    {
                        DirectPurchaseRequestId = DirectPurchaseRequestId
                    }).First<DirectPurchaseRequest>();

                    return objDirectPurchaseRequest;
                }
            }
            catch (InvalidOperationException iox)
            {
                throw iox;
            }
            catch (SqlException sx)
            {
                throw sx;
            }
            catch (NullReferenceException nx)
            {
                throw nx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<DirectPurchaseRequestItem> GetDirectPurchaseRequestItems(int DirectPurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT ItemName,PartNo,UnitName UoM,* FROM DirectPurchaseRequestItem D
                               INNER JOIN Item I ON I.ItemId=D.ItemId
                               INNER JOIN Unit U ON U.UnitId=I.ItemUnitId
                               WHERE DirectPurchaseRequestId = @DirectPurchaseRequestId;";

                var objDirectPurchaseRequestItems = connection.Query<DirectPurchaseRequestItem>(sql, new { DirectPurchaseRequestId = DirectPurchaseRequestId }).ToList<DirectPurchaseRequestItem>();

                return objDirectPurchaseRequestItems;
            }
        }

        public object GetPurchaseIndentList(DateTime? from, DateTime? to,string id, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //the model used for Purchase Indent is DirectPurchaseRequest
                string query = @"SELECT
	                                PurchaseRequestId DirectPurchaseRequestId,
	                                PurchaseRequestNo,
	                                CONVERT(VARCHAR, PurchaseRequestDate, 106) PurchaseRequestDate,
	                                SpecialRemarks,
	                                CONVERT(VARCHAR, RequiredDate, 106) RequiredDate
                                FROM PurchaseRequest PR
                                WHERE PR.OrganizationId = @org
                                AND WorkShopRequestId IS NULL
                                and ISNULL(PR.PurchaseRequestNo,'') like'%'+@id+'%'
                                and PR.PurchaseRequestDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) 
                                ORDER BY PurchaseRequestId DESC, PurchaseRequestNo DESC";
                return connection.Query<DirectPurchaseRequest>(query, new { org = organizationId, from = from,to=to,id=id}).ToList();
            }
        }

        public DirectPurchaseRequest GetPurchaseIndent(int id, int organizationId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                //the model used for Purchase Indent is DirectPurchaseRequest
                string query = @"SELECT
	                                PurchaseRequestId DirectPurchaseRequestId, *
                                FROM PurchaseRequest
                                WHERE PurchaseRequestId = @id
                                AND OrganizationId = @org";
                DirectPurchaseRequest model = connection.Query<DirectPurchaseRequest>(query, new { org = organizationId, @id = id }, txn).FirstOrDefault();
                string sql = @"SELECT
	                                PRI.PurchaseRequestItemId DirectPurchaseRequestItemId, PRI.*,I.ItemName,I.PartNo,
									U.UnitName UoM
                                FROM PurchaseRequestItem PRI
								INNER JOIN Item I ON PRI.ItemId = I.ItemId
								INNER JOIN Unit U ON I.ItemUnitId = U.UnitId
                                WHERE PurchaseRequestId = @id";
                model.items = connection.Query<DirectPurchaseRequestItem>(sql, new { id = id }, txn).ToList();

                #region checking isUsed
                try
                {
                    sql = @" DELETE FROM PurchaseRequestItem WHERE PurchaseRequestId=@Id;
                             DELETE FROM PurchaseRequest OUTPUT deleted.PurchaseRequestNo WHERE PurchaseRequestId = @Id;";

                    string output = connection.Query<string>(sql, new { Id = id }, txn).First();
                    txn.Rollback();
                    model.isUsed = false;
                }
                catch (Exception ex)
                {
                    model.isUsed = true;
                    txn.Rollback();
                } 
                #endregion

                return model;
            }
        }

        public int UpdatePurchaseIndent(DirectPurchaseRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string query = @"UPDATE PurchaseRequest SET
                                PurchaseRequestDate = @PurchaseRequestDate,
                                SpecialRemarks = @SpecialRemarks,
                                RequiredDate = @RequiredDate,
                                CreatedBy = @CreatedBy,
                                CreatedDate = @CreatedDate
                                WHERE PurchaseRequestId = @DirectPurchaseRequestId";
                    int id = connection.Execute(query, model, txn);
                    if (id <= 0) throw new Exception();
                    id = new DirectPurchaseItemRepository().UpdatePurchaseIndentItem(model, connection, txn);
                    if (id <= 0) throw new Exception();
                    txn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public int CHECK(int DirectPurchaseRequestId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT Count(DirectPurchaseRequestId)Count FROM DirectPurchaseRequest 
                                WHERE isApproved=1 AND DirectPurchaseRequestId=@DirectPurchaseRequestId";

                var id = connection.Query<int>(sql, new { DirectPurchaseRequestId = DirectPurchaseRequestId }).FirstOrDefault();

                return id;

            }

        }

        /// <summary>
        /// Delete HD Details
        /// </summary>
        /// <returns></returns>
        public int DeleteDirectPurchaseHD(int Id, string CreatedBy)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM DirectPurchaseRequest WHERE DirectPurchaseRequestId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    InsertLoginHistory(dataConnection, CreatedBy, "Delete", "Direct Purchase", id.ToString(), "0");
                    return id;

                }

            }
        }
        /// <summary>
        /// Delete DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteDirectPurchaseDT(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM DirectPurchaseRequestItem WHERE DirectPurchaseRequestId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

        public string InsertPurchaseIndent(DirectPurchaseRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    model.PurchaseRequestNo = DatabaseCommonRepository.GetNewDocNo(connection, model.OrganizationId, 36, true, txn);
                    string sql = @"insert  into PurchaseRequest
                                (PurchaseRequestNo,PurchaseRequestDate,SpecialRemarks,RequiredDate,CreatedBy,CreatedDate,OrganizationId) Values (@PurchaseRequestNo,@PurchaseRequestDate,@SpecialRemarks,@RequiredDate,@CreatedBy,@CreatedDate,@OrganizationId);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                    var id = connection.Query<int>(sql, model, txn).FirstOrDefault();
                    foreach (DirectPurchaseRequestItem item in model.items)
                    {
                        item.DirectPurchaseRequestId = id;
                        if (item.Quantity == null || item.Quantity == 0) continue;
                        new DirectPurchaseItemRepository().InsertPurchaseIndentItem(item, connection, txn);
                    }
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Purchase Request", id.ToString(), "0");
                    txn.Commit();
                    return model.PurchaseRequestNo;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public decimal GetStockQuantity(int itemId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"select sum(ISNULL(Quantity,0)) StockQuantity from StockUpdate where ItemId=@ItemId group by ItemId";
                return connection.Query<decimal>(query, new { ItemId = itemId }).FirstOrDefault();
            }
        }

        public string DeletePurchaseIndent(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction();
                try
                {
                    string sql = @" DELETE FROM PurchaseRequestItem WHERE PurchaseRequestId=@Id;
                                    DELETE FROM PurchaseRequest OUTPUT deleted.PurchaseRequestNo WHERE PurchaseRequestId = @Id;";
                                   
                    string output = connection.Query<string>(sql, new { Id = Id }, txn).First();
                    txn.Commit();
                    return output;
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    throw ex;
                }
            }
        }


    }
}
