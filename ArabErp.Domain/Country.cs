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
   public class Country
    {
       public int CountryId { get; set; }
       public string CountryRefNo { get; set; }
        [Required]
       public string CountryName { get; set; }
        public string CountryRemarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
