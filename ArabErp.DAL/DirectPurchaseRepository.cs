using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using ArabErp.Domain;

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
	                                [isApproved]
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
                                    0
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
    }
}
