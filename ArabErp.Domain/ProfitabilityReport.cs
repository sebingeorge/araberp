using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ProfitabilityReport
    {
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal Purchase { get; set; }
        public decimal Expense { get; set; }
        public decimal Labour { get; set; }
        public decimal SalesInvoice { get; set; }
        public string ItemName { get; set; }
        public string Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        //---------------Expense
        public string ExpenseNo { get; set; }
        public string ExpenseDate { get; set; }
        public string Supplier { get; set; }
        //---------------Labour
        public string EmployeeName { get; set; }
        public string JobCardTaskName { get; set; }
        public string ActualHours { get; set; }

    }
}
