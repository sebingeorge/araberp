using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SalesQuotation
    {
        public SalesQuotation()
        {
            SalesQuotationItems = new List<SalesQuotationItem>();
        }

        public int SalesQuotationId { get; set; }
        public string QuotationRefNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerAddress { get; set; }
        public string ContactPerson { get; set; }
        public int? SalesExecutiveId { get; set; }
        public DateTime PredictedClosingDate { get; set; }
        public DateTime QuotationValidToDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public bool? IsQuotationApproved { get; set; }
        public int ApprovedBy { get; set; }
        public decimal? Amount { get; set; }
        public int? QuotationStatus { get; set; }
        public string Remarks { get; set; }
        public int SalesQuotationRejectReasonId { get; set; }
        public string QuotationRejectReason { get; set; }
        public string Competitors { get; set; }
        public string PaymentTerms { get; set; }
        public string DiscountRemarks { get; set; }
        public int? CurrencyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public bool isActive { get; set; }
        public List<SalesQuotationItem> SalesQuotationItems { get; set; } 
    }
}
