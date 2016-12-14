using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class MaterialCostForService : WorkShopRequestItem
    {
        public string WorkShopRequestRefNo { get; set; }
        public string StoreIssueRefNo { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
