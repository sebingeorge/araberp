using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public  class SalesTarget
    {
        public int SalesTargetId { get; set; }
        public string SalesTargetRefNo { get; set; }
        public int MonthId { get; set; }
        public string MonthName { get; set; }
        public int WorkDescriptionId { get; set; }
        public string WorkDescrShortName { get; set; }
        public decimal Target { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int FyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
