using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
   public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerRefNo { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string CustomerPrintName { get; set; }
        public int CategoryId { get; set; }
        [Required]
        public int LeadSourceId { get; set; }
        public int CurrencyId { get; set; }
        [Required]
        public string DoorNo { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public int Country { get; set; }
        [Required]
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        [Required]
        public string ContactPerson { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public string CusCategoryName { get; set; }
        public string CountryName { get; set; }
        public int? CreditPeriod { get; set; }
    }
   public enum CategoryId
   { Category1, Category2}

   public enum LeadSourceId
   { Lead_Source1, Lead_Source2 }

   public enum CurrencyId
   { Currency1, Currency2 }

   public enum Country
   { Country1, Country2 }
}
