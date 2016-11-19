using System;
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
        public decimal TotalMaterialAmount { get; set; }
        public decimal TotalWorkAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int? QuotationStatus { get; set; }
        public string  SalesQuotationStatusName { get; set; }
        public string Remarks { get; set; }
        [Required]
        public int SalesQuotationStatusId { get; set; }
       [Required]
        public string QuotationStage { get; set; }
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
        public bool isProjectBased { get; set; }
        public bool isAfterSales { get; set; }
        public string Description { get; set; }
       
         public int? CancelStatus { get; set; }
        public List<SalesQuotationItem> SalesQuotationItems { get; set; }
        public List<SalesQuotationMaterial> Materials { get; set; }
        public int Ageing { get; set; }
        public int DaysLeft { get; set; }
        public bool isWarranty { get; set; }
        public bool IsUsed { get; set; }
        public int DeliveryChallanId { get; set; }

        public int ProjectCompletionId { get; set; }
        public decimal? Discount { get; set; }
        public DateTime WarrantyExpiryDate { get; set; }
        public string WorkDescription { get; set; }

        public string DoorNo { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public int Country { get; set; }
        public string CountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string CurrencyName { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        //public string ContactPerson { get; set; }
        
        public string OrganizationName { get; set; }
        public string Image1 { get; set; }
        public string EmpNmae { get; set; }
        public string EmpDesignation { get; set; }
        public string ApprovedUsersig { get; set; }
        public ProjectCompletion ProjectCompleionDetails { get; set; }
        public DeliveryChallan DeliveryChallanDetails { get; set; }

        public int VehicleModelId { get; set; }
    }
}
