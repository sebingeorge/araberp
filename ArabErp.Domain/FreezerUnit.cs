using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class FreezerUnit
    {
        public int FreezerUnitId { get; set; }
        public int FreezerUnitRefNo { get; set; }
        public string FreezerUnitName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
