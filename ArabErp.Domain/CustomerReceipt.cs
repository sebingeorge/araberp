using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class CustomerReceipt
    {
        public int CustomerReceiptId { get; set; }
        [Required]
        public string CustomerReceiptRefNo { get; set; }
        [Required]
        public DateTime CustomerReceiptDate { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public string Against { get; set; }
        public int? SaleOrderId { get; set; }
        public int? JobCardId { get; set; }
        public int? SalesInvoiceId { get; set; }
        [Required]
        public Decimal Amount { get; set; }
        public string SpecialRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string CustomerName { get; set; }
    }
}
