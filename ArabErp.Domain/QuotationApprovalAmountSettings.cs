using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class QuotationApprovalAmountSettings
    {
        public int ApprovalCode { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
    }
}
