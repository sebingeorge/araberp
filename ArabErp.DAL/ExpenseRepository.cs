using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using ArabErp.Domain;
using System.Data;


namespace ArabErp.DAL
{
    public class ExpenseRepository : BaseRepository
    {
        static string dataConnection = GetConnectionString("arab");
        public List<Dropdown> FillSupplier()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SupplierId Id,SupplierName Name from Supplier order by SupplierName").ToList();
            }
        }
        public List<Dropdown> FillAddition()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select AddDedId Id, AddDedName Name from AdditionDeduction where AddDedType = 1 order by AddDedName").ToList();
            }
        }
        public List<Dropdown> FillDeduction()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select AddDedId Id, AddDedName Name from AdditionDeduction where AddDedType = 2 order by AddDedName").ToList();
            }
        }
        public List<Dropdown> FillSO()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SaleOrderId Id, SaleOrderRefNo Name from SaleOrder").ToList();
            }
        }
        public List<Dropdown> FillJC()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select JobCardId Id, JobCardNo Name from JobCard").ToList();
            }
        }

        public List<Dropdown> FillSI()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();
                return connection.Query<Dropdown>("select SalesInvoiceId Id, SalesInvoiceRefNo Name from SalesInvoice").ToList();
            }
        }

        public IEnumerable<Dropdown> FillCurrency()
        {

            using (IDbConnection connection = OpenConnection(dataConnection))
            {

                return connection.Query<Dropdown>("SELECT CurrencyId Id,CurrencyName Name FROM Currency").ToList();
            }
        }

        public string Insert(ExpenseBill expenseBill)
        {
            int id = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                IDbTransaction trn = connection.BeginTransaction();
                try
                {
                    expenseBill.ExpenseNo = "EXP/" + DatabaseCommonRepository.GetInternalIDFromDatabase(connection, trn, typeof(ExpenseBill).Name, "0", 1);

                    if (expenseBill.SoOrJc == "JC")
                    {
                        expenseBill.SaleOrderId = null;
                    }
                    else
                    {
                        expenseBill.JobCardId = null;
                    }
                    string sql = string.Empty;
                    sql += "insert into ExpenseBill(ExpenseNo,ExpenseDate,ExpenseBillRef,ExpenseBillDate,ExpenseBillDueDate,SupplierId,ExpenseRemarks,TotalAddition,TotalDeduction,TotalAmount,CurrencyId,SaleOrderId,JobCardId)";
                    sql += " values(@ExpenseNo,@ExpenseDate,@ExpenseBillRef,@ExpenseBillDate,@ExpenseBillDueDate,@SupplierId,@ExpenseRemarks,@TotalAddition,@TotalDeduction,@TotalAmount,@CurrencyId,@SaleOrderId,@JobCardId);";
                    sql += " SELECT CAST(SCOPE_IDENTITY() as int);";

                    id = connection.Query<int>(sql, expenseBill, trn).Single();

                    foreach (var item in expenseBill.ExpenseBillItem)
                    {
                        sql = string.Empty;
                        sql += "insert into ExpenseBillItem(ExpenseId, AddDedId, ExpenseItemRate, ExpenseItemQty, ExpenseItemAmount, ExpenseItemAddDed)";
                        sql += " values(@ExpenseId, @AddDedId, @ExpenseItemRate, @ExpenseItemQty, @ExpenseItemAmount, @ExpenseItemAddDed)";

                        item.ExpenseId = id;
                        item.ExpenseItemAddDed = 1;

                        connection.Query(sql, item, trn);
                    }
                    foreach (var item in expenseBill.deductions)
                    {
                        sql = string.Empty;
                        sql += "insert into ExpenseBillItem(ExpenseId, AddDedId, ExpenseItemRate, ExpenseItemQty, ExpenseItemAmount, ExpenseItemAddDed)";
                        sql += " values(@ExpenseId, @AddDedId, @ExpenseItemRate, @ExpenseItemQty, @ExpenseItemAmount, @ExpenseItemAddDed)";

                        item.ExpenseId = id;
                        item.ExpenseItemAddDed = 2;

                        connection.Query(sql, item, trn);
                    }
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }

            }
            return expenseBill.ExpenseNo;
        }
        public IEnumerable<ExpenseBillListViewModel> GetList()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select E.ExpenseId, E.ExpenseNo, E.ExpenseDate, S.SupplierName, E.ExpenseBillRef, E.TotalAmount";
                sql += " from ExpenseBill E";
                sql += " inner join Supplier S on E.SupplierId = S.SupplierId";

                return connection.Query<ExpenseBillListViewModel>(sql);
            }
        }

        public DateTime GetDueDate(DateTime d, int sup)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                var param = new DynamicParameters();

                ExpenseBill ExpenseBill = connection.Query<ExpenseBill>(
                    "select DATEADD(day,CreditPeriod,@date) ExpenseBillDueDate FROM Supplier WHERE SupplierId= " + sup,
                    new { date = d }).Single<ExpenseBill>();
                DateTime duedate = System.DateTime.Today;
                if (ExpenseBill != null)
                {
                    duedate = ExpenseBill.ExpenseBillDueDate;
                }
                return duedate;
            }
        }

        /// <summary>
        /// Return all unapproved expense bills
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExpenseBillListViewModel> GetUnapprovedExpenseBills()
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT 
	                            EB.ExpenseId,
	                            EB.ExpenseNo,
	                            CONVERT(VARCHAR, EB.ExpenseDate, 106) ExpenseDate,
	                            ExpenseBillRef,
	                            CONVERT(VARCHAR, EB.ExpenseBillDate, 106) ExpenseBillDate,
	                            CONVERT(VARCHAR, EB.ExpenseBillDueDate, 106) ExpenseBillDueDate,
	                            S.SupplierName,
	                            EB.TotalAmount,
	                            SO.SaleOrderRefNo,
	                            CONVERT(VARCHAR, So.SaleOrderDate, 106) SaleOrderDate,
	                            JC.JobCardNo,
	                            CONVERT(VARCHAR, JC.JobCardDate, 106) JobCardDate,
	                            CASE EB.SaleOrderId WHEN NULL THEN 'SO' ELSE 'JC' END AS [Type]
                            FROM ExpenseBill EB
                            INNER JOIN Supplier S ON EB.SupplierId = S.SupplierId
                            LEFT JOIN SaleOrder SO ON EB.SaleOrderId = SO.SaleOrderId
                            LEFT JOIN JobCard JC ON EB.JobCardId = JC.JobCardId
                            WHERE EB.isApproved = 0";
                return connection.Query<ExpenseBillListViewModel>(query).ToList();
            }
        }

        public int Approve(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"UPDATE ExpenseBill SET isApproved = 1 WHERE ExpenseId = @id";
                return connection.Execute(query, new { id = id });
            }
        }

        public ExpenseBill GetExpenseBill(int id)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"SELECT *, CASE WHEN SaleOrderId IS NULL THEN 'JC' ELSE 'SO' END SoOrJc FROM ExpenseBill WHERE ExpenseId = @id";
                ExpenseBill model = connection.Query<ExpenseBill>(query, new { id = id }).Single();

                query = @"SELECT * FROM ExpenseBillItem WHERE ExpenseId = @id AND ExpenseItemAddDed = 1";
                model.ExpenseBillItem = connection.Query<ExpenseBillItem>(query, new { id = id }).ToList() ?? new List<ExpenseBillItem>();

                query = @"SELECT * FROM ExpenseBillItem WHERE ExpenseId = @id AND ExpenseItemAddDed = 2";
                model.deductions = connection.Query<ExpenseBillItem>(query, new { id = id }).ToList() ?? new List<ExpenseBillItem>();

                return model;
            }
        }
    }
}
