using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class ItemSellingPrice
    {
        public int ItemSellingPriceId { get; set; }
       public int ItemId { get; set; }
        [Required]
        public string ItemName { get; set; }
        public string PartNo { get; set; }
        public string CategoryName { get; set; }
        public string ItemGroupName { get; set; }
        public string ItemSubGroupName { get; set; }
        public string UnitName { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? Average { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }

        public string OrganizationRefNo { get; set; }
       
        public string OrganizationName { get; set; }
       
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
     
        public string DoorNo { get; set; }
       
        public string Street { get; set; }
        
        public string State { get; set; }
    
        public int Country { get; set; }
        public string CountryName { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public int cmpCode { get; set; }
        public string CompanyName { get; set; }
        public string Image1 { get; set; }
    }
  
   
}

