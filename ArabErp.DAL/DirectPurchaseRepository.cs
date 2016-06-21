using System;
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

        public int InsertDirectPurchaseRequest(DirectPurchaseRequest model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction(); try
                {
                    if (model.SoOrJc == "JC")
                    {
                        model.SaleOrderId = null;
                    }
                    else
                    {
                        model.JobCardId = null;
                    }
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
                                    [JobCardId]
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
                                    @JobCardId
                                )
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    var id = connection.Query<int>(sql, model, txn).Single();

                    foreach (var item in model.items)
                    {
                        item.DirectPurchaseRequestId = id;
                        new DirectPurchaseItemRepository().InsertDirectPurchaseRequestItem(item, connection, txn);
                    }
                    txn.Commit();
                    return id;
                }
                catch (Exception)
                {
                    txn.Rollback();
                    return 0;
                }
            }
        }
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
                        SELECT TOP 1 CONVERT(VARCHAR, EffectiveDate, 106)+'|'+ISNULL((SELECT SymbolName FROM #SYM), '')+' '+CAST(Limit AS VARCHAR) FROM DirectPurchaseRequestLimit WHERE EffectiveDate <= GETDATE() AND OrganizationId = @organizationId ORDER BY EffectiveDate DESC;
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
        public List<DirectPurchaseRequest> GetUnApprovedRequests()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<DirectPurchaseRequest>(@"SELECT
                                                DirectPurchaseRequestId,
	                                            ISNULL(PurchaseRequestNo, '') + ' - ' + CONVERT(VARCHAR, PurchaseRequestDate, 106) PurchaseRequestNo,
	                                            SpecialRemarks,
	                                            TotalAmount
                                            FROM DirectPurchaseRequest D
                                            WHERE ISNULL(D.isActive, 1) = 1
                                            AND ISNULL(isApproved, 0) = 0;").ToList();
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
    }
}
