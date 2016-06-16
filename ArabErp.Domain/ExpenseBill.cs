using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ExpenseBill
    {
        public int? ExpenseId { get; set; }
        public string ExpenseNo { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string ExpenseBillRef { get; set; }
        public DateTime ExpenseBillDate { get; set; }
        public DateTime ExpenseBillDueDate { get; set; }
        public int? SupplierId { get; set; }
        public string ExpenseRemarks { get; set; }
    }
}
