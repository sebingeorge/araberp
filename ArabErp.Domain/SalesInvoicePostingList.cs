using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SalesInvoicePostingList
    {
        public int? SalesInvoiceId { get; set; }
        public string SalesInvoiceRefNo { get; set; }
        public DateTime SalesInvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string PaymentTerms { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyName { get; set; }
        public int IsSelected { get; set; }
        public string PostStatus { get; set; }
    }
    public class PendingSalesBillsForPosting
    {
        public List<SalesInvoicePostingList> SalesInvoicePostingList { get; set; }
    }
}
