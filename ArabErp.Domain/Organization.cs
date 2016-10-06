using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ArabErp.Domain
{
   public class Organization
    {
        public int OrganizationId { get; set; }
        [Required]
        public string OrganizationRefNo { get; set; }
        [Required]
        public string OrganizationName { get; set; }
        [Required]
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
        public string CreatedBy { get; set; }
        [Required]
        public int? FyId { get; set; }

     } 

}
