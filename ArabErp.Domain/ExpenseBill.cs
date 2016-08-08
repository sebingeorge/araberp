using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ExpenseBill
    {
        public int ExpenseId { get; set; }
        public string ExpenseNo { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string ExpenseBillRef { get; set; }
        public DateTime ExpenseBillDate { get; set; }
        public DateTime ExpenseBillDueDate { get; set; }
        public int? SupplierId { get; set; }
        public string ExpenseRemarks { get; set; }
        public decimal TotalAddition { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalAmount { get; set; }
        public int CurrencyId { get; set; }
        public string SoOrJc { get; set; }
        public int? SaleOrderId { get; set; }
        public int? JobCardId { get; set; }
        public List<ExpenseBillItem> ExpenseBillItem{get; set;}
        public List<ExpenseBillItem> deductions { get; set; }
        public int ApprovedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ExpenseBillItem
    {
        public int? ExpenseRowId { get; set; }
        public int? ExpenseId { get; set; }
        public int? AddDedId { get; set; }
        public decimal ExpenseItemRate { get;set;}
        public decimal ExpenseItemQty { get; set; }
        public decimal ExpenseItemAmount { get; set; }
        public int ExpenseItemAddDed { get; set; }
    }
    public class ExpenseBillListViewModel
    {
        public int ExpenseId { get; set; }
        public string ExpenseNo { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string SupplierName { get; set; }
        public string ExpenseBillRef { get; set; }
        public decimal TotalAmount { get; set; }
        public string ExpenseBillDate { get; set; }
        public string ExpenseBillDueDate { get; set; }
        public string SaleOrderRefNo { get; set; }
        public string SaleOrderDate { get; set; }
        public string JobCardNo { get; set; }
        public string JobCardDate { get; set; }
        public string Type { get; set; }
    }
}
