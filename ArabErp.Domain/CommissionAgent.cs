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
    public class CommissionAgent
    {
        public int CommissionAgentId { get; set; }
         [Required(ErrorMessage = "Please Enter Code")]
        public string CommissionAgentRefNo { get; set; }
         [Required(ErrorMessage = "Please Enter Name")]
        public string CommissionAgentName { get; set; }
         [Required(ErrorMessage = "Please Enter Door No")]
        public string Address1 { get; set; }
         [Required(ErrorMessage = "Please Enter Street")]
        public string Address2 { get; set; }
         [Required(ErrorMessage = "Please Enter Area & City")]
        public string Address3 { get; set; }
         [Required(ErrorMessage = "Please Enter Phone")]
        public string Phone { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
