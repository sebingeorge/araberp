using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class ConsumptionItem
    {
        public int ConsumptionItemId { get; set; }
        public int ConsumptionId { get; set; }
        public int ItemId { get; set; }
        public string PartNo { get; set; }
        public string Remarks { get; set; }
        public decimal Amount { get; set; }
        public bool isActive { get; set; }
    }
}
