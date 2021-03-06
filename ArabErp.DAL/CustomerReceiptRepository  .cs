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
    public class CustomerReceiptRepository : BaseRepository
    {
        private SqlConnection connection;
        static string dataConnection = GetConnectionString("arab");
        public string ConnectionString()
        {
            return dataConnection;
        }
        public CustomerReceiptRepository()
        {
            if (connection == null)
            {
                connection = ConnectionManager.connection;
            }
        }
        public List<CustomerReceipt> GetCustomerReceipt()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                string sql = @" SELECT CustomerReceiptId,CustomerReceiptRefNo,CustomerReceiptDate,CustomerName,
                                CASE WHEN SaleOrderId IS NOT NULL THEN 'Sale Order' 
                                     WHEN JobCardId IS NOT NULL THEN 'Job Card' 
	                                 ELSE 'Sales Invoice' END AS Against,Amount 
                                FROM CustomerReceipt CR
                                INNER JOIN Customer C ON C.CustomerId=CR.CustomerId
                                WHERE CR.isActive=1";

                var objCustomerReceipt = connection.Query<CustomerReceipt>(sql).ToList<CustomerReceipt>();

                return objCustomerReceipt;
            }
        }
        public int InsertCustomerReceipt(CustomerReceipt model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction txn = connection.BeginTransaction(); 
                try
                {
                    int internalId = DatabaseCommonRepository.GetInternalIDFromDatabase(connection, txn, typeof(CustomerReceipt).Name, "0", 1);

                    model.CustomerReceiptRefNo = "CR/" + internalId;

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
                    string sql = @"INSERT INTO CustomerReceipt
                                 (CustomerReceiptRefNo,CustomerReceiptDate,CustomerId,
	                             SaleOrderId,JobCardId,SalesInvoiceId,SaleOrderAmount,
                                 Amount,SpecialRemarks,CreatedBy,CreatedDate,OrganizationId,isActive)
                                 VALUES
                                (@CustomerReceiptRefNo,@CustomerReceiptDate,@CustomerId,
                                 @SaleOrderId,@JobCardId,@SalesInvoiceId,@SaleOrderAmount,
                                 @Amount,@SpecialRemarks,@CreatedBy,@CreatedDate,@OrganizationId,1)
                                 SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    var id = connection.Query<int>(sql, model, txn).Single();
                    InsertLoginHistory(dataConnection, model.CreatedBy, "Create", "Customer Receipt", id.ToString(), "0");
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
        public CustomerReceipt GetCustomerReceipt(int CustomerReceiptId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @"SELECT CASE WHEN SaleOrderId IS NOT NULL THEN 'SO' 
                                           WHEN JobCardId IS NOT NULL THEN 'JC' 
	                                       ELSE 'SI' END AS Against,* FROM CustomerReceipt
                              WHERE CustomerReceiptId=@CustomerReceiptId";

                var objCustomerReceipt = connection.Query<CustomerReceipt>(sql, new
                {
                    CustomerReceiptId = CustomerReceiptId
                }).First<CustomerReceipt>();

                return objCustomerReceipt;
            }
        }
        public CustomerReceipt UpdateCustomerReceipt(CustomerReceipt objCustomerReceipt)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                if (objCustomerReceipt.Against == "JC")
                {
                    objCustomerReceipt.SaleOrderId = null;
                    objCustomerReceipt.SalesInvoiceId = null;
                }
                if (objCustomerReceipt.Against == "SO")
                {
                    objCustomerReceipt.JobCardId = null;
                    objCustomerReceipt.SalesInvoiceId = null;
                }
                if (objCustomerReceipt.Against == "SI")
                {
                    objCustomerReceipt.JobCardId = null;
                    objCustomerReceipt.SaleOrderId = null;
                }

                string sql = @" UPDATE CustomerReceipt SET CustomerReceiptRefNo = @CustomerReceiptRefNo ,CustomerReceiptDate=@CustomerReceiptDate,CustomerId=@CustomerId,
                                SaleOrderId=@SaleOrderId,JobCardId=@JobCardId,SalesInvoiceId=@SalesInvoiceId,SaleOrderAmount=@SaleOrderAmount,Amount=@Amount,
                                SpecialRemarks=@SpecialRemarks,CreatedBy = @CreatedBy,CreatedDate = @CreatedDate,OrganizationId = @OrganizationId
                                WHERE CustomerReceiptId = @CustomerReceiptId";

                var id = connection.Execute(sql, objCustomerReceipt);
                InsertLoginHistory(dataConnection, objCustomerReceipt.CreatedBy, "Update", "Customer Receipt", id.ToString(), "0");
                return objCustomerReceipt;
            }
        }
        public int DeleteCustomerReceipt(CustomerReceipt objCustomerReceipt)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" UPDATE CustomerReceipt SET isActive=0 WHERE CustomerReceiptId=@CustomerReceiptId";
                try
                {

                    var id = connection.Execute(sql, objCustomerReceipt);
                    objCustomerReceipt.CustomerReceiptId = id;
                    result = 0;
                    InsertLoginHistory(dataConnection, objCustomerReceipt.CreatedBy, "Delete", "Customer Receipt", id.ToString(), "0");
                }
                catch (SqlException ex)
                {
                    int err = ex.Errors.Count;
                    if (ex.Errors.Count > 0) // Assume the interesting stuff is in the first error
                    {
                        switch (ex.Errors[0].Number)
                        {
                            case 547: // Foreign Key violation
                                result = 1;
                                break;

                            default:
                                result = 2;
                                break;
                        }
                    }

                }

                return result;
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

        public List<Dropdown> FillSO(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("SELECT SaleOrderId Id, SaleOrderRefNo Name FROM SaleOrder WHERE CustomerId=@ID", new { ID = Id }).ToList();
            }
        }

        public List<Dropdown> FillJC(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("SELECT JobCardId Id, JobCardNo Name FROM JobCard J INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId WHERE S.CustomerId=@ID", new { ID = Id }).ToList();
            }
        }

        public List<Dropdown> FillSI(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("SELECT SalesInvoiceId Id, SalesInvoiceRefNo Name FROM SalesInvoice J INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId WHERE S.CustomerId=@ID", new { ID = Id }).ToList();
            }
        }

        public string GetSOAmount(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                 {
                    string query = @"SELECT isnull(TotalAmount,0)TotalAmount FROM SaleOrder WHERE SaleOrderId= @Id";
                    return connection.Query<string>(query, new { Id = Id }).First();
                }
               
            }
        }


        public string GetJCAmount(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                {
                    string query = @"SELECT isnull(TotalAmount,0)TotalAmount FROM JobCard J  
                                     INNER JOIN SaleOrder S ON S.SaleOrderId=J.SaleOrderId
                                     WHERE J.JobCardId= @Id";
                    return connection.Query<string>(query, new { Id = Id }).First();
                }

            }
        }

        public string GetSIAmount(int Id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                {
                    string query = @"SELECT sum(isnull(Amount,0))Amount FROM SalesInvoice HD
                                     INNER JOIN SalesInvoiceItem DT ON HD.SalesInvoiceId=DT.SalesInvoiceId
                                     WHERE  HD.SalesInvoiceId= @Id";
                    return connection.Query<string>(query, new { Id = Id }).First();
                }

            }
        }

    }

}
