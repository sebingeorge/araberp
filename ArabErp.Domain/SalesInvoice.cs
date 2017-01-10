using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class SalesInvoice
    {
        public int SalesInvoiceId { get; set; }
        public int SaleOrderId { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public string CurrentDate { get; set; }
        public string SalesInvoiceNo { get; set; }
        public string SalesInvoiceRefNo { get; set; }
        public DateTime SalesInvoiceDate { get; set; }
        public DateTime SalesInvoiceDueDate { get; set; }
        public string DeliveryChallanRefNo { get; set; }
        public string Customer { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerOrderRef { get; set; }
        public string WorkDescription { get; set; }
        public string VehicleRegChasisNo { get; set; } 
        public int JobCardId { get; set; }
        public string SpecialRemarks { get; set; }
        public string PaymentTerms { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string SaleOrderRefNoWithDate { get; set; }
        public decimal? Addition { get; set; }
        public decimal? Deduction { get; set; }
        public string AdditionRemarks { get; set; }
        public string DeductionRemarks { get; set; }
        public string InvoiceType { get; set; }
        public int isProjectBased { get; set; }
        public string RegistrationNo { get; set; }
        public string ChasisNo { get; set; }
        public string JobCardNo { get; set; }
        public string VehicleOutPassNo { get; set; }
        public DateTime IsApprovedDate { get; set; }
        public int IsApprovedBy { get; set; }
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
        public string ApproveUser { get; set; }
        public string ApproveSig { get; set; }
        public int isService { get; set; }
        public string CreatedDes { get; set; }
        public string ApprovedDes { get; set; }
     
        public List<PrintDescription> PrintDescriptions { get; set; }
        public List<SalesInvoiceItem> SaleInvoiceItems { get; set; }
    }
}
