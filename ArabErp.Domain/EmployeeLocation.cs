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
   public class EmployeeLocation
    {
        public int LocationId { get; set; }
        public string LocationRefNo { get; set; }
        [Required]
        public string LocationName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
