using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SalesQuotationList
    {
        public int SalesQuotationId { get; set; }
        public string QuotationRefNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public decimal Amount { get; set; }
    }
}
