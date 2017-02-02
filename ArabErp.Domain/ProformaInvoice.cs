using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
  public  class ProformaInvoice
    {
        public int ProformaInvoiceId { get; set; }
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
   
        public string ProformaInvoiceRefNo { get; set; }
        public DateTime ProformaInvoiceDate { get; set; }
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
        public decimal PrintTotalAmount { get; set; }
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
        public string OrganizationRefNo { get; set; }
        public string DoorNo { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string CountryName { get; set; }
        public string CurrencyName { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string Zip { get; set; }
        public string CreateUser { get; set; }
        public string CreateSig { get; set; }
        public string CreatedDes { get; set; }
        public List<ProformaInvoiceItem> Items { get; set; }
        public List<PrintDescription> PrintDescriptions { get; set; }
    }
}
