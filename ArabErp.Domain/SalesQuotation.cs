﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public int QuerySheetId { get; set; }
        
        public string QuotationRefNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public string SalesExecutiveName { get; set; }
        public string CustomerAddress { get; set; }
        public string ContactPerson { get; set; }
        public int SalesExecutiveId { get; set; }
        public DateTime PredictedClosingDate { get; set; }
        [Required]
        public DateTime QuotationValidToDate { get; set; }
        [Required]
        [ValidateDateGreaterThan("QuotationDate")]
        //[Display(Name = "foo")]
        public DateTime ExpectedDeliveryDate { get; set; }
       
        public bool? IsQuotationApproved { get; set; }
        public int? ApprovedBy { get; set; }
        public decimal Amount { get; set; }
        public int? QuotationStatus { get; set; }
        public string Remarks { get; set; }
        [Required]
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
        public string RevisionReason { get; set; }
        public int ParentId { get; set; }
        public int RevisionNo { get; set; }
        public int GrantParentId { get; set; }
        public int isProjectBased { get; set; }
         public string ReasonDescription { get; set; }
         public string WorkDescription { get; set; }
         public int? CancelStatus { get; set; }
        public List<SalesQuotationItem> SalesQuotationItems { get; set; }
        public List<SalesQuotationMaterial> Materials { get; set; }
        public int Ageing { get; set; }
        public int DaysLeft { get; set; }
        public bool isWarranty { get; set; }
    }
}
