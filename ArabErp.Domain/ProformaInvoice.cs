using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class ProformaInvoice
    {
        public int PorformaInvoiceId { get; set; }
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public string PorformaInvoiceNo { get; set; }
        public string PorformaInvoiceRefNo { get; set; }
        public DateTime? PorformaInvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerOrderRef { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string SymbolName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int isProjectBased { get; set; }
        public List<ProformaInvoiceItem> Items { get; set; }
    }
}
