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

     } 

}
