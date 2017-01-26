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

                return connection.Query<Dropdown>("SELECT CurrencyId Id,CurrencyName Name FROM Currency WHERE isActive=1").ToList();
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
                    var internalId = DatabaseCommonRepository.GetNewDocNo(connection, expenseBill.OrganizationId, 14, true, trn);

                    expenseBill.ExpenseNo = internalId;

                    if (expenseBill.SoOrJc == "JC")
                    {
                        expenseBill.SaleOrderId = null;
                    }
                    else
                    {
                        expenseBill.JobCardId = null;
                    }
                    string sql = string.Empty;
                    sql += "insert into ExpenseBill(ExpenseNo,ExpenseDate,ExpenseBillRef,ExpenseBillDate,ExpenseBillDueDate,SupplierId,ExpenseRemarks,TotalAddition,TotalDeduction,TotalAmount,CurrencyId,SaleOrderId,JobCardId,OrganizationId)";
                    sql += " values(@ExpenseNo,@ExpenseDate,@ExpenseBillRef,@ExpenseBillDate,@ExpenseBillDueDate,@SupplierId,@ExpenseRemarks,@TotalAddition,@TotalDeduction,@TotalAmount,@CurrencyId,@SaleOrderId,@JobCardId,@OrganizationId);";
                    sql += " SELECT CAST(SCOPE_IDENTITY() as int);";

                    id = connection.Query<int>(sql, expenseBill, trn).Single();

                    foreach (var item in expenseBill.ExpenseBillItem)
                    {
                        sql = string.Empty;
                        sql += "insert into ExpenseBillItem(ExpenseId, AddDedId, ExpenseItemRate, ExpenseItemQty, ExpenseItemAmount, ExpenseItemAddDed)";
                        sql += " values(@ExpenseId,ISNULL(@AddDedId,0),  @ExpenseItemRate, @ExpenseItemQty, @ExpenseItemAmount, @ExpenseItemAddDed)";

                        item.ExpenseId = id;
                        item.ExpenseItemAddDed = 1;

                        connection.Query(sql, item, trn);
                    }
                    foreach (var item in expenseBill.deductions)
                    {
                        sql = string.Empty;
                        sql += "insert into ExpenseBillItem(ExpenseId, AddDedId, ExpenseItemRate, ExpenseItemQty, ExpenseItemAmount, ExpenseItemAddDed)";
                        sql += " values(@ExpenseId, ISNULL(@AddDedId,0),  @ExpenseItemRate, @ExpenseItemQty, @ExpenseItemAmount, @ExpenseItemAddDed)";

                        item.ExpenseId = id;
                        item.ExpenseItemAddDed = 2;

                        connection.Query(sql, item, trn);
                    }
                    InsertLoginHistory(dataConnection, expenseBill.CreatedBy, "Create", "Expense Bill", id.ToString(), "0");
                    trn.Commit();
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                }

            }
            return expenseBill.ExpenseNo;
        }
       public string InsertDT(ExpenseBill expenseBill)
       {
           int id = 0;
           using (IDbConnection connection = OpenConnection(dataConnection))
           {
               IDbTransaction trn = connection.BeginTransaction();
               try
               {
                   var internalId = DatabaseCommonRepository.GetNewDocNo(connection, expenseBill.OrganizationId, 14, true, trn);

                   expenseBill.ExpenseNo = internalId;

                   if (expenseBill.SoOrJc == "JC")
                   {
                       expenseBill.SaleOrderId = null;
                   }
                   else
                   {
                       expenseBill.JobCardId = null;
                   }
                   string sql = string.Empty;
                   //sql += "insert into ExpenseBill(ExpenseNo,ExpenseDate,ExpenseBillRef,ExpenseBillDate,ExpenseBillDueDate,SupplierId,ExpenseRemarks,TotalAddition,TotalDeduction,TotalAmount,CurrencyId,SaleOrderId,JobCardId,OrganizationId)";
                   //sql += " values(@ExpenseNo,@ExpenseDate,@ExpenseBillRef,@ExpenseBillDate,@ExpenseBillDueDate,@SupplierId,@ExpenseRemarks,@TotalAddition,@TotalDeduction,@TotalAmount,@CurrencyId,@SaleOrderId,@JobCardId,@OrganizationId);";
                   //sql += " SELECT CAST(SCOPE_IDENTITY() as int);";

                   //id = connection.Query<int>(sql, expenseBill, trn).Single();

                   foreach (var item in expenseBill.ExpenseBillItem)
                   {
                       sql = string.Empty;
                       sql += "insert into ExpenseBillItem(ExpenseId, AddDedId, ExpenseItemRate, ExpenseItemQty, ExpenseItemAmount, ExpenseItemAddDed)";
                       sql += " values(@ExpenseId,ISNULL(@AddDedId,0), @ExpenseItemRate, @ExpenseItemQty, @ExpenseItemAmount, @ExpenseItemAddDed)";

                       item.ExpenseId = expenseBill.ExpenseId;
                       item.ExpenseItemAddDed = 1;

                       connection.Query(sql, item, trn);
                   }
                   foreach (var item in expenseBill.deductions)
                   {
                       sql = string.Empty;
                       sql += "insert into ExpenseBillItem(ExpenseId, AddDedId, ExpenseItemRate, ExpenseItemQty, ExpenseItemAmount, ExpenseItemAddDed)";
                       sql += " values(@ExpenseId, ISNULL(@AddDedId,0), @ExpenseItemRate, @ExpenseItemQty, @ExpenseItemAmount, @ExpenseItemAddDed)";

                       item.ExpenseId = expenseBill.ExpenseId;
                       item.ExpenseItemAddDed = 2;

                       connection.Query(sql, item, trn);
                   }
                   InsertLoginHistory(dataConnection, expenseBill.CreatedBy, "Create", "Expense Bill", id.ToString(), "0");
                   trn.Commit();
               }
               catch (Exception ex)
               {
                   trn.Rollback();
               }

           }
           return expenseBill.ExpenseNo;
       }
        public IList<ExpenseBillListViewModel> GetList(int id, int supid, DateTime? from, DateTime? to)

        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = string.Empty;
                sql += " select E.ExpenseId, E.ExpenseNo, E.ExpenseDate, S.SupplierName, E.ExpenseBillRef, E.TotalAmount";
                sql += " from ExpenseBill E";
                sql += " inner join Supplier S on E.SupplierId = S.SupplierId";
                sql += " where E.ExpenseId = ISNULL(NULLIF(@id, 0), E.ExpenseId)";
                sql += " and E.SupplierId = ISNULL(NULLIF(@supid, 0), E.SupplierId)";
                sql += " and E.ExpenseDate BETWEEN ISNULL(@from, DATEADD(MONTH, -1, GETDATE())) AND ISNULL(@to, GETDATE()) ";

                return connection.Query<ExpenseBillListViewModel>(sql, new { id = id, supid = supid, to = to, from = from }).ToList();
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

        public int Approve(ExpenseBill model)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string query = @"UPDATE ExpenseBill SET isApproved = 1, ApprovedBy = @ApprovedBy WHERE ExpenseId = @ExpenseId";
                return connection.Execute(query, model);
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

        public int CHECK(int ExpenseId)
        {
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" SELECT Count(ExpenseId)Count FROM ExpenseBill
                                WHERE isApproved =1 and ExpenseId=@ExpenseId";

                var id = connection.Query<int>(sql, new { ExpenseId = ExpenseId }).FirstOrDefault();

                return id;

            }

        }

        /// <summary>
        /// Delete ExpenseBill HD Details
        /// </summary>
        /// <returns></returns>
        public string UpdateExpenseBillHD(ExpenseBill objExpenseBill)
        {
            //string result;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                //string sql = @" DELETE FROM ExpenseBill WHERE ExpenseId=@Id";
                string sql = @"UPDATE ExpenseBill SET ExpenseDate = @ExpenseDate,ExpenseBillRef=@ExpenseBillRef,
                               ExpenseBillDate=@ExpenseBillDate,ExpenseBillDueDate=@ExpenseBillDueDate,
                               SupplierId = @SupplierId,ExpenseRemarks=@ExpenseRemarks,TotalAddition=@TotalAddition,TotalDeduction=@TotalDeduction,
                               TotalAmount=@TotalAmount,CurrencyId=@CurrencyId,SaleOrderId=@SaleOrderId,
                               JobCardId = @JobCardId,OrganizationId = @OrganizationId
                               OUTPUT INSERTED.ExpenseNo WHERE ExpenseId = @ExpenseId";

                {

                    var id = connection.Execute(sql, objExpenseBill);
                    return objExpenseBill.ExpenseNo;

                }

            }
        }
        /// <summary>
        /// Delete SO DT Details
        /// </summary>
        /// <returns></returns>
        public int DeleteExpenseBillDT(int Id)
        {
            int result3 = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM ExpenseBillItem WHERE ExpenseId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }

        public int DeleteExpenseBillHD(int Id)
        {
            int result = 0;
            using (IDbConnection connection = OpenConnection(dataConnection))
            {
                string sql = @" DELETE FROM ExpenseBill WHERE ExpenseId=@Id";

                {

                    var id = connection.Execute(sql, new { Id = Id });
                    return id;

                }

            }
        }
       
    }
}
