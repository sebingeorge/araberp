using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class FreezerUnit
    {
        public int FreezerUnitId { get; set; }
        public string FreezerUnitRefNo { get; set; }
        [Required]
        public string FreezerUnitName { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
