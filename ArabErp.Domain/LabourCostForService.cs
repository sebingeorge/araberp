using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class LabourCostForService
    {
        public string JobCardTaskName { get; set; }
        public decimal ActualHours { get; set; }
        public decimal MinimumRate { get; set; }
        public decimal Amount { get; set; }
    }
}
