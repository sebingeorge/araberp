using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
     public class SupplyOrderFollowUpList
    {
        public SupplyOrderFollowUpList()
        {
            SupplyOrderFollowups = new List<SupplyOrderFollowup>();
        }
        //public int dummy { get; set; }
        public List<SupplyOrderFollowup> SupplyOrderFollowups { get; set; }
    }
}
