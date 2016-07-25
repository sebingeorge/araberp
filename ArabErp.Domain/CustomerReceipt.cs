using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class CustomerReceipt
    {
        public int CustomerReceiptId { get; set; }
        public string CustomerReceiptRefNo { get; set; }
        public DateTime CustomerReceiptDate { get; set; }
        public int CustomerId { get; set; }
        public string Against { get; set; }
        public int? SaleOrderId { get; set; }
        public int? JobCardId { get; set; }
        public int? SalesInvoiceId { get; set; }
        public Decimal Amount { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
