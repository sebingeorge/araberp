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
        public decimal GrandTotal { get; set; }
        public string Status { get; set; }
        public string WorkDescr { get; set; }
        public string RevisionNo { get; set; }
        public string RevisionReason { get; set; }

    }
}
