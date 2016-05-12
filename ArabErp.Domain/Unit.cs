using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class Unit
    {
       public int UnitId { get; set; }
       public string UnitRefNo { get; set; }
       public string UnitName { get; set; }
       public string CreatedBy { get; set; }
       public DateTime? CreatedDate { get; set; }
       public int OrganizationId { get; set; }
    }
}
