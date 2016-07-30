using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class Supplier
    {
        public int? SupplierId { get; set; }
        public string SupplierRefNo { get; set; }
        [Required]
        public string SupplierName { get; set; }
        [Required]
        public string SupplierPrintName { get; set; }
        [Required]
        public int? SupCategoryId { get; set; }
        [Required]
        public DateTime ContractDate { get; set; }
        public int? PurchaseTypeId { get; set; }
        [Required]
        public int? Active { get; set; }
        public string DoorNo { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public int? CountryId { get; set; }
        [Required]
        public string PostBoxNo { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Fax { get; set; }
        public string Email { get; set; }
        [Required]
        public string SupRefAccNo { get; set; }
        public string ContactPerson { get; set; }
        public string Bank { get; set; }
        [Required]
        public string Branch { get; set; }
        public string AccountDetails { get; set; }
        public string SwiftCode { get; set; }
        public string RtgsNo { get; set; }
        public string AccountNo { get; set; }
        public int? DiscountTermsId { get; set; }
        public string DiscountRate { get; set; }
        public int? CurrencyId { get; set; }
        [Required]
        public string PanNo { get; set; }
        public string TinNo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public int? OrganizationId { get; set; }
        public string isActive { get; set; }
        public string SupCategoryName { get; set; }
        public string PurchaseTypeName { get; set; }
        public int? CreditPeriod { get; set; }
        public Decimal? CreditLimit { get; set; }
        public string PaymentTerms { get; set; }
    }
}
