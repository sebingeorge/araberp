using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class RateSettingsItems
    {
        public int WorkDescriptionId { get; set; }
        public string WorkDescr { get; set; }
        public decimal MinRate { get; set; }
        public decimal MediumRate { get; set; }
        public decimal MaxRate { get; set; }
    }
}
