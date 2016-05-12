using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public string CustomerPrintName { get; set; }
        public int CategoryId { get; set; }

        public int LeadSourceId { get; set; }
        public int CurrencyId { get; set; }
        public string DoorNo { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public int Country { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
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
