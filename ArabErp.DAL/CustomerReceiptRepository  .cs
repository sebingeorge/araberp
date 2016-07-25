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
    public class CustomerReceiptRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");

        public int InsertCustomerReceipt(CustomerReceipt model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction(); try
                {
                    if (model.Against == "JC")
                    {
                        model.SaleOrderId = null;
                        model.SalesInvoiceId = null;
                    }
                    if (model.Against == "SO")
                    {
                        model.JobCardId = null;
                        model.SalesInvoiceId = null;
                    }
                    if (model.Against == "SI")
                    {
                        model.JobCardId = null;
                        model.SaleOrderId = null;
                    }
                    string sql =@"INSERT INTO CustomerReceipt
                                (CustomerReceiptRefNo,CustomerReceiptDate,CustomerId,
	                            SaleOrderId,JobCardId,SalesInvoiceId,Amount,SpecialRemarks,
	                            CreatedBy,CreatedDate,OrganizationId,isActive)
                                VALUES
                               (@CustomerReceiptRefNo,@CustomerReceiptDate,@CustomerId,
                                @SaleOrderId,@JobCardId,@SalesInvoiceId,@Amount,@SpecialRemarks,
                                @CreatedBy,@CreatedDate,@OrganizationId,1)
                            SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    var id = connection.Query<int>(sql, model, txn).Single();

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
    
        public List<DirectPurchaseRequest> GetUnApprovedRequests()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                return connection.Query<DirectPurchaseRequest>(@"SELECT
                                                DirectPurchaseRequestId,
												PurchaseRequestDate,
												CreatedDate,
	                                            ISNULL(PurchaseRequestNo, '') + ' - ' + CONVERT(VARCHAR, PurchaseRequestDate, 106) PurchaseRequestNo,
	                                            ISNULL(SpecialRemarks, '-')SpecialRemarks,
	                                            TotalAmount
                                            FROM DirectPurchaseRequest D
                                            WHERE ISNULL(D.isActive, 1) = 1
                                            AND ISNULL(isApproved, 0) = 0
											ORDER BY PurchaseRequestDate DESC, CreatedDate DESC;").ToList();
            }
        }

     
    }
}
